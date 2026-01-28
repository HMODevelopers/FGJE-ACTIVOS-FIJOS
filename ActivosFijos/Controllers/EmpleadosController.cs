using ActivosFijos.Helpers;
using Helpers;
using ActivosFijos.Services.Pdf;
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
                    var commonBuilder = new PdfCommonBuilder(_db, Server);
                    var empleadosBuilder = new EmpleadosPdfBuilder();

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
                        commonBuilder.RenderDatosEmpleado(document, (int)empleado.NumeroEmpleado);
                        commonBuilder.RenderLeyenda1(document);

                        // Obtener los activos para esta página
                        var activosParaPagina = inventarioActual.Skip(activosProcesados).Take(20).ToList();
                        empleadosBuilder.RenderTablaActivos(document, activosParaPagina); // Usa activosParaPagina en lugar de inventarioActual
                        commonBuilder.RenderLeyenda2(document, (int)empleado.NumeroEmpleado);
                        commonBuilder.RenderFirmaReviso(document);
                        commonBuilder.RenderFirmaRecibio(document, (int)empleado.NumeroEmpleado);
                        commonBuilder.RenderLeyendaHoja(document, activosProcesados / 20 + 1, totalPages);

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



    }
}
