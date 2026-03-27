using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ActivosFijos.Models;
using ActivosFijos.Models.ViewModels;
using OfficeOpenXml;

namespace ActivosFijos.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ModelContext _db = new ModelContext();
        private const string ExcelMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        // ============================================================
        // INDEX (opcional)
        // ============================================================
        public ActionResult Index()
        {
            return View();
        }

        // ============================================================
        // INVENTARIO FÍSICO 2025 (lo que ya tenías)
        // ============================================================
        private List<ReporteInventario2025ViewModel> ReporteInventarioFisico2025()
        {
            return _db.Database
                     .SqlQuery<ReporteInventario2025ViewModel>("EXEC OBTENER_REPORTE_INVENTARIO_FISICO_2025")
                     .ToList();
        }

        public ActionResult DescargarExcelReporteInventario2025()
        {
            var reporte = ReporteInventarioFisico2025();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Reporte Inventario Fisico 2025");
                worksheet.Cells.LoadFromCollection(reporte, true);
                worksheet.Cells["A:Z"].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"Reporte_Inventario_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(stream, ExcelMimeType, fileName);
            }
        }

        // ============================================================
        // ACTIVIDAD POR USUARIO (NUEVO)
        // ============================================================

        // Vista con filtro de fechas + tabla
        // GET /Reportes/ActividadUsuarios?de=YYYY-MM-DD&a=YYYY-MM-DD
        public ActionResult ActividadUsuarios(DateTime? de, DateTime? a)
        {
            // Default: últimos 7 días
            var desde = de ?? DateTime.Today.AddDays(-6);
            var hasta = a ?? DateTime.Today;

            var list = ObtenerActividadUsuarios(desde, hasta);

            ViewBag.Desde = desde.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta.ToString("yyyy-MM-dd");

            return View(list);
        }

        // Descarga Excel del mismo dataset
        // GET /Reportes/DescargarExcelActividadUsuarios?de=YYYY-MM-DD&a=YYYY-MM-DD
        public ActionResult DescargarExcelActividadUsuarios(DateTime? de, DateTime? a)
        {
            var desde = de ?? DateTime.Today.AddDays(-6);
            var hasta = a ?? DateTime.Today;

            var data = ObtenerActividadUsuarios(desde, hasta);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Actividad por Usuario");

                // Encabezados
                ws.Cells[1, 1].Value = "IDUSUARIO";
                ws.Cells[1, 2].Value = "NOMBRE COMPLETO";
                ws.Cells[1, 3].Value = "ALTAS - # ACTIVOS";
                ws.Cells[1, 4].Value = "ALTAS - # FOLIOS";
                ws.Cells[1, 5].Value = "CAMBIOS - # ACTIVOS";
                ws.Cells[1, 6].Value = "CAMBIOS - # FOLIOS";
                ws.Cells[1, 7].Value = "INVENTARIOS - # ACTIVOS";
                ws.Cells[1, 8].Value = "INVENTARIOS - # FOLIOS";
                ws.Cells[1, 9].Value = "BAJAS - # ACTIVOS";
                ws.Cells[1, 10].Value = "BAJAS - # FOLIOS";

                int row = 2;
                foreach (var r in data)
                {
                    ws.Cells[row, 1].Value = r.IdUsuario;
                    ws.Cells[row, 2].Value = r.NombreCompleto;
                    ws.Cells[row, 3].Value = r.ALTAS_NUMERO_ACTIVOS;
                    ws.Cells[row, 4].Value = r.ALTAS_FOLIOS_ATENDIDOS;
                    ws.Cells[row, 5].Value = r.CAMBIOS_NUMERO_ACTIVOS;
                    ws.Cells[row, 6].Value = r.CAMBIOS_FOLIOS_ATENDIDOS;
                    ws.Cells[row, 7].Value = r.INVENTARIOS_NUMERO_ACTIVOS;
                    ws.Cells[row, 8].Value = r.INVENTARIOS_FOLIOS_ATENDIDOS;
                    ws.Cells[row, 9].Value = r.BAJAS_NUMERO_ACTIVOS;
                    ws.Cells[row, 10].Value = r.BAJAS_FOLIOS_ATENDIDOS;
                    row++;
                }

                // Estética básica
                using (var rng = ws.Cells[1, 1, 1, 10])
                {
                    rng.Style.Font.Bold = true;
                }
                ws.Cells.AutoFitColumns();

                // Opcional: filtros
                ws.Cells[1, 1, row - 1, 10].AutoFilter = true;

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"ActividadUsuarios_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx";
                return File(stream, ExcelMimeType, fileName);
            }
        }

        // Helper que ejecuta el SP con parámetros
        private List<ActividadUsuariosViewModel> ObtenerActividadUsuarios(DateTime desde, DateTime hasta)
        {
            var p1 = new SqlParameter("@FECHADE", desde.Date);
            var p2 = new SqlParameter("@FECHAA", hasta.Date);

            const string sql = "EXEC DBO.RPT_CONTEOACTIVIDAD_TODOS @FECHADE, @FECHAA";
            return _db.Database.SqlQuery<ActividadUsuariosViewModel>(sql, p1, p2).ToList();
        }

        // Descarga directa de reporte general de activos.
        // GET /Reportes/DescargarExcelReporteGeneralActivos
        [HttpGet]
        public ActionResult DescargarExcelReporteGeneralActivos()
        {
            try
            {
                var reporte = ObtenerReporteGeneralActivos();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add("Reporte General de Activos");

                    for (int i = 0; i < reporte.Encabezados.Count; i++)
                    {
                        ws.Cells[1, i + 1].Value = reporte.Encabezados[i];
                        ws.Cells[1, i + 1].Style.Font.Bold = true;
                    }

                    for (int row = 0; row < reporte.Filas.Count; row++)
                    {
                        var valores = reporte.Filas[row];
                        for (int col = 0; col < valores.Length; col++)
                        {
                            ws.Cells[row + 2, col + 1].Value = valores[col];
                        }
                    }

                    ws.Cells.AutoFitColumns();
                    if (reporte.Encabezados.Count > 0)
                    {
                        ws.Cells[1, 1, Math.Max(1, reporte.Filas.Count + 1), reporte.Encabezados.Count].AutoFilter = true;
                    }

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"Reporte_General_Activos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(stream, ExcelMimeType, fileName);
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "No fue posible generar el Reporte General de Activos.";
                return RedirectToAction("Index", "Home");
            }
        }

        private ReporteTabularExcelViewModel ObtenerReporteGeneralActivos()
        {
            var salida = new ReporteTabularExcelViewModel();
            var conexion = _db.Database.Connection;
            var abrirConexion = conexion.State != ConnectionState.Open;

            try
            {
                if (abrirConexion)
                {
                    conexion.Open();
                }

                using (var cmd = conexion.CreateCommand())
                {
                    cmd.CommandText = "dbo.sp_ReporteGeneralActivos";
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var reader = cmd.ExecuteReader())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            salida.Encabezados.Add(reader.GetName(i));
                        }

                        while (reader.Read())
                        {
                            var fila = new object[reader.FieldCount];
                            reader.GetValues(fila);
                            salida.Filas.Add(fila);
                        }
                    }
                }
            }
            finally
            {
                if (abrirConexion && conexion.State == ConnectionState.Open)
                {
                    conexion.Close();
                }
            }

            return salida;
        }

        private sealed class ReporteTabularExcelViewModel
        {
            public List<string> Encabezados { get; } = new List<string>();
            public List<object[]> Filas { get; } = new List<object[]>();
        }
    }
}
