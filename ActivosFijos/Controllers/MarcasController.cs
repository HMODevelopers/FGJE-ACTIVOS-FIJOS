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
    public class MarcasController : Controller
    {
        ModelContext _db = new ModelContext();
        MarcasHelper MarcaB = new MarcasHelper();

        // GET: Marcas
        public ActionResult Index(string sOrder = "", string NombreMarca = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.NombreMarca = NombreMarca;

            ViewBag.IdMarcaSortParam = sOrder == "#" ? "#_desc" : "#";
            ViewBag.NombreMarcaSortParam = sOrder == "NombreMarca" ? "NombreMarca_desc" : "NombreMarca";

            var vModel = MarcaB.GetAll(sOrder, NombreMarca, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaMarcas", vModel);
            }

            return View(vModel);
        }

        public ActionResult Agregar()
        {
            return View();
        }

        public ActionResult Editar(int id)
        {
            var datos = _db.PLU_CAT_MarcaActivo.Find(id);
            return View("Editar", datos);
        }

        [HttpPost]
        public JsonResult Guardar(PLU_CAT_MarcaActivo Marca)
        {
            var rm = new ResponseModel();

            if (ModelState.IsValid)
            {
                if (Marca.IdMarca == 0)
                {
                    Marca.FechaCreacion = DateTime.Now;

                    rm = MarcaB.Add(Marca);

                    if (rm.response)
                    {

                        rm.message = "Marca agregada con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Marcas");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar marca.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
                else
                {
                    rm = MarcaB.Edit(Marca);

                    if (rm.response)
                    {

                        rm.message = "Marca se edito con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Marcas");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar marca.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
            }

            return Json(rm);
        }
    }
}
