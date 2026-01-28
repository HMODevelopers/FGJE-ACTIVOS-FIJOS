using ActivosFijos.Helpers;
using ActivosFijos.Models;
using Helpers;
using PagedList;
using System;
using System.Data.Entity;
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
        public ActionResult Index(string sOrder = "", string FolioFactura = "", int? IdProveedor = null, int? IdRecurso = null, DateTime? FechaDesde = null, DateTime? FechaHasta = null, int iPagina = 1, int iPerPage = 10)
        {
            if (IdProveedor.HasValue && IdProveedor.Value <= 0)
            {
                IdProveedor = null;
            }

            if (IdRecurso.HasValue && IdRecurso.Value <= 0)
            {
                IdRecurso = null;
            }

            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.FolioFactura = FolioFactura;
            ViewBag.IdProveedor = IdProveedor;
            ViewBag.IdRecurso = IdRecurso;
            ViewBag.FechaDesde = FechaDesde;
            ViewBag.FechaHasta = FechaHasta;

            ViewBag.IdFacturaSortParam = sOrder == "#" ? "#_desc" : "#";
            ViewBag.FolioFacturaSortParam = sOrder == "FolioFactura" ? "FolioFactura_desc" : "FolioFactura";

            CargarCatalogos(IdProveedor, IdRecurso);

            var vModelQuery = _db.PLU_CAT_Facturas
                .Include(f => f.PLU_CAT_Proveedores)
                .Include(f => f.PLU_CAT_Recursos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(FolioFactura))
            {
                vModelQuery = vModelQuery.Where(r => r.FolioFactura.Contains(FolioFactura));
            }

            if (IdProveedor.HasValue && IdProveedor.Value > 0)
            {
                vModelQuery = vModelQuery.Where(r => r.IdProveedor == IdProveedor.Value);
            }

            if (IdRecurso.HasValue && IdRecurso.Value > 0)
            {
                vModelQuery = vModelQuery.Where(r => r.IdRecurso == IdRecurso.Value);
            }

            if (FechaDesde.HasValue)
            {
                vModelQuery = vModelQuery.Where(r => DbFunctions.TruncateTime(r.FechaFactura) >= FechaDesde.Value.Date);
            }

            if (FechaHasta.HasValue)
            {
                vModelQuery = vModelQuery.Where(r => DbFunctions.TruncateTime(r.FechaFactura) <= FechaHasta.Value.Date);
            }

            switch (sOrder)
            {
                case "#":
                    vModelQuery = vModelQuery.OrderByDescending(r => r.Activo).ThenBy(r => r.IdFactura);
                    break;
                case "#_desc":
                    vModelQuery = vModelQuery.OrderByDescending(r => r.Activo).ThenByDescending(r => r.IdFactura);
                    break;
                case "FolioFactura":
                    vModelQuery = vModelQuery.OrderByDescending(r => r.Activo).ThenBy(r => r.FolioFactura);
                    break;
                case "FolioFactura_desc":
                    vModelQuery = vModelQuery.OrderByDescending(r => r.Activo).ThenByDescending(r => r.FolioFactura);
                    break;
                default:
                    vModelQuery = vModelQuery.OrderByDescending(r => r.Activo).ThenBy(r => r.IdFactura);
                    break;
            }

            var vModel = vModelQuery.ToPagedList(iPagina, iPerPage);

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


        public void CargarCatalogos(int? proveedorId = null, int? recursoId = null)
        {
            
            var proveedor = _db.PLU_CAT_Proveedores.Where(x => x.Activo == true).OrderBy(x => x.RazonSocial).ToList();
            ViewBag.Proveedores = new SelectList(proveedor, "IdProveedor", "RazonSocial", proveedorId);

            var Recursos = _db.PLU_CAT_Recurso.Where(x => x.Activo == true).OrderBy(x => x.NombreRecurso).ToList();
            ViewBag.Recursos = new SelectList(Recursos, "IdRecurso", "NombreRecurso", recursoId);

        }
    }
}
