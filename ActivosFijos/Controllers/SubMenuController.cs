using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ActivosFijos.Models;
using static EDUES_ADMIN.Filters.AdminFilters;

namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class SubMenuController : Controller
    {
        ModelContext _db = new ModelContext();
        public ActionResult Index()
        {
            var menu = _db.PLU_CONF_Menu.ToList();
            ViewBag.menu = new SelectList(menu, "IdMenu", "TituloMenu");
            return View();
        }


        [HttpGet]
        public JsonResult GetSubMenuTable()
        {
            var data = _db.PLU_CONF_SubMenu.Select(x => new { x.IdSubMenu, x.PLU_CONF_Menu.TituloMenu, x.TituloSubMenu, x.Controlador, x.Accion, x.Orden, x.Activo, x.FechaCreacion }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Guardar(PLU_CONF_SubMenu plu_submenu)
        {

            var rm = new ResponseModel();
            SubMenuHelper submenu = new SubMenuHelper();
            plu_submenu.FechaCreacion = DateTime.Now;

            if (ModelState.IsValid)
            {

                rm = submenu.Agregar(plu_submenu);

                if (rm.response)
                {

                    rm.message = "SubMenú agregado con exito.";
                    rm.function = "CargarData();$('#close').trigger('click');";
                    rm.error = false;
                }
                else
                {
                    rm.message = "Error al agregar SubMenú.";
                    rm.function = "CargarData();$('#close').trigger('click');";
                    rm.error = true;
                }

            }

            return Json(rm);
        }
    }
}
