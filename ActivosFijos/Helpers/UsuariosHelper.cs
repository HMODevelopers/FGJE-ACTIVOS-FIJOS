using ActivosFijos.Models;
using System.Linq;
using PagedList;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web;
using System;

namespace Helpers
{
    public class UsuariosHelper
    {
        ModelContext _db = new ModelContext();

        public IPagedList<PLU_CONF_Usuario> GetAll(string sOrden, string sUserName , string nombres, string apellidos, string sRolId, string sActive , int iPagina , int iPerPage)
        {
            
            int iRolId = 0;
            int.TryParse(sRolId, out iRolId);

            bool bActive = true;
            bool.TryParse(sActive, out bActive);

            var vModel = _db.PLU_CONF_Usuario.Where(r => (sUserName.Length == 0 || r.Username.Contains(sUserName)) && 
            (nombres.Length == 0 || r.Nombres.Contains(nombres)) &&
            (apellidos.Length == 0 || r.Apellidos.Contains(apellidos))&&
            (iRolId == 0 || r.IdRol == iRolId) && 
            (sActive.Length == 0 || r.Activo == bActive));

            switch (sOrden)
            {
                case "Username":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.Username);
                    break;
                case "Username_Desc":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenByDescending(r => r.Username);
                    break;
                case "RolId":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.PLU_CAT_Roles.IdRol);
                    break;
                case "RolId_Desc":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenByDescending(r => r.PLU_CAT_Roles.IdRol);
                    break;
                case "Activo":
                    vModel = vModel.OrderBy(r => r.Activo);
                    break;
                case "Activo_Desc":
                    vModel = vModel.OrderByDescending(r => r.Activo);
                    break;
                default:
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.Username);
                    break;
            }

            return vModel.ToPagedList(iPagina, iPerPage);
        }


        public ResponseModel Add(PLU_CONF_Usuario usuario)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Guardar el activo en la base de datos
                    ctx.Entry(usuario).State = EntityState.Added;
                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al agregar Usuario: " + ex.Message);

            }

            return rm;
        }

        public ResponseModel Edit(PLU_CONF_Usuario usuario)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Guardar el activo en la base de datos
                    ctx.Entry(usuario).State = EntityState.Modified;
                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al editar Usuario: " + ex.Message);
            }

            return rm;
        }
    }
}