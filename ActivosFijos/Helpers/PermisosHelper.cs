using ActivosFijos.Models;
using Helpers;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ActivosFijos.Helpers
{
    public class PermisosHelper
    {

        ModelContext _db = new ModelContext();
        public IPagedList<PLU_CAT_Roles> GetAll( int iPagina = 1, int iPerPage = 10)
        {

            var vModel = ObtenerRoles();


            return vModel.ToPagedList(iPagina, iPerPage);

        }

        private IQueryable<PLU_CAT_Roles> ObtenerRoles()
        {
            return _db.PLU_CAT_Roles.OrderBy(a => a.IdRol);
        }

        public ResponseModel Add(PLU_CAT_Roles Roles)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Guardar el activo en la base de datos
                    ctx.Entry(Roles).State = EntityState.Added;
                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al agregar Rol: " + ex.Message);

            }

            return rm;
        }

        public ResponseModel Edit(PLU_CAT_Roles Roles)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Guardar el activo en la base de datos
                    ctx.Entry(Roles).State = EntityState.Modified;
                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al editar Rol: " + ex.Message);
            }

            return rm;
        }

    }
}