using ActivosFijos.Models;
using System.Linq;
using System.Web.Mvc;
using static EDUES_ADMIN.Filters.AdminFilters;

namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class ClasificadoresController : Controller
    {
        ModelContext _db = new ModelContext();
        // GET: Clasificadores
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetClasificadores()
        {
            var data = _db.PLU_CAT_Clasificadores.Select(x => new {
                x.IdClasificadores,
                x.ClasificadorDescripcion,
                x.Activo,
                x.FechaCreacion
            }).ToList();

            var jsonResult = Json(new { data }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}