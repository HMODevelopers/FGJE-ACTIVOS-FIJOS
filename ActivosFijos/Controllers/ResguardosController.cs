using ActivosFijos.Helpers;
using ActivosFijos.Models;
using ActivosFijos.Models.ViewModels;
using Helpers;
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

        public ActionResult ListarCambios(string Orden = "", int iPagina = 1, int iPerPage = 10 , string NombreResguardante = "", DateTime? FechaCambio = null) 
        {
            ViewBag.Order = Orden;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;


            ViewBag.FolioCambioSortParam = Orden == "FolioCambio" ? "FolioCambio_desc" : "FolioCambio";
            ViewBag.UsuarioResponsableSortParam = Orden == "UsuarioCambio" ? "UsuarioCambio_desc" : "UsuarioCambio";
            ViewBag.NombreResguardanteSortParam = Orden == "NombreResguardanteCambio" ? "NombreResguardanteCambio_desc" : "NombreResguardanteCambio";
            ViewBag.FechaCambioSortParam = Orden == "FechaCreacion" ? "FechaCreacion_desc" : "FechaCreacion";

            ViewBag.NombreResguardante = NombreResguardante;
            ViewBag.FechaAlta = FechaCambio;

            var vModel = ResguardoB.GetAllCambiosActivos(Orden, iPagina, iPerPage, NombreResguardante, FechaCambio);

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
                        Encabezado(document);
                        DatosEmpleado(document, NumeroEmpleado);
                        Leyenda1(document);
                        // Obtener los activos para esta página
                        var activosParaPagina = activosJson.Skip(activosProcesados).Take(20).ToList(); // 20 activos por página
                        if(TipoMovimiento == 1)
                        {
                            TablaActivosAgregar(document, activosParaPagina, FechaCreacion);
                        }
                        else if (TipoMovimiento == 2)
                        {
                            TablaActivosCambios(document, activosParaPagina, FechaCreacion);
                        }
                        
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

        public void TablaActivosAgregar(Document document, List<int> activos , DateTime FechaCreacion)
        {
            var dataActivos = _db.PLU_OP_Activos.Where(x => activos.Contains(x.IdActivos)).ToList();

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
            foreach (var activo in dataActivos)
            {
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_CAT_Facturas?.FolioFactura ?? "SIN FACTURA", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_CAT_CategoriaActivo?.NombreCategoria ?? "SIN CATEGORIA",10), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.Descripcion ?? "SIN DESCRIPCION", 66), FontFactory.GetFont(FontFactory.HELVETICA, 5))));
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.NumeroSerie ?? "SIN N/S", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_CAT_MarcaActivo?.NombreMarca ?? "SIN MARCA", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.NumeroInventario ?? "SIN N/I", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase("ASIGNADO", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(FechaCreacion.ToString("dd/MM/yyyy"), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
            }

            // Calcular cuántas filas en blanco se necesitan para completar 20 filas
            int rowCount = dataActivos.Count;
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

        public void TablaActivosCambios(Document document, List<int> activos, DateTime FechaCreacion)
        {
            var dataActivos = _db.PLU_OP_Activos.Where(x => activos.Contains(x.IdActivos)).ToList();

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
            foreach (var activo in dataActivos)
            {
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_CAT_Facturas?.FolioFactura ?? "SIN FACTURA", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_CAT_CategoriaActivo?.NombreCategoria ?? "SIN CATEGORIA",10), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.Descripcion ?? "SIN DESCRIPCION", 66), FontFactory.GetFont(FontFactory.HELVETICA, 5))));
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.NumeroSerie ?? "SIN N/S",15), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_CAT_MarcaActivo?.NombreMarca ?? "SIN MARCA",10), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.NumeroInventario ?? "SIN N/I", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase("CAMBIO RESGUARDO", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(FechaCreacion.ToString("dd/MM/yyyy"), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
            }

            // Calcular cuántas filas en blanco se necesitan para completar 20 filas
            int rowCount = dataActivos.Count;
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
    }
}