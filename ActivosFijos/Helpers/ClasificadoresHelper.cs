using ActivosFijos.Models;
using Helpers;
using PagedList;
using System.Linq;

namespace ActivosFijos.Helpers
{
    public class ClasificadoresHelper
    {
        ModelContext _db = new ModelContext();

        public IPagedList<PLU_CAT_Clasificadores> GetAll(string sOrden, string clasificador, int iPagina, int iPerPage)
        {
            var vModel = _db.PLU_CAT_Clasificadores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(clasificador))
            {
                vModel = vModel.Where(r => r.ClasificadorDescripcion.Contains(clasificador));
            }

            switch (sOrden)
            {
                case "#":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.IdClasificadores);
                    break;
                case "#_desc":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenByDescending(r => r.IdClasificadores);
                    break;
                case "ClasificadorDescripcion":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.ClasificadorDescripcion);
                    break;
                case "ClasificadorDescripcion_desc":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenByDescending(r => r.ClasificadorDescripcion);
                    break;
                default:
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.IdClasificadores);
                    break;
            }

            return vModel.ToPagedList(iPagina, iPerPage);
        }
    }
}
