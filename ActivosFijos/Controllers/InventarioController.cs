using ActivosFijos.Helpers;
using ActivosFijos.Models;
using Helpers;
using ActivosFijos.Services.Pdf;
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
                    var commonBuilder = new PdfCommonBuilder(_db, Server);
                    var inventarioBuilder = new InventarioPdfBuilder();

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
                        commonBuilder.RenderEncabezado(document);
                        commonBuilder.RenderDatosEmpleado(document, NumeroEmpleado);
                        commonBuilder.RenderLeyenda1(document);
                        // Obtener los activos para esta página
                        var activosParaPagina = inventarioActual.Skip(activosProcesados).Take(20).ToList(); // 20 activos por página
                        inventarioBuilder.RenderTablaActivos(document, activosParaPagina);
                        commonBuilder.RenderLeyenda2(document, NumeroEmpleado);
                        commonBuilder.RenderFirmaReviso(document);
                        commonBuilder.RenderFirmaRecibio(document, NumeroEmpleado);
                        commonBuilder.RenderLeyendaHoja(document, activosProcesados / 20 + 1, totalPages); // Agregar la leyenda de la hoja

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
                    var commonBuilder = new PdfCommonBuilder(_db, Server);
                    var inventarioBuilder = new InventarioPdfBuilder();

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
                        commonBuilder.RenderEncabezado(document);
                        commonBuilder.RenderDatosEmpleado(document, datainv.NumeroEmpleado);
                        commonBuilder.RenderLeyenda1(document);
                        // Obtener los activos para esta página
                        var activosParaPagina = inventarioActual.Skip(activosProcesados).Take(20).ToList(); // 20 activos por página
                        inventarioBuilder.RenderTablaActivos2(document, activosParaPagina);
                        commonBuilder.RenderLeyenda2(document, datainv.NumeroEmpleado);
                        commonBuilder.RenderFirmaReviso(document);
                        commonBuilder.RenderFirmaRecibio(document, datainv.NumeroEmpleado);
                        commonBuilder.RenderLeyendaHoja(document, activosProcesados / 20 + 1, totalPages); // Agregar la leyenda de la hoja

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
