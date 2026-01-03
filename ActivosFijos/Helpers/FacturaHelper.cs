using ActivosFijos.Models;
using Helpers;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ActivosFijos.Helpers
{
    public class FacturaHelper
    {
        ModelContext _db = new ModelContext();

        public IPagedList<PLU_CAT_Facturas> GetAll(string sOrden, string FolioFactura , int iPagina, int iPerPage)
        {



            var vModel = _db.PLU_CAT_Facturas.Where(r => (FolioFactura.Length == 0 || r.FolioFactura.Contains(FolioFactura)));

            switch (sOrden)
            {
                case "#":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.IdFactura);
                    break;
                case "#_desc":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenByDescending(r => r.IdFactura);
                    break;
                case "FolioFactura":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.FolioFactura);
                    break;
                case "FolioFactura_desc":
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenByDescending(r => r.FolioFactura);
                    break;
                default:
                    vModel = vModel.OrderByDescending(r => r.Activo).ThenBy(r => r.IdFactura);
                    break;
            }

            return vModel.ToPagedList(iPagina, iPerPage);
        }


        public ResponseModel Add(PLU_CAT_Facturas facturas)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Guardar el activo en la base de datos
                    ctx.Entry(facturas).State = EntityState.Added;
                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                // Capturar errores de validación de entidad
                var errorMessages = dbEx.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => e.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(dbEx.Message, " Errores de validación: ", fullErrorMessage);

                rm.SetResponse(false, "Error de validación: " + exceptionMessage);
            }
            catch (DbUpdateException dbUpdateEx)
            {
                // Capturar errores relacionados con la actualización de la base de datos
                var innerException = dbUpdateEx.InnerException?.Message ?? dbUpdateEx.Message;
                rm.SetResponse(false, "Error al actualizar la base de datos: " + innerException);
            }
            catch (Exception ex)
            {
                // Capturar errores generales
                var innerException = ex.InnerException?.Message ?? ex.Message;
                rm.SetResponse(false, "Error general: " + innerException);
            }

            return rm;
        }


        public ResponseModel Edit(PLU_CAT_Facturas facturas)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Marcar la entidad como modificada
                    ctx.Entry(facturas).State = EntityState.Modified;
                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                // Capturar errores de validación de entidad
                var errorMessages = dbEx.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => e.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(dbEx.Message, " Errores de validación: ", fullErrorMessage);

                rm.SetResponse(false, "Error de validación: " + exceptionMessage);
            }
            catch (DbUpdateConcurrencyException concurrencyEx)
            {
                // Capturar errores de concurrencia
                rm.SetResponse(false, "Error de concurrencia al actualizar la base de datos: " + concurrencyEx.Message);
            }
            catch (DbUpdateException dbUpdateEx)
            {
                // Capturar errores relacionados con la actualización de la base de datos
                var innerException = dbUpdateEx.InnerException?.Message ?? dbUpdateEx.Message;
                rm.SetResponse(false, "Error al actualizar la base de datos: " + innerException);
            }
            catch (Exception ex)
            {
                // Capturar errores generales
                var innerException = ex.InnerException?.Message ?? ex.Message;
                rm.SetResponse(false, "Error general: " + innerException);
            }

            return rm;
        }
    }
}