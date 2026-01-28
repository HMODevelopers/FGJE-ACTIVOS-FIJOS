using ActivosFijos.Helpers;
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
        ClasificadoresHelper ClasificadoresB = new ClasificadoresHelper();
        // GET: Clasificadores
        public ActionResult Index(string sOrder = "", string Clasificador = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.Clasificador = Clasificador;

            ViewBag.IdClasificadorSortParam = sOrder == "#" ? "#_desc" : "#";
            ViewBag.NombreClasificadorSortParam = sOrder == "ClasificadorDescripcion" ? "ClasificadorDescripcion_desc" : "ClasificadorDescripcion";

            var vModel = ClasificadoresB.GetAll(sOrder, Clasificador, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaClasificadores", vModel);
            }

            return View(vModel);
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
