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
    public class EstadoFisicoController : Controller
    {
        ModelContext _db = new ModelContext();
        EstadoFisicoHelper EstadoFisicoB = new EstadoFisicoHelper();

        // GET: EstadoFisico
        public ActionResult Index(string sOrder = "", string EstadoFisico = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;

            ViewBag.IdEstadoFSortParam = sOrder == "#" ? "#_desc" : "#";
            ViewBag.NombreEstadoFSortParam = sOrder == "NombreEstadoF" ? "NombreEstadoF_desc" : "NombreEstadoF";

            var vModel = EstadoFisicoB.GetAll(sOrder, EstadoFisico, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaEstadosFisico", vModel);
            }

            return View(vModel);
        }

        public ActionResult Agregar()
        {
            return View();
        }

        public ActionResult Editar(int id)
        {
            var datos = _db.PLU_CAT_EstadoFisicoActivo.Find(id);
            return View("Editar", datos);
        }

        [HttpPost]
        public JsonResult Guardar(PLU_CAT_EstadoFisicoActivo EstadoFisico)
        {
            var rm = new ResponseModel();

            if (ModelState.IsValid)
            {
                if (EstadoFisico.IdEstadoFisico == 0)
                {
                    EstadoFisico.FechaCreacion = DateTime.Now;

                    rm = EstadoFisicoB.Add(EstadoFisico);

                    if (rm.response)
                    {

                        rm.message = "Estado Fisico agregado con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "EstadoFisico");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar estado fisico.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
                else
                {
                    rm = EstadoFisicoB.Edit(EstadoFisico);

                    if (rm.response)
                    {

                        rm.message = "Estado Fisico se edito con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "EstadoFisico");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar estado fisico.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
            }

            return Json(rm);
        }
    }
}