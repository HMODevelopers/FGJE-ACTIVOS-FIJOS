using ActivosFijos.Models;
using Helpers;
using System;
using System.Data.Entity.Validation;
using System.Data.Entity;


namespace Helpers
{
    public class SubMenuHelper
    {
        public ResponseModel Agregar(PLU_CONF_SubMenu plu_submenu)
        {
            var rm = new ResponseModel();
            try
            {
                using (var ctx = new ModelContext())
                {

                    ctx.Entry(plu_submenu).State = EntityState.Added;
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