using ActivosFijos.Models;
using ActivosFijos.Models.ViewModels;
using PagedList;
using System;
using System.Data.Entity;
using System.Linq;

namespace ActivosFijos.Helpers
{
    public class InventarioHelper
    {
        ModelContext _db = new ModelContext();
        public IPagedList<PLU_OP_Activos> GetAll(int numemp, string numeroinventario, string descripcion, int IdCategoria, int IdMarca, int iPagina = 1, int iPerPage = 10)
        {
            
            var vModel = ObtenerActivos(numemp);

            vModel = vModel.Where(a => (numeroinventario.Length == 0 || a.NumeroInventario.Contains(numeroinventario))
                && (descripcion.Length == 0 || a.Descripcion.Contains(descripcion))
                && (IdCategoria == 0 || a.IdCategoria == IdCategoria)
                && (IdMarca == 0 || a.IdMarca == IdMarca)
                );

            return vModel.ToPagedList(iPagina, iPerPage);

        }

        private IQueryable<PLU_OP_Activos> ObtenerActivos(int numemp)
        {
            int currentYear = DateTime.Now.Year;

            return _db.PLU_OP_Activos
                .Where(a => a.PLU_OP_Resguardo.NumeroEmpleado == numemp && (!_db.PLU_OP_InventarioFisico.Any(i => i.IdActivo == a.IdActivos && i.FechaInventario.Year == currentYear && i.Activo)))
                .OrderBy(a => a.IdActivos);
        }


        public IPagedList<InventariosViewModel> GetAllInventarios(int IPagina, int iPerPage, string Nombres, string Adscripcion, DateTime? Fechainventario)
        {
            // Normaliza entradas
            Nombres = string.IsNullOrWhiteSpace(Nombres) ? null : Nombres.Trim();
            Adscripcion = string.IsNullOrWhiteSpace(Adscripcion) ? null : Adscripcion.Trim();

            // Subconsulta: última adscripción por empleado (IdEmpleado, FechaMax)
            var ultAds = from ad in _db.PLU_OP_Adscripcion
                         group ad by ad.IdEmpleado into g
                         select new { IdEmpleado = g.Key, FechaMax = g.Max(x => x.FechaInicioAdscripcion) };

            // Base con JOINs explícitos (evita Any+nav)
            var baseQ =
                from inv in _db.PLU_OP_InventarioFisico.AsNoTracking()
                join act in _db.PLU_OP_Activos on inv.IdActivo equals act.IdActivos
                join res in _db.PLU_OP_Resguardo on act.IdResguardo equals res.IdResguardo
                join emp in _db.PLU_OP_Empleados on res.IdEmpleado equals emp.IdEmpleado
                join ua in ultAds on emp.IdEmpleado equals ua.IdEmpleado into jua
                from ua in jua.DefaultIfEmpty()
                join ad in _db.PLU_OP_Adscripcion
                     on new { emp.IdEmpleado, Fecha = ua.FechaMax }
                     equals new { ad.IdEmpleado, Fecha = ad.FechaInicioAdscripcion } into jad
                from ad in jad.DefaultIfEmpty()
                select new
                {
                    inv.FolioInventario,
                    inv.FechaInventario,
                    inv.Activo,
                    emp.NumeroEmpleado,
                    emp.NombreCompleto,
                    Corporacion = ad.Corporacion
                };

            // FILTROS antes del GroupBy
            if (Fechainventario.HasValue)
            {
                var d = Fechainventario.Value.Date;
                var d2 = d.AddDays(1);
                baseQ = baseQ.Where(x => x.FechaInventario >= d && x.FechaInventario < d2);
            }
            if (Nombres != null)
            {
                // Opción 1 (mejor con Full-Text): CONTAINS
                // Opción 2 (si aceptable): StartsWith para índice
                baseQ = baseQ.Where(x => x.NombreCompleto.StartsWith(Nombres));
                // Si necesitas "contiene": x.NombreCompleto.Contains(Nombres) → considera Full-Text
            }
            if (Adscripcion != null)
            {
                baseQ = baseQ.Where(x => x.Corporacion.StartsWith(Adscripcion));
            }

            // Ahora sí, agrupa y proyecta
            var vModel =
                from x in baseQ
                group x by x.FolioInventario into g
                orderby g.Key, g.Min(y => y.FechaInventario)
                select new InventariosViewModel
                {
                    FolioInventario = g.Key,
                    NumeroEmpleado = g.Select(y => y.NumeroEmpleado).FirstOrDefault(),
                    NombreCompleto = g.Select(y => y.NombreCompleto).FirstOrDefault(),
                    Adscripcion = g.Select(y => y.Corporacion).FirstOrDefault(),
                    FechaInventario = g.Min(y => y.FechaInventario),
                    TotalActivos = g.Count(),
                    Encontrados = g.Count(y => y.Activo),
                    Pendientes = g.Count(y => !y.Activo)
                };

            return vModel.ToPagedList(IPagina, iPerPage);
        }



    }
}