using ActivosFijos.Helpers;
using ActivosFijos.Models;
using Helpers;
using System;
using System.Linq;
using System.Web.Mvc;
using static EDUES_ADMIN.Filters.AdminFilters;

namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class ConceptosController : Controller
    {
        ModelContext _db = new ModelContext();
        ConceptosHelper ConceptoB = new ConceptosHelper();

        // GET: Conceptos
        public ActionResult Index(string sOrder = "", string Conceptos = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.Conceptos = Conceptos;

            ViewBag.IdConceptoSortParam = sOrder == "#" ? "#_desc" : "#";
            ViewBag.NombreConceptoSortParam = sOrder == "NombreConcepto" ? "NombreConcepto_desc" : "NombreConcepto";

            var vModel = ConceptoB.GetAll(sOrder, Conceptos, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaConceptos", vModel);
            }

            return View(vModel);
        }

        public ActionResult Agregar()
        {
            return View();
        }

        public ActionResult Editar(int id)
        {
            var datos = _db.PLU_CAT_Conceptos.Find(id);
            return View("Editar", datos);
        }

        [HttpPost]
        public JsonResult Guardar(PLU_CAT_Conceptos conceptos)
        {
            var rm = new ResponseModel();

            if (ModelState.IsValid)
            {
                if (conceptos.IdConceptos == 0)
                {
                    conceptos.FechaCreacion = DateTime.Now;

                    rm = ConceptoB.Add(conceptos);

                    if (rm.response)
                    {

                        rm.message = "Concepto agregado con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Conceptos");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar concepto.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
                else
                {
                    rm = ConceptoB.Edit(conceptos);

                    if (rm.response)
                    {

                        rm.message = "Concepto se edito con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Conceptos");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar concepto.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
            }

            return Json(rm);
        }
    }
}
