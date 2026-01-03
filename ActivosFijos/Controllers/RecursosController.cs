using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static EDUES_ADMIN.Filters.AdminFilters;

namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class RecursosController : Controller
    {
        // GET: Recursos
        public ActionResult Index()
        {
            return View();
        }
    }
}