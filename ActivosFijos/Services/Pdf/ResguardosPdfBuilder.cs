using System;
using System.Collections.Generic;
using System.Linq;
using ActivosFijos.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ActivosFijos.Services.Pdf
{
    public class ResguardosPdfBuilder
    {
        private readonly ModelContext _db;

        public ResguardosPdfBuilder(ModelContext db)
        {
            _db = db;
        }

        public void RenderTablaActivosAgregar(Document document, List<int> activos, DateTime fechaCreacion)
        {
            var dataActivos = _db.PLU_OP_Activos.Where(x => activos.Contains(x.IdActivos)).ToList();

            PdfPTable bienesTable = new PdfPTable(8);
            bienesTable.WidthPercentage = 100;

            float[] columnWidths1 = { .5f, .8f, 3f, .9f, .5f, 1f, 1f, 1f };
            bienesTable.SetWidths(columnWidths1);

            bienesTable.AddCell(new PdfPCell(new Phrase("FACTURA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("CATEGORIA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("DESCRIPCIÓN", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("SERIE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("MARCA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("CLAVE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("SALIDA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("FECHA SALIDA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });

            foreach (var activo in dataActivos)
            {
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_CAT_Facturas?.FolioFactura ?? "SIN FACTURA", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_CAT_CategoriaActivo?.NombreCategoria ?? "SIN CATEGORIA", 10), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.Descripcion ?? "SIN DESCRIPCION", 66), FontFactory.GetFont(FontFactory.HELVETICA, 5))));
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.NumeroSerie ?? "SIN N/S", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_CAT_MarcaActivo?.NombreMarca ?? "SIN MARCA", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.NumeroInventario ?? "SIN N/I", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase("ALTA RESGUARDO", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(fechaCreacion.ToString("dd/MM/yyyy"), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
            }

            AddEmptyRows(bienesTable, dataActivos.Count);
            document.Add(bienesTable);
        }

        public void RenderTablaActivosCambios(Document document, List<int> activos, DateTime fechaCreacion)
        {
            var dataActivos = _db.PLU_OP_Activos.Where(x => activos.Contains(x.IdActivos)).ToList();

            PdfPTable bienesTable = new PdfPTable(8);
            bienesTable.WidthPercentage = 100;

            float[] columnWidths1 = { .5f, .8f, 3f, .9f, .5f, 1f, 1f, 1f };
            bienesTable.SetWidths(columnWidths1);

            bienesTable.AddCell(new PdfPCell(new Phrase("FACTURA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("CATEGORIA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("DESCRIPCIÓN", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("SERIE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("MARCA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("CLAVE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("SALIDA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });
            bienesTable.AddCell(new PdfPCell(new Phrase("FECHA SALIDA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7))) { HorizontalAlignment = Element.ALIGN_CENTER });

            foreach (var activo in dataActivos)
            {
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.PLU_CAT_Facturas?.FolioFactura ?? "SIN FACTURA", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_CAT_CategoriaActivo?.NombreCategoria ?? "SIN CATEGORIA", 10), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.Descripcion ?? "SIN DESCRIPCION", 66), FontFactory.GetFont(FontFactory.HELVETICA, 5))));
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.NumeroSerie ?? "SIN N/S", 15), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(Truncate(activo.PLU_CAT_MarcaActivo?.NombreMarca ?? "SIN MARCA", 10), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(activo.NumeroInventario ?? "SIN N/I", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase("CAMBIO RESGUARDO", FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
                bienesTable.AddCell(new PdfPCell(new Phrase(fechaCreacion.ToString("dd/MM/yyyy"), FontFactory.GetFont(FontFactory.HELVETICA, 5))) { HorizontalAlignment = Element.ALIGN_CENTER });
            }

            AddEmptyRows(bienesTable, dataActivos.Count);
            document.Add(bienesTable);
        }

        private static void AddEmptyRows(PdfPTable bienesTable, int rowCount)
        {
            int remainingRows = 20 - rowCount;
            if (remainingRows <= 0)
            {
                return;
            }

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

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
