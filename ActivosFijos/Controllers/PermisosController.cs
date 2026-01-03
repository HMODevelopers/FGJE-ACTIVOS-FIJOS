using ActivosFijos.Helpers;
using ActivosFijos.Models;
using ActivosFijos.Models.ViewModels;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ActivosFijos.Controllers
{
    public class PermisosController : Controller
    {
        private ModelContext _db = new ModelContext();
        PermisosHelper PermisosB = new PermisosHelper();
        // GET: Permisos

        public ActionResult Index(int? page)
        {

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var vModel = PermisosB.GetAll(pageNumber, pageSize);



            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaRoles", vModel);
            }

            return View(vModel);
        }

        public ActionResult Agregar()
        {
            return View();
        }

        public ActionResult Editar(int id)
        {
            var Model = _db.PLU_CAT_Roles.Where(x => x.IdRol == id).FirstOrDefault();
            return View("Editar", Model);
        }

        public ActionResult Permisos(int id)
        {
            var permisos = _db.PLU_CONF_PermisosMenu.Where(x => x.IdRol == id).ToList();

            if (permisos.Count == 0)
            {
                var menus = _db.PLU_CONF_Menu.ToList();
                var nuevosPermisos = new List<PLU_CONF_PermisosMenu>();

                foreach (var menu in menus)
                {
                    var permisoMenu = new PLU_CONF_PermisosMenu
                    {
                        IdRol = id,
                        IdMenu = menu.IdMenu,
                        PermisoVisualizacion = false,
                        PermisoEspecial = false,
                        Activo = true,
                        FechaCreacion = DateTime.Now
                    };
                    nuevosPermisos.Add(permisoMenu);

                    foreach (var subMenu in menu.PLU_CONF_SubMenu)
                    {
                        var permisoSubMenu = new PLU_CONF_PermisosMenu
                        {
                            IdRol = id,
                            IdSubMenu = subMenu.IdSubMenu,
                            PermisoVisualizacion = false,
                            PermisoEspecial = false,
                            Activo = true,
                            FechaCreacion = DateTime.Now
                        };
                        nuevosPermisos.Add(permisoSubMenu);
                    }
                }

                _db.PLU_CONF_PermisosMenu.AddRange(nuevosPermisos);
                _db.SaveChanges();

                permisos = _db.PLU_CONF_PermisosMenu.Where(x => x.IdRol == id).ToList();
            }

            var viewModel = new List<MenuViewModel>();
            var menusOrdenados = _db.PLU_CONF_Menu.OrderBy(m => m.Orden).ToList();

            foreach (var menu in menusOrdenados)
            {
                var menuViewModel = new MenuViewModel
                {
                    IdMenu = menu.IdMenu,
                    TituloMenu = menu.TituloMenu,
                    PermisoVisualizacion = permisos.Any(p => p.IdMenu == menu.IdMenu && p.PermisoVisualizacion),
                    PermisoEspecial = permisos.Any(p => p.IdMenu == menu.IdMenu && p.PermisoEspecial),
                    SubMenus = menu.PLU_CONF_SubMenu
                                    .OrderBy(s => s.Orden)
                                    .Select(s => new SubMenuViewModels
                                    {
                                        IdSubMenu = s.IdSubMenu,
                                        IdMenu = s.IdMenu,
                                        TituloSubMenu = s.TituloSubMenu,
                                        Controlador = s.Controlador,
                                        Accion = s.Accion,
                                        PermisoVisualizacion = permisos.Any(p => p.IdSubMenu == s.IdSubMenu && p.PermisoVisualizacion),
                                        PermisoEspecial = permisos.Any(p => p.IdSubMenu == s.IdSubMenu && p.PermisoEspecial)
                                    })
                                    .ToList()
                };

                viewModel.Add(menuViewModel);
            }

            ViewBag.IdRol = id; // Asegúrate de tener el IdRol disponible en la ViewBag

            return View("Permisos", viewModel);
        }

        [HttpPost]
        public JsonResult Guardar(PLU_CAT_Roles roles)
        {
            var rm = new ResponseModel();

            if (ModelState.IsValid)
            {
                if (roles.IdRol == 0)
                {

                    roles.Activo = true;
                    roles.FechaCreacion = DateTime.Now;
                    rm = PermisosB.Add(roles);

                    if (rm.response)
                    {

                        rm.message = "Rol agregado con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Permisos");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar Rol.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
                else
                {
                    rm = PermisosB.Edit(roles);

                    if (rm.response)
                    {

                        rm.message = "Rol se Edito con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Permisos");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar Rol.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
            }

            return Json(rm);
        }


        [HttpPost]
        public JsonResult ActualizarPermisos(List<MenuViewModel> model, int IdRol)
        {
            var rm = new ResponseModel();

            try
            {
                if (ModelState.IsValid)
                {
                    foreach (var menu in model)
                    {
                        // Actualizar o insertar permisos del menú
                        var permisoMenu = _db.PLU_CONF_PermisosMenu.FirstOrDefault(p => p.IdMenu == menu.IdMenu && p.IdRol == IdRol);
                        if (permisoMenu != null)
                        {
                            permisoMenu.PermisoVisualizacion = menu.PermisoVisualizacion;
                            permisoMenu.PermisoEspecial = menu.PermisoEspecial;
                        }
                        else
                        {
                            permisoMenu = new PLU_CONF_PermisosMenu
                            {
                                IdMenu = menu.IdMenu,
                                IdRol = IdRol,
                                PermisoVisualizacion = menu.PermisoVisualizacion,
                                PermisoEspecial = menu.PermisoEspecial,
                                Activo = true,
                                FechaCreacion = DateTime.UtcNow
                            };
                            _db.PLU_CONF_PermisosMenu.Add(permisoMenu);
                            _db.SaveChanges(); // Guardar cambios inmediatamente para nuevas inserciones
                        }

                        // Actualizar o insertar permisos de los submenús
                        foreach (var subMenu in menu.SubMenus)
                        {
                            var permisoSubMenu = _db.PLU_CONF_PermisosMenu.FirstOrDefault(p => p.IdSubMenu == subMenu.IdSubMenu && p.IdRol == IdRol);
                            if (permisoSubMenu != null)
                            {
                                permisoSubMenu.PermisoVisualizacion = subMenu.PermisoVisualizacion;
                                permisoSubMenu.PermisoEspecial = subMenu.PermisoEspecial;
                            }
                            else
                            {
                                permisoSubMenu = new PLU_CONF_PermisosMenu
                                {
                                    IdSubMenu = subMenu.IdSubMenu,
                                    IdRol = IdRol,
                                    PermisoVisualizacion = subMenu.PermisoVisualizacion,
                                    PermisoEspecial = subMenu.PermisoEspecial,
                                    Activo = true,
                                    FechaCreacion = DateTime.UtcNow
                                    
                                };
                                _db.PLU_CONF_PermisosMenu.Add(permisoSubMenu);
                                _db.SaveChanges(); // Guardar cambios inmediatamente para nuevas inserciones
                            }
                        }
                    }

                    _db.SaveChanges(); // Guardar todos los cambios restantes al final

                    rm.response = true;
                    rm.message = "Permisos actualizados correctamente.";
                    rm.function = "HideLoading();";
                    rm.href = Url.Action("Index", "Permisos");
                    rm.error = false;
                }
                else
                {
                    rm.response = false;
                    rm.message = "Modelo no válido, verifique los datos ingresados.";
                    rm.function = "HideLoading();";
                    rm.error = true;
                }
            }
            catch (Exception ex)
            {
                rm.response = false;
                rm.message = "Error al intentar actualizar permisos: " + ex.Message;
                rm.function = "HideLoading();";
                rm.error = true;
            }

            return Json(rm);
        }
    }
}