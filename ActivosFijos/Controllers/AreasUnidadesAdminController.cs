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
        // GET: AreasUnidadesAdmin
        public ActionResult Index()
        {
            return View();
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