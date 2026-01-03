using ActivosFijos.Helpers;
using Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static EDUES_ADMIN.Filters.AdminFilters;
using ActivosFijos.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class EmpleadosController : Controller
    {
        EmpleadosHelper EmpleadosB = new EmpleadosHelper();
        ModelContext _db = new ModelContext();

        // GET: Empleados
        public ActionResult Index(string Orden = "",int CveEmpleado = 0, string Paterno = "", string Materno = "", string Nombre = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = Orden;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;

            ViewBag.CveEmpleadoSortParam = Orden == "CVE_EMPLEADO" ? "CVE_EMPLEADO_desc" : "CVE_EMPLEADO";
            ViewBag.ApePaternoSortParm = Orden == "APE_PATERNO" ? "APE_PATERNO_desc" : "APE_PATERNO";
            ViewBag.ApeMaternoSortParm = Orden == "APE_MATERNO" ? "APE_MATERNO_desc" : "APE_MATERNO";
            ViewBag.NombreSortParm = Orden == "NOMBRE" ? "NOMBRE_desc" : "NOMBRE";

            var vModel = EmpleadosB.GetAllEmpleados(Orden, CveEmpleado, Paterno, Materno, Nombre, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaEmpleados", vModel);
            }

            return View(vModel);
        }


        [HttpPost]
        public JsonResult EjecutarStoredProcedure()
        {
            var rm = new ResponseModel();

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ModelContext"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("OBT_EMPLEADOS_ALL", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                rm.response = true;
                rm.href = Url.Action("Index", "Empleados"); // O la acción que quieras recargar
            }
            catch (Exception ex)
            {
                rm.response = false;
                rm.message = ex.Message;
            }

            return Json(rm);
        }


        public ActionResult Adscripcion(int id)
        {
            var datos = _db.PLU_OP_Adscripcion.Where(x => x.IdEmpleado == id).OrderBy(x => x.FechaInicioAdscripcion).ToList();

            return View(datos);
        }

        public ActionResult Editar(int id)
        {
            var Municipio = _db.PLU_CAT_Municipios.Where(x => x.Activo == true).OrderBy(x => x.NombreMunicipio).ToList();
            ViewBag.Municipio = new SelectList(Municipio, "IdMunicipo", "NombreMunicipio");

            var unidadAdministrativa = _db.PLU_CAT_UnidadesAdministrativas.Where(x => x.Activo == true).OrderBy(x => x.UnidadAdministrativa).ToList();
            ViewBag.UnidadAdministrativa = new SelectList(unidadAdministrativa, "UnidadAdministrativa", "UnidadAdministrativa");



            var data = _db.PLU_OP_Adscripcion.Where(x => x.IdAdscripcion == id).FirstOrDefault();
            
            return View("Editar", data);
        }

        public JsonResult GetAreasAdscripcion(string Unidad)
        {

            var unidad = _db.PLU_CAT_UnidadesAdministrativas.Where(x => x.UnidadAdministrativa == Unidad).FirstOrDefault();

            // Aquí debes obtener las áreas relacionadas según el ID de la Corporación.
            var areas = _db.PLU_CAT_AreasUnidadesAdministrativas.Where(a => a.IdUnidadAdministrativa == unidad.IdUnidadAdministrativa)
                                .Select(a => new SelectListItem
                                {
                                    Value = a.NombreArea.ToString(),
                                    Text = a.NombreArea
                                }).ToList();

            return Json(areas, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Guardar(PLU_OP_Adscripcion adscripcion)
        {
            var rm = new ResponseModel();

            if (ModelState.IsValid)
            {
               
                    rm = EmpleadosB.Edit(adscripcion);

                    if (rm.response)
                    {

                        rm.message = "Adscripcion Editada con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Adscripcion", "Empleados" , new { id = adscripcion.IdEmpleado});
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar Adscripcion.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Adscripcion", "Empleados", new { id = adscripcion.IdEmpleado });
                        rm.error = true;
                    }
                
            }

            return Json(rm);
        }


        [HttpGet]
        public ActionResult ImprimirResguardo(int id)
        {
            int currentYear = DateTime.Now.Year;
            var inventarioActual = _db.PLU_OP_Activos.Where(i => i.PLU_OP_Resguardo.IdEmpleado == id).ToList();
            var empleado = _db.PLU_OP_Empleados.Where(x => x.IdEmpleado == id).Select(x => new { x.NumeroEmpleado }).FirstOrDefault();

            using (var memoryStream = new MemoryStream())
            {
                // Crear el documento PDF en orientación horizontal
                Document document = new Document(PageSize.A4.Rotate(), 30, 30, 15, 15);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                try
                {
                    Font FontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                    Font FontNormal = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

                    int activosProcesados = 0;
                    int totalPages = (int)Math.Ceiling((double)inventarioActual.Count / 20);

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
                        DatosEmpleado(document, (int)empleado.NumeroEmpleado);
                        Leyenda1(document);

                        // Obtener los activos para esta página
                        var activosParaPagina = inventarioActual.Skip(activosProcesados).Take(20).ToList();
                        TablaActivos(document, activosParaPagina); // Usa activosParaPagina en lugar de inventarioActual
                        Leyenda2(document, (int)empleado.NumeroEmpleado);
                        FirmaReviso(document);
                        FirmaRecibio(document, (int)empleado.NumeroEmpleado);
                        LeyendaHoja(document, activosProcesados / 20 + 1, totalPages);

                        // Actualizar el contador de activos procesados
                        activosProcesados += activosParaPagina.Count;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al generar el PDF: " + ex.Message);
                    throw;
                }
                finally
                {
                    // Asegúrate de que el documento esté cerrado
                    if (document.IsOpen())
                    {
                        document.Close();
                    }
                }

                var pdfBytes = memoryStream.ToArray();
                return new FileStreamResult(new MemoryStream(pdfBytes), "application/pdf");
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
            #region
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

            #endregion
        }

        public void TablaActivos(Document document, List<PLU_OP_Activos> activos)
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
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_CAT_Facturas?.FolioFactura ?? "SIN FACTURA", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_CAT_CategoriaActivo?.NombreCategoria ?? "SIN CATEGORIA", 20), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.Descripcion ?? "SIN DESCRIPCION", 66), FontFactory.GetFont(FontFactory.HELVETICA, 5))));
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.NumeroSerie ?? "SIN N/S", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_CAT_MarcaActivo?.NombreMarca ?? "SIN MARCA", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.NumeroInventario ?? "SIN N/I", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                if (activo.Activo == true)
                {
                    bienesTable.AddCell(new PdfPCell(new Phrase("Resguardo", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
                }
                else
                {
                    bienesTable.AddCell(new PdfPCell(new Phrase("Resguardo", FontFactory.GetFont(FontFactory.HELVETICA, 6))) { HorizontalAlignment = Element.ALIGN_CENTER });
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