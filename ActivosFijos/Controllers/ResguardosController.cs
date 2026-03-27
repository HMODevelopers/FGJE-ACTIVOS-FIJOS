using ActivosFijos.Helpers;
using ActivosFijos.Models;
using ActivosFijos.Models.ViewModels;
using Helpers;
using ActivosFijos.Services.Pdf;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Newtonsoft.Json;
using static EDUES_ADMIN.Filters.AdminFilters;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;
using System.Web.Helpers;

namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class ResguardosController : Controller
    {
        ModelContext _db = new ModelContext();
        ResponseModel _rm = new ResponseModel();
        ResguardosHelper ResguardoB = new ResguardosHelper();

        // GET: Activos
        public ActionResult Index(string Orden = "", string numeroinventario = "", string descripcion = "", int IdCategoria = 0, int numeroEmpleado = 0, string apellidoPaterno = "", string apellidoMaterno = "", string nombreEmpleado = "", string NumeroSerie = "", int iPagina = 1, int iPerPage = 10)
        
        {
            ViewBag.Order = Orden;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;

            ViewBag.IdActivosSortParam = Orden == "#" ? "#_desc" : "#";
            ViewBag.NumeroResguardoSortPa_rm = Orden == "numeroResguardo" ? "numeroResguardo_desc" : "numeroResguardo";
            ViewBag.NumeroInventarioSortPa_rm = Orden == "numInventario" ? "numInventario_desc" : "numInventario";
            ViewBag.DescripcionSortPa_rm = Orden == "descripcion" ? "descripcion_desc" : "descripcion";
            ViewBag.CategoriaSortPa_rm = Orden == "categoria" ? "categoria_desc" : "categoria";
            ViewBag.EstadoFisocoSortParam = Orden == "estadofisico" ? "estadofisico_desc" : "estadofisico";
            ViewBag.NombreEmpleadoSortParam = Orden == "nombreEmpleado" ? "nombreEmpleado_desc" : "nombreEmpleado";

            var vModel = ResguardoB.GetAll(Orden, numeroinventario, descripcion, IdCategoria, numeroEmpleado, apellidoPaterno, apellidoMaterno, nombreEmpleado,NumeroSerie , iPagina, iPerPage);

            ViewBag.numeroinventario = numeroinventario;
            ViewBag.descripcion = descripcion;
            ViewBag.IdCategoria = IdCategoria;
            ViewBag.numeroEmpleado = numeroEmpleado;
            ViewBag.apellidoPaterno = apellidoPaterno;
            ViewBag.apellidoMaterno = apellidoMaterno;
            ViewBag.nombreEmpleado = nombreEmpleado;
            ViewBag.NumeroSerie = NumeroSerie;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaActivos", vModel);
            }

            return View(vModel);
        }


        public ActionResult Detalles(int id)
        {

            var activo = _db.PLU_OP_Activos.Where(x => x.IdActivos == id).Select(x => new DetalleActivosViewModel
            {

                IdActivos = x.IdActivos,
                NumeroInventario = x.NumeroInventario,
                NumeroSerie = x.NumeroSerie,
                Descripcion = x.Descripcion,
                Categoria = x.PLU_CAT_CategoriaActivo.NombreCategoria,
                Marca = x.PLU_CAT_MarcaActivo.NombreMarca,
                Concepto = x.PLU_CAT_Conceptos.NombreConcepto,
                Clasificador = x.PLU_CAT_Clasificadores.ClasificadorDescripcion,
                EstadoFisico = x.PLU_CAT_EstadoFisicoActivo.Descripcion,
                EstadoActivo = x.PLU_CAT_EstatusActivo.Descripcion,
                Almacen = x.PLU_CAT_Almacenes.NombreAlmacen,
                Fotos = x.PLU_OP_FotosActivos.Where(f => f.Activo == true).Select(f => new FotosActivosViewModel { IdFoto = f.IdFoto, RutaFoto = f.RutaFoto }).ToList()

            }).FirstOrDefault();

            return View("Detalles", activo);
        }

        public ActionResult ListarAltas(string Orden = "" , int iPagina = 1, int iPerPage = 10, string NombreResguardante = "", DateTime? FechaAlta = null)
        {
            ViewBag.Order = Orden;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;


            ViewBag.FolioAltaSortParam = Orden == "FolioAlta" ? "FolioAlta_desc" : "FolioAlta";
            ViewBag.UsuarioResponsableSortParam = Orden == "UsuarioAlta" ? "UsuarioAlta_desc" : "UsuarioAlta";
            ViewBag.NombreResguardanteSortParam = Orden == "NombreResguardanteAlta" ? "NombreResguardanteAlta_desc" : "NombreResguardanteAlta";
            ViewBag.FechaAltaSortParam = Orden == "FechaCreacion" ? "FechaCreacion_desc" : "FechaCreacion";

            ViewBag.NombreResguardante = NombreResguardante;
            ViewBag.FechaAlta = FechaAlta;

            var vModel = ResguardoB.GetAllAltasActivos(Orden, iPagina, iPerPage, NombreResguardante, FechaAlta);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaAltas", vModel);
            }

            return View(vModel);
        }

        public ActionResult DetallesAltas(string id)
        {
            var activos = _db.PLU_OP_AltasActivos
                 .Where(x => x.FolioAlta == id)
                 .GroupBy(x => new
                 {
                     x.FolioAlta,
                     x.PLU_OP_Activos.NumeroInventario,
                     x.PLU_OP_Activos.PLU_CAT_CategoriaActivo.NombreCategoria,
                     x.PLU_OP_Activos.Descripcion,
                     x.PLU_OP_Activos.NumeroSerie,
                     x.PLU_OP_Activos.PLU_CAT_MarcaActivo.NombreMarca,
                     x.FechaCreacion
                 })
                 .Select(g => new DetalleAltasActivosViewModel
                 {
                     FolioAlta = g.Key.FolioAlta,
                     NumeroInventario = g.Key.NumeroInventario,
                     Categoria = g.Key.NombreCategoria,
                     Descripcion = g.Key.Descripcion,
                     NumeroSerie = g.Key.NumeroSerie,
                     Marca = g.Key.NombreMarca,
                     FechaAlta = g.Key.FechaCreacion
                 })
                .ToList();


            return View("DetallesAltas", activos);
        }

        [HttpGet]
        public JsonResult ObtenerActivosPorFolioAltas(string FolioAlta)
        {
            var activos = _db.PLU_OP_AltasActivos.Where(a => a.FolioAlta == FolioAlta).ToList();

            if (!activos.Any())
            {
                return Json(new { success = false, message = "No se encontraron activos para este folio." }, JsonRequestBehavior.AllowGet);
            }

            var idActivosArray = activos.Select(a => a.IdActivos).ToArray(); // Array de IdActivos
            var IdEmpleado = activos.FirstOrDefault().IdEmpleado;
            var NumeroEmpleado = _db.PLU_OP_Empleados.Where(x => x.IdEmpleado == IdEmpleado).Select(x => new { x.NumeroEmpleado }).FirstOrDefault();
            var datosGenerales = new
            {
                NumeroEmpleado = NumeroEmpleado.NumeroEmpleado, // Se asume que todos los registros tienen el mismo IdEmpleado
                FechaCreacion = activos.First().FechaCreacion // Se asume que todos los registros tienen la misma FechaCreacion
            };

            return Json(new { success = true, IdActivos = idActivosArray, DatosGenerales = datosGenerales }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListarCambios(
            string Orden = "",
            int iPagina = 1,
            int iPerPage = 10,
            string FolioCambio = "",
            string NumeroInventario = "",
            string Descripcion = "",
            string NumeroSerie = "",
            string NumeroEmpleadoAnterior = "",
            string EmpleadoAnterior = "",
            string NumeroEmpleadoActual = "",
            string EmpleadoActual = "",
            string FolioOficio = "",
            DateTime? FechaInicio = null,
            DateTime? FechaFin = null,
            bool? Activo = null,
            string NombreResguardante = "",
            DateTime? FechaCambio = null)
        {
            ViewBag.Order = Orden;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;


            ViewBag.FolioCambioSortParam = Orden == "FolioCambio" ? "FolioCambio_desc" : "FolioCambio";
            ViewBag.UsuarioResponsableSortParam = Orden == "UsuarioCambio" ? "UsuarioCambio_desc" : "UsuarioCambio";
            ViewBag.NombreResguardanteSortParam = Orden == "NombreResguardanteCambio" ? "NombreResguardanteCambio_desc" : "NombreResguardanteCambio";
            ViewBag.FechaCambioSortParam = Orden == "FechaCreacion" ? "FechaCreacion_desc" : "FechaCreacion";
            ViewBag.NumeroActivosSortParam = Orden == "NumeroCambios" ? "NumeroCambios_desc" : "NumeroCambios";
            ViewBag.OficioCambioSortParam = Orden == "OficioCambio" ? "OficioCambio_desc" : "OficioCambio";

            if (!string.IsNullOrWhiteSpace(NombreResguardante) && string.IsNullOrWhiteSpace(EmpleadoActual))
            {
                EmpleadoActual = NombreResguardante;
            }

            if (FechaCambio.HasValue && !FechaInicio.HasValue && !FechaFin.HasValue)
            {
                FechaInicio = FechaCambio.Value.Date;
                FechaFin = FechaCambio.Value.Date;
            }

            ViewBag.FolioCambio = FolioCambio;
            ViewBag.NumeroInventario = NumeroInventario;
            ViewBag.Descripcion = Descripcion;
            ViewBag.NumeroSerie = NumeroSerie;
            ViewBag.NumeroEmpleadoAnterior = NumeroEmpleadoAnterior;
            ViewBag.EmpleadoAnterior = EmpleadoAnterior;
            ViewBag.NumeroEmpleadoActual = NumeroEmpleadoActual;
            ViewBag.EmpleadoActual = EmpleadoActual;
            ViewBag.FolioOficio = FolioOficio;
            ViewBag.FechaInicio = FechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = FechaFin?.ToString("yyyy-MM-dd");
            ViewBag.Activo = Activo;

            var vModel = ResguardoB.GetAllCambiosActivos(
                Orden,
                iPagina,
                iPerPage,
                FolioCambio,
                NumeroInventario,
                Descripcion,
                NumeroSerie,
                NumeroEmpleadoAnterior,
                EmpleadoAnterior,
                NumeroEmpleadoActual,
                EmpleadoActual,
                FolioOficio,
                FechaInicio,
                FechaFin,
                Activo);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaCambios", vModel);
            }

            return View(vModel);
        }

        public ActionResult DetallesCambios(string id)
        {
            var activos = _db.PLU_OP_CambiosActivos
                 .Where(x => x.FolioCambio == id)
                 .GroupBy(x => new
                 {
                     x.FolioCambio,
                     x.PLU_OP_Activos.NumeroInventario,
                     x.PLU_OP_Activos.PLU_CAT_CategoriaActivo.NombreCategoria,
                     x.PLU_OP_Activos.Descripcion,
                     x.PLU_OP_Activos.NumeroSerie,
                     x.PLU_OP_Activos.PLU_CAT_MarcaActivo.NombreMarca,
                     x.FechaCreacion
                 })
                 .Select(g => new DetalleCambioActivosViewModel
                 {
                     FolioCambio = g.Key.FolioCambio,
                     NumeroInventario = g.Key.NumeroInventario,
                     Categoria = g.Key.NombreCategoria,
                     Descripcion = g.Key.Descripcion,
                     NumeroSerie = g.Key.NumeroSerie,
                     Marca = g.Key.NombreMarca,
                     FechaCambio = g.Key.FechaCreacion
                 })
                .ToList();


            return View("DetallesCambios", activos);
        }

        [HttpGet]
        public JsonResult ObtenerActivosPorFolioCambios(string FolioCambio)
        {
            var activos = _db.PLU_OP_CambiosActivos.Where(a => a.FolioCambio == FolioCambio).ToList();

            if (!activos.Any())
            {
                return Json(new { success = false, message = "No se encontraron activos para este folio." }, JsonRequestBehavior.AllowGet);
            }

            var idActivosArray = activos.Select(a => a.IdActivos).ToArray(); // Array de IdActivos
            var IdEmpleado = activos.FirstOrDefault().IdEmpleadoActual;
            var NumeroEmpleado = _db.PLU_OP_Empleados.Where(x => x.IdEmpleado == IdEmpleado).Select(x => new { x.NumeroEmpleado }).FirstOrDefault();
            var datosGenerales = new
            {
                NumeroEmpleado = NumeroEmpleado.NumeroEmpleado, // Se asume que todos los registros tienen el mismo IdEmpleado
                FechaCreacion = activos.First().FechaCreacion // Se asume que todos los registros tienen la misma FechaCreacion
            };

            return Json(new { success = true, IdActivos = idActivosArray, DatosGenerales = datosGenerales }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Editar(int? page, string numeroinventario = "", string descripcion = "", int numeroempleado = 0, string nombres = "", string apelllidop = "", string NumeroSerie = "")
        {


            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var vModel = ResguardoB.GetAllCambiar(numeroinventario, descripcion, numeroempleado, nombres, apelllidop, NumeroSerie , pageNumber, pageSize);

            ViewBag.NumeroInvetario = numeroinventario;
            ViewBag.Descripcion = descripcion;
            ViewBag.NumEmp = numeroempleado;
            ViewBag.Nombres = nombres;
            ViewBag.Apellidop = apelllidop;
            ViewBag.NumeroSerie = NumeroSerie;  

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaActivosCambiar", vModel);
            }

            return View(vModel);
        }

        [HttpPost]
        public async Task<ActionResult> Editar(int NumeroEmpleado, string FolioOficio, HttpPostedFileBase file, string selectedActivos, DateTime FechaCreacion)
        {
            // Deserializar la lista de activos seleccionados
            var activosSeleccionados = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(selectedActivos);

            // Llamar al método CambiarResguardoAsync usando await
            var _rm = await ResguardoB.CambiarResguardoAsync(NumeroEmpleado, FolioOficio, file, activosSeleccionados, FechaCreacion);

            if (_rm.response)
            {
                var fechaCreacionStr = FechaCreacion.ToString("yyyy-MM-dd");
                _rm.message = "Cambio de resguardo agregado con éxito.";
                _rm.function = $"HideLoading();GenerarYAbrirPdf({JsonConvert.SerializeObject(activosSeleccionados)}, {NumeroEmpleado}, {2}, '{fechaCreacionStr}');";
                _rm.href = Url.Action("Index", "Resguardos");
                _rm.error = false;
            }
            else
            {
                _rm.message = "Error al cambiar resguardo.";
                _rm.function = "HideLoading();";
                _rm.error = true;
            }

            // Devolver el resultado como JSON
            return Json(_rm);
        }


        [HttpGet]
        public ActionResult Agregar(int? page, string numeroinventario = "", string descripcion = "" , string NumeroSerie = "")
        {


            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var vModel = ResguardoB.GetAllAgregar(numeroinventario, descripcion, NumeroSerie, pageNumber, pageSize);

            ViewBag.NumeroInvetario = numeroinventario;
            ViewBag.Descripcion = descripcion;
            ViewBag.NumeroSerie = NumeroSerie;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaActivosAgregar", vModel);
            }

            return View(vModel);
        }

        [HttpPost]
        public async Task<ActionResult> Agregar(int NumeroEmpleado, string FolioOficio, HttpPostedFileBase file, string selectedActivos, DateTime FechaCreacion)
        {
            try
            {
                // Deserializar los activos seleccionados
                var activosSeleccionados = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(selectedActivos);

                // Llamar al método asíncrono para agregar el resguardo
                var responseModel = await ResguardoB.AgregarResguardoAsync(NumeroEmpleado, FolioOficio, file, activosSeleccionados, FechaCreacion);

                if (responseModel.response)
                {
                    var fechaCreacionStr = FechaCreacion.ToString("yyyy-MM-dd");
                    responseModel.message = "Resguardo agregado con éxito.";
                    responseModel.function = $"HideLoading();GenerarYAbrirPdf({JsonConvert.SerializeObject(activosSeleccionados)}, {NumeroEmpleado}, {1}, '{fechaCreacionStr}');";
                    responseModel.href = Url.Action("Index", "Resguardos");
                    responseModel.error = false;
                }
                else
                {
                    responseModel.message = responseModel.message ?? "Error al agregar resguardo.";
                    responseModel.function = "HideLoading();";
                    responseModel.error = true;
                }

                return Json(responseModel);
            }
            catch (Exception ex)
            {
                // Capturar cualquier error inesperado y devolver mensaje genérico
                return Json(new ResponseModel
                {
                    response = false,
                    message = $"Error inesperado: {ex.Message}",
                    function = "HideLoading();",
                    error = true
                });
            }
        }

        [HttpGet]
        public JsonResult GetEmpleados(string searchTerm)
        {
            var result = _db.PLU_OP_Empleados
                .Where(e => e.NombreCompleto.ToLower().Contains(searchTerm.ToLower()))
                .Select(e => new { id = e.NumeroEmpleado, text = e.NumeroEmpleado + " - " + e.NombreCompleto })
                .ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GenerarPdf(string activos, int NumeroEmpleado, int TipoMovimiento, DateTime FechaCreacion)
        {
            try
            {
                var activosJson = JsonConvert.DeserializeObject<List<int>>(activos);

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
                    int totalPages = (int)Math.Ceiling((double)activosJson.Count / 20); // Calculamos el total de páginas necesarias
                    var commonBuilder = new PdfCommonBuilder(_db, Server);
                    var resguardosBuilder = new ResguardosPdfBuilder(_db);

                    // Bucle para agregar páginas adicionales según sea necesario
                    while (activosProcesados < activosJson.Count)
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
                        var activosParaPagina = activosJson.Skip(activosProcesados).Take(20).ToList(); // 20 activos por página
                        if(TipoMovimiento == 1)
                        {
                            resguardosBuilder.RenderTablaActivosAgregar(document, activosParaPagina, FechaCreacion);
                        }
                        else if (TipoMovimiento == 2)
                        {
                            resguardosBuilder.RenderTablaActivosCambios(document, activosParaPagina, FechaCreacion);
                        }
                        
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

    }
}
