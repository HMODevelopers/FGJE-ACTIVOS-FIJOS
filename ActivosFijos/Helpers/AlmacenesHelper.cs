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
    public class AlmacenesHelper
    {
        ModelContext _db = new ModelContext();

        public IPagedList<PLU_CAT_Almacenes> GetAll(string sOrden, string Almacen, int iPagina, int iPerPage)
        {

           

            var vModel = _db.PLU_CAT_Almacenes.Where(r => (Almacen.Length == 0 || r.NombreAlmacen.Contains(Almacen)) );

            switch (sOrden)
            {
                case "#":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.IdAlmacen);
                    break;
                case "#_desc":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenByDescending(r => r.IdAlmacen);
                    break;
                case "NombreAlmacen":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.NombreAlmacen);
                    break;
                case "NombreAlmacen_desc":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenByDescending(r => r.NombreAlmacen);
                    break;
                default:
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.IdAlmacen);
                    break;
            }

            return vModel.ToPagedList(iPagina, iPerPage);
        }


        public ResponseModel Add(PLU_CAT_Almacenes almacen)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Guardar el activo en la base de datos
                    ctx.Entry(almacen).State = EntityState.Added;
                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al agregar almacen: " + ex.Message);

            }

            return rm;
        }

        public ResponseModel Edit(PLU_CAT_Almacenes almacen)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Guardar el activo en la base de datos
                    ctx.Entry(almacen).State = EntityState.Modified;
                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al editar almacen: " + ex.Message);
            }

            return rm;
        }
    }
}