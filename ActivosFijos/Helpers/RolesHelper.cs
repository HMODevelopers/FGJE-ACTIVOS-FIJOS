using ActivosFijos.Models;
using System.Collections.Generic;
using System.Linq;


namespace Helpers
{
    public class RolesHelper
    {
        ModelContext _db = new ModelContext();
        public List<PLU_CAT_Roles> ObtenerLista()
        {
            var vModel = _db.PLU_CAT_Roles.Where(r => r.Activo).ToList();
            return vModel;
        }
    }
}