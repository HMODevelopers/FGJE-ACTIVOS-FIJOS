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
    public class FacturasController : Controller
    {
        ModelContext _db = new ModelContext();
        
        FacturaHelper FacturaB = new FacturaHelper();

        // GET: EstadoFisico
        public ActionResult Index(string sOrder = "", string FolioFactura = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;

            ViewBag.IdFacturaSortParam = sOrder == "#" ? "#_desc" : "#";
            ViewBag.FolioFacturaSortParam = sOrder == "FolioFactura" ? "FolioFactura_desc" : "FolioFactura";

            var vModel = FacturaB.GetAll(sOrder, FolioFactura, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaFacturas", vModel);
            }

            return View(vModel);
        }

        public ActionResult Agregar()
        {
            CargarCatalogos();
            return View();
        }

        public ActionResult Editar(int id)
        {
            CargarCatalogos();
            var datos = _db.PLU_CAT_Facturas.Find(id);
            return View("Editar", datos);
        }

        [HttpPost]
        public JsonResult Guardar(PLU_CAT_Facturas facturas)
        {
            var rm = new ResponseModel();

            if (ModelState.IsValid)
            {
                if (facturas.IdFactura == 0)
                {
                    facturas.FechaCreacion = DateTime.Now;

                    rm = FacturaB.Add(facturas);

                    if (rm.response)
                    {

                        rm.message = "Factura agregada con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Facturas");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar factura.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
                else
                {
                    rm = FacturaB.Edit(facturas);

                    if (rm.response)
                    {

                        rm.message = "factura se edito con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Facturas");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar factura.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
            }

            return Json(rm);
        }


        public void CargarCatalogos()
        {
            
            var proveedor = _db.PLU_CAT_Proveedores.Where(x => x.Activo == true).OrderBy(x => x.RazonSocial).ToList();
            ViewBag.Proveedores = new SelectList(proveedor, "IdProveedor", "RazonSocial");

            var Recursos = _db.PLU_CAT_Recurso.Where(x => x.Activo == true).OrderBy(x => x.NombreRecurso).ToList();
            ViewBag.Recursos = new SelectList(Recursos, "IdRecurso", "NombreRecurso");

        }
    }
}