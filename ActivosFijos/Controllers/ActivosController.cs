using ActivosFijos.Helpers;
using ActivosFijos.Models;
using ActivosFijos.Models.ViewModels;
using Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static EDUES_ADMIN.Filters.AdminFilters;


namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class ActivosController : Controller
    {

        ModelContext _db = new ModelContext();
        ActivosHelper ActivosB = new ActivosHelper();

        // GET: Activos
        [HttpGet]
        // [OutputCache(Duration = 15, VaryByParam = "*")] // opcional: micro-cache 15s
        public ActionResult Index(string Orden = null,string numeroinventario = null,string descripcion = null, int IdCategoria = 0,int IdConcepto = 0,string NumeroSerie = null, int IdAlmacen = 0, int IdEstadoFisico = 0, int IdEstatusActivo = 0, int iPagina = 1, int iPerPage = 10)
        {
            // ===== Normalización segura =====
            string NI = (numeroinventario ?? "").Trim().ToUpperInvariant();
            string DESC = (descripcion ?? "").Trim().ToUpperInvariant();
            string NS = (NumeroSerie ?? "").Trim().ToUpperInvariant();

            // ===== Bounds y defaults =====
            if (iPagina < 1) iPagina = 1;

            // tamaños permitidos (evita page sizes gigantes por error)
            var allowedPageSizes = new[] { 10, 25, 50, 100 };
            if (!allowedPageSizes.Contains(iPerPage)) iPerPage = 10;

            Orden = Orden ?? "#";

            // ===== Helpers =====
            string Toggle(string current, string key)
            {
                return string.Equals(current, key, StringComparison.OrdinalIgnoreCase)
                    ? key + "_desc"
                    : key;
            }

            // ===== ViewBags de estado (para la vista/paginador) =====
            ViewBag.Order = Orden;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;

            ViewBag.IdActivosSortParam = Toggle(Orden, "#");
            ViewBag.NumeroInventarioSortParm = Toggle(Orden, "numInventario");
            ViewBag.DescripcionSortParm = Toggle(Orden, "descripcion");
            ViewBag.CategoriaSortParm = Toggle(Orden, "categoria");
            ViewBag.ConceptoSortParm = Toggle(Orden, "concepto");
            ViewBag.ClasificadorSortParm = Toggle(Orden, "clasificador");
            ViewBag.EstadoFisocoSortParam = Toggle(Orden, "estadofisico");

            // Mantén filtros para re-pintar la UI y construir links
            ViewBag.NumeroInvetario = NI;
            ViewBag.Descripcion = DESC;
            ViewBag.Categoria = IdCategoria;
            ViewBag.Concepto = IdConcepto;
            ViewBag.NumeroSerie = NS;
            ViewBag.Almacen = IdAlmacen;
            ViewBag.EstadosFisicos = IdEstadoFisico;
            ViewBag.EstatusActivos = IdEstatusActivo;

            // ===== Data =====
            // Nota: tu BLL ya acepta strings; le pasamos los normalizados (NI, DESC, NS)
            var vModel = ActivosB.GetAll(Orden,NI,DESC,IdCategoria,IdConcepto,NS,IdAlmacen,IdEstadoFisico,IdEstatusActivo,iPagina,iPerPage);

            CargarCatalogos();

            if (Request.IsAjaxRequest())
                return PartialView("_ListaActivos", vModel);

            return View(vModel);
        }

        public ActionResult Agregar()
        {
            CargarCatalogos();
            return View();
        }

        public ActionResult Editar(int Id)
        {
            CargarCatalogos();
            var activo = _db.PLU_OP_Activos.Include("PLU_OP_FotosActivos").FirstOrDefault(x => x.IdActivos == Id);
            ViewBag.FotosActivos = activo.PLU_OP_FotosActivos.Where(x => x.Activo == true).ToList();
            return View("Editar", activo);
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

            return View("Detalle", activo);
        }

        public ActionResult ImprimirEtiqueta(int id)
        {
            // Obtener el ítem de la base de datos
            var item = _db.PLU_OP_Activos.Find(id);

            // Tamaño de página para la etiqueta
            float widthInches = 4; // Ancho de la etiqueta en pulgadas
            float heightInches = 3; // Altura de la etiqueta en pulgadas

            // Crear un nuevo documento PDF con el tamaño de página adecuado
            Document document = new Document(new Rectangle(widthInches * 72, heightInches * 72));

            // Escribir el documento en la memoria
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            try
            {
                // Crear una tabla principal con dos columnas
                PdfPTable mainTable = new PdfPTable(2);
                mainTable.WidthPercentage = 100;
                float[] mainWidths = new float[] { 70f, 30f }; // Ajusta el ancho de las columnas
                mainTable.SetWidths(mainWidths);

                // Columna 1: Texto del activo
                PdfPCell textCell = new PdfPCell();
                textCell.Border = PdfPCell.NO_BORDER;
                textCell.VerticalAlignment = Element.ALIGN_TOP;
                textCell.PaddingLeft = 10;
                textCell.PaddingTop = -20; // Ajustar el espacio superior del texto

                // Agregar el contenido del texto a la celda
                // Crear celdas de texto con valores condicionales
                string categoria = !string.IsNullOrWhiteSpace(item.PLU_CAT_CategoriaActivo?.NombreCategoria)
                    ? item.PLU_CAT_CategoriaActivo.NombreCategoria
                    : "SIN CATEGORIA";

                string descripcion = !string.IsNullOrWhiteSpace(item.Descripcion)
                    ? item.Descripcion
                    : "SIN DESC";

                string numeroSerie = !string.IsNullOrWhiteSpace(item.NumeroSerie)
                    ? item.NumeroSerie
                    : "SIN N/S";

                string marca = !string.IsNullOrWhiteSpace(item.PLU_CAT_MarcaActivo?.NombreMarca)
                    ? item.PLU_CAT_MarcaActivo.NombreMarca
                    : "SIN MARCA";

                string numeroInventario = !string.IsNullOrWhiteSpace(item.NumeroInventario)
                    ? item.NumeroInventario
                    : "SIN N/I";

                // Agregar elementos a la celda de texto
                textCell.AddElement(new Paragraph(categoria, new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL)));
                textCell.AddElement(new Paragraph(descripcion, new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL)));
                textCell.AddElement(new Paragraph(numeroSerie, new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL)));
                textCell.AddElement(new Paragraph(marca, new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL)));
                textCell.AddElement(new Paragraph(numeroInventario, new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD)));

                // Agregar la celda de texto a la tabla principal
                mainTable.AddCell(textCell);

                // Columna 2: Imágenes (logo y código QR)
                PdfPTable imageTable = new PdfPTable(1); // Una tabla interna para las imágenes
                imageTable.WidthPercentage = 100;

                // Celda para el logo
                PdfPCell logoCell = new PdfPCell();
                logoCell.Border = PdfPCell.NO_BORDER;
                logoCell.VerticalAlignment = Element.ALIGN_TOP;
                logoCell.PaddingTop = -20; // Ajustar el espacio superior del logo
                logoCell.PaddingLeft = -30;

                // Cargar la imagen del logo
                string logoPath = Server.MapPath("~/Content/assets/images/logo-sm-fgje-negro.png"); // Ruta correcta a tu logotipo
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(40, 40); // Ajustar el tamaño del logotipo

                // Alinear el logo a la derecha dentro de la celda
                logo.Alignment = Element.ALIGN_RIGHT;

                // Agregar el logotipo a la celda del logo
                logoCell.AddElement(logo);

                // Agregar la celda del logo a la tabla interna de imágenes
                imageTable.AddCell(logoCell);

                // Celda para el código QR
                PdfPCell qrCell = new PdfPCell();
                qrCell.Border = PdfPCell.NO_BORDER;
                qrCell.VerticalAlignment = Element.ALIGN_TOP;
                qrCell.PaddingTop = 5; // Ajustar el espacio superior del código QR
                logoCell.PaddingLeft = -30;

                // Generar el código QR usando la clase QrCodeHelper
                string qrText = item.NumeroInventario;
                Image qrCodeImage = QrCodeHelper.GenerateQRCodeImage(qrText);

                // Cargar la imagen del código QR
                Image qr = Image.GetInstance(qrCodeImage);
                qr.ScaleToFit(40, 40); // Ajustar el tamaño del código QR

                // Alinear el código QR a la derecha dentro de la celda
                qr.Alignment = Element.ALIGN_RIGHT;

                // Agregar el código QR a la celda del código QR
                qrCell.AddElement(qr);

                // Agregar la celda del código QR a la tabla interna de imágenes
                imageTable.AddCell(qrCell);

                // Agregar la tabla interna de imágenes a la columna 2 de la tabla principal
                PdfPCell imageCell = new PdfPCell(imageTable);
                imageCell.Border = PdfPCell.NO_BORDER;
                mainTable.AddCell(imageCell);

                // Agregar la tabla principal al documento
                document.Add(mainTable);
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir durante la generación del PDF
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }
            finally
            {
                // Cerrar el documento
                if (document.IsOpen())
                {
                    document.Close();
                }
            }

            // Preparar el PDF para abrir en una nueva pestaña
            var content = stream.ToArray();
            return new FileStreamResult(new MemoryStream(content), "application/pdf");
        }

        public ActionResult ImprimirEtiquetaGrande(int id)
        {
            // Obtener el ítem de la base de datos
            var item = _db.PLU_OP_Activos.Find(id);

            // Tamaño de página para la etiqueta
            float widthInches = 4; // Ancho de la etiqueta en pulgadas
            float heightInches = 3; // Altura de la etiqueta en pulgadas

            // Crear un nuevo documento PDF con el tamaño de página adecuado
            Document document = new Document(new Rectangle(widthInches * 72, heightInches * 72));

            // Escribir el documento en la memoria
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            try
            {
                // Crear una tabla principal con dos columnas
                PdfPTable mainTable = new PdfPTable(2);
                mainTable.WidthPercentage = 100;
                float[] mainWidths = new float[] { 10f, 30f }; // Ajusta el ancho de las columnas
                mainTable.SetWidths(mainWidths);

                // Columna 1: Texto del activo
                PdfPCell textCell = new PdfPCell();
                textCell.Border = PdfPCell.NO_BORDER;
                textCell.VerticalAlignment = Element.ALIGN_TOP;
                textCell.PaddingLeft = -20;
                textCell.PaddingTop = -20; // Ajustar el espacio superior del texto

                // Crear celdas de texto con valores condicionales
                string categoria = !string.IsNullOrWhiteSpace(item.PLU_CAT_CategoriaActivo?.NombreCategoria)
                    ? item.PLU_CAT_CategoriaActivo.NombreCategoria
                    : "SIN CAT";

                string descripcion = !string.IsNullOrWhiteSpace(item.Descripcion)
                    ? item.Descripcion
                    : "SIN DESC";

                string numeroSerie = !string.IsNullOrWhiteSpace(item.NumeroSerie)
                    ? item.NumeroSerie
                    : "SIN N/S";

                string marca = !string.IsNullOrWhiteSpace(item.PLU_CAT_MarcaActivo?.NombreMarca)
                    ? item.PLU_CAT_MarcaActivo.NombreMarca
                    : "SIN MARCA";

                string numeroInventario = !string.IsNullOrWhiteSpace(item.NumeroInventario)
                    ? item.NumeroInventario
                    : "SIN N/I";

                // Agregar el contenido del texto a la celda
                textCell.AddElement(new Paragraph(categoria, new Font(Font.FontFamily.HELVETICA, 5, Font.NORMAL)));
                textCell.AddElement(new Paragraph(" ", new Font(Font.FontFamily.HELVETICA, 2, Font.NORMAL)));
                textCell.AddElement(new Paragraph(descripcion, new Font(Font.FontFamily.HELVETICA, 5, Font.NORMAL)));
                textCell.AddElement(new Paragraph(" ", new Font(Font.FontFamily.HELVETICA, 2, Font.NORMAL)));
                textCell.AddElement(new Paragraph(numeroSerie, new Font(Font.FontFamily.HELVETICA, 5, Font.NORMAL)));
                textCell.AddElement(new Paragraph(" ", new Font(Font.FontFamily.HELVETICA, 2, Font.NORMAL)));
                textCell.AddElement(new Paragraph(marca, new Font(Font.FontFamily.HELVETICA, 5, Font.NORMAL)));
                textCell.AddElement(new Paragraph(" ", new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL)));
                textCell.AddElement(new Paragraph(numeroInventario, new Font(Font.FontFamily.HELVETICA, 7, Font.BOLD)));

                // Agregar la celda de texto a la tabla principal
                mainTable.AddCell(textCell);

                // Columna 2: Imágenes (logo y código QR)
                PdfPTable imageTable = new PdfPTable(1); // Una tabla interna para las imágenes
                imageTable.WidthPercentage = 100;
                imageTable.PaddingTop = -30;

                // Celda para el logo
                PdfPCell logoCell = new PdfPCell();
                logoCell.Border = PdfPCell.NO_BORDER;
                logoCell.VerticalAlignment = Element.ALIGN_TOP;
                logoCell.PaddingTop = -25; // Ajustar el espacio superior del logo
                logoCell.PaddingRight = 120;

                // Cargar la imagen del logo
                string logoPath = Server.MapPath("~/Content/assets/images/logo-sm-fgje-negro.png"); // Ruta correcta a tu logotipo
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(30, 30); // Ajustar el tamaño del logotipo

                // Alinear el logo a la derecha dentro de la celda
                logo.Alignment = Element.ALIGN_RIGHT;

                // Agregar el logotipo a la celda del logo
                logoCell.AddElement(logo);

                // Agregar la celda del logo a la tabla interna de imágenes
                imageTable.AddCell(logoCell);

                // Celda para el código QR
                PdfPCell qrCell = new PdfPCell();
                qrCell.Border = PdfPCell.NO_BORDER;
                qrCell.VerticalAlignment = Element.ALIGN_TOP;
                qrCell.PaddingTop = 10; // Ajustar el espacio superior del código QR
                qrCell.PaddingRight = 120;


                // Generar el código QR usando la clase QrCodeHelper
                string qrText = item.NumeroInventario;
                Image qrCodeImage = QrCodeHelper.GenerateQRCodeImage(qrText);

                // Cargar la imagen del código QR
                Image qr = Image.GetInstance(qrCodeImage);
                qr.ScaleToFit(30, 30); // Ajustar el tamaño del código QR

                // Alinear el código QR a la derecha dentro de la celda
                qr.Alignment = Element.ALIGN_RIGHT;

                // Agregar el código QR a la celda del código QR
                qrCell.AddElement(qr);

                // Agregar la celda del código QR a la tabla interna de imágenes
                imageTable.AddCell(qrCell);

                // Agregar la tabla interna de imágenes a la columna 2 de la tabla principal
                PdfPCell imageCell = new PdfPCell(imageTable);
                imageCell.Border = PdfPCell.NO_BORDER;
                mainTable.AddCell(imageCell);

                // Agregar la tabla principal al documento
                document.Add(mainTable);
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir durante la generación del PDF
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }
            finally
            {
                // Cerrar el documento
                if (document.IsOpen())
                {
                    document.Close();
                }
            }

            // Preparar el PDF para abrir en una nueva pestaña
            var content = stream.ToArray();
            return new FileStreamResult(new MemoryStream(content), "application/pdf");
        }

        public ActionResult CargaMasiva()
        {

            return View();
        }

        [HttpPost]
        public ActionResult CargarArchivo(HttpPostedFileBase file)
        {
            if (file == null || (file.ContentType != "application/vnd.ms-excel" &&
                                 file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" &&
                                 file.ContentType != "text/csv"))
            {
                ModelState.AddModelError("file", "Por favor, suba un archivo válido de tipo Excel (.xls, .xlsx) o CSV (.csv).");
                return View();
            }

            List<PLU_OP_Activos> activos = ProcesarArchivoExcel(file);

           

            // Devolver una vista con los activos procesados para su revisión antes de guardarlos en la base de datos
            return PartialView("_VistaPreviaActivos", activos);
        }

        [HttpPost]
        public JsonResult GuardarActivosMasivos()
        {
            var rm = new ResponseModel();

            try
            {
                // Leer y descomprimir el cuerpo de la solicitud
                using (var stream = new MemoryStream())
                {
                    Request.InputStream.CopyTo(stream);
                    var compressedData = stream.ToArray();

                    // Descomprimir los datos
                    string jsonString;
                    using (var decompressionStream = new GZipStream(new MemoryStream(compressedData), CompressionMode.Decompress))
                    using (var reader = new StreamReader(decompressionStream, Encoding.UTF8))
                    {
                        jsonString = reader.ReadToEnd();
                    }

                    // Deserializar el JSON en la lista de activos
                    var activos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PLU_OP_Activos>>(jsonString);

                    if (activos == null || !activos.Any())
                    {
                        rm.SetResponse(false, "No se recibieron activos o el JSON estaba vacío.");
                        return Json(rm);
                    }

                    // Procesar los activos (tu lógica actual)
                    ActivosHelper act = new ActivosHelper();
                    rm = act.AddMasivo(activos);

                    if (rm.response)
                    {
                        rm.message = "Activos agregados con éxito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Activos");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar activos.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, $"Error procesando la solicitud: {ex.Message}");
            }

            return Json(rm);
        }

        private List<PLU_OP_Activos> ProcesarArchivoExcel(HttpPostedFileBase file)
        {
            List<PLU_OP_Activos> activos = new List<PLU_OP_Activos>();

            // Verificar si el archivo no es nulo y tiene una extensión válida
            if (file != null && (Path.GetExtension(file.FileName).ToLower() == ".xls" || Path.GetExtension(file.FileName).ToLower() == ".xlsx" || Path.GetExtension(file.FileName).ToLower() == ".csv"))
            {
                // Leer y procesar el archivo Excel utilizando EPPlus
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.First();

                    // Obtener el número total de filas y columnas en la hoja de cálculo
                    int rowCount = worksheet.Dimension.Rows;
                    int columnCount = worksheet.Dimension.Columns;

                    // Obtener el año actual en formato de dos dígitos
                    string añoActual = DateTime.Now.Year.ToString().Substring(2, 2);

                    // Obtener el último número de inventario de la base de datos
                    string ultimoNumeroInventario = _db.PLU_OP_Activos
                                                       .OrderByDescending(a => a.IdActivos)
                                                       .Select(a => a.NumeroInventario)
                                                       .FirstOrDefault();

                    // Extraer el prefijo y la parte numérica del último número de inventario
                    string prefijoBase = "FGJE-IF"; // Parte fija del prefijo
                    string prefijo = $"{prefijoBase}{añoActual}-"; // Prefijo dinámico con el año actual
                    int ultimoConsecutivo = 0;

                    if (!string.IsNullOrEmpty(ultimoNumeroInventario))
                    {
                        int index = ultimoNumeroInventario.LastIndexOf('-');
                        if (index != -1 && int.TryParse(ultimoNumeroInventario.Substring(index + 1), out int parsedNumber))
                        {
                            ultimoConsecutivo = parsedNumber;
                        }
                    }

                    // Iterar sobre las filas del archivo Excel
                    for (int row = 2; row <= rowCount; row++) // Se asume que la primera fila contiene encabezados y se empieza desde la segunda fila
                    {
                        // Crear una nueva instancia de Activo
                        PLU_OP_Activos activo = new PLU_OP_Activos();

                        // Incrementar el consecutivo
                        ultimoConsecutivo++;
                        activo.NumeroInventario = $"{prefijo}{ultimoConsecutivo:D4}";

                        // Iterar sobre las columnas del archivo Excel
                        for (int col = 1; col <= columnCount; col++)
                        {
                            // Obtener el valor de la celda en la fila y columna actual
                            string cellValue = worksheet.Cells[row, col].Value?.ToString();

                            // Asignar el valor al campo correspondiente del activo
                            switch (col)
                            {
                                case 1:
                                    activo.IdCategoria = !string.IsNullOrEmpty(cellValue) ? Convert.ToInt32(cellValue) : (int?)null;
                                    break;
                                case 2:
                                    activo.IdMarca = !string.IsNullOrEmpty(cellValue) ? Convert.ToInt32(cellValue) : (int?)null;
                                    break;
                                case 3:
                                    activo.IdProveedor = !string.IsNullOrEmpty(cellValue) ? Convert.ToInt32(cellValue) : (int?)null;
                                    break;
                                case 4:
                                    activo.IdFactura = !string.IsNullOrEmpty(cellValue) ? Convert.ToInt32(cellValue) : (int?)null;
                                    break;
                                case 5:
                                    activo.IdConcepto = !string.IsNullOrEmpty(cellValue) ? Convert.ToInt32(cellValue) : (int?)null;
                                    break;
                                case 6:
                                    activo.IdClasificadores = !string.IsNullOrEmpty(cellValue) ? Convert.ToInt32(cellValue) : (int?)null;
                                    break;
                                case 7:
                                    activo.IdRecurso = !string.IsNullOrEmpty(cellValue) ? Convert.ToInt32(cellValue) : (int?)null;
                                    break;
                                case 8:
                                    activo.IdEstadoFisico = !string.IsNullOrEmpty(cellValue) ? Convert.ToInt32(cellValue) : (int?)null;
                                    break;
                                case 9:
                                    activo.IdEstatusActivo = !string.IsNullOrEmpty(cellValue) ? Convert.ToInt32(cellValue) : (int?)null;
                                    break;
                                case 10:
                                    // El campo NumeroInventario ya se ha asignado anteriormente
                                    break;
                                case 11:
                                    activo.Descripcion = cellValue;
                                    break;
                                case 12:
                                    activo.NumeroSerie = cellValue;
                                    break;
                                // Agregar asignación de otros campos aquí...
                                default:
                                    break;
                            }
                        }

                        // Agregar el activo a la lista
                        activos.Add(activo);
                    }
                }
            }
            else
            {
                // Manejar el caso cuando el archivo es nulo o no tiene una extensión válida
                throw new ArgumentException("El archivo es nulo o no tiene una extensión válida.");
            }

            return activos;
        }


        [HttpGet]
        public ActionResult Baja(int Id)
        {
            // Verificar si el activo con el ID proporcionado existe y no está dado de baja
            var activo1 = _db.PLU_OP_Activos.FirstOrDefault(x => x.IdActivos == Id);

            if (activo1.IdEstatusActivo == 1)
            {
                // Si el activo no es válido, redireccionar a una página de error o mostrar un mensaje
                return RedirectToAction("Index", "Activos");
            }

            var activo = _db.PLU_OP_Activos.Where(x => x.IdActivos == Id).Select(x => new DetalleActivosViewModel
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

            // Obtener información del otro modelo que deseas mostrar
            var BajaActivo = new PLU_OP_BajasActivos();

            // Crear el ViewModel personalizado y asignar los modelos correspondientes
            var viewModel = new DetalleBajaViewModel
            {
                DetalleActivo = activo,
                PLU_OP_BajasActivos = BajaActivo
            };

            return View("Baja", viewModel);
        }

        [HttpPost]
        public JsonResult Baja(PLU_OP_BajasActivos PLU_OP_BajasActivos, HttpPostedFileBase files)
        {
            var rm = new ResponseModel();
            ActivosHelper act = new ActivosHelper();
            PLU_OP_BajasActivos.Activo = true;
            PLU_OP_BajasActivos.FechaCreacion = DateTime.Now;
            PLU_OP_BajasActivos.PLU_OP_OficiosBajas.FechaCreacion = DateTime.Now;
            if (ModelState.IsValid)
            {
                
                    rm = act.Baja(PLU_OP_BajasActivos, files);

                    if (rm.response)
                    {

                        rm.message = "Baja agregado con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Activos");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar Baja.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                   }
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult Guardar(PLU_OP_Activos activo, IEnumerable<HttpPostedFileBase> files)
        {
            var rm = new ResponseModel();
            ActivosHelper act = new ActivosHelper();
            activo.Activo = true;
            activo.FechaCreacion = DateTime.Now;

            if (ModelState.IsValid)
            {
                if (activo.IdActivos == 0)
                {
                    activo.IdEstatusActivo = 3;

                    rm = act.Add(activo, files);

                    if (rm.response)
                    {

                        rm.message = "Activo agregado con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Activos");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar Activo.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
                else
                {
                    rm = act.Edit(activo, files);

                    if (rm.response)
                    {

                        rm.message = "Activo se edito con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Activos");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar Activo.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }



            }

            return Json(rm);
        }

        [HttpPost]
        public ActionResult EliminarFoto(int IdFoto)
        {
            var rm = new ResponseModel();
            ActivosHelper act = new ActivosHelper();
            var foto = _db.PLU_OP_FotosActivos.Where(x => x.IdFoto == IdFoto).FirstOrDefault();
            
            if (foto == null)
            {
                rm.message = "No se encontró el foto especificada.";
                rm.error = true;
                return Json(rm);
            }

            try
            {

                rm = act.DeleteFoto(IdFoto);

                if (rm.response)
                {
                    rm.message = "Foto dado de baja con éxito.";
                    rm.function = "HideLoading();";
                    rm.error = false;
                }
                else
                {
                    rm.message = "Error al dar de baja foto.";
                    rm.function = "HideLoading();";
                    rm.error = true;
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que ocurra durante el proceso
                rm.message = "Ocurrió un error al intentar eliminar la foto.\n Detalles: " + ex.Message;
                rm.error = true;
            }

            return Json(rm);
        }

        [HttpGet]
        public JsonResult GetClasificadores(int IdConcepto)
        {
            var clasificadores = _db.PLU_CAT_Clasificadores
                                   .Where(c => c.IdConcepto == IdConcepto)
                                   .Select(x => new { x.IdClasificadores, x.ClasificadorDescripcion })
                                   .OrderBy(x => x.ClasificadorDescripcion)
                                   .ToList();
            return Json(clasificadores, JsonRequestBehavior.AllowGet);
        }

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
            ViewBag.Facturas = new SelectList(facturas, "IdFactura", "FolioFactura");

            var proveedor = _db.PLU_CAT_Proveedores.Where(x => x.Activo == true).OrderBy(x => x.RazonSocial).ToList();
            ViewBag.Proveedores = new SelectList(proveedor, "IdProveedor", "RazonSocial");

            var Almacenes = _db.PLU_CAT_Almacenes.Where(x => x.Activo == true).OrderBy(x => x.NombreAlmacen).ToList();
            ViewBag.Almacenes = new SelectList(Almacenes, "IdAlmacen", "NombreAlmacen");

            var estadofisico = _db.PLU_CAT_EstadoFisicoActivo.Where(x => x.Activo == true).OrderBy(x => x.Descripcion).ToList();
            ViewBag.EstadoFisico = new SelectList(estadofisico, "IdEstadoFisico", "Descripcion");

            var Recursos = _db.PLU_CAT_Recurso.Where(x => x.Activo == true).OrderBy(x => x.NombreRecurso).ToList();
            ViewBag.Recursos = new SelectList(Recursos, "IdRecurso", "NombreRecurso");

            var estatusActivo = _db.PLU_CAT_EstatusActivo.Where(x => x.Activo == true && x.IdEstatusActivo != 2 && x.IdEstatusActivo != 4).OrderBy(x => x.Descripcion).ToList();
            ViewBag.EstatusActivo = new SelectList(estatusActivo, "IdEstatusActivo", "Descripcion");


        }


        [HttpGet]
        public JsonResult ObtenerNuevoNumeroInventario()
        {
            // Obtener el año actual en formato de dos dígitos
            string añoActual = DateTime.Now.Year.ToString().Substring(2, 2);

            // Construir el prefijo con el año actual
            string prefijo = $"FGJE-IF{añoActual}-";

            // Obtener el último número de inventario de la base de datos
            string ultimoNumeroInventario = _db.PLU_OP_Activos
                                                .OrderByDescending(a => a.IdActivos)
                                                .Select(a => a.NumeroInventario)
                                                .FirstOrDefault();

            // Extraer la parte numérica del último número de inventario
            int ultimoConsecutivo = 0;
            if (!string.IsNullOrEmpty(ultimoNumeroInventario))
            {
                int index = ultimoNumeroInventario.LastIndexOf('-');
                if (index != -1 && int.TryParse(ultimoNumeroInventario.Substring(index + 1), out int parsedNumber))
                {
                    ultimoConsecutivo = parsedNumber;
                }
            }

            // Incrementar el último consecutivo en uno
            int nuevoConsecutivo = ultimoConsecutivo + 1;

            // Construir el nuevo número de inventario
            string nuevoNumeroInventario = $"{prefijo}{nuevoConsecutivo}";

            // Devolver el nuevo número de inventario como JSON
            return Json(new { nuevoNumeroInventario }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult BackAlmacen(int id)
        {
            var rm = new ResponseModel();
            ActivosHelper act = new ActivosHelper();

            try
            {
                // Buscar el activo por ID
                var activo = _db.PLU_OP_Activos.Find(id);

                if (activo != null)
                {
                    // Actualizar los campos deseados
                    activo.IdEstadoFisico = 2; // Asume que 1 es el estado "Almacén"
                    activo.IdEstatusActivo = 3; // Asume que 2 es el estatus "En Almacén"

                    _db.Entry(activo).State = EntityState.Modified;
                    _db.SaveChanges();

                    rm.response = true;
                    rm.message = "Activo regresado al almacén con éxito.";
                    rm.function = "HideLoading();";
                    rm.href = Url.Action("Index", "Activos");
                    rm.error = false;
                }
                else
                {
                    rm.response = false;
                    rm.message = "Activo no encontrado.";
                    rm.function = "HideLoading();";
                    rm.error = true;
                }
            }
            catch (Exception ex)
            {
                rm.response = false;
                rm.message = $"Error al regresar activo al almacén: {ex.Message}";
                rm.function = "HideLoading();";
                rm.error = true;
            }

            return Json(rm, JsonRequestBehavior.AllowGet);
        }


        public ActionResult BajaDefinitiva()
        {
            return View();  
        }

        [HttpPost]
        public ActionResult BuscarInventario(string NumeroInventario, string NumeroSerie)
        {
            // Reemplazar las comillas simples por guiones en el número de inventario
            NumeroInventario = NumeroInventario?.Replace("'", "-");

            // Inicializar la consulta de activos
            IQueryable<PLU_OP_Activos> query = _db.PLU_OP_Activos.Where(a => a.IdEstatusActivo == 1 || a.IdEstatusActivo == 3 );

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
                NumeroInventario = a.NumeroInventario,
                Descripcion = a.Descripcion,
                NumeroSerie = a.NumeroSerie,
                Categoria = a.PLU_CAT_CategoriaActivo.NombreCategoria,
                Marca = a.PLU_CAT_MarcaActivo.NombreMarca

            }).ToList();

            return Json(activos);
        }


        [HttpPost]
        public JsonResult GuardarBajaDefinitiva(int numeroLote, string selectedActivos)
        {
            var rm = new ResponseModel();
            try
            {
                using (var db = new ModelContext())
                {
                    List<int> activosSeleccionados = JsonConvert.DeserializeObject<List<int>>(selectedActivos);
                    int userId = SessionHelper.GetUser(); // Obtener el ID del usuario actual
                    string folioBaja = GenerarFolioBaja(); // Generar un folio único

                    // Guardar en la tabla PLU_OP_BajasActivos
                    foreach (var idActivo in activosSeleccionados)
                    {
                        var activo = db.PLU_OP_Activos.FirstOrDefault(a => a.IdActivos == idActivo);
                        if (activo != null)
                        {
                            var baja = new PLU_OP_BajasActivos
                            {
                                FolioBaja = folioBaja,
                                IdActivos = idActivo,
                                IdOficioBaja = null,
                                NumeroLote = numeroLote,
                                IdUsuario = userId,
                                Activo = true,
                                FechaCreacion = DateTime.Now
                            };
                            db.PLU_OP_BajasActivos.Add(baja);
                        }
                    }

                    db.SaveChanges(); // Guardar cambios en la tabla de bajas

                    // Llamar al método externo para actualizar el estado de los activos
                    ActualizarEstadoActivo(activosSeleccionados);

                    rm.response = true;
                    rm.message = "Baja definitiva guardada correctamente.";
                    rm.function = $"HideLoading();";
                    rm.href = Url.Action("BajaDefinitiva", "Activos");
                    rm.error = false;
                }
            }
            catch (DbEntityValidationException ex)
            {
                rm = ManejarErrorValidacion(ex);
            }

            return Json(rm);
        }

        private void ActualizarEstadoActivo(List<int> activosSeleccionados)
        {
            using (var db = new ModelContext())
            {
                var activosParaActualizar = db.PLU_OP_Activos.Where(a => activosSeleccionados.Contains(a.IdActivos)).ToList();

                foreach (var activo in activosParaActualizar)
                {
                    activo.IdEstatusActivo = 4; // 4 = Baja Definitiva
                    activo.IdResguardo = null;
                    activo.NumeroResguardo = null;
                    activo.NumeroEmpleado = null;
                    activo.IdAlmacen = null;

                    db.Entry(activo).State = EntityState.Modified;
                }

                db.SaveChanges(); // Guardar cambios en la base de datos
            }
        }


        private ResponseModel ManejarErrorValidacion(DbEntityValidationException ex)
        {
            var errorMessages = ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage);
            return new ResponseModel
            {
                response = false,
                message = "Error al guardar el activo: " + string.Join("; ", errorMessages),
                function = "HideLoading();",
                href = Url.Action("BajaDefinitiva", "Activos"),
                error = true
            };
        }


        public string GenerarFolioBaja()
        {
            string year = DateTime.Now.Year.ToString().Substring(2); // '24' para el año 2024
            int nuevoConsecutivo = 1;

            var ultimoFolio = _db.PLU_OP_BajasActivos
                                  .Where(d => d.FolioBaja.StartsWith(year + "-"))
                                  .OrderByDescending(d => d.FolioBaja)
                                  .Select(d => d.FolioBaja)
                                  .FirstOrDefault();

            if (!string.IsNullOrEmpty(ultimoFolio))
            {
                var partes = ultimoFolio.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimoConsecutivo))
                {
                    nuevoConsecutivo = ultimoConsecutivo + 1;
                }
            }

            return $"{year}-{nuevoConsecutivo:D6}";
        }


        public ActionResult ListaBajas(string Orden = "",  int iPagina = 1,int iPerPage = 10, string FolioBaja = "", string NumeroLote = "",string NumeroInventario = "", DateTime? FechaDesde = null,  DateTime? FechaHasta = null)
        {
            ViewBag.Order = Orden;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;

            // Persistir filtros
            ViewBag.FolioBaja = FolioBaja;
            ViewBag.NumeroLote = NumeroLote;
            ViewBag.NumeroInventario = NumeroInventario;
            ViewBag.FechaDesde = FechaDesde;
            ViewBag.FechaHasta = FechaHasta;

            // Sort params para columnas del detalle
            ViewBag.FolioBajaSortParam = Orden == "FolioBaja" ? "FolioBaja_desc" : "FolioBaja";
            ViewBag.NumeroLoteSortParam = Orden == "NumeroLote" ? "NumeroLote_desc" : "NumeroLote";
            ViewBag.NumeroInventarioSortParam = Orden == "NumeroInventario" ? "NumeroInventario_desc" : "NumeroInventario";
            ViewBag.CategoriaSortParam = Orden == "Categoria" ? "Categoria_desc" : "Categoria";
            ViewBag.DescripcionSortParam = Orden == "Descripcion" ? "Descripcion_desc" : "Descripcion";
            ViewBag.NumeroSerieSortParam = Orden == "NumeroSerie" ? "NumeroSerie_desc" : "NumeroSerie";
            ViewBag.MarcaSortParam = Orden == "Marca" ? "Marca_desc" : "Marca";
            ViewBag.FechaBajaSortParam = Orden == "FechaBaja" ? "FechaBaja_desc" : "FechaBaja";

            // *** Importante: ahora usamos el método de DETALLE ***
            var vModel = ActivosB.GetAllBajasActivosDetalle(
                sOrder: Orden,
                iPagina: iPagina,
                iPerPage: iPerPage,
                folioBaja: FolioBaja,
                numeroLote: NumeroLote,
                numeroInventario: NumeroInventario,
                fechaDesde: FechaDesde,
                fechaHasta: FechaHasta
            );

            if (Request.IsAjaxRequest())
                return PartialView("_ListaBajasDefinitivas", vModel);

            return View(vModel);
        }

        [HttpGet]
        public ActionResult ExportarBajasExcel(string FolioBaja = "", string NumeroLote = "",string NumeroInventario = "",DateTime? FechaDesde = null,DateTime? FechaHasta = null)
        {
            // Trae todo el set filtrado (sin paginar)
            var data = ActivosB.GetAllBajasActivosDetalle(
                sOrder: "",            // puedes reutilizar el orden actual si quieres
                iPagina: 1,
                iPerPage: int.MaxValue,
                folioBaja: FolioBaja,
                numeroLote: NumeroLote,
                numeroInventario: NumeroInventario,
                fechaDesde: FechaDesde,
                fechaHasta: FechaHasta
            );

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("BajasActivos");

                // Encabezados
                string[] headers = {
                                        "Folio Baja","Número Lote","Número Inventario",
                                        "Categoría","Descripción","Número Serie",
                                        "Marca","Fecha Baja"
                                    };

                for (int c = 0; c < headers.Length; c++)
                {
                    ws.Cells[1, c + 1].Value = headers[c];
                    ws.Cells[1, c + 1].Style.Font.Bold = true;
                    ws.Cells[1, c + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[1, c + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Datos
                int row = 2;
                foreach (var item in data)
                {
                    ws.Cells[row, 1].Value = item.FolioBajas;
                    ws.Cells[row, 2].Value = item.NumeroLote;
                    ws.Cells[row, 3].Value = item.NumeroInventario;
                    ws.Cells[row, 4].Value = item.Categoria;
                    ws.Cells[row, 5].Value = item.Descripcion;
                    ws.Cells[row, 6].Value = item.NumeroSerie;
                    ws.Cells[row, 7].Value = item.Marca;

                    // Fecha (formatea como fecha)
                    ws.Cells[row, 8].Value = item.FechaBaja;
                    ws.Cells[row, 8].Style.Numberformat.Format = "yyyy-mm-dd";
                    row++;
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                var bytes = package.GetAsByteArray();
                return File(
                    fileContents: bytes,
                    contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileDownloadName: "BajasActivos.xlsx"
                );
            }
        }


        public ActionResult DetallesBajas(string id)
        {
            var activos = _db.PLU_OP_BajasActivos
                 .Where(x => x.FolioBaja == id)
                 .GroupBy(x => new
                 {
                     x.FolioBaja,
                     x.NumeroLote,
                     x.PLU_OP_Activos.NumeroInventario,
                     x.PLU_OP_Activos.PLU_CAT_CategoriaActivo.NombreCategoria,
                     x.PLU_OP_Activos.Descripcion,
                     x.PLU_OP_Activos.NumeroSerie,
                     x.PLU_OP_Activos.PLU_CAT_MarcaActivo.NombreMarca,
                     x.FechaCreacion
                 })
                 .Select(g => new DetalleBajasActivosViewModel
                 {
                     FolioBajas = g.Key.FolioBaja,
                     NumeroLote = g.Key.NumeroLote,
                     NumeroInventario = g.Key.NumeroInventario,
                     Categoria = g.Key.NombreCategoria,
                     Descripcion = g.Key.Descripcion,
                     NumeroSerie = g.Key.NumeroSerie,
                     Marca = g.Key.NombreMarca,
                     FechaBaja = g.Key.FechaCreacion
                 })
                .ToList();


            return View("DetallesBajas", activos);
        }


    }
}