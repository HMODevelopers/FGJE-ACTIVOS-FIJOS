using ActivosFijos.Models;
using System.Data.Entity;
using System;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PagedList;
using Antlr.Runtime.Misc;
using System.Reflection;
using ActivosFijos.Models.ViewModels;

namespace Helpers
{
    public class ActivosHelper
    {
        ModelContext _db = new ModelContext();

        public IPagedList<PLU_OP_Activos> GetAll(
     string sOrder,
     string numeroinventario,
     string descripcion,
     int IdCategoria,
     int IdConcepto,
     string NumeroSerie,
     int IdAlmacen,
     int IdEstadoFisico,
     int IdEstatusActivo,
     int iPagina = 1,
     int iPerPage = 10)
        {
            // Normaliza parámetros
            numeroinventario = (numeroinventario ?? "").Trim().ToUpperInvariant();
            descripcion = (descripcion ?? "").Trim().ToUpperInvariant();
            NumeroSerie = (NumeroSerie ?? "").Trim().ToUpperInvariant();

            var q = _db.PLU_OP_Activos
                .AsNoTracking()
                .Where(a => a.IdEstatusActivo != 2 && a.IdEstatusActivo != 4);

            // Filtros SÓLO si vienen
            if (!string.IsNullOrEmpty(numeroinventario))
                q = q.Where(a => a.NumeroInventario.StartsWith(numeroinventario));

            if (!string.IsNullOrEmpty(descripcion))
                q = q.Where(a => a.Descripcion.StartsWith(descripcion));

            if (IdCategoria > 0)
                q = q.Where(a => a.IdCategoria == IdCategoria);

            if (IdConcepto > 0)
                q = q.Where(a => a.IdConcepto == IdConcepto);

            if (!string.IsNullOrEmpty(NumeroSerie))
                q = q.Where(a => a.NumeroSerie.StartsWith(NumeroSerie));

            if (IdAlmacen > 0)
                q = q.Where(a => a.IdAlmacen == IdAlmacen);

            if (IdEstadoFisico > 0)
                q = q.Where(a => a.IdEstadoFisico == IdEstadoFisico);

            if (IdEstatusActivo > 0)
                q = q.Where(a => a.IdEstatusActivo == IdEstatusActivo);

            // Ordenamiento: usa un diccionario para mantenerlo limpio y evitar múltiples switches
            // Ojo: si no necesitas ordenar por navegación, prefiere columnas locales.
            switch (sOrder)
            {
                case "#": q = q.OrderBy(a => a.IdActivos); break;
                case "#_desc": q = q.OrderByDescending(a => a.IdActivos); break;
                case "numInventario": q = q.OrderBy(a => a.NumeroInventario); break;
                case "numInventario_desc": q = q.OrderByDescending(a => a.NumeroInventario); break;
                case "descripcion": q = q.OrderBy(a => a.Descripcion); break;
                case "descripcion_desc": q = q.OrderByDescending(a => a.Descripcion); break;
                case "categoria": q = q.OrderBy(a => a.PLU_CAT_CategoriaActivo.NombreCategoria); break;
                case "categoria_desc": q = q.OrderByDescending(a => a.PLU_CAT_CategoriaActivo.NombreCategoria); break;
                case "concepto": q = q.OrderBy(a => a.PLU_CAT_Conceptos.NombreConcepto); break;
                case "concepto_desc": q = q.OrderByDescending(a => a.PLU_CAT_Conceptos.NombreConcepto); break;
                case "clasificador": q = q.OrderBy(a => a.PLU_CAT_Clasificadores.ClasificadorDescripcion); break;
                case "clasificador_desc": q = q.OrderByDescending(a => a.PLU_CAT_Clasificadores.ClasificadorDescripcion); break;
                case "estadofisico": q = q.OrderBy(a => a.PLU_CAT_EstadoFisicoActivo.Descripcion); break;
                case "estadofisico_desc": q = q.OrderByDescending(a => a.PLU_CAT_EstadoFisicoActivo.Descripcion); break;
                default: q = q.OrderBy(a => a.IdActivos); break;
            }

            // Si sólo listás en tabla, considera proyectar a un DTO con las columnas necesarias
            // (esto reduce ancho de banda y puede acelerar).
            return q.ToPagedList(iPagina, iPerPage);
        }

        public ResponseModel Add(PLU_OP_Activos activo, IEnumerable<HttpPostedFileBase> files)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Guardar el activo en la base de datos
                    ctx.Entry(activo).State = EntityState.Added;
                    ctx.SaveChanges();

                    // Si se proporcionaron archivos
                    if (files != null && files.Any())
                    {
                        int index = 0;
                        foreach (var file in files)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                // Generar el nombre del archivo
                                var fileName = $"{activo.IdActivos}_{index}{Path.GetExtension(file.FileName)}";

                                // Guardar el archivo en el servidor
                                var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/FotosActivos"), fileName);
                                file.SaveAs(path);

                                // Crear una nueva instancia de PLU_OP_FotosActivos
                                var fotoActivo = new PLU_OP_FotosActivos
                                {
                                    IdActivos = activo.IdActivos,
                                    RutaFoto = fileName,
                                    Activo = true,
                                    FechaCreacion = DateTime.Now
                                };

                                // Guardar la ruta del archivo en la base de datos
                                ctx.PLU_OP_FotosActivos.Add(fotoActivo);

                                // Incrementar el índice para el siguiente archivo
                                index++;
                            }
                        }

                        // Guardar todos los cambios en la base de datos
                        ctx.SaveChanges();
                    }

                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al agregar activo: " + ex.Message);
                
            }

            return rm;
        }

        public ResponseModel AddMasivo(List<PLU_OP_Activos> activos)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Obtener el último numero de inventario
                    var ultimoActivo = ctx.PLU_OP_Activos
                        .OrderByDescending(a => a.IdActivos)
                        .FirstOrDefault();

                    string ultimoNumeroInventario = ultimoActivo != null ? ultimoActivo.NumeroInventario : "FGJE-CJPMO-0000";
                    string prefijo = ultimoNumeroInventario.Substring(0, ultimoNumeroInventario.LastIndexOf('-') + 1);
                    int ultimoConsecutivo = int.Parse(ultimoNumeroInventario.Substring(ultimoNumeroInventario.LastIndexOf('-') + 1));

                    foreach (var activo in activos)
                    {
                        ultimoConsecutivo++;
                        activo.NumeroInventario = prefijo + ultimoConsecutivo.ToString("D4");

                        // Asignar otros valores si es necesario
                        activo.Activo = true;
                        activo.FechaCreacion = DateTime.Now;
                        activo.IdEstatusActivo = 3;

                        // Guardar el activo en la base de datos
                        ctx.Entry(activo).State = EntityState.Added;
                    }

                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al agregar activos: " + ex.Message);
            }

            return rm;
        }

        public ResponseModel Edit(PLU_OP_Activos activo, IEnumerable<HttpPostedFileBase> files)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // 1) Cargar el activo original desde la BD (trackeado por EF)
                    var activoDb = ctx.PLU_OP_Activos.Find(activo.IdActivos);
                    if (activoDb == null)
                    {
                        rm.SetResponse(false, "No se encontró el activo.");
                        return rm;
                    }

                    var entry = ctx.Entry(activoDb);

                    // 2) Copiar todos los valores del modelo a la entidad cargada
                    //    (aquí todavía no estamos decidiendo qué se va a guardar)
                    entry.CurrentValues.SetValues(activo);

                    // 3) Revisar propiedad por propiedad qué SÍ se debe guardar
                    foreach (var propName in entry.OriginalValues.PropertyNames)
                    {
                        // Si quieres que la llave nunca se marque como modificada:
                        if (propName == "IdActivos")
                        {
                            entry.Property(propName).IsModified = false;
                            continue;
                        }

                        var original = entry.OriginalValues[propName]; // valor en BD
                        var current = entry.CurrentValues[propName];  // valor que viene del modelo

                        // --- REGLA 1: NO borres datos con null si antes había algo ---
                        // Esto evita que campos que no mandaste en el form (vienen null)
                        // sobrescriban valores existentes.
                        if (current == null && original != null)
                        {
                            entry.CurrentValues[propName] = original;  // regresamos valor original
                            entry.Property(propName).IsModified = false;
                            continue;
                        }

                        // --- REGLA 2: Solo marcar como modificado si realmente cambió ---
                        if (Equals(original, current))
                        {
                            // No cambió → no se manda en el UPDATE
                            entry.Property(propName).IsModified = false;
                        }
                        else
                        {
                            // Sí cambió → se manda en el UPDATE
                            entry.Property(propName).IsModified = true;
                        }
                    }

                    // 4) FOTOS (igual que ya lo tenías, usando activoDb)
                    var fotosDelActivo = ctx.PLU_OP_FotosActivos
                                             .Where(x => x.IdActivos == activoDb.IdActivos)
                                             .Select(x => x.RutaFoto)
                                             .ToList();

                    int maximoNumero = 0;
                    if (fotosDelActivo.Any())
                    {
                        foreach (var foto in fotosDelActivo)
                        {
                            // ej: "15_3.jpg" → partes: ["15","3","jpg"]
                            string[] partesRuta = foto.Split('_', '.');
                            string ultimoNumero = partesRuta[partesRuta.Length - 2];

                            if (int.TryParse(ultimoNumero, out int numero) && numero > maximoNumero)
                            {
                                maximoNumero = numero;
                            }
                        }
                    }

                    int index = maximoNumero + 1;

                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                var fileName = $"{activoDb.IdActivos}_{index}{Path.GetExtension(file.FileName)}";
                                var path = Path.Combine(
                                    HttpContext.Current.Server.MapPath("~/Content/FotosActivos"),
                                    fileName
                                );
                                file.SaveAs(path);

                                var fotoActivo = new PLU_OP_FotosActivos
                                {
                                    IdActivos = activoDb.IdActivos,
                                    RutaFoto = fileName,
                                    Activo = true,
                                    FechaCreacion = DateTime.Now
                                };

                                ctx.PLU_OP_FotosActivos.Add(fotoActivo);
                                index++;
                            }
                        }
                    }

                    // 5) Guardar cambios
                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al editar activo: " + ex.Message);
            }

            return rm;
        }


        public ResponseModel Baja(PLU_OP_BajasActivos bajaActivo, HttpPostedFileBase file)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Guardar el archivo en una carpeta específica y actualizar el oficio existente
                    if (file != null && file.ContentLength > 0)
                    {
                        var uploadDirectory = HttpContext.Current.Server.MapPath("~/Content/OficiosBajas");
                        if (!Directory.Exists(uploadDirectory))
                        {
                            Directory.CreateDirectory(uploadDirectory);
                        }

                        // Asignar el nombre del archivo
                        var fileName = $"{bajaActivo.PLU_OP_OficiosBajas.FolioOficio}_{bajaActivo.IdBaja}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadDirectory, fileName);

                        // Guardar el archivo en el servidor
                        file.SaveAs(filePath);


                        // Guardar el registro de baja activo en la base de datos
                        bajaActivo.PLU_OP_OficiosBajas.FolioOficio = bajaActivo.PLU_OP_OficiosBajas.FolioOficio;
                        bajaActivo.PLU_OP_OficiosBajas.RutaOficio = fileName;
                        bajaActivo.IdUsuario = SessionHelper.GetUser();
                        bajaActivo.PLU_OP_OficiosBajas.Activo = true;
                        bajaActivo.PLU_OP_OficiosBajas.FechaCreacion = DateTime.Now;
                       
                    }

                    ctx.Entry(bajaActivo).State = EntityState.Added;


                    // Obtener el activo relacionado y actualizar su estado
                    var activo = ctx.PLU_OP_Activos.Find(bajaActivo.IdActivos);
                    if (activo != null)
                    {
                        activo.IdEstatusActivo = 1; // Asigna el nuevo IdEstatusActivo aquí
                        ctx.Entry(activo).State = EntityState.Modified;
                    }

                    // Guardar todos los cambios en la base de datos
                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al crear baja del activo: " + ex.Message);
            }

            return rm;
        }

        public ResponseModel DeleteFoto(int IdFoto)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Cargar el activo desde la base de datos
                    var foto = ctx.PLU_OP_FotosActivos.Find(IdFoto);
                    if (foto == null)
                    {
                        rm.SetResponse(false, "No se encontró la foto con ID: " + IdFoto);
                        return rm;
                    }

                    // Marcar la foto como inactiva
                    foto.Activo = false;

                    // Guardar cambios en la base de datos
                    ctx.SaveChanges();

                    // Ruta del archivo
                    var filePath = HttpContext.Current.Server.MapPath($"~/Content/FotosActivos/{foto.RutaFoto}");

                    // Verificar si el archivo existe y eliminarlo
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al borrar activo: " + ex.Message);
            }

            return rm;
        }

        private IQueryable<PLU_OP_Activos> ObtenerActivos()
        {
            return _db.PLU_OP_Activos.Where(a => a.IdEstatusActivo != 2 && a.IdEstatusActivo != 4);
        }

        public IPagedList<BajasActivosViewModel> GetAllBajasActivos(string sOrder, int iPagina, int iPerPage, string folioBaja, string numeroLote, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var vModel = ObtenerBajasActivos();

            if (!string.IsNullOrWhiteSpace(folioBaja))
                vModel = vModel.Where(a => a.FolioBaja.Contains(folioBaja));
            // NumeroLote (int?)
            if (!string.IsNullOrWhiteSpace(numeroLote))
            {
                if (int.TryParse(numeroLote, out var nLote))
                    vModel = vModel.Where(a => a.NumeroLote.HasValue && a.NumeroLote.Value == nLote);
                else
                    // Si escriben algo no numérico, no filtra por NumeroLote
                    // (o podrías decidir limpiar el campo en UI)
                    vModel = vModel;
            }

            // Rango de fechas (inclusivo)
            if (fechaDesde.HasValue)
            {
                var d = fechaDesde.Value.Date;
                vModel = vModel.Where(a => DbFunctions.TruncateTime(a.FechaCreacion) >= d);
            }
            if (fechaHasta.HasValue)
            {
                var h = fechaHasta.Value.Date;
                vModel = vModel.Where(a => DbFunctions.TruncateTime(a.FechaCreacion) <= h);
            }

            // Ordenamiento
            switch (sOrder)
            {
                case "FolioBaja":
                    vModel = vModel.OrderBy(x => x.FolioBaja);
                    break;
                case "FolioBaja_desc":
                    vModel = vModel.OrderByDescending(x => x.FolioBaja);
                    break;
                case "UsuarioCambio":
                    vModel = vModel.OrderBy(x => x.UsuarioBaja);
                    break;
                case "UsuarioCambio_desc":
                    vModel = vModel.OrderByDescending(x => x.UsuarioBaja);
                    break;
                case "FechaCreacion":
                    vModel = vModel.OrderBy(x => x.FechaCreacion);
                    break;
                case "FechaCreacion_desc":
                    vModel = vModel.OrderByDescending(x => x.FechaCreacion);
                    break;
                case "NumeroActivos":
                    vModel = vModel.OrderBy(x => x.NumeroBajas);
                    break;
                case "NumeroActivos_desc":
                    vModel = vModel.OrderByDescending(x => x.NumeroBajas);
                    break;
                case "OficioCambio":
                    vModel = vModel.OrderBy(x => x.OficioBaja);
                    break;
                case "OficioCambio_desc":
                    vModel = vModel.OrderByDescending(x => x.OficioBaja);
                    break;
                default:
                    vModel = vModel.OrderBy(x => x.FolioBaja).ThenBy(x => x.FechaCreacion);
                    break;
            }

            return vModel.ToPagedList(iPagina, iPerPage);
        }

        public IPagedList<DetalleBajasActivosViewModel> GetAllBajasActivosDetalle(string sOrder, int iPagina, int iPerPage, string folioBaja,string numeroLote, string numeroInventario,   DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var q = ObtenerBajasActivosDetalle(); // IQueryable<DetalleBajasActivosViewModel>

            // Filtros
            if (!string.IsNullOrWhiteSpace(folioBaja))
                q = q.Where(a => a.FolioBajas.Contains(folioBaja));

            if (!string.IsNullOrWhiteSpace(numeroLote) && int.TryParse(numeroLote, out var nLote))
                q = q.Where(a => a.NumeroLote.HasValue && a.NumeroLote.Value == nLote);

            if (!string.IsNullOrWhiteSpace(numeroInventario))
                q = q.Where(a => a.NumeroInventario.Contains(numeroInventario));

            if (fechaDesde.HasValue)
                q = q.Where(a => DbFunctions.TruncateTime(a.FechaBaja) >= fechaDesde.Value.Date);

            if (fechaHasta.HasValue)
                q = q.Where(a => DbFunctions.TruncateTime(a.FechaBaja) <= fechaHasta.Value.Date);

            // Ordenamiento
            switch (sOrder)
            {
                case "FolioBaja": q = q.OrderBy(x => x.FolioBajas); break;
                case "FolioBaja_desc": q = q.OrderByDescending(x => x.FolioBajas); break;

                case "NumeroLote": q = q.OrderBy(x => x.NumeroLote); break;
                case "NumeroLote_desc": q = q.OrderByDescending(x => x.NumeroLote); break;

                case "NumeroInventario": q = q.OrderBy(x => x.NumeroInventario); break;
                case "NumeroInventario_desc": q = q.OrderByDescending(x => x.NumeroInventario); break;

                case "Categoria": q = q.OrderBy(x => x.Categoria); break;
                case "Categoria_desc": q = q.OrderByDescending(x => x.Categoria); break;

                case "Descripcion": q = q.OrderBy(x => x.Descripcion); break;
                case "Descripcion_desc": q = q.OrderByDescending(x => x.Descripcion); break;

                case "NumeroSerie": q = q.OrderBy(x => x.NumeroSerie); break;
                case "NumeroSerie_desc": q = q.OrderByDescending(x => x.NumeroSerie); break;

                case "Marca": q = q.OrderBy(x => x.Marca); break;
                case "Marca_desc": q = q.OrderByDescending(x => x.Marca); break;

                case "FechaBaja": q = q.OrderBy(x => x.FechaBaja); break;
                case "FechaBaja_desc": q = q.OrderByDescending(x => x.FechaBaja); break;

                default:
                    q = q.OrderBy(x => x.FolioBajas).ThenBy(x => x.FechaBaja);
                    break;
            }

            return q.ToPagedList(iPagina, iPerPage);
        }

        private IQueryable<DetalleBajasActivosViewModel> ObtenerBajasActivosDetalle()
        {
            // Ajusta nombres de tablas/relaciones según tu modelo EF.
            // Tomo base de tu detalle existente.
            return _db.PLU_OP_BajasActivos
                .Where(b => b.Activo)
                .Select(b => new DetalleBajasActivosViewModel
                {
                    FolioBajas = b.FolioBaja,
                    NumeroLote = b.NumeroLote,
                    NumeroInventario = b.PLU_OP_Activos.NumeroInventario, // ajusta si el campo está en otra entidad
                    Categoria =   b.PLU_OP_Activos.PLU_CAT_CategoriaActivo.NombreCategoria, // ajusta navegación
                    Descripcion = b.PLU_OP_Activos.Descripcion,
                    NumeroSerie = b.PLU_OP_Activos.NumeroSerie,
                    Marca = b.PLU_OP_Activos.PLU_CAT_MarcaActivo.NombreMarca,
                    FechaBaja = b.FechaCreacion // o el campo real de fecha de baja si es otro
                });
        }


        private IQueryable<BajasActivosViewModel> ObtenerBajasActivos()
        {
            return _db.PLU_OP_BajasActivos
                .Where(x => x.Activo)
                .GroupBy(x => x.FolioBaja)
                .Select(g => new BajasActivosViewModel
                {
                    IdBajasActivo = g.FirstOrDefault() != null ? g.FirstOrDefault().IdBaja : 0,
                    FolioBaja = g.Key,
                    NumeroLote = g.FirstOrDefault().NumeroLote,
                    NumeroBajas = g.Select(x => x.IdActivos).Distinct().Count(),
                    OficioBaja = g.FirstOrDefault().PLU_OP_OficiosBajas != null ? g.FirstOrDefault().PLU_OP_OficiosBajas.RutaOficio : "Sin Oficio",
                    UsuarioBaja = g.FirstOrDefault().PLU_CONF_Usuario != null ? g.FirstOrDefault().PLU_CONF_Usuario.Nombres + " " + g.FirstOrDefault().PLU_CONF_Usuario.Apellidos : "Usuario Desconocido",
                    FechaCreacion = g.FirstOrDefault() != null ? g.FirstOrDefault().FechaCreacion : DateTime.MinValue
                })
                .OrderBy(x => x.FolioBaja)
                .ThenBy(x => x.FechaCreacion);
        }


    }
}