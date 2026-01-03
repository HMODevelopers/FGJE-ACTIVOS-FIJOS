
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
    public class MenuController : Controller
    {
        ModelContext _db = new ModelContext();
      
        // GET: Menu
        public ActionResult Index()
        {
            
            
            return View();
        }

        [HttpGet]
        public JsonResult GetMenu()
        {
            var data = _db.PLU_CONF_Menu.Select(x => new { x.IdMenu, x.TituloMenu, x.Icono, x.Orden, x.Activo, x.FechaCreacion }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CambiarStatus(int IdMenu, bool activo)
        {

            var rm = new ResponseModel();
            MenuHelper menu = new MenuHelper();
            PLU_CONF_Menu menus = new PLU_CONF_Menu();
            var data = _db.PLU_CONF_Menu.Where(x => x.IdMenu == IdMenu).FirstOrDefault();
            menus.IdMenu = data.IdMenu;
            menus.TituloMenu = data.TituloMenu;
            menus.Icono = data.Icono;
            menus.Orden = data.Orden;
            menus.FechaCreacion = data.FechaCreacion;

            if (activo)
            {
                menus.Activo = false;
            }
            else
            {
                menus.Activo = true;
            }

            rm = menu.CambiarStatus(menus);

            if (rm.response)
            {
                rm.message = "Se cambio el estatus del menú con exito.";
                rm.error = false;
            }
            else
            {
                rm.message = "Error al cambiar estatus";
                rm.error = true;
            }

            return Json(rm);
        }


        [HttpPost]
        public JsonResult Guardar(PLU_CONF_Menu plu_menu)
        {

            var rm = new ResponseModel();
            MenuHelper menu = new MenuHelper();
            plu_menu.FechaCreacion = DateTime.Now;

            if (ModelState.IsValid)
            {

                rm = menu.Agregar(plu_menu);

                if (rm.response)
                {

                    rm.message = "Menu agregado con exito.";
                    rm.function = "CargarData();$('#close').trigger('click');";
                    rm.error = false;
                }
                else
                {
                    rm.message = "Error al agregar Menu.";
                    rm.function = "CargarData();$('#close').trigger('click');";
                    rm.error = true;
                }

            }

            return Json(rm);
        }

    }
}