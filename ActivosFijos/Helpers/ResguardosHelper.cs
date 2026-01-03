using ActivosFijos.Models;
using ActivosFijos.Models.ViewModels;
using Helpers;
using Microsoft.Ajax.Utilities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ActivosFijos.Helpers
{
    public class ResguardosHelper
    {
        ModelContext _db = new ModelContext();

        public IPagedList<PLU_OP_Activos> GetAll(string sOrder, string numeroinventario, string descripcion, int IdCategoria,int numeroEmpleado,string apellidoPaterno, string apellidoMaterno,string nombreEmpleado, string NumeroSerie, int iPagina , int iPerPage )
        {
            var vModel = ObtenerActivos();


            vModel = vModel
                    .Where(a =>
                        (string.IsNullOrEmpty(numeroinventario) || a.NumeroInventario.Contains(numeroinventario)) &&
                        (string.IsNullOrEmpty(descripcion) || a.Descripcion.Contains(descripcion)) &&
                        (IdCategoria == 0 || a.IdCategoria == IdCategoria) &&
                        (numeroEmpleado == 0 || (a.PLU_OP_Resguardo != null && a.PLU_OP_Resguardo.PLU_OP_Empleados != null && a.PLU_OP_Resguardo.PLU_OP_Empleados.NumeroEmpleado == numeroEmpleado)) &&
                        (string.IsNullOrEmpty(apellidoPaterno) || (a.PLU_OP_Resguardo != null && a.PLU_OP_Resguardo.PLU_OP_Empleados != null && a.PLU_OP_Resguardo.PLU_OP_Empleados.ApellidoP == apellidoPaterno)) &&
                        (string.IsNullOrEmpty(apellidoMaterno) || (a.PLU_OP_Resguardo != null && a.PLU_OP_Resguardo.PLU_OP_Empleados != null && a.PLU_OP_Resguardo.PLU_OP_Empleados.ApellidoM == apellidoMaterno)) &&
                        (string.IsNullOrEmpty(nombreEmpleado) || (a.PLU_OP_Resguardo != null && a.PLU_OP_Resguardo.PLU_OP_Empleados != null && a.PLU_OP_Resguardo.PLU_OP_Empleados.Nombres == nombreEmpleado)) &&
                        (string.IsNullOrEmpty(NumeroSerie) || a.NumeroSerie == NumeroSerie)
                    )
                    .Select(a => new { a.IdActivos }) // Solo tomamos IdActivos para eliminar duplicados
                    .Distinct() // Eliminamos los duplicados
                    .Join(_db.PLU_OP_Activos, x => x.IdActivos, a => a.IdActivos, (x, a) => a);

            var count = vModel.Count();

            switch (sOrder)
            {
                case "#":
                    vModel = vModel.OrderBy(a => a.IdActivos);
                    break;
                case "#_desc":
                    vModel = vModel.OrderByDescending(a => a.IdActivos);
                    break;
                case "numeroResguardo":
                    vModel = vModel.OrderBy(a => a.NumeroResguardo);
                    break;
                case "numeroResguardo_desc":
                    vModel = vModel.OrderByDescending(a => a.NumeroResguardo);
                    break;
                case "numInventario":
                    vModel = vModel.OrderBy(a => a.NumeroInventario);
                    break;
                case "numInventario_desc":
                    vModel = vModel.OrderByDescending(a => a.NumeroInventario);
                    break;
                case "descripcion":
                    vModel = vModel.OrderBy(a => a.Descripcion);
                    break;
                case "descripcion_desc":
                    vModel = vModel.OrderByDescending(a => a.Descripcion);
                    break;
                case "categoria":
                    vModel = vModel.OrderBy(a => a.PLU_CAT_CategoriaActivo.NombreCategoria);
                    break;
                case "categoria_desc":
                    vModel = vModel.OrderByDescending(a => a.PLU_CAT_CategoriaActivo.NombreCategoria);
                    break;
                case "estadofisico":
                    vModel = vModel.OrderBy(a => a.PLU_CAT_EstadoFisicoActivo.Descripcion);
                    break;
                case "estadofisico_desc":
                    vModel = vModel.OrderByDescending(a => a.PLU_CAT_EstadoFisicoActivo.Descripcion);
                    break;
                case "nombreEmpleado":
                    vModel = vModel.OrderBy(a => a.PLU_OP_Resguardo.PLU_OP_Empleados.NombreCompleto);
                    break;
                case "nombreEmpleado_desc":
                    vModel = vModel.OrderByDescending(a => a.PLU_OP_Resguardo.PLU_OP_Empleados.NombreCompleto);
                    break;
                default:
                    vModel = vModel.OrderBy(a => a.IdActivos);
                    break;
            }


            return vModel.ToPagedList(iPagina, iPerPage);

        }

        private IQueryable<PLU_OP_Activos> ObtenerActivos()
        {
           var activos = _db.PLU_OP_Activos.Where(a => a.IdEstatusActivo == 2).GroupBy(a => a.IdActivos).Select(g => g.OrderBy(a => a.IdActivos).FirstOrDefault()); 
           
           return activos;
        }


        public IPagedList<PLU_OP_Activos> GetAllAgregar(string numeroinventario, string descripcion, string numeroserie, int iPagina = 1, int iPerPage = 10)
        {

            var vModel = ObtenerActivosAgregar();

            vModel = vModel.Where(a => (numeroinventario.Length == 0 || a.NumeroInventario.Contains(numeroinventario))
                && (descripcion.Length == 0 || a.Descripcion.Contains(descripcion))
                && (numeroserie.Length == 0 || a.NumeroSerie.Contains(numeroserie))
                );

            return vModel.ToPagedList(iPagina, iPerPage);

        }

        private IQueryable<PLU_OP_Activos> ObtenerActivosAgregar()
        {
            return _db.PLU_OP_Activos.Where(a => a.IdEstatusActivo == 3).OrderBy(a => a.IdActivos); ;
        }


        public IPagedList<PLU_OP_Activos> GetAllCambiar(string numeroinventario, string descripcion,int numeroempleado,string nombres,string apelllidop,string NumeroSerie,  int iPagina = 1, int iPerPage = 10)
        {

            var vModel = ObtenerActivosCambiar();

            vModel = vModel.Where(a => (numeroinventario.Length == 0 || a.NumeroInventario.Contains(numeroinventario))
                && (descripcion.Length == 0 || a.Descripcion.Contains(descripcion))
                && (numeroempleado  == 0 ||  a.PLU_OP_Resguardo.NumeroEmpleado == numeroempleado)
                && (NumeroSerie.Length == 0 || a.NumeroSerie.Contains(NumeroSerie))
                );

            return vModel.ToPagedList(iPagina, iPerPage);

        }

        private IQueryable<PLU_OP_Activos> ObtenerActivosCambiar()
        {
            return _db.PLU_OP_Activos.Where(a => a.IdEstatusActivo == 2).OrderBy(a => a.IdActivos); 
        }


        public IPagedList<AltasActivosViewModel> GetAllAltasActivos(string sOrder, int iPagina, int iPerPage , string NombreResguardante , DateTime? FechaAlta)
        {

            var vModel = ObtenerAltasActivos();

            vModel = vModel.Where(a => (!FechaAlta.HasValue || DbFunctions.TruncateTime(a.FechaCreacion) == DbFunctions.TruncateTime(FechaAlta.Value))
            && (NombreResguardante.Length == 0 || a.NombreReguardante.Contains(NombreResguardante)));

            // Ordenar según el parámetro sOrder
            switch (sOrder)
            {
                case "FolioAlta":
                    vModel = vModel.OrderBy(x => x.FolioAlta);
                    break;
                case "FolioAlta_desc":
                    vModel = vModel.OrderByDescending(x => x.FolioAlta);
                    break;
                case "UsuarioAlta":
                    vModel = vModel.OrderBy(x => x.UsuarioAlta);
                    break;
                case "UsuarioAlta_desc":
                    vModel = vModel.OrderByDescending(x => x.UsuarioAlta);
                    break;
                case "NombreResguardanteAlta":
                    vModel = vModel.OrderBy(x => x.NombreReguardante);
                    break;
                case "NombreResguardanteAlta_desc":
                    vModel = vModel.OrderByDescending(x => x.NombreReguardante);
                    break;
                case "FechaCreacion":
                    vModel = vModel.OrderBy(x => x.FechaCreacion);
                    break;
                case "FechaCreacion_desc":
                    vModel = vModel.OrderByDescending(x => x.FechaCreacion);
                    break;
                default:
                    vModel = vModel.OrderBy(x => x.FolioAlta); // Orden predeterminado
                    break;
            }

            // Aplicar paginación
            return vModel.ToPagedList(iPagina, iPerPage);
        }


        private IQueryable<AltasActivosViewModel> ObtenerAltasActivos()
        {
            return _db.PLU_OP_AltasActivos
                .Where(x => x.Activo)
                .GroupBy(x => x.FolioAlta)
                .Select(g => new AltasActivosViewModel
                {
                    IdAltaActivos = g.FirstOrDefault() != null ? g.FirstOrDefault().IdAltaActivos : 0,
                    FolioAlta = g.Key,
                    NumeroAltas =  g.Select(x => x.IdActivos).Distinct().Count(),
                    OficioAlta = g.FirstOrDefault().PLU_OP_OficiosAltas != null ? g.FirstOrDefault().PLU_OP_OficiosAltas.RutaOficio : "Sin Oficio",
                    NombreReguardante = g.FirstOrDefault().PLU_OP_Activos.PLU_OP_Resguardo.PLU_OP_Empleados != null ? g.FirstOrDefault().PLU_OP_Activos.PLU_OP_Resguardo.PLU_OP_Empleados.Nombres + " " +  g.FirstOrDefault().PLU_OP_Activos.PLU_OP_Resguardo.PLU_OP_Empleados.ApellidoP + " " + g.FirstOrDefault().PLU_OP_Activos.PLU_OP_Resguardo.PLU_OP_Empleados.ApellidoM : "Sin Responsable",
                    UsuarioAlta = g.FirstOrDefault().PLU_CONF_Usuario != null ? g.FirstOrDefault().PLU_CONF_Usuario.Nombres + " " + g.FirstOrDefault().PLU_CONF_Usuario.Apellidos : "Usuario Desconocido",
                    FechaCreacion = g.FirstOrDefault() != null ? g.FirstOrDefault().FechaCreacion : DateTime.MinValue
                })
                .OrderBy(x => x.FolioAlta)
                .ThenBy(x => x.FechaCreacion);
        }



        public IPagedList<CambiosActivosViewModel> GetAllCambiosActivos(string sOrder, int iPagina, int iPerPage, string NombreResguardante, DateTime? FechaCambio)
        {

            var vModel = ObtenerCambiosActivos();

            vModel = vModel.Where(a => (!FechaCambio.HasValue || DbFunctions.TruncateTime(a.FechaCreacion) == DbFunctions.TruncateTime(FechaCambio.Value))
            && (NombreResguardante.Length == 0 || a.NombreReguardante.Contains(NombreResguardante)));

            // Ordenar según el parámetro sOrder
            switch (sOrder)
            {
                case "FolioCambio":
                    vModel = vModel.OrderBy(x => x.FolioCambio);
                    break;
                case "FolioCambio_desc":
                    vModel = vModel.OrderByDescending(x => x.FolioCambio);
                    break;
                case "UsuarioCambio":
                    vModel = vModel.OrderBy(x => x.UsuarioCambio);
                    break;
                case "UsuarioCambio_desc":
                    vModel = vModel.OrderByDescending(x => x.UsuarioCambio);
                    break;
                case "NombreResguardanteCambio":
                    vModel = vModel.OrderBy(x => x.NombreReguardante);
                    break;
                case "NombreResguardanteCambio_desc":
                    vModel = vModel.OrderByDescending(x => x.NombreReguardante);
                    break;
                case "FechaCreacion":
                    vModel = vModel.OrderBy(x => x.FechaCreacion);
                    break;
                case "FechaCreacion_desc":
                    vModel = vModel.OrderByDescending(x => x.FechaCreacion);
                    break;
                default:
                    vModel = vModel.OrderBy(x => x.FolioCambio); // Orden predeterminado
                    break;
            }

            // Aplicar paginación
            return vModel.ToPagedList(iPagina, iPerPage);
        }


        private IQueryable<CambiosActivosViewModel> ObtenerCambiosActivos()
        {
            return _db.PLU_OP_CambiosActivos
                .Where(x => x.Activo)
                .GroupBy(x => x.FolioCambio)
                .Select(g => new CambiosActivosViewModel
                {
                    IdCambioActivo = g.FirstOrDefault() != null ? g.FirstOrDefault().IdCambioActivo : 0,
                    FolioCambio = g.Key,
                    NumeroCambios = g.Select(x => x.IdActivos).Distinct().Count(),
                    OficioCambio = g.FirstOrDefault().PLU_OP_OficiosCambios != null ? g.FirstOrDefault().PLU_OP_OficiosCambios.RutaOficio : "Sin Oficio",
                    NombreReguardante = g.FirstOrDefault().PLU_OP_Activos.PLU_OP_Resguardo.PLU_OP_Empleados != null ? g.FirstOrDefault().PLU_OP_Activos.PLU_OP_Resguardo.PLU_OP_Empleados.Nombres + " " + g.FirstOrDefault().PLU_OP_Activos.PLU_OP_Resguardo.PLU_OP_Empleados.ApellidoP + " " + g.FirstOrDefault().PLU_OP_Activos.PLU_OP_Resguardo.PLU_OP_Empleados.ApellidoM : "Sin Responsable",
                    UsuarioCambio = g.FirstOrDefault().PLU_CONF_Usuario != null ? g.FirstOrDefault().PLU_CONF_Usuario.Nombres + " " + g.FirstOrDefault().PLU_CONF_Usuario.Apellidos : "Usuario Desconocido",
                    FechaCreacion = g.FirstOrDefault() != null ? g.FirstOrDefault().FechaCreacion : DateTime.MinValue
                })
                .OrderBy(x => x.FolioCambio)
                .ThenBy(x => x.FechaCreacion);
        }



        /*----------------------------------------------------------- Altas de Resguardo  -------------------------------------------------------------------------------------------------*/


        public async Task<ResponseModel> AgregarResguardoAsync(int NumeroEmpleado, string FolioOficio, HttpPostedFileBase file, List<int> activosSeleccionados, DateTime FechaCreacion)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Guardar el archivo del oficio
                    var oficioAlta = await GuardarOficioAltasAsync(ctx, FolioOficio, file, FechaCreacion);

                    // Obtener o crear el resguardo del empleado
                    var resguardo = await ObtenerOCrearResguardoAsync(ctx, NumeroEmpleado, FechaCreacion);

                    // Asignar los activos seleccionados al resguardo
                    await AsignarActivosAsync(ctx, activosSeleccionados, resguardo, oficioAlta, FechaCreacion);

                    // Guardar los cambios finales
                    await ctx.SaveChangesAsync();
                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ObtenerMensajeError(ex);
                rm.SetResponse(false, errorMessage);
            }

            return rm;
        }

        // Función para guardar el oficio
        private async Task<PLU_OP_OficiosAltas> GuardarOficioAltasAsync(ModelContext ctx, string FolioOficio, HttpPostedFileBase file, DateTime FechaCreacion)
        {
            try
            {
                if (file == null || file.ContentLength <= 0)
                    return null;

                string fileName = $"{FolioOficio}{Path.GetExtension(file.FileName)}";
                string uploadDirectory = HttpContext.Current.Server.MapPath("~/Content/OficiosAltas");

                if (!Directory.Exists(uploadDirectory))
                    Directory.CreateDirectory(uploadDirectory);

                string filePath = Path.Combine(uploadDirectory, fileName);
                file.SaveAs(filePath); // No hay versión async para esto

                var oficioAlta = new PLU_OP_OficiosAltas
                {
                    FolioOficio = FolioOficio,
                    RutaOficio = fileName,
                    Activo = true,
                    FechaCreacion = FechaCreacion
                };

                ctx.PLU_OP_OficiosAltas.Add(oficioAlta);
                await ctx.SaveChangesAsync();

                return oficioAlta;
            }
            catch (Exception ex)
            {
                throw new Exception(ObtenerMensajeError(ex));
            }
        }

        // Función para obtener o crear un resguardo
        private async Task<PLU_OP_Resguardo> ObtenerOCrearResguardoAsync(ModelContext ctx, int NumeroEmpleado, DateTime FechaCreacion)
        {
            try
            {
                var resguardo = await ctx.PLU_OP_Resguardo.FirstOrDefaultAsync(r => r.NumeroEmpleado == NumeroEmpleado);

                if (resguardo == null)
                {
                    var empleado = await ctx.PLU_OP_Empleados.FirstOrDefaultAsync(x => x.NumeroEmpleado == NumeroEmpleado);
                    if (empleado == null)
                        throw new Exception($"Empleado con NúmeroEmpleado {NumeroEmpleado} no encontrado.");

                    var ultimoResguardo = await ctx.PLU_OP_Resguardo.OrderByDescending(r => r.NumeroResguardo).FirstOrDefaultAsync();
                    int nuevoNumeroResguardo = ultimoResguardo != null ? ultimoResguardo.NumeroResguardo + 1 : 1;

                    resguardo = new PLU_OP_Resguardo
                    {
                        IdEmpleado = empleado.IdEmpleado,
                        NumeroEmpleado = NumeroEmpleado,
                        NumeroResguardo = nuevoNumeroResguardo,
                        Activo = true,
                        FechaCreacion = FechaCreacion
                    };

                    ctx.PLU_OP_Resguardo.Add(resguardo);
                    await ctx.SaveChangesAsync();
                }

                return resguardo;
            }
            catch (Exception ex)
            {
                throw new Exception(ObtenerMensajeError(ex));
            }
        }

        // Función para asignar activos seleccionados al resguardo
        private async Task AsignarActivosAsync(ModelContext ctx, List<int> activosSeleccionados, PLU_OP_Resguardo resguardo, PLU_OP_OficiosAltas oficioAlta, DateTime FechaCreacion)
        {
            using (var transaction = ctx.Database.BeginTransaction())
            {
                try
                {
                    string Folio;

                    // Generar un folio único
                    do
                    {
                        Folio = GenerarFolioAgregar();

                    } while (await ctx.PLU_OP_AltasActivos.AnyAsync(a => a.FolioAlta == Folio)); // Verificar si ya existe

                    foreach (var activoId in activosSeleccionados)
                    {
                        var altaActivo = new PLU_OP_AltasActivos
                        {
                            IdActivos = activoId,
                            FolioAlta = Folio,
                            IdEmpleado = resguardo.IdEmpleado,
                            IdOficioAlta = oficioAlta?.IdOficioAlta,
                            IdUsuario = SessionHelper.GetUser(),
                            Activo = true,
                            FechaCreacion = FechaCreacion
                        };
                        ctx.PLU_OP_AltasActivos.Add(altaActivo);

                        // Actualizar el ID de resguardo en la tabla de activos
                        var activo = await ctx.PLU_OP_Activos.FindAsync(activoId);
                        if (activo != null)
                        {
                            activo.IdResguardo = resguardo.IdResguardo;
                            activo.NumeroResguardo = resguardo.NumeroResguardo;
                            activo.NumeroEmpleado = resguardo.NumeroEmpleado;
                            activo.IdEstatusActivo = 2;
                            ctx.Entry(activo).State = EntityState.Modified;
                        }

                        // Verificar si ya existe un registro en InventarioFisico para el activo y el año actual
                        var year = FechaCreacion.Year;
                        var inventarioExistente = await ctx.PLU_OP_InventarioFisico.Where(i => i.NumeroEmpleado == resguardo.NumeroEmpleado && i.FechaInventario.Year == year).FirstOrDefaultAsync();

                        if (inventarioExistente != null)
                        {
                            // Si existe el registro de InventarioFisico, proceder con la inserción
                            var nuevoInventarioFisico = new PLU_OP_InventarioFisico
                            {

                                FolioInventario = inventarioExistente.FolioInventario, // Usamos el FolioInventario existente
                                IdActivo = activoId,
                                IdUsuario = inventarioExistente.IdUsuario,
                                NumeroEmpleado = inventarioExistente.NumeroEmpleado,
                                FechaInventario = inventarioExistente.FechaInventario,
                                Activo = false,
                                FechaCreacion = DateTime.Now

                            };

                            // Agregar el nuevo registro en InventarioFisico
                            ctx.PLU_OP_InventarioFisico.Add(nuevoInventarioFisico);
                        }
                        else
                        {
                            // Si no existe el registro, no hacemos nada
                            Console.WriteLine($"No se encuentra el registro de InventarioFisico para el activo {activoId} este año.");
                        }


                    }

                    ctx.SaveChanges(); // Guardar cambios en BD
                    transaction.Commit(); // Confirmar transacción
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Revertir cambios en caso de error
                    throw new Exception(ObtenerMensajeError(ex));
                }
            }
        }

        /*----------------------------------------------------------- Altas de Resguardo  -------------------------------------------------------------------------------------------------*/


        /*-----------------------------------------------------  Cambio de Resguardo  -------------------------------------------------------------------------------------------------*/


        public async Task<ResponseModel> CambiarResguardoAsync(int NumeroEmpleado, string FolioOficio, HttpPostedFileBase file, List<int> activosSeleccionados, DateTime FechaCreacion)
        {
            var rm = new ResponseModel();
            try
            {
                using (var ctx = new ModelContext())
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        // Guardar el oficio y obtener su ID
                        var oficioCambio = await GuardarOficioAsync(ctx, FolioOficio, file, FechaCreacion);

                        // Determinar si el empleado es "Almacen"
                        bool esAlmacen = NumeroEmpleado == 999998;

                        // Obtener o crear el resguardo
                        var resguardo = esAlmacen ? null : await ObtenerOcrearResguardoAsync(ctx, NumeroEmpleado, FechaCreacion);

                        string Folio;
                        // Generar un folio único
                        do
                        {
                            Folio = GenerarFolioCambio(); 

                        } while (await ctx.PLU_OP_AltasActivos.AnyAsync(a => a.FolioAlta == Folio)); // Verificar si ya existe

                        // Procesar los activos seleccionados
                        foreach (var activoId in activosSeleccionados)
                        {
                            await ProcesarActivoAsync(ctx, activoId, NumeroEmpleado, esAlmacen, resguardo, oficioCambio, Folio, FechaCreacion);
                        }

                        await ctx.SaveChangesAsync();
                        transaction.Commit();
                        rm.SetResponse(true, "Cambio de resguardo realizado exitosamente.");
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo detallado de errores
                rm.SetResponse(false, ObtenerMensajeError(ex));
            }

            return rm;
        }

        private async Task<PLU_OP_OficiosCambios> GuardarOficioAsync(ModelContext ctx, string FolioOficio, HttpPostedFileBase file, DateTime FechaCreacion)
        {
            if (file == null || file.ContentLength <= 0) return null;

            var uploadDirectory = HttpContext.Current.Server.MapPath("~/Content/OficiosCambios");
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            var fileName = $"{FolioOficio}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadDirectory, fileName);
            file.SaveAs(filePath);

            var oficioCambio = new PLU_OP_OficiosCambios
            {
                FolioOficio = FolioOficio,
                RutaOficio = fileName,
                Activo = true,
                FechaCreacion = FechaCreacion
            };

            ctx.PLU_OP_OficiosCambios.Add(oficioCambio);
            await ctx.SaveChangesAsync();
            return oficioCambio;
        }

        private async Task<PLU_OP_Resguardo> ObtenerOcrearResguardoAsync(ModelContext ctx, int NumeroEmpleado, DateTime FechaCreacion)
        {
            var resguardo = await ctx.PLU_OP_Resguardo.FirstOrDefaultAsync(r => r.NumeroEmpleado == NumeroEmpleado);
            if (resguardo != null) return resguardo;

            var ultimoResguardo = await ctx.PLU_OP_Resguardo.OrderByDescending(r => r.NumeroResguardo).FirstOrDefaultAsync();
            int nuevoNumeroResguardo = ultimoResguardo != null ? ultimoResguardo.NumeroResguardo + 1 : 1;

            var empleado = await ctx.PLU_OP_Empleados.FirstOrDefaultAsync(x => x.NumeroEmpleado == NumeroEmpleado);
            if (empleado == null) throw new Exception($"Empleado con NúmeroEmpleado {NumeroEmpleado} no encontrado.");

            resguardo = new PLU_OP_Resguardo
            {
                IdEmpleado = empleado.IdEmpleado,
                NumeroEmpleado = NumeroEmpleado,
                NumeroResguardo = nuevoNumeroResguardo,
                Activo = true,
                FechaCreacion = FechaCreacion
            };

            ctx.PLU_OP_Resguardo.Add(resguardo);
            await ctx.SaveChangesAsync();
            return resguardo;
        }

        private async Task ProcesarActivoAsync(ModelContext ctx, int activoId, int NumeroEmpleado, bool esAlmacen, PLU_OP_Resguardo resguardo, PLU_OP_OficiosCambios oficioCambio, string folioCambio, DateTime FechaCreacion)
        {
            var activo = await ctx.PLU_OP_Activos.FindAsync(activoId);
            if (activo == null) throw new Exception($"Activo con Id {activoId} no encontrado.");

            var empleadoActual = await ctx.PLU_OP_Empleados.FirstOrDefaultAsync(x => x.NumeroEmpleado == NumeroEmpleado);
            if (empleadoActual == null) throw new Exception($"Empleado con NúmeroEmpleado {NumeroEmpleado} no encontrado.");

            // Obtener el año actual basado en la fecha de creación
            var year = FechaCreacion.Year;

            // Buscar si el empleado ya tiene un inventario físico en el año actual
            var inventarioExistente = await ctx.PLU_OP_InventarioFisico.Where(i => i.NumeroEmpleado == NumeroEmpleado && i.FechaInventario.Year == year).FirstOrDefaultAsync();

            if (inventarioExistente != null)
            {
                // Verificar si el activo ya está en el inventario del empleado este año
                var existeActivoEnInventario = await ctx.PLU_OP_InventarioFisico.AnyAsync(i => i.IdActivo == activoId && i.NumeroEmpleado == NumeroEmpleado && i.FechaInventario.Year == year);

                if (!existeActivoEnInventario) // Solo agregar si NO existe
                {
                    var nuevoInventarioFisico = new PLU_OP_InventarioFisico
                    {
                        FolioInventario = inventarioExistente.FolioInventario, // Reutiliza el folio existente
                        IdActivo = activoId,
                        IdUsuario = inventarioExistente.IdUsuario,
                        NumeroEmpleado = NumeroEmpleado,
                        FechaInventario = inventarioExistente.FechaInventario,
                        Activo = false, // El activo se agrega como activo
                        FechaCreacion = DateTime.Now
                    };

                    // Agregar el nuevo registro en InventarioFisico
                    ctx.PLU_OP_InventarioFisico.Add(nuevoInventarioFisico);
                }
                else
                {
                    Console.WriteLine($"El activo {activoId} ya está en el inventario físico del empleado {NumeroEmpleado} este año.");
                }
            }
            else
            {
                Console.WriteLine($"No existe un inventario físico registrado para el empleado {NumeroEmpleado} este año.");
            }

            // Crear el cambio de activo
            var cambioActivos = new PLU_OP_CambiosActivos
            {
                IdActivos = activoId,
                FolioCambio = folioCambio,
                IdOficioCambio = oficioCambio?.IdOficioCambio,
                IdEmpleadoActual = empleadoActual.IdEmpleado,
                IdEmpleadoAnterior = activo.PLU_OP_Resguardo.PLU_OP_Empleados.IdEmpleado,
                IdUsuario = SessionHelper.GetUser(),
                Inventario = false,
                Activo = true,
                FechaCreacion = FechaCreacion.Date
            };
            ctx.PLU_OP_CambiosActivos.Add(cambioActivos);

            // Actualizar los datos del activo
            if (esAlmacen)
            {
                activo.IdResguardo = null;
                activo.IdAlmacen = 1;
                activo.IdEstatusActivo = 3; // En Almacén
                activo.NumeroResguardo = null;
                activo.NumeroEmpleado = null;
            }
            else
            {
                activo.IdResguardo = resguardo.IdResguardo;
                activo.NumeroResguardo = resguardo.NumeroResguardo;
                activo.NumeroEmpleado = resguardo.NumeroEmpleado;
            }

            ctx.Entry(activo).State = EntityState.Modified;

            // Guardar cambios en la base de datos
            await ctx.SaveChangesAsync();
        }

        private string ObtenerMensajeError(Exception ex)
        {
            if (ex is DbEntityValidationException dbEx)
            {
                var validationErrors = new StringBuilder();
                foreach (var validationErrorsInEntity in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrorsInEntity.ValidationErrors)
                    {
                        validationErrors.AppendLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                    }
                }
                return "Error de validación: " + validationErrors.ToString();
            }
            return $"Error: {ex.Message}";
        }

        /*-----------------------------------------------------  Cambio de Resguardo  -------------------------------------------------------------------------------------------------*/

        public string GenerarFolioCambio()
        {
            string year = DateTime.Now.Year.ToString().Substring(2); // '24' para el año 2024
            int nuevoConsecutivo = 1;

            // Obtener el último número de folio existente para el año actual
            var ultimoFolio = _db.PLU_OP_CambiosActivos
                                 .Where(d => d.FolioCambio.StartsWith(year + "-"))
                                 .OrderByDescending(d => d.FolioCambio)
                                 .Select(d => d.FolioCambio)
                                 .FirstOrDefault();

            if (!string.IsNullOrEmpty(ultimoFolio))
            {
                // Extraer el número secuencial
                var partes = ultimoFolio.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimoConsecutivo))
                {
                    nuevoConsecutivo = ultimoConsecutivo + 1;
                }
            }

            // Generar el nuevo folio con formato "AA-NNNNNN"
            return $"{year}-{nuevoConsecutivo:D6}";
        }


        public string GenerarFolioAgregar()
        {
            string year = DateTime.Now.Year.ToString().Substring(2); // '24' para el año 2024
            int nuevoConsecutivo = 1;

            // Obtener el último número de folio existente para el año actual
            var ultimoFolio = _db.PLU_OP_AltasActivos
                                 .Where(d => d.FolioAlta.StartsWith(year + "-"))
                                 .OrderByDescending(d => d.FolioAlta)
                                 .Select(d => d.FolioAlta)
                                 .FirstOrDefault();

            if (!string.IsNullOrEmpty(ultimoFolio))
            {
                // Extraer el número secuencial
                var partes = ultimoFolio.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimoConsecutivo))
                {
                    nuevoConsecutivo = ultimoConsecutivo + 1;
                }
            }

            // Generar el nuevo folio con formato "AA-NNNNNN"
            return $"{year}-{nuevoConsecutivo:D6}";
        }


    }



}