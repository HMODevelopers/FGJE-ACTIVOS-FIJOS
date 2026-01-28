using ActivosFijos.Helpers;
using ActivosFijos.Models;
using System.Linq;
using System.Web.Mvc;
using static EDUES_ADMIN.Filters.AdminFilters;

namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class AreasUnidadesAdminController : Controller
    {
        ModelContext _db = new ModelContext();
        AreasUnidadesAdminHelper AreasUnidadesAdminB = new AreasUnidadesAdminHelper();
        // GET: AreasUnidadesAdmin
        public ActionResult Index(string sOrder = "", string UnidadAdministrativa = "", string NombreArea = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.UnidadAdministrativa = UnidadAdministrativa;
            ViewBag.NombreArea = NombreArea;

            ViewBag.IdAreaSortParam = sOrder == "#" ? "#_desc" : "#";
            ViewBag.UnidadAdministrativaSortParam = sOrder == "UnidadAdministrativa" ? "UnidadAdministrativa_desc" : "UnidadAdministrativa";
            ViewBag.NombreAreaSortParam = sOrder == "NombreArea" ? "NombreArea_desc" : "NombreArea";

            var vModel = AreasUnidadesAdminB.GetAll(sOrder, UnidadAdministrativa, NombreArea, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaAreasUnidadesAdmin", vModel);
            }

            return View(vModel);
        }


        public JsonResult GetAreasUnidadesAdmin()
        {
            var data = _db.PLU_CAT_AreasUnidadesAdministrativas.Select(x => new {
                x.IdAreaUnidadAdministrativa,
                x.PLU_CAT_UnidadesAdministrativas.UnidadAdministrativa,
                x.NombreArea,
                x.Activo,
                x.FechaCreacion
            }).ToList();

            var jsonResult = Json(new { data }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}
