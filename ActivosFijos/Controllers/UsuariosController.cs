using ActivosFijos.Models;
using Helpers;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ActivosFijos.Models.ViewModels;
using static EDUES_ADMIN.Filters.AdminFilters;


namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class UsuariosController : Controller
    {
        private ModelContext _db = new ModelContext();
        private UsuariosHelper UsuariosB = new UsuariosHelper();
        private RolesHelper RolesB = new RolesHelper();
        

        public ActionResult Index(string sOrder = "", string sUserName = "", string sRolId = "", string Nombres = "", string Apellidos = "",string sActive = "", int iPagina = 1, int iPerPage = 10)
        {
            ViewBag.Order = sOrder;
            ViewBag.PerPage = iPerPage;
            ViewBag.Pagina = iPagina;

            ViewBag.Roles = RolesB.ObtenerLista();
            var vModel = UsuariosB.GetAll(sOrder, sUserName,Nombres, Apellidos, sRolId, sActive, iPagina, iPerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListaUsuarios", vModel);
            }

            return View(vModel);
        }

        public ActionResult Agregar()
        {
            var roles = _db.PLU_CAT_Roles.Where(x => x.Activo == true && x.IdRol != 1).OrderBy(x => x.IdRol).ToList();
            ViewBag.Roles = new SelectList(roles, "IdRol", "NombreRol");
            return View();
        }

        public ActionResult Editar(int id)
        {
            var Model = _db.PLU_CONF_Usuario.Where(x => x.IdUsuario == id).FirstOrDefault();
            var roles = _db.PLU_CAT_Roles.Where(x => x.Activo == true && x.IdRol != 1).OrderBy(x => x.IdRol).ToList();
            ViewBag.Roles = new SelectList(roles, "IdRol", "NombreRol");
            return View("Editar",Model);
        }

        public JsonResult Guardar(PLU_CONF_Usuario usuario)
        {
            var rm = new ResponseModel();
            
            if (ModelState.IsValid)
            {
                if (usuario.IdUsuario == 0)
                {
                    usuario.Pass = HashHelper.SHA256(usuario.Pass);
                    usuario.Activo = true;
                    usuario.ForcePasswordChange = false;
                    usuario.FechaCreacion = DateTime.Now;
                    rm = UsuariosB.Add(usuario);

                    if (rm.response)
                    {

                        rm.message = "Usuario agregado con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Usuarios");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al agregar Usuario.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
                else
                {
                    rm = UsuariosB.Edit(usuario);

                    if (rm.response)
                    {

                        rm.message = "Usuario se Edito con exito.";
                        rm.function = "HideLoading();";
                        rm.href = Url.Action("Index", "Usuarios");
                        rm.error = false;
                    }
                    else
                    {
                        rm.message = "Error al editar Usuario.";
                        rm.function = "HideLoading();";
                        rm.error = true;
                    }
                }
            }

            return Json(rm);
        }

        public ActionResult Perfil()
        {
            int idUsuario = SessionHelper.GetUser();

            var usuario = _db.PLU_CONF_Usuario.Where(x => x.IdUsuario == idUsuario).FirstOrDefault();
            
            return View("Perfil",usuario);
        }

        public ActionResult CambiarPassword(string returnUrl = "")
        {
            var model = new ChangePasswordViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CambiarPassword(ChangePasswordViewModel model)
        {
            var rm = new ResponseModel();

            if (!ModelState.IsValid)
            {
                rm.message = "Modelo no válido, verifique los datos ingresados.";
                rm.error = true;
                return Json(rm);
            }

            int idUsuario = SessionHelper.GetUser();
            var usuario = _db.PLU_CONF_Usuario.FirstOrDefault(x => x.IdUsuario == idUsuario);

            if (usuario == null)
            {
                rm.message = "No se encontró el usuario en sesión.";
                rm.error = true;
                return Json(rm);
            }

            var currentHash = HashHelper.SHA256(model.CurrentPassword);

            if (!string.Equals(usuario.Pass, currentHash, StringComparison.OrdinalIgnoreCase))
            {
                rm.message = "La contraseña actual es incorrecta.";
                rm.error = true;
                return Json(rm);
            }

            usuario.Pass = HashHelper.SHA256(model.NewPassword);
            usuario.ForcePasswordChange = false;
            _db.SaveChanges();

            rm.SetResponse(true);
            rm.message = "Contraseña actualizada correctamente.";
            rm.href = string.IsNullOrWhiteSpace(model.ReturnUrl) ? Url.Action("Perfil", "Usuarios") : model.ReturnUrl;
            rm.error = false;

            return Json(rm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RestablecerPassword(int id)
        {
            var rm = new ResponseModel();

            var usuario = _db.PLU_CONF_Usuario.FirstOrDefault(x => x.IdUsuario == id);
            if (usuario == null)
            {
                rm.message = "Usuario no encontrado.";
                rm.error = true;
                return Json(rm);
            }

            usuario.Pass = HashHelper.SHA256("123456789$");
            usuario.ForcePasswordChange = true;
            _db.SaveChanges();

            rm.SetResponse(true);
            rm.message = "Contraseña restablecida correctamente.";
            rm.error = false;

            return Json(rm);
        }



    }
}
