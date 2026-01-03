using ActivosFijos.Models;
using ActivosFijos.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static EDUES_ADMIN.Filters.AdminFilters;

namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class HomeController : Controller
    {
        public ModelContext _db = new ModelContext();
        public ActionResult Index()
        {
            // Filtrar activos según su estatus
            var enAlmacen = _db.PLU_OP_Activos.Count(a => a.PLU_CAT_EstatusActivo.Descripcion == "En Almacén");
            var enResguardo = _db.PLU_OP_Activos.Count(a => a.PLU_CAT_EstatusActivo.Descripcion == "En Resguardo");
            var dadosDeBaja = _db.PLU_OP_Activos.Count(a => a.PLU_CAT_EstatusActivo.Descripcion == "Baja");

            // Total de activos
            var totalActivos = enAlmacen + enResguardo + dadosDeBaja;

            // Pasar los datos al modelo de la vista
            var model = new DashboardViewModel
            {
                ActivosEnAlmacen = enAlmacen,
                ActivosEnResguardo = enResguardo,
                ActivosDadosDeBaja = dadosDeBaja,
                TotalActivos = totalActivos
            };

            return View(model);
        }
        public JsonResult GetOperacionesPorMes(int? anio)
        {
            // Usa el año actual si no se proporciona un año
            int yearToFilter = anio ?? DateTime.Now.Year;

            var operacionesPorMes = _db.PLU_OP_AltasActivos
                .GroupBy(a => new { a.FechaCreacion.Year, a.FechaCreacion.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Tipo = "Altas", Count = g.Count() })
                .Union(_db.PLU_OP_CambiosActivos
                    .GroupBy(c => new { c.FechaCreacion.Year, c.FechaCreacion.Month })
                    .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Tipo = "Cambios", Count = g.Count() }))
                .Union(_db.PLU_OP_BajasActivos
                    .GroupBy(b => new { b.FechaCreacion.Year, b.FechaCreacion.Month })
                    .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Tipo = "Bajas", Count = g.Count() }))
                .Where(o => o.Year == yearToFilter) // Filtrar por el año seleccionado
                .ToList();

            return Json(operacionesPorMes, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetInventariosPorMes(int? anio)
        {
            // Determinar el año a filtrar: usar el año actual si no se proporciona uno
            int yearToFilter = anio ?? DateTime.Now.Year;

            // Consultar los datos desde la base de datos
            var inventarios = _db.PLU_OP_InventarioFisico
                .Where(i => i.FechaInventario.Year == yearToFilter) // Filtrar por el año dinámico
                .GroupBy(i => new { Año = i.FechaInventario.Year, Mes = i.FechaInventario.Month, i.NumeroEmpleado })
                .Select(g => new
                {
                    Año = g.Key.Año,
                    Mes = g.Key.Mes,
                    Count = g.Select(x => x.IdInventario).Distinct().Count()
                })
                .GroupBy(i => new { i.Año, i.Mes })
                .Select(g => new
                {
                    Año = g.Key.Año,
                    Mes = g.Key.Mes,
                    Count = g.Count()
                })
                .OrderBy(i => i.Año)
                .ThenBy(i => i.Mes)
                .ToList();

            return Json(inventarios, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetInventariosPorUsuario(int? anio)
        {
            // Determinar el año a filtrar: usar el año actual si no se proporciona uno
            int yearToFilter = anio ?? DateTime.Now.Year;

            var result = _db.PLU_OP_InventarioFisico
            .Where(i => i.FechaInventario.Year == yearToFilter) // Filtrar por año
            .GroupBy(i => new { i.IdUsuario, i.NumeroEmpleado, i.FechaInventario })
            .Select(g => new
            {
                UsuarioId = g.Key.IdUsuario,
                NombreCompleto = _db.PLU_CONF_Usuario
                    .Where(u => u.IdUsuario == g.Key.IdUsuario)
                    .Select(u => u.Nombres + " " + u.Apellidos)
                    .FirstOrDefault(),
                InventariosHechos = 1,
                Pendientes = g.Any(i => i.Activo == false)
            })
            .GroupBy(x => new { x.UsuarioId, x.NombreCompleto })
            .Select(g => new
            {
                UsuarioId = g.Key.UsuarioId,
                NombreCompleto = g.Key.NombreCompleto,
                TotalInventarios = g.Count(),
                Pendientes = g.Count(x => x.Pendientes),
                Completos = g.Count() - g.Count(x => x.Pendientes)
            })
            .OrderByDescending(x => x.TotalInventarios) // Ordenar por TotalInventarios de mayor a menor
            .ToList();


            return Json(result, JsonRequestBehavior.AllowGet);
        }


    }
}