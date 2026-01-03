using ActivosFijos.Models;
using Helpers;
using PagedList;
using System;
using System.Linq;
using System.Data.Entity;


namespace ActivosFijos.Helpers
{
    public class EmpleadosHelper
    {
        ModelContext _db = new ModelContext();
        public IPagedList<PLU_OP_Empleados> GetAllEmpleados(string sOrder = "", int CveEmpleado = 0, string Paterno = "", string Materno = "", string Nombre = "", int iPagina = 1, int iPerPage = 10)
        {
            var vModel = ObtenerEmpleados();

            Paterno = Paterno?.ToUpper();
            Materno = Materno?.ToUpper();
            Nombre = Nombre?.ToUpper();

         

            vModel = vModel.Where(e =>
                         (string.IsNullOrEmpty(Paterno) || e.ApellidoP != null && e.ApellidoP.Contains(Paterno))
                         && (string.IsNullOrEmpty(Materno) || e.ApellidoM != null && e.ApellidoM.Contains(Materno))
                         && (string.IsNullOrEmpty(Nombre) || e.Nombres != null && e.Nombres.Contains(Nombre))
                         && (CveEmpleado == 0 || e.NumeroEmpleado == CveEmpleado)
                     );

            

            switch (sOrder)
            {
                case "CVE_EMPLEADO":
                    vModel = vModel.OrderBy(e => e.NumeroEmpleado);
                    break;
                case "CVE_EMPLEADO_desc":
                    vModel = vModel.OrderByDescending(e => e.NumeroEmpleado);
                    break;
                case "APE_PATERNO":
                    vModel = vModel.OrderBy(e => e.ApellidoP);
                    break;
                case "APE_PATERNO_desc":
                    vModel = vModel.OrderByDescending(e => e.ApellidoP);
                    break;
                case "APE_MATERNO":
                    vModel = vModel.OrderBy(e => e.ApellidoM);
                    break;
                case "APE_MATERNO_desc":
                    vModel = vModel.OrderByDescending(e => e.ApellidoM);
                    break;
                case "NOMBRE":
                    vModel = vModel.OrderBy(e => e.Nombres);
                    break;
                case "NOMBRE_desc":
                    vModel = vModel.OrderByDescending(e => e.Nombres);
                    break;
                default:
                    vModel = vModel.OrderBy(e => e.NumeroEmpleado);
                    break;
            }

            return vModel.ToPagedList(iPagina, iPerPage);
        }

        private IQueryable<PLU_OP_Empleados> ObtenerEmpleados()
        {
            return _db.PLU_OP_Empleados.Where(x => x.Activo == true);
        }


        public ResponseModel Edit(PLU_OP_Adscripcion adscripcion)
        {
            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    // Guardar el activo en la base de datos
                    ctx.Entry(adscripcion).State = EntityState.Modified;
                    ctx.SaveChanges();
                    rm.SetResponse(true);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, "Error al editar adscripición: " + ex.Message);
            }

            return rm;
        }
    }
}