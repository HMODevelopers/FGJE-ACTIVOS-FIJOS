using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static EDUES_ADMIN.Filters.AdminFilters;

namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class UnidadesAdministrativasController : Controller
    {
        // GET: UnidadesAdministrativas
        public ActionResult Index()
        {
            return View();
        }
    }
}