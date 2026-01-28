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
    public class CategoriasController : Controller
    {
        ModelContext _db = new ModelContext();
        CategoriasHelper CategoriaB = new CategoriasHelper();

        // GET: Categorias
        public ActionResult Index(string sOrder = "", string Categoria = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.Categoria = Categoria;

            ViewBag.IdCategoriaSortParam = sOrder == "#" ? "#_desc" : "#";
            ViewBag.NombreCategoriaSortParam = sOrder == "NombreCategoria" ? "NombreCategoria_desc" : "NombreCategoria";

            var vModel = CategoriaB.GetAll(sOrder, Categoria, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaCategorias", vModel);
            }

            return View(vModel);
        }

        public ActionResult Agregar()
        {
            return View();
        }

        public ActionResult Editar(int id)
        {
            var datos = _db.PLU_CAT_CategoriaActivo.Find(id);
            return View("Editar", datos);
        }

        [HttpPost]
        public JsonResult Guardar(PLU_CAT_CategoriaActivo categoria)
        {
            var rm = new ResponseModel();

            if (ModelState.IsValid)
            {
                if (categoria.IdCategoria == 0)
                {
                    categoria.FechaCreacion = DateTime.Now;

                    rm = CategoriaB.Add(categoria);

                    if (rm.response)
                    {

                        rm.message = "Categoria agregado con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Categorias");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar categoria.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
                else
                {
                    rm = CategoriaB.Edit(categoria);

                    if (rm.response)
                    {

                        rm.message = "Categoria se edito con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Categorias");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar categoria.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
            }

            return Json(rm);
        }

    }
}
