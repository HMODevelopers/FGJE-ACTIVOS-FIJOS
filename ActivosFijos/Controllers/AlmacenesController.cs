using ActivosFijos.Helpers;
using ActivosFijos.Models;
using Helpers;
using System;
using System.Web.Mvc;
using static EDUES_ADMIN.Filters.AdminFilters;

namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class AlmacenesController : Controller
    {
        ModelContext _db = new ModelContext();
        AlmacenesHelper AlmacenB = new AlmacenesHelper();
        // GET: Almacenes

        public ActionResult Index(string sOrder = "", string Almacen = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.Almacen = Almacen;

            ViewBag.IdAlmacenSortParam = sOrder == "#" ? "#_desc" : "#";
            ViewBag.NombreAlmacenSortParam = sOrder == "NombreAlmacen" ? "NombreAlmacen_desc" : "NombreAlmacen";

            var vModel = AlmacenB.GetAll(sOrder, Almacen, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaAlmacenes", vModel);
            }

            return View(vModel);
        }

        public ActionResult Agregar()
        {
            return View();
        }

        public ActionResult Editar(int id)
        {
            var datos = _db.PLU_CAT_Almacenes.Find(id);
            return View("Editar",datos);
        }

        [HttpPost]
        public JsonResult Guardar(PLU_CAT_Almacenes almacen)
        {
            var rm = new ResponseModel();

            if (ModelState.IsValid)
            {
                if (almacen.IdAlmacen == 0)
                {

                   
                    rm = AlmacenB.Add(almacen);

                    if (rm.response)
                    {

                        rm.message = "Almacen agregado con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Almacenes");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar Almacen.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
                else
                {
                    rm = AlmacenB.Edit(almacen);

                    if (rm.response)
                    {

                        rm.message = "Almacen se Edito con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Almacenes");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar Almacen.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
            }

            return Json(rm);
        }
    }
}
