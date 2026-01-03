using ActivosFijos.Models;
using Helpers;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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



    }
}