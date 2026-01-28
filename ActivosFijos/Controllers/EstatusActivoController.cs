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
    public class EstatusActivoController : Controller
    {
        ModelContext _db = new ModelContext();
        // GET: EstatusActivo
        EstatusActivoHelper EstatusActivoB = new EstatusActivoHelper();

        // GET: EstadoFisico
        public ActionResult Index(string sOrder = "", string EstatusActivo = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.EstatusActivo = EstatusActivo;

            ViewBag.IdEstatusActivoSortParam = sOrder == "#" ? "#_desc" : "#";
            ViewBag.NombreEstatusActivoSortParam = sOrder == "NombreEstatusA" ? "NombreEstatusA_desc" : "NombreEstatusA";

            var vModel = EstatusActivoB.GetAll(sOrder, EstatusActivo, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaEstatusActivo", vModel);
            }

            return View(vModel);
        }

        public ActionResult Agregar()
        {
            return View();
        }

        public ActionResult Editar(int id)
        {
            var datos = _db.PLU_CAT_EstatusActivo.Find(id);
            return View("Editar", datos);
        }

        [HttpPost]
        public JsonResult Guardar(PLU_CAT_EstatusActivo EstatusActivo)
        {
            var rm = new ResponseModel();

            if (ModelState.IsValid)
            {
                if (EstatusActivo.IdEstatusActivo == 0)
                {
                    EstatusActivo.FechaCreacion = DateTime.Now;

                    rm = EstatusActivoB.Add(EstatusActivo);

                    if (rm.response)
                    {

                        rm.message = "Estatus Activo agregado con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "EstatusActivo");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar estatus activo.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
                else
                {
                    rm = EstatusActivoB.Edit(EstatusActivo);

                    if (rm.response)
                    {

                        rm.message = "Estatus Activo se edito con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "EstatusActivo");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar estatus activo.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
            }

            return Json(rm);
        }
    }
}
