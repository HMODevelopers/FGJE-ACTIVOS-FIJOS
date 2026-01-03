using ActivosFijos.Helpers;
using ActivosFijos.Models;
using Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static EDUES_ADMIN.Filters.AdminFilters;



namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class InventarioController : Controller
    {
        ModelContext _db = new ModelContext();
        InventarioHelper InventarioB = new InventarioHelper();
        // GET: Inventario
        
        public ActionResult Index()
        {
            CargarCatalogos();
            return View();
        }

        [HttpPost]
        public ActionResult Buscar(int numemp, int? page, string numeroinventario = "", string descripcion = "", int IdCategoria = 0, int IdMarca = 0)
        {
            CargarCatalogos();
            ViewBag.NumEmp = numemp;

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var vModel = InventarioB.GetAll(numemp, numeroinventario, descripcion, IdCategoria, IdMarca, pageNumber, pageSize);

            ViewBag.NumEmp = numemp;
            ViewBag.NumeroInvetario = numeroinventario;
            ViewBag.Descripcion = descripcion;
            ViewBag.Categoria = IdCategoria;
            ViewBag.Marcas = IdMarca;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaActivos", vModel);
            }

            return PartialView("_ListaActivos", vModel);
        }


        public ActionResult Inventarios(int? iPagina, string Nombres = "", string Adscripcion = "", DateTime? FechaInventario = null)
        {
            int pageSize = 10;
            int pageNumber = (iPagina ?? 1);

            // Pasar FechaInventario al método GetAllInventarios
            var vModel = InventarioB.GetAllInventarios(pageNumber, pageSize, Nombres, Adscripcion, FechaInventario);

            ViewBag.Nombres = Nombres;
            ViewBag.Adscripcion = Adscripcion;  
            ViewBag.Fecha = FechaInventario;



            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaInventarios", vModel);
            }

            return View(vModel);
        }

        public ActionResult InventarioFisico()
        {

            return View();
        }

        [HttpPost]
        public ActionResult BuscarInventario(string NumeroInventario, string NumeroSerie)
        {
            // Reemplazar las comillas simples por guiones en el número de inventario
            NumeroInventario = NumeroInventario?.Replace("'", "-");

            // Inicializar la consulta de activos
            IQueryable<PLU_OP_Activos> query = _db.PLU_OP_Activos.Where(a => a.IdEstatusActivo == 2);

            if (!string.IsNullOrWhiteSpace(NumeroInventario))
            {
                // Buscar por Número de Inventario si está lleno
                NumeroInventario = NumeroInventario.Trim().ToLower();
                query = query.Where(a => a.NumeroInventario.Trim().ToLower() == NumeroInventario);
            }
            else if (!string.IsNullOrWhiteSpace(NumeroSerie))
            {
                // Si el Número de Inventario está vacío, buscar por Número de Serie
                NumeroSerie = NumeroSerie.Trim().ToLower();
                query = query.Where(a => a.NumeroSerie.Trim().ToLower() == NumeroSerie);
            }

            // Seleccionar los campos deseados
            var activos = query.Select(a => new
            {
                IdActivos = a.IdActivos,
                NombreEmpleado = a.PLU_OP_Resguardo.PLU_OP_Empleados.NombreCompleto,
                NumeroInventario = a.NumeroInventario,
                Descripcion = a.Descripcion,
                NumeroSerie = a.NumeroSerie,
                Categoria = a.PLU_CAT_CategoriaActivo.NombreCategoria,
                Marca = a.PLU_CAT_MarcaActivo.NombreMarca

            }).ToList();

            return Json(activos);
        }

        

        /*----------------------------------------------------------------DETALLE INVENTARIO-----------------------------------------------------------------------------------------------------*/
        [HttpGet]
        public ActionResult DetalleInventario(string id)
        {
            var inventarioFisico = ObtenerInventarioPorFolio(id);

            if (!inventarioFisico.Any())
                return View(new List<PLU_OP_InventarioFisico>());

            var numeroEmpleado = inventarioFisico.First().NumeroEmpleado;
            var fechaInventario = inventarioFisico.First().FechaInventario;
            var idUsuario = inventarioFisico.First().IdUsuario;

            if (numeroEmpleado != null)
            {
                ActualizarActivosNuevos(id, numeroEmpleado, fechaInventario, idUsuario);
                EliminarActivosRemovidos(id, numeroEmpleado);
            }

            var activosFinal = ObtenerInventarioPorFolio(id);
            var activosValidos = FiltrarActivosValidos(activosFinal);

            return View(activosValidos);
        }

        private List<PLU_OP_InventarioFisico> ObtenerInventarioPorFolio(string folio)
        {
            return _db.PLU_OP_InventarioFisico
                      .Include(i => i.PLU_OP_Activos)
                      .Include(i => i.PLU_OP_Activos.PLU_OP_Resguardo)
                      .Include(i => i.PLU_OP_Activos.PLU_OP_Resguardo.PLU_OP_Empleados)
                      .Where(x => x.FolioInventario == folio)
                      .ToList();
        }

        private void ActualizarActivosNuevos(string folio, int numeroEmpleado, DateTime fechaInventario, int idUsuario)
        {
            var activosEmpleado = _db.PLU_OP_Activos
                                     .Where(x => x.NumeroEmpleado == numeroEmpleado)
                                     .Select(x => x.IdActivos)
                                     .ToList();

            var activosInventario = _db.PLU_OP_InventarioFisico
                                       .Where(x => x.FolioInventario == folio)
                                       .Select(x => x.IdActivo)
                                       .ToList();

            var nuevosActivos = activosEmpleado.Except(activosInventario).ToList();

            if (nuevosActivos.Any())
            {
                var nuevosRegistros = nuevosActivos.Select(id => new PLU_OP_InventarioFisico
                {
                    IdActivo = id,
                    NumeroEmpleado = numeroEmpleado,
                    FechaInventario = fechaInventario,
                    IdUsuario = idUsuario,
                    Activo = false,
                    FechaCreacion = DateTime.Now,
                    FolioInventario = folio
                }).ToList();

                _db.PLU_OP_InventarioFisico.AddRange(nuevosRegistros);
                _db.SaveChanges();
            }
        }

        private void EliminarActivosRemovidos(string folio, int numeroEmpleado)
        {
            var activosEmpleado = _db.PLU_OP_Activos
                                     .Where(x => x.NumeroEmpleado == numeroEmpleado)
                                     .Select(x => x.IdActivos)
                                     .ToList();

            var inventario = _db.PLU_OP_InventarioFisico
                                .Where(x => x.FolioInventario == folio)
                                .ToList();

            var activosRemovidos = inventario
                                    .Where(x => !activosEmpleado.Contains(x.IdActivo))
                                    .ToList();

            if (activosRemovidos.Any())
            {
                _db.PLU_OP_InventarioFisico.RemoveRange(activosRemovidos);
                _db.SaveChanges();
            }
        }

        private List<PLU_OP_InventarioFisico> FiltrarActivosValidos(List<PLU_OP_InventarioFisico> inventario)
        {
            return inventario
                    .Where(i => _db.PLU_OP_Activos.Any(a =>
                        a.IdActivos == i.IdActivo &&
                        a.PLU_OP_Resguardo.NumeroEmpleado == i.NumeroEmpleado))
                    .ToList();
        }



        /*---------------------------------------------------------------------------- Guardar Inventario -----------------------------------------------------------------------------------------------------------------*/
        /*---------------------------------------------------------------------------- Guardar Inventario -----------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
        public JsonResult GuardarInventario2(int numEmp, string selectedActivos, bool crearNuevo = false, bool usarExistente = false)
        {
            var rm = new ResponseModel();
            string folioInventarioUsado = null; // 🔴 declarar aquí

            try
            {
                using (var db = new ModelContext())
                {
                    List<int> activosSeleccionados = JsonConvert.DeserializeObject<List<int>>(selectedActivos ?? "[]");
                    int userId = SessionHelper.GetUser();
                    int currentYear = DateTime.Now.Year;

                    var todosActivos = db.PLU_OP_Activos.Where(a => a.PLU_OP_Resguardo.NumeroEmpleado == numEmp).ToList();

                    // Trae TODO el inventario del empleado a memoria (lo filtraremos después según el caso)
                    var inventarioActual = db.PLU_OP_InventarioFisico.Where(i => i.NumeroEmpleado == numEmp).ToList();

                    // ===== FOLIO MÁS RECIENTE POR FECHA (sin importar año) =====

                    var folioMasReciente = db.PLU_OP_InventarioFisico
                        .Where(i => i.NumeroEmpleado == numEmp)
                        .GroupBy(i => i.FolioInventario)
                        .Select(g => new
                        {
                            Folio = g.Key,
                            UltimoDia = g.Max(i => DbFunctions.TruncateTime(i.FechaInventario)), // día más reciente
                            UltimaFecha = g.Max(i => i.FechaInventario)                          // fecha/hora más reciente
                        })
                        .OrderByDescending(x => x.UltimoDia)     // 1) día más reciente
                        .ThenByDescending(x => x.UltimaFecha)    // 2) dentro del día, la hora más reciente
                        .ThenByDescending(x => x.Folio)          // 3) si sigue empate, el folio “más grande”
                        .Select(x => x.Folio)
                        .FirstOrDefault();

                    // Si existe un folio previo y no hay decisión explícita -> pedir confirmación
                    if (!crearNuevo && !usarExistente && !string.IsNullOrEmpty(folioMasReciente))
                    {
                        rm.response = true;
                        rm.message = $"Ya existe un inventario registrado previamente con folio {folioMasReciente}.";
                        rm.function = $"ConfirmarNuevoInventario({numEmp}, '{HttpUtility.JavaScriptStringEncode(selectedActivos ?? "[]")}');";
                        rm.error = false;
                        return Json(rm);
                    }

                    // Resolver folio según la decisión
                    string folioInventario;
                    if (usarExistente)
                    {
                        if (string.IsNullOrEmpty(folioMasReciente))
                        {
                            return Json(new ResponseModel
                            {
                                response = false,
                                error = true,
                                message = "No se encontró un folio previo para este empleado. Debe crear un folio nuevo."
                            });
                        }

                        folioInventario = folioMasReciente;
                        folioInventarioUsado = folioInventario; // 🔴 asignar aquí

                        // Si voy a actualizar el existente, trabajo SOLO con ese folio
                        inventarioActual = inventarioActual
                            .Where(i => i.FolioInventario == folioInventario)
                            .ToList();
                    }
                    else
                    {
                        // crearNuevo == true o no hay folio previo: generar folio nuevo
                        folioInventario = GenerarFolioInventario();
                        folioInventarioUsado = folioInventario; // 🔴 asignar aquí también
                    }

                    // Cambios de resguardo cuando aplique
                    ManejarCambioResguardo(db, activosSeleccionados, numEmp, userId);

                    // Actualiza/Inserta según bandera y folio seleccionado
                    ActualizarInventario(
                        db,
                        todosActivos,
                        activosSeleccionados,
                        inventarioActual,
                        folioInventario,
                        numEmp,
                        userId,
                        crearNuevo,
                        usarExistente
                    );

                    db.SaveChanges();
                }

                var rmOk = new ResponseModel
                {
                    response = true,
                    message = "Inventario guardado con éxito.",
                    // Pasamos el folio usado dentro de 'function' para que el front llame GenerarYAbrirPdf2(folio)
                    function = $"HideLoading();GenerarYAbrirPdf2('{folioInventarioUsado}');",
                    href = Url.Action("InventarioFisico", "Inventario"),
                    error = false
                };

                return Json(rmOk);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(ManejarErrorValidacion(ex));
            }
        }


        private string ObtenerFolioInventario(ModelContext db, int numEmp, int year)
        {
            var folioExistente = db.PLU_OP_InventarioFisico
                .Where(i => i.NumeroEmpleado == numEmp && i.FechaInventario.Year == year)
                .Select(i => i.FolioInventario)
                .FirstOrDefault();

            return folioExistente ?? GenerarFolioInventario();
        }

        private void ManejarCambioResguardo(ModelContext db, List<int> activosSeleccionados, int numEmp, int userId)
        {
            var todosActivos = db.PLU_OP_Activos
                .Where(a => a.PLU_OP_Resguardo.NumeroEmpleado == numEmp)
                .ToList();

            string folioCambio = GenerarFolioCambio();

            foreach (var idActivo in activosSeleccionados)
            {
                if (!todosActivos.Any(a => a.IdActivos == idActivo))
                {
                    var activoCambiado = db.PLU_OP_Activos.FirstOrDefault(a => a.IdActivos == idActivo);
                    if (activoCambiado != null)
                    {
                        var nuevoResguardo = db.PLU_OP_Resguardo.FirstOrDefault(r => r.NumeroEmpleado == numEmp);
                        if (nuevoResguardo != null)
                        {
                            int? idResguardoAnterior = activoCambiado.IdResguardo;
                            activoCambiado.IdResguardo = nuevoResguardo.IdResguardo;
                            activoCambiado.NumeroResguardo = nuevoResguardo.NumeroResguardo;
                            activoCambiado.NumeroEmpleado = nuevoResguardo.NumeroEmpleado;
                            db.Entry(activoCambiado).State = EntityState.Modified;

                            db.PLU_OP_CambiosActivos.Add(new PLU_OP_CambiosActivos
                            {
                                IdActivos = idActivo,
                                FolioCambio = folioCambio,
                                IdOficioCambio = null,
                                IdEmpleadoActual = nuevoResguardo.IdEmpleado,
                                IdEmpleadoAnterior = db.PLU_OP_Resguardo
                                                        .Where(r => r.IdResguardo == idResguardoAnterior)
                                                        .Select(r => r.IdEmpleado)
                                                        .FirstOrDefault(),
                                IdUsuario = userId,
                                Inventario = true,
                                Activo = true,
                                FechaCreacion = DateTime.Now.Date,
                            });
                        }
                    }
                }
            }
        }

        private void ActualizarInventario( ModelContext db, List<PLU_OP_Activos> todosActivos, List<int> activosSeleccionados,List<PLU_OP_InventarioFisico> inventarioActual, string folioInventario,int numEmp, int userId, bool crearNuevo, bool usarExistente)
        {
            int currentYear = DateTime.Now.Year;

            // Activos del empleado + los seleccionados (por si cambiaron de resguardo)
            var activosParaInventario = todosActivos
                .Concat(db.PLU_OP_Activos.Where(a => activosSeleccionados.Contains(a.IdActivos)))
                .Distinct()
                .ToList();

            // ===== RUTA: USAR FOLIO EXISTENTE (actualiza o inserta faltantes dentro de ESE folio) =====
            if (usarExistente)
            {
                foreach (var activo in activosParaInventario)
                {
                    bool seEncontro = activosSeleccionados.Contains(activo.IdActivos);

                    // 🔴 Buscar DENTRO DEL MISMO FOLIO (no por año)
                    var invExistente = inventarioActual
                        .FirstOrDefault(i => i.IdActivo == activo.IdActivos
                                          && i.FolioInventario == folioInventario);

                    if (invExistente != null)
                    {
                        // Si ya estaba marcado como encontrado en este folio, no tocar
                        if (invExistente.Activo) continue;

                        // Si ahora sí se encontró, actualizar flags y fecha
                        if (seEncontro)
                        {
                            invExistente.Activo = true;
                            invExistente.FechaInventario = DateTime.Now;
                            invExistente.IdUsuario = userId;
                            db.Entry(invExistente).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // No existía este activo en el folio seleccionado: insertarlo en ese folio
                        db.PLU_OP_InventarioFisico.Add(new PLU_OP_InventarioFisico
                        {
                            IdActivo = activo.IdActivos,
                            FolioInventario = folioInventario,
                            IdUsuario = userId,
                            NumeroEmpleado = numEmp,
                            FechaInventario = DateTime.Now,
                            Activo = seEncontro,
                            FechaCreacion = DateTime.Now
                        });
                    }
                }

                // (Opcional) Desmarcar como no encontrado a los del MISMO folio que no vinieron en el escaneo actual:
                // var idsEncontrados = new HashSet<int>(activosSeleccionados);
                // foreach (var fila in inventarioActual.Where(i => i.FolioInventario == folioInventario))
                // {
                //     if (!idsEncontrados.Contains(fila.IdActivo) && fila.Activo)
                //     {
                //         fila.Activo = false;
                //         fila.FechaInventario = DateTime.Now;
                //         fila.IdUsuario = userId;
                //         db.Entry(fila).State = EntityState.Modified;
                //     }
                // }

                return;
            }

            // ===== RUTA: CREAR FOLIO NUEVO (inserta SIEMPRE nuevas filas con el folio nuevo) =====
            if (crearNuevo)
            {
                foreach (var activo in activosParaInventario)
                {
                    bool seEncontro = activosSeleccionados.Contains(activo.IdActivos);

                    db.PLU_OP_InventarioFisico.Add(new PLU_OP_InventarioFisico
                    {
                        IdActivo = activo.IdActivos,
                        FolioInventario = folioInventario,   // NUEVO FOLIO
                        IdUsuario = userId,
                        NumeroEmpleado = numEmp,
                        FechaInventario = DateTime.Now,
                        Activo = seEncontro,
                        FechaCreacion = DateTime.Now
                    });
                }
                return;
            }

            // ===== RUTA: COMPORTAMIENTO POR DEFECTO =====
            // (cuando no hay folios previos o no hubo necesidad de confirmar)
            foreach (var activo in activosParaInventario)
            {
                bool seEncontro = activosSeleccionados.Contains(activo.IdActivos);

                var invExistente = inventarioActual
                    .FirstOrDefault(i => i.IdActivo == activo.IdActivos && i.FechaInventario.Year == currentYear);

                if (invExistente != null)
                {
                    if (invExistente.Activo) continue;
                    if (seEncontro)
                    {
                        invExistente.Activo = true;
                        invExistente.FechaInventario = DateTime.Now;
                        invExistente.IdUsuario = userId;
                        db.Entry(invExistente).State = EntityState.Modified;
                    }
                }
                else
                {
                    db.PLU_OP_InventarioFisico.Add(new PLU_OP_InventarioFisico
                    {
                        IdActivo = activo.IdActivos,
                        FolioInventario = folioInventario,
                        IdUsuario = userId,
                        NumeroEmpleado = numEmp,
                        FechaInventario = DateTime.Now,
                        Activo = seEncontro,
                        FechaCreacion = DateTime.Now
                    });
                }
            }
        }

        private ResponseModel ManejarErrorValidacion(DbEntityValidationException ex)
        {
            var errorMessages = ex.EntityValidationErrors
                .SelectMany(x => x.ValidationErrors)
                .Select(x => x.ErrorMessage);

            return new ResponseModel
            {
                response = false,
                message = "Error al guardar el inventario: " + string.Join("; ", errorMessages),
                function = "HideLoading();",
                href = Url.Action("InventarioFisico", "Inventario"),
                error = true
            };
        }


        /*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        public void CargarCatalogos()
        {
            var categorias = _db.PLU_CAT_CategoriaActivo.Where(x => x.Activo == true).OrderBy(x => x.NombreCategoria).ToList();
            ViewBag.Categorias = new SelectList(categorias, "IdCategoria", "NombreCategoria");

            var marca = _db.PLU_CAT_MarcaActivo.Where(x => x.Activo == true).OrderBy(x => x.NombreMarca).ToList();
            ViewBag.Marca = new SelectList(marca, "IdMarca", "NombreMarca");

            var Conceptos = _db.PLU_CAT_Conceptos.Where(x => x.Activo == true).OrderBy(x => x.NombreConcepto).ToList();
            ViewBag.Conceptos = new SelectList(Conceptos, "IdConcepto", "NombreConcepto");

            var Clasificadores = _db.PLU_CAT_Clasificadores.Where(x => x.Activo == true).OrderBy(x => x.ClasificadorDescripcion).ToList();
            ViewBag.Clasificadores = new SelectList(Clasificadores, "IdClasificadores", "ClasificadorDescripcion");

            var facturas = _db.PLU_CAT_Facturas.Where(x => x.Activo == true).OrderBy(x => x.FolioFactura).ToList();
            ViewBag.Facturas = new SelectList(facturas, "IdFactura", "FoliFactura");

            var proveedor = _db.PLU_CAT_Proveedores.Where(x => x.Activo == true).OrderBy(x => x.RazonSocial).ToList();
            ViewBag.Proveedores = new SelectList(proveedor, "IdProveedor", "RazonSocial");

            var Almacenes = _db.PLU_CAT_Almacenes.Where(x => x.Activo == true).OrderBy(x => x.NombreAlmacen).ToList();
            ViewBag.Almacenes = new SelectList(Almacenes, "IdAlmacen", "NombreAlmacen");

            var estadofisico = _db.PLU_CAT_EstadoFisicoActivo.Where(x => x.Activo == true).OrderBy(x => x.Descripcion).ToList();
            ViewBag.EstadoFisico = new SelectList(estadofisico, "IdEstadoFisico", "Descripcion");

            var Recursos = _db.PLU_CAT_Recurso.Where(x => x.Activo == true).OrderBy(x => x.NombreRecurso).ToList();
            ViewBag.Recursos = new SelectList(Recursos, "IdRecurso", "NombreRecurso");

            var estatusActivo = _db.PLU_CAT_EstatusActivo.Where(x => x.Activo == true && x.IdEstatusActivo != 2).OrderBy(x => x.Descripcion).ToList();
            ViewBag.EstatusActivo = new SelectList(estatusActivo, "IdEstatusActivo", "Descripcion");


        }


        [HttpPost]
        public JsonResult GenerarPdf(int NumeroEmpleado)
        {
            try
            {
                int currentYear = DateTime.Now.Year;
                var inventarioActual = _db.PLU_OP_InventarioFisico
                                            .Where(i => i.NumeroEmpleado == NumeroEmpleado
                                                   && i.FechaInventario.Year == currentYear
                                                   && _db.PLU_OP_Activos
                                                  .Any(a => a.IdActivos == i.IdActivo && a.PLU_OP_Resguardo.NumeroEmpleado == i.NumeroEmpleado)).OrderBy(x => x.PLU_OP_Activos.NumeroInventario).ToList();

                using (var memoryStream = new MemoryStream())
                {
                    // Crear el documento PDF en orientación horizontal
                    Document document = new Document(PageSize.A4.Rotate(), 30, 30, 15, 15);
                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();

                    // Encabezado del PDF
                    Font FontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                    Font FontNormal = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

                    // Variable para llevar el conteo de activos procesados
                    int activosProcesados = 0;
                    int totalPages = (int)Math.Ceiling((double)inventarioActual.Count / 20); // Calculamos el total de páginas necesarias

                    // Bucle para agregar páginas adicionales según sea necesario
                    while (activosProcesados < inventarioActual.Count)
                    {
                        // Agregar una nueva página al documento
                        document.NewPage();

                        // Crear el borde de la página en cada página nueva
                        PdfContentByte content = writer.DirectContent;
                        Rectangle rectangle = new Rectangle(document.PageSize);
                        rectangle.Left += 30;
                        rectangle.Right -= 30;
                        rectangle.Top -= 15;
                        rectangle.Bottom += 15;
                        content.SetColorStroke(BaseColor.BLACK);
                        content.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
                        content.Stroke();

                        // Agregar contenido a la página
                        Encabezado(document);
                        DatosEmpleado(document, NumeroEmpleado);
                        Leyenda1(document);
                        // Obtener los activos para esta página
                        var activosParaPagina = inventarioActual.Skip(activosProcesados).Take(20).ToList(); // 20 activos por página
                        TablaActivos(document, activosParaPagina);
                        Leyenda2(document, NumeroEmpleado);
                        FirmaReviso(document);
                        FirmaRecibio(document, NumeroEmpleado);
                        LeyendaHoja(document, activosProcesados / 20 + 1, totalPages); // Agregar la leyenda de la hoja

                        // Actualizar el contador de activos procesados
                        activosProcesados += activosParaPagina.Count;
                    }

                    document.Close();
                    writer.Close();

                    var pdfBytes = memoryStream.ToArray();
                    var pdfBase64 = Convert.ToBase64String(pdfBytes);

                    // Configurar el JsonResult con la longitud máxima permitida
                    var jsonResult = Json(new { Datos = "", PdfBase64 = pdfBase64 }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al generar el PDF: " + ex.Message);

                // Configurar el JsonResult con la longitud máxima permitida en caso de error
                var jsonResult = Json(new
                {
                    Datos = "",
                    PdfBase64 = "null"
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = Int32.MaxValue;

                return jsonResult;
            }
        }


        [HttpPost]
        public ActionResult GenerarPdf2(string FolioInventario)
        {
            try
            {
                var datainv = _db.PLU_OP_InventarioFisico.Where(a => a.FolioInventario == FolioInventario).Select( a => new { a.NumeroEmpleado }).FirstOrDefault();
                var inventarioActual = _db.PLU_OP_InventarioFisico
                                            .Where(i => i.FolioInventario == FolioInventario && _db.PLU_OP_Activos
                                            .Any(a => a.IdActivos == i.IdActivo && a.PLU_OP_Resguardo.NumeroEmpleado == i.NumeroEmpleado)).OrderBy(x => x.PLU_OP_Activos.NumeroInventario).ToList();

                using (var memoryStream = new MemoryStream())
                {
                    // Crear el documento PDF en orientación horizontal
                    Document document = new Document(PageSize.A4.Rotate(), 30, 30, 15, 15);
                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();

                    // Encabezado del PDF
                    Font FontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                    Font FontNormal = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

                    // Variable para llevar el conteo de activos procesados
                    int activosProcesados = 0;
                    int totalPages = (int)Math.Ceiling((double)inventarioActual.Count / 20); // Calculamos el total de páginas necesarias

                    // Bucle para agregar páginas adicionales según sea necesario
                    while (activosProcesados < inventarioActual.Count)
                    {
                        // Agregar una nueva página al documento
                        document.NewPage();

                        // Crear el borde de la página en cada página nueva
                        PdfContentByte content = writer.DirectContent;
                        Rectangle rectangle = new Rectangle(document.PageSize);
                        rectangle.Left += 30;
                        rectangle.Right -= 30;
                        rectangle.Top -= 15;
                        rectangle.Bottom += 15;
                        content.SetColorStroke(BaseColor.BLACK);
                        content.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
                        content.Stroke();

                        // Agregar contenido a la página
                        Encabezado(document);
                        DatosEmpleado(document, datainv.NumeroEmpleado);
                        Leyenda1(document);
                        // Obtener los activos para esta página
                        var activosParaPagina = inventarioActual.Skip(activosProcesados).Take(20).ToList(); // 20 activos por página
                        TablaActivos2(document, activosParaPagina);
                        Leyenda2(document, datainv.NumeroEmpleado);
                        FirmaReviso(document);
                        FirmaRecibio(document, datainv.NumeroEmpleado);
                        LeyendaHoja(document, activosProcesados / 20 + 1, totalPages); // Agregar la leyenda de la hoja

                        // Actualizar el contador de activos procesados
                        activosProcesados += activosParaPagina.Count;
                    }

                    document.Close();
                    writer.Close();

                    var pdfBytes = memoryStream.ToArray();
                    var pdfBase64 = Convert.ToBase64String(pdfBytes);

                    // Configurar la respuesta JSON con el tamaño máximo permitido
                    var result = new
                    {
                        Datos = "",
                        PdfBase64 = pdfBase64
                    };

                    // Utilizar el JsonResult para especificar la configuración de serialización
                    var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                    // Establecer el tamaño máximo del JSON para la respuesta
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al generar el PDF: " + ex.Message);

                // Configurar la respuesta JSON con el tamaño máximo permitido
                var result = new
                {
                    Datos = "",
                    PdfBase64 = "null"
                };

                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = Int32.MaxValue;

                return jsonResult;
            }
        }

        public void Encabezado(Document document)
        {

            // Añadir el encabezado con borde
            PdfPTable headerTable = new PdfPTable(1);
            headerTable.WidthPercentage = 100;
            PdfPCell headerCell = new PdfPCell(new Phrase("RESGUARDO - UNIDAD DE INVENTARIO", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Border = PdfPCell.BOX,
                BorderWidth = 1f,
                Padding = 2
            };
            headerTable.AddCell(headerCell);
            document.Add(headerTable);

        }

        public void DatosEmpleado(Document document, int NumeroEmpleado)
        {
            Font FontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
            Font FontNormal = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            ModelContext _db = new ModelContext();
            var Puesto = "";
            var UnidadResponsable = "";
            var Municipio = "";

            var Empleado = _db.PLU_OP_Empleados.Where(x => x.NumeroEmpleado == NumeroEmpleado).FirstOrDefault();
            var Resguardo = _db.PLU_OP_Resguardo.Where(x => x.IdEmpleado == Empleado.IdEmpleado).FirstOrDefault();

            // Obtener la fecha actual
            var fechaActual = DateTime.Now;


            var Adscripcion = Empleado.PLU_OP_Adscripcion.Where(x => x.IdEmpleado == Empleado.IdEmpleado).FirstOrDefault();

            Puesto = string.IsNullOrEmpty(Adscripcion?.PuestoFuncional) ? " " : Adscripcion.PuestoFuncional;
            UnidadResponsable = string.IsNullOrEmpty(Adscripcion?.Area) ? Adscripcion.Corporacion : Adscripcion.Area;
            Municipio = string.IsNullOrEmpty(Adscripcion?.PLU_CAT_Municipios?.NombreMunicipio) ? " " : Adscripcion.PLU_CAT_Municipios.NombreMunicipio;


            // Crear la tabla de datos con 2 columnas
            PdfPTable datosTable = new PdfPTable(2)
            {
                WidthPercentage = 100, // Ajusta el porcentaje del ancho de la tabla según tus necesidades
                HorizontalAlignment = Element.ALIGN_LEFT,
                SpacingAfter = 0 // Establece el espacio después de la tabla en 0 para eliminar el espacio en blanco adicional
            };

            // Definir los anchos relativos de las columnas
            float[] columnWidths = { 1f, 2f }; // Ajusta las proporciones de las columnas según sea necesario
            datosTable.SetWidths(columnWidths);

            // Crear celdas para cada dato sin bordes individuales
            datosTable.AddCell(new PdfPCell(new Phrase("Numero Resguardo:", FontNormal)) { Border = PdfPCell.NO_BORDER });
            if (Resguardo == null)
            {
                datosTable.AddCell(new PdfPCell(new Phrase("En Almacen", FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });
            }
            else
            {
                datosTable.AddCell(new PdfPCell(new Phrase(Resguardo.NumeroResguardo.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });
            }


            datosTable.AddCell(new PdfPCell(new Phrase("Clave de Empleado:", FontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase(Empleado.NumeroEmpleado.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            datosTable.AddCell(new PdfPCell(new Phrase("Responsable:", FontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase(Empleado.ApellidoP + " " + Empleado.ApellidoM + " " + Empleado.Nombres, FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            datosTable.AddCell(new PdfPCell(new Phrase("Puesto:", FontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase(Puesto.ToUpper(), FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            datosTable.AddCell(new PdfPCell(new Phrase("Unidad Responsable:", FontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase(UnidadResponsable.ToUpper(), FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            datosTable.AddCell(new PdfPCell(new Phrase("Adscripción:", FontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase("FISCALIA GENERAL DE JUSTICIA DEL ESTADO", FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            datosTable.AddCell(new PdfPCell(new Phrase("Municipio:", FontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase(Municipio.ToUpper(), FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            // Crear una celda para encapsular la tabla de datos y aplicarle borde y padding
            PdfPCell encapsulatedTableCell = new PdfPCell(datosTable)
            {
                Border = PdfPCell.BOX,
                Padding = 5
            };

            // Crear la tabla de diseño
            PdfPTable layoutTable = new PdfPTable(2)
            {
                WidthPercentage = 80,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            // Añadir la celda con la tabla de datos encapsulada a la tabla de diseño
            layoutTable.AddCell(encapsulatedTableCell);

            // Añadir la tabla de diseño al documento
            document.Add(layoutTable);


            /* logotipo*/
            #region
            string logoPath = Server.MapPath("~/Content/assets/images/logo-sm-fgje.png"); // Ajusta la ruta de la imagen del logotipo
            Image logo = Image.GetInstance(logoPath);
            logo.ScaleToFit(120f, 120f);
            PdfPCell logoCell = new PdfPCell(logo)
            {
                Border = PdfPCell.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_TOP,
                PaddingTop = -5f, // Ajusta según sea necesario para manipular la posición vertical
                PaddingRight = -150f // Ajusta según sea necesario para manipular la posición horizontal
            };
            layoutTable.AddCell(logoCell);

            document.Add(layoutTable);
            #endregion
            /* Fin  logotipo*/

            // Crear espacio en blanco entre las tablas (puede ser un párrafo vacío)
            document.Add(new Paragraph("\n"));
        }

        public void Leyenda1(Document document)
        {
            
            // Añadir el texto informativo
            // Crear el texto del párrafo con el font en negrita
            Paragraph infoText = new Paragraph("ESTOS BIENES SON PROPIEDAD DEL ESTADO, POR LO CUAL ES RESPONSABILIDAD DEL TITULAR DE ESTE RESGUARDO EL CUIDADO DE DICHO EQUIPO, EN CASO DE EXTRAVIO EL IMPORTE DEL BIEN SERA CUBIERTO POR LA PERSONA QUE FIRMA EL RESGUARDO.", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6))
            {
                SpacingBefore = 20,
                SpacingAfter = 20
            };

            // Crear una celda de tabla para agregar padding
            PdfPCell cellWithPadding = new PdfPCell(infoText)
            {
                Padding = 5,
                Border = PdfPCell.NO_BORDER // Si no quieres un borde alrededor de la celda
            };

            // Crear una tabla para contener la celda
            PdfPTable table = new PdfPTable(1)
            {
                WidthPercentage = 100
            };
            table.AddCell(cellWithPadding);

            // Agregar la tabla al documento
            document.Add(table);

            
        }

        public void TablaActivos(Document document, List<PLU_OP_InventarioFisico> activos)
        {
            //var dataActivos = _db.PLU_OP_Activos.Where(x => activos.Contains(x.IdActivos)).ToList();

            PdfPTable bienesTable = new PdfPTable(8);
            bienesTable.WidthPercentage = 100;

            // Definir anchos relativos de las columnas
            float[] columnWidths1 = { .5f, .8f, 3f, .9f, .5f, 1f, 1f, 1f }; // Ajusta los valores según sea necesario
            bienesTable.SetWidths(columnWidths1);

            // Encabezados de la tabla de bienes
            bienesTable.AddCell(new PdfPCell(new Phrase("FACTURA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("CATEGORIA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("DESCRIPCIÓN", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("SERIE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("MARCA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("CLAVE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("SALIDA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("FECHA SALIDA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });

            // Iterar sobre los activos y agregar las filas a la tabla
            foreach (var activo in activos)
            {
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_OP_Activos.PLU_CAT_Facturas?.FolioFactura ?? "SIN FACTURA", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_OP_Activos.PLU_CAT_CategoriaActivo?.NombreCategoria ?? "SIN CATEGORIA",20), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_OP_Activos.Descripcion ?? "SIN DESCRIPCION", 66), FontFactory.GetFont(FontFactory.HELVETICA, 5))));
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_OP_Activos.NumeroSerie ?? "SIN N/S", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_OP_Activos.PLU_CAT_MarcaActivo?.NombreMarca ?? "SIN MARCA", 10), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_OP_Activos.NumeroInventario ?? "SIN N/I", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                if (activo.Activo == true)
                {
                    bienesTable.AddCell(new PdfPCell(new Phrase("IF", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                }
                else
                {
                    bienesTable.AddCell(new PdfPCell(new Phrase("Pendiente", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                }
                
                bienesTable.AddCell(new PdfPCell(new Phrase(DateTime.Now.ToString("dd/MM/yyyy"), FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
            }

            // Calcular cuántas filas en blanco se necesitan para completar 20 filas
            int rowCount = activos.Count;
            int remainingRows = 20 - rowCount;
            if (remainingRows > 0)
            {
                for (int i = 0; i < remainingRows; i++)
                {
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                }
            }

            document.Add(bienesTable);
        }

        public void TablaActivos2(Document document, List<PLU_OP_InventarioFisico> activos)
        {
            //var dataActivos = _db.PLU_OP_Activos.Where(x => activos.Contains(x.IdActivos)).ToList();

            PdfPTable bienesTable = new PdfPTable(8);
            bienesTable.WidthPercentage = 100;

            // Definir anchos relativos de las columnas
            float[] columnWidths1 = { .5f, .8f, 3f, .9f, .5f, 1f, 1f, 1f }; // Ajusta los valores según sea necesario
            bienesTable.SetWidths(columnWidths1);

            // Encabezados de la tabla de bienes
            bienesTable.AddCell(new PdfPCell(new Phrase("FACTURA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("CATEGORIA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("DESCRIPCIÓN", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("SERIE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("MARCA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("CLAVE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("SALIDA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("FECHA SALIDA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });

            // Iterar sobre los activos y agregar las filas a la tabla
            foreach (var activo in activos)
            {
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_OP_Activos.PLU_CAT_Facturas?.FolioFactura ?? "SIN FACTURA", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_OP_Activos.PLU_CAT_CategoriaActivo?.NombreCategoria ?? "SIN CATEGORIA", 20), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_OP_Activos.Descripcion ?? "SIN DESCRIPCION", 66), FontFactory.GetFont(FontFactory.HELVETICA, 5))));
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_OP_Activos.NumeroSerie ?? "SIN N/S", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_OP_Activos.PLU_CAT_MarcaActivo?.NombreMarca ?? "SIN MARCA", 10), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_OP_Activos.NumeroInventario ?? "SIN N/I", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                if (activo.Activo == true)
                {
                    bienesTable.AddCell(new PdfPCell(new Phrase("IF", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                }
                else
                {
                    bienesTable.AddCell(new PdfPCell(new Phrase("Pendiente", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                }

                bienesTable.AddCell(new PdfPCell(new Phrase(activo.FechaCreacion.ToString("dd/MM/yyyy"), FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
            }

            // Calcular cuántas filas en blanco se necesitan para completar 20 filas
            int rowCount = activos.Count;
            int remainingRows = 20 - rowCount;
            if (remainingRows > 0)
            {
                for (int i = 0; i < remainingRows; i++)
                {
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                    bienesTable.AddCell(new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                }
            }

            document.Add(bienesTable);
        }

        public void Leyenda2(Document document, int NumeroEmpleado)
        {
            var Empleado = _db.PLU_OP_Empleados.Where(x => x.NumeroEmpleado == NumeroEmpleado).FirstOrDefault();
            #region

            // Crear el texto del párrafo con el font en negrita
            Paragraph leyendaFinal = new Paragraph($"Yo {Empleado.NombreCompleto} me hago responsable de los bienes contenidos en este resguardo, que son propiedad de la Fiscalía General de Justicia del Estado. Debiendo informar a la UNIDAD DE INVENTARIOS el cambio de ubicación, pérdida o cualquier movimiento, en el entendido de que al no hacerlo será acreedor a una acta administrativa.\nEste documento será utilizado por el personal del Departamento de Inventarios de esta Fiscalía al realizar entrega o bajas de mobiliario y/o equipo tecnológico, así como levantamiento físico en su área de trabajo y/o áreas a su cargo.\nFundamentado en el artículo 12 fracción XVI de la Ley Orgánica No. 180 de la Fiscalía General de Justicia del Estado de Sonora.", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6))
            {
                SpacingBefore = 20,
                SpacingAfter = 20,
                Alignment = Element.ALIGN_CENTER
            };

            // Crear una celda de tabla para agregar padding
            PdfPCell cellWithPadding1 = new PdfPCell(leyendaFinal)
            {
                Padding = 5,
                Border = PdfPCell.NO_BORDER, // Si no quieres un borde alrededor de la celda
                HorizontalAlignment = Element.ALIGN_CENTER // Centrar el contenido horizontalmente
            };

            // Establecer la justificación del párrafo
            leyendaFinal.Alignment = Element.ALIGN_JUSTIFIED;

            // Crear una tabla para contener la celda
            PdfPTable table1 = new PdfPTable(1)
            {
                WidthPercentage = 90,
                HorizontalAlignment = Element.ALIGN_CENTER // Centrar la tabla en la página
            };
            table1.AddCell(cellWithPadding1);

            // Agregar la tabla al documento
            document.Add(table1);
            /*Fin Leyenda de abajo de tabla*/

            #endregion
        }

        public void FirmaReviso(Document document)
        {

            var IdUsuario = SessionHelper.GetUser();

            var Usuario = _db.PLU_CONF_Usuario.Where(x => x.IdUsuario == IdUsuario).FirstOrDefault();
            #region
            // Crear el párrafo
            Paragraph reviso = new Paragraph("Reviso:", FontFactory.GetFont(FontFactory.HELVETICA, 7))
            {
                Alignment = Element.ALIGN_CENTER
            };

            // Crear una celda y añadir el párrafo
            PdfPCell revisoCell = new PdfPCell(reviso)
            {
                Border = PdfPCell.NO_BORDER, // Sin borde si no lo necesitas
                PaddingTop = 40f, // Padding superior
                PaddingBottom = 10f, // Padding inferior
                PaddingLeft = 120f, // Padding izquierdo
                PaddingRight = 15f // Padding derecho
            };

            // Crear una tabla para contener la celda
            PdfPTable revisoTable = new PdfPTable(1)
            {
                WidthPercentage = 100 // Ajusta según necesites
            };

            // Añadir la celda a la tabla
            revisoTable.AddCell(revisoCell);

            // Añadir la tabla al documento
            document.Add(revisoTable);

            // Crear el párrafo de líneas
            Paragraph lineaGuiones = new Paragraph("_______________________________________", FontFactory.GetFont(FontFactory.HELVETICA, 8))
            {
                Alignment = Element.ALIGN_CENTER
            };

            // Crear una celda y añadir el párrafo de líneas
            PdfPCell lineaGuionesCell = new PdfPCell(lineaGuiones)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = 5f,
                PaddingBottom = 5f,
                PaddingLeft = 45f,
                PaddingRight = 15f
            };

            // Crear una tabla para contener la celda
            PdfPTable lineasTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            // Añadir la celda a la tabla
            lineasTable.AddCell(lineaGuionesCell);

            // Añadir la tabla al documento
            document.Add(lineasTable);

            // Crear el párrafo del nombre de usuario
            Paragraph nombreUsuario = new Paragraph(Usuario.Apellidos + " " + Usuario.Nombres, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8))
            {
                Alignment = Element.ALIGN_CENTER
            };

            // Crear una celda y añadir el párrafo del nombre de usuario
            PdfPCell nombreUsuarioCell = new PdfPCell(nombreUsuario)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = 5f,
                PaddingBottom = 5f,
                PaddingLeft = 75f,
                PaddingRight = 15f
            };

            // Crear una tabla para contener la celda
            PdfPTable nombreUsuarioTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            // Añadir la celda a la tabla
            nombreUsuarioTable.AddCell(nombreUsuarioCell);

            // Añadir la tabla al documento
            document.Add(nombreUsuarioTable);

            // Crear el párrafo de la leyenda del departamento
            Paragraph leyendaInventarios = new Paragraph("DEPARTAMENTO DE CONTROL DE INVENTARIOS", FontFactory.GetFont(FontFactory.HELVETICA, 7))
            {
                Alignment = Element.ALIGN_CENTER
            };

            // Crear una celda y añadir el párrafo de la leyenda del departamento
            PdfPCell leyendaInventariosCell = new PdfPCell(leyendaInventarios)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = 5f,
                PaddingBottom = 20f,
                PaddingLeft = 50f,
                PaddingRight = 15f
            };

            // Crear una tabla para contener la celda
            PdfPTable leyendaInventariosTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            // Añadir la celda a la tabla
            leyendaInventariosTable.AddCell(leyendaInventariosCell);


            // Añadir la tabla al documento
            document.Add(leyendaInventariosTable);
            #endregion
        }

        public void FirmaRecibio(Document document, int NumeroEmpleado)
        {
            var Empleado = _db.PLU_OP_Empleados.Where(x => x.NumeroEmpleado == NumeroEmpleado).FirstOrDefault();
            #region
            // Crear el párrafo
            Paragraph recibio = new Paragraph("Recibio:", FontFactory.GetFont(FontFactory.HELVETICA, 7))
            {
                Alignment = Element.ALIGN_CENTER
            };

            // Crear una celda y añadir el párrafo
            PdfPCell recibioCell = new PdfPCell(recibio)
            {
                Border = PdfPCell.NO_BORDER, // Sin borde si no lo necesitas
                PaddingTop = -82f, // Padding superior
                PaddingBottom = 10f, // Padding inferior
                PaddingLeft = 622f, // Padding izquierdo
                PaddingRight = 15f // Padding derecho
            };

            // Crear una tabla para contener la celda
            PdfPTable reciboTable = new PdfPTable(1)
            {
                WidthPercentage = 100 // Ajusta según necesites
            };

            // Añadir la celda a la tabla
            reciboTable.AddCell(recibioCell);

            // Añadir la tabla al documento
            document.Add(reciboTable);

            // Crear el párrafo de líneas
            Paragraph lineaGuiones2 = new Paragraph("_______________________________________", FontFactory.GetFont(FontFactory.HELVETICA, 8))
            {
                Alignment = Element.ALIGN_CENTER
            };

            // Crear una celda y añadir el párrafo de líneas
            PdfPCell lineaGuionesCell2 = new PdfPCell(lineaGuiones2)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = -62f,
                PaddingBottom = 5f,
                PaddingLeft = 550f,
                PaddingRight = 15f
            };

            // Crear una tabla para contener la celda
            PdfPTable lineasTable2 = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            // Añadir la celda a la tabla
            lineasTable2.AddCell(lineaGuionesCell2);

            // Añadir la tabla al documento
            document.Add(lineasTable2);

            // Crear el párrafo del nombre de usuario
            Paragraph nombreUsuario2 = new Paragraph(Empleado.ApellidoP + " " + Empleado.ApellidoM + " " + Empleado.Nombres, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8))
            {
                Alignment = Element.ALIGN_CENTER
            };

            // Crear una celda y añadir el párrafo del nombre de usuario
            PdfPCell nombreUsuarioCell2 = new PdfPCell(nombreUsuario2)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = -42f,
                PaddingBottom = 5f,
                PaddingLeft = 560f,
                PaddingRight = 15f
            };

            // Crear una tabla para contener la celda
            PdfPTable nombreUsuarioTable2 = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            // Añadir la celda a la tabla
            nombreUsuarioTable2.AddCell(nombreUsuarioCell2);

            // Añadir la tabla al documento
            document.Add(nombreUsuarioTable2);

            // Crear el párrafo de la leyenda del departamento
            Paragraph leyendaInventarios2 = new Paragraph("FIRMA DEL RESPONSABLE", FontFactory.GetFont(FontFactory.HELVETICA, 7))
            {
                Alignment = Element.ALIGN_CENTER
            };

            // Crear una celda y añadir el párrafo de la leyenda del departamento
            PdfPCell leyendaInventariosCell2 = new PdfPCell(leyendaInventarios2)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = -25f,
                PaddingBottom = 20f,
                PaddingLeft = 590f,
                PaddingRight = 15f
            };

            // Crear una tabla para contener la celda
            PdfPTable leyendaInventariosTable2 = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            // Añadir la celda a la tabla
            leyendaInventariosTable2.AddCell(leyendaInventariosCell2);


            // Añadir la tabla al documento
            document.Add(leyendaInventariosTable2);
            #endregion
        }

        private void LeyendaHoja(Document document, int numeroHoja, int totalHojas)
        {
            Font fontLeyenda = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.NORMAL, BaseColor.BLACK);
            Paragraph paragraph = new Paragraph($"Hoja {numeroHoja} de {totalHojas}", fontLeyenda);
            paragraph.Alignment = Element.ALIGN_RIGHT;

            // Establecer márgenes izquierdo y derecho
            paragraph.IndentationLeft = 10f; // Margen izquierdo
            paragraph.IndentationRight = 15f; // Margen derecho

            document.Add(paragraph);
        }

        public string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }


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

        public string GenerarFolioInventario()
        {
            string year = DateTime.Now.Year.ToString().Substring(2); // '24' para el año 2024
            int nuevoConsecutivo = 1;

            // Obtener el último número de folio existente para el año actual en la tabla PLU_OP_InventarioFisico
            var ultimoFolio = _db.PLU_OP_InventarioFisico
                                 .Where(d => d.FolioInventario.StartsWith(year + "-"))
                                 .OrderByDescending(d => d.FolioInventario)
                                 .Select(d => d.FolioInventario)
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