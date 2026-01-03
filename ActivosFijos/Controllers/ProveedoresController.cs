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
    public class ProveedoresController : Controller
    {
        ModelContext _db = new ModelContext();
        ProveedorHelper ProveedorB = new ProveedorHelper();

        // GET: Proveedores
        public ActionResult Index(string sOrder = "", string NombreProveedor = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;

            ViewBag.IdProveedorSortParam = sOrder == "#" ? "#_desc" : "#";
            ViewBag.NombreProveedorSortParam = sOrder == "NombreProveedor" ? "NombreProveedor_desc" : "NombreProveedor";

            var vModel = ProveedorB.GetAll(sOrder, NombreProveedor, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaProveedores", vModel);
            }

            return View(vModel);
        }

        public ActionResult Agregar()
        {
            return View();
        }

        public ActionResult Editar(int id)
        {
            var datos = _db.PLU_CAT_Proveedores.Find(id);
            return View("Editar", datos);
        }

        [HttpPost]
        public JsonResult Guardar(PLU_CAT_Proveedores proveedores)
        {
            var rm = new ResponseModel();

            if (ModelState.IsValid)
            {
                if (proveedores.IdProveedor == 0)
                {
                    proveedores.FechaCreacion = DateTime.Now;

                    rm = ProveedorB.Add(proveedores);

                    if (rm.response)
                    {

                        rm.message = "Proveedor agregada con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Proveedores");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar proveedor.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
                else
                {
                    rm = ProveedorB.Edit(proveedores);

                    if (rm.response)
                    {

                        rm.message = "Proveedor se edito con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Proveedores");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar proveedor.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
            }

            return Json(rm);
        }
    }
}