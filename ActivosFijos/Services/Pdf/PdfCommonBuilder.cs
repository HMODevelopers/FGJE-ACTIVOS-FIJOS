using System;
using System.Linq;
using System.Web;
using ActivosFijos.Models;
using Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ActivosFijos.Services.Pdf
{
    public class PdfCommonBuilder
    {
        private readonly ModelContext _db;
        private readonly HttpServerUtilityBase _server;

        public PdfCommonBuilder(ModelContext db, HttpServerUtilityBase server)
        {
            _db = db;
            _server = server;
        }

        public void RenderEncabezado(Document document)
        {
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

        public void RenderDatosEmpleado(Document document, int numeroEmpleado)
        {
            Font fontNormal = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            var puesto = string.Empty;
            var unidadResponsable = string.Empty;
            var municipio = string.Empty;

            var empleado = _db.PLU_OP_Empleados.FirstOrDefault(x => x.NumeroEmpleado == numeroEmpleado);
            var resguardo = _db.PLU_OP_Resguardo.FirstOrDefault(x => x.IdEmpleado == empleado.IdEmpleado);

            var adscripcion = empleado.PLU_OP_Adscripcion.FirstOrDefault(x => x.IdEmpleado == empleado.IdEmpleado);

            puesto = string.IsNullOrEmpty(adscripcion?.PuestoFuncional) ? " " : adscripcion.PuestoFuncional;
            unidadResponsable = string.IsNullOrEmpty(adscripcion?.Area) ? adscripcion.Corporacion : adscripcion.Area;
            municipio = string.IsNullOrEmpty(adscripcion?.PLU_CAT_Municipios?.NombreMunicipio) ? " " : adscripcion.PLU_CAT_Municipios.NombreMunicipio;

            PdfPTable datosTable = new PdfPTable(2)
            {
                WidthPercentage = 100,
                HorizontalAlignment = Element.ALIGN_LEFT,
                SpacingAfter = 0
            };

            float[] columnWidths = { 1f, 2f };
            datosTable.SetWidths(columnWidths);

            datosTable.AddCell(new PdfPCell(new Phrase("Numero Resguardo:", fontNormal)) { Border = PdfPCell.NO_BORDER });
            if (resguardo == null)
            {
                datosTable.AddCell(new PdfPCell(new Phrase("En Almacen", FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });
            }
            else
            {
                datosTable.AddCell(new PdfPCell(new Phrase(resguardo.NumeroResguardo.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });
            }

            datosTable.AddCell(new PdfPCell(new Phrase("Clave de Empleado:", fontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase(empleado.NumeroEmpleado.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            datosTable.AddCell(new PdfPCell(new Phrase("Responsable:", fontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase(empleado.ApellidoP + " " + empleado.ApellidoM + " " + empleado.Nombres, FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            datosTable.AddCell(new PdfPCell(new Phrase("Puesto:", fontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase(puesto.ToUpper(), FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            datosTable.AddCell(new PdfPCell(new Phrase("Unidad Responsable:", fontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase(unidadResponsable.ToUpper(), FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            datosTable.AddCell(new PdfPCell(new Phrase("Adscripción:", fontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase("FISCALIA GENERAL DE JUSTICIA DEL ESTADO", FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            datosTable.AddCell(new PdfPCell(new Phrase("Municipio:", fontNormal)) { Border = PdfPCell.NO_BORDER });
            datosTable.AddCell(new PdfPCell(new Phrase(municipio.ToUpper(), FontFactory.GetFont(FontFactory.HELVETICA, 8))) { Border = PdfPCell.NO_BORDER });

            PdfPCell encapsulatedTableCell = new PdfPCell(datosTable)
            {
                Border = PdfPCell.BOX,
                Padding = 5
            };

            PdfPTable layoutTable = new PdfPTable(2)
            {
                WidthPercentage = 80,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            layoutTable.AddCell(encapsulatedTableCell);
            document.Add(layoutTable);

            string logoPath = _server.MapPath("~/Content/assets/images/logo-sm-fgje.png");
            Image logo = Image.GetInstance(logoPath);
            logo.ScaleToFit(120f, 120f);
            PdfPCell logoCell = new PdfPCell(logo)
            {
                Border = PdfPCell.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_TOP,
                PaddingTop = -5f,
                PaddingRight = -150f
            };
            layoutTable.AddCell(logoCell);

            document.Add(layoutTable);

            document.Add(new Paragraph("\n"));
        }

        public void RenderLeyenda1(Document document)
        {
            Paragraph infoText = new Paragraph("ESTOS BIENES SON PROPIEDAD DEL ESTADO, POR LO CUAL ES RESPONSABILIDAD DEL TITULAR DE ESTE RESGUARDO EL CUIDADO DE DICHO EQUIPO, EN CASO DE EXTRAVIO EL IMPORTE DEL BIEN SERA CUBIERTO POR LA PERSONA QUE FIRMA EL RESGUARDO.", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6))
            {
                SpacingBefore = 20,
                SpacingAfter = 20
            };

            PdfPCell cellWithPadding = new PdfPCell(infoText)
            {
                Padding = 5,
                Border = PdfPCell.NO_BORDER
            };

            PdfPTable table = new PdfPTable(1)
            {
                WidthPercentage = 100
            };
            table.AddCell(cellWithPadding);

            document.Add(table);
        }

        public void RenderLeyenda2(Document document, int numeroEmpleado)
        {
            var empleado = _db.PLU_OP_Empleados.FirstOrDefault(x => x.NumeroEmpleado == numeroEmpleado);

            Paragraph leyendaFinal = new Paragraph($"Yo {empleado.NombreCompleto} me hago responsable de los bienes contenidos en este resguardo, que son propiedad de la Fiscalía General de Justicia del Estado. Debiendo informar a la UNIDAD DE INVENTARIOS el cambio de ubicación, pérdida o cualquier movimiento, en el entendido de que al no hacerlo será acreedor a una acta administrativa.\nEste documento será utilizado por el personal del Departamento de Inventarios de esta Fiscalía al realizar entrega o bajas de mobiliario y/o equipo tecnológico, así como levantamiento físico en su área de trabajo y/o áreas a su cargo.\nFundamentado en el artículo 12 fracción XVI de la Ley Orgánica No. 180 de la Fiscalía General de Justicia del Estado de Sonora.", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6))
            {
                SpacingBefore = 20,
                SpacingAfter = 20,
                Alignment = Element.ALIGN_CENTER
            };

            PdfPCell cellWithPadding1 = new PdfPCell(leyendaFinal)
            {
                Padding = 5,
                Border = PdfPCell.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER
            };

            leyendaFinal.Alignment = Element.ALIGN_JUSTIFIED;

            PdfPTable table1 = new PdfPTable(1)
            {
                WidthPercentage = 90,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            table1.AddCell(cellWithPadding1);

            document.Add(table1);
        }

        public void RenderFirmaReviso(Document document)
        {
            var idUsuario = SessionHelper.GetUser();

            var usuario = _db.PLU_CONF_Usuario.FirstOrDefault(x => x.IdUsuario == idUsuario);

            Paragraph reviso = new Paragraph("Reviso:", FontFactory.GetFont(FontFactory.HELVETICA, 7))
            {
                Alignment = Element.ALIGN_CENTER
            };

            PdfPCell revisoCell = new PdfPCell(reviso)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = 40f,
                PaddingBottom = 10f,
                PaddingLeft = 120f,
                PaddingRight = 15f
            };

            PdfPTable revisoTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            revisoTable.AddCell(revisoCell);
            document.Add(revisoTable);

            Paragraph lineaGuiones = new Paragraph("_______________________________________", FontFactory.GetFont(FontFactory.HELVETICA, 8))
            {
                Alignment = Element.ALIGN_CENTER
            };

            PdfPCell lineaGuionesCell = new PdfPCell(lineaGuiones)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = 5f,
                PaddingBottom = 5f,
                PaddingLeft = 45f,
                PaddingRight = 15f
            };

            PdfPTable lineasTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            lineasTable.AddCell(lineaGuionesCell);
            document.Add(lineasTable);

            Paragraph nombreUsuario = new Paragraph(usuario.Apellidos + " " + usuario.Nombres, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8))
            {
                Alignment = Element.ALIGN_CENTER
            };

            PdfPCell nombreUsuarioCell = new PdfPCell(nombreUsuario)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = 5f,
                PaddingBottom = 5f,
                PaddingLeft = 75f,
                PaddingRight = 15f
            };

            PdfPTable nombreUsuarioTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            nombreUsuarioTable.AddCell(nombreUsuarioCell);
            document.Add(nombreUsuarioTable);

            Paragraph leyendaInventarios = new Paragraph("DEPARTAMENTO DE CONTROL DE INVENTARIOS", FontFactory.GetFont(FontFactory.HELVETICA, 7))
            {
                Alignment = Element.ALIGN_CENTER
            };

            PdfPCell leyendaInventariosCell = new PdfPCell(leyendaInventarios)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = 5f,
                PaddingBottom = 20f,
                PaddingLeft = 50f,
                PaddingRight = 15f
            };

            PdfPTable leyendaInventariosTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            leyendaInventariosTable.AddCell(leyendaInventariosCell);
            document.Add(leyendaInventariosTable);
        }

        public void RenderFirmaRecibio(Document document, int numeroEmpleado)
        {
            var empleado = _db.PLU_OP_Empleados.FirstOrDefault(x => x.NumeroEmpleado == numeroEmpleado);

            Paragraph recibio = new Paragraph("Recibio:", FontFactory.GetFont(FontFactory.HELVETICA, 7))
            {
                Alignment = Element.ALIGN_CENTER
            };

            PdfPCell recibioCell = new PdfPCell(recibio)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = -82f,
                PaddingBottom = 10f,
                PaddingLeft = 622f,
                PaddingRight = 15f
            };

            PdfPTable reciboTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            reciboTable.AddCell(recibioCell);
            document.Add(reciboTable);

            Paragraph lineaGuiones2 = new Paragraph("_______________________________________", FontFactory.GetFont(FontFactory.HELVETICA, 8))
            {
                Alignment = Element.ALIGN_CENTER
            };

            PdfPCell lineaGuionesCell2 = new PdfPCell(lineaGuiones2)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = -62f,
                PaddingBottom = 5f,
                PaddingLeft = 550f,
                PaddingRight = 15f
            };

            PdfPTable lineasTable2 = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            lineasTable2.AddCell(lineaGuionesCell2);
            document.Add(lineasTable2);

            Paragraph nombreEmpleado = new Paragraph(empleado.ApellidoP + " " + empleado.ApellidoM + " " + empleado.Nombres, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8))
            {
                Alignment = Element.ALIGN_CENTER
            };

            PdfPCell nombreEmpleadoCell = new PdfPCell(nombreEmpleado)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = -42f,
                PaddingBottom = 5f,
                PaddingLeft = 560f,
                PaddingRight = 15f
            };

            PdfPTable nombreEmpleadoTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            nombreEmpleadoTable.AddCell(nombreEmpleadoCell);
            document.Add(nombreEmpleadoTable);

            Paragraph leyendaInventarios = new Paragraph("FIRMA DEL RESPONSABLE", FontFactory.GetFont(FontFactory.HELVETICA, 7))
            {
                Alignment = Element.ALIGN_CENTER
            };

            PdfPCell puestoEmpleadoCell = new PdfPCell(leyendaInventarios)
            {
                Border = PdfPCell.NO_BORDER,
                PaddingTop = -25f,
                PaddingBottom = 20f,
                PaddingLeft = 590f,
                PaddingRight = 15f
            };

            PdfPTable puestoEmpleadoTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            puestoEmpleadoTable.AddCell(puestoEmpleadoCell);
            document.Add(puestoEmpleadoTable);
        }

        // El resguardo requiere que ambas firmas permanezcan en la misma hoja.
        // A diferencia de las rutinas anteriores, esta tabla se agrega como una
        // sola unidad, por lo que iTextSharp no puede separar sus renglones entre
        // dos páginas.
        public void RenderFirmasResguardo(Document document, int numeroEmpleado)
        {
            var idUsuario = SessionHelper.GetUser();
            var usuario = _db.PLU_CONF_Usuario.FirstOrDefault(x => x.IdUsuario == idUsuario);
            var empleado = _db.PLU_OP_Empleados.FirstOrDefault(x => x.NumeroEmpleado == numeroEmpleado);

            var firmasTable = new PdfPTable(2)
            {
                WidthPercentage = 100,
                KeepTogether = true,
                // Llevar el área de firmas hacia la parte baja del formato.
                SpacingBefore = 75f,
                SpacingAfter = 0f
            };
            firmasTable.SetWidths(new[] { 1f, 1f });
            firmasTable.AddCell(new PdfPCell(CrearTablaFirma(
                "Reviso:",
                (usuario.Apellidos + " " + usuario.Nombres).Trim(),
                "DEPARTAMENTO DE CONTROL DE INVENTARIOS"))
            {
                Border = PdfPCell.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingTop = 2f,
                PaddingBottom = 2f
            });
            firmasTable.AddCell(new PdfPCell(CrearTablaFirma(
                "Recibio:",
                (empleado.ApellidoP + " " + empleado.ApellidoM + " " + empleado.Nombres).Trim(),
                "FIRMA DEL RESPONSABLE"))
            {
                Border = PdfPCell.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingTop = 2f,
                PaddingBottom = 2f
            });

            document.Add(firmasTable);
        }

        private static PdfPTable CrearTablaFirma(string etiqueta, string nombre, string cargo)
        {
            var fontEtiqueta = FontFactory.GetFont(FontFactory.HELVETICA, 7);
            var fontNombre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            var firmaTable = new PdfPTable(1) { WidthPercentage = 100 };

            firmaTable.AddCell(CrearCeldaFirma(etiqueta, fontEtiqueta, 0f, 3f));
            // Espacio reservado para la firma manuscrita.
            firmaTable.AddCell(new PdfPCell(new Phrase(" ", fontEtiqueta))
            {
                Border = PdfPCell.NO_BORDER,
                FixedHeight = 24f
            });
            firmaTable.AddCell(CrearCeldaFirma("_______________________________________", fontEtiqueta, 0f, 5f));
            firmaTable.AddCell(CrearCeldaFirma(nombre, fontNombre, 0f, 3f));
            firmaTable.AddCell(CrearCeldaFirma(cargo, fontEtiqueta, 0f, 0f));

            return firmaTable;
        }

        private static PdfPCell CrearCeldaFirma(string texto, Font font, float paddingTop, float paddingBottom)
        {
            return new PdfPCell(new Phrase(texto, font))
            {
                Border = PdfPCell.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingTop = paddingTop,
                PaddingBottom = paddingBottom,
                NoWrap = true
            };
        }

        public void RenderLeyendaHoja(Document document, int numeroHoja, int totalHojas)
        {
            Font fontLeyenda = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.NORMAL, BaseColor.BLACK);
            Paragraph paragraph = new Paragraph($"Hoja {numeroHoja} de {totalHojas}", fontLeyenda);
            paragraph.Alignment = Element.ALIGN_RIGHT;
            paragraph.SpacingBefore = 5f;
            paragraph.IndentationLeft = 10f;
            paragraph.IndentationRight = 15f;
            document.Add(paragraph);
        }
    }
}
