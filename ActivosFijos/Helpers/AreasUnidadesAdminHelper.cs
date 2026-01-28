using ActivosFijos.Models;
using Helpers;
using PagedList;
using System.Data.Entity;
using System.Linq;

namespace ActivosFijos.Helpers
{
    public class AreasUnidadesAdminHelper
    {
        ModelContext _db = new ModelContext();

        public IPagedList<PLU_CAT_AreasUnidadesAdministrativas> GetAll(string sOrden, string unidadAdministrativa, string nombreArea, int iPagina, int iPerPage)
        {
            var vModel = _db.PLU_CAT_AreasUnidadesAdministrativas
                .Include(r => r.PLU_CAT_UnidadesAdministrativas)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(unidadAdministrativa))
            {
                vModel = vModel.Where(r => r.PLU_CAT_UnidadesAdministrativas != null
                    && r.PLU_CAT_UnidadesAdministrativas.UnidadAdministrativa.Contains(unidadAdministrativa));
            }

            if (!string.IsNullOrWhiteSpace(nombreArea))
            {
                vModel = vModel.Where(r => r.NombreArea.Contains(nombreArea));
            }

            switch (sOrden)
            {
                case "#":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.IdAreaUnidadAdministrativa);
                    break;
                case "#_desc":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenByDescending(r => r.IdAreaUnidadAdministrativa);
                    break;
                case "UnidadAdministrativa":
                    vModel = vModel.OrderByDescending(r => r.Activo)
                        .ThenBy(r => r.PLU_CAT_UnidadesAdministrativas.UnidadAdministrativa);
                    break;
                case "UnidadAdministrativa_desc":
                    vModel = vModel.OrderByDescending(r => r.Activo)
                        .ThenByDescending(r => r.PLU_CAT_UnidadesAdministrativas.UnidadAdministrativa);
                    break;
                case "NombreArea":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.NombreArea);
                    break;
                case "NombreArea_desc":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenByDescending(r => r.NombreArea);
                    break;
                default:
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.IdAreaUnidadAdministrativa);
                    break;
            }

            return vModel.ToPagedList(iPagina, iPerPage);
        }
    }
}
