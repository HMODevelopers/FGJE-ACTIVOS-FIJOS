using ActivosFijos.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Linq;
using ActivosFijos.Models.ViewModels;



namespace Helpers
{
    public class MenuHelper
    {
        ModelContext _db = new ModelContext();


        public List<MenuViewModel> GetMenu(int idRol)
        {
            // Obtener los menús que tienen permisos asociados al rol y los submenús que tienen PermisoVisualizacion activado
            var menuItems = _db.PLU_CONF_Menu
                                .Where(m => _db.PLU_CONF_PermisosMenu.Any(p => p.IdRol == idRol && p.IdMenu == m.IdMenu && p.PermisoVisualizacion))
                                .OrderBy(m => m.Orden)
                                .Select(m => new MenuViewModel
                                {
                                    IdMenu = m.IdMenu,
                                    TituloMenu = m.TituloMenu,
                                    Icono = m.Icono,
                                    Orden = m.Orden,
                                    SubMenus = m.PLU_CONF_SubMenu
                                                .Where(sm => sm.Activo && _db.PLU_CONF_PermisosMenu.Any(p => p.IdRol == idRol && p.IdSubMenu == sm.IdSubMenu && p.PermisoVisualizacion))
                                                .OrderBy(sm => sm.Orden)
                                                .Select(sm => new SubMenuViewModels
                                                {
                                                    IdSubMenu = sm.IdSubMenu,
                                                    IdMenu = sm.IdMenu,
                                                    TituloSubMenu = sm.TituloSubMenu,
                                                    Controlador = sm.Controlador,
                                                    Accion = sm.Accion,
                                                    PermisoVisualizacion = _db.PLU_CONF_PermisosMenu
                                                                            .Any(p => p.IdRol == idRol && p.IdSubMenu == sm.IdSubMenu && p.PermisoVisualizacion),
                                                    PermisoEspecial = _db.PLU_CONF_PermisosMenu
                                                                        .Any(p => p.IdRol == idRol && p.IdSubMenu == sm.IdSubMenu && p.PermisoEspecial)
                                                })
                                                .ToList()
                                })
                                .ToList();

            return menuItems;
        }

        public List<SubMenuViewModels> GetSubMenu(int idMenu, int idRol)
        {
            var subMenuItems = _db.PLU_CONF_SubMenu
                                    .Where(s => s.IdMenu == idMenu && s.Activo)
                                    .Select(x => new SubMenuViewModels
                                    {
                                        IdSubMenu = x.IdSubMenu,
                                        IdMenu = x.IdMenu,
                                        TituloSubMenu = x.TituloSubMenu,
                                        Controlador = x.Controlador,
                                        Accion = x.Accion,
                                        PermisoVisualizacion = _db.PLU_CONF_PermisosMenu
                                                                .Any(p => p.IdRol == idRol && p.IdSubMenu == x.IdSubMenu && p.PermisoVisualizacion),
                                        PermisoEspecial = _db.PLU_CONF_PermisosMenu
                                                            .Any(p => p.IdRol == idRol && p.IdSubMenu == x.IdSubMenu && p.PermisoEspecial)
                                    })
                                    .OrderBy(x => x.Orden)
                                    .ToList();

            return subMenuItems;
        }


        public ResponseModel CambiarStatus(PLU_CONF_Menu plu_menu)
        {
            var rm = new ResponseModel();
            try
            {
                using (var ctx = new ModelContext())
                {

                    ctx.Entry(plu_menu).State = EntityState.Modified;
                    ctx.SaveChanges();
                    rm.SetResponse(true);

                }
            }
            catch (DbEntityValidationException e)
            {
                throw e;
            }
            catch (Exception)
            {

                throw;
            }

            return rm;
        }

        public ResponseModel Agregar(PLU_CONF_Menu plu_menu)
        {
            var rm = new ResponseModel();
            try
            {
                using (var ctx = new ModelContext())
                {

                    ctx.Entry(plu_menu).State = EntityState.Added;
                    ctx.SaveChanges();
                    rm.SetResponse(true);

                }
            }
            catch (DbEntityValidationException e)
            {
                throw e;
            }
            catch (Exception)
            {

                throw;
            }

            return rm;
        }

    }
}