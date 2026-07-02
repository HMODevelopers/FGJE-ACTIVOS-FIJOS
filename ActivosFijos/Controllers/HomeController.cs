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

            int anioSeguimiento = DateTime.Now.Year;

            // Pasar los datos al modelo de la vista
            var model = new DashboardViewModel
            {
                ActivosEnAlmacen = enAlmacen,
                ActivosEnResguardo = enResguardo,
                ActivosDadosDeBaja = dadosDeBaja,
                TotalActivos = totalActivos,
                AnioSeguimiento = anioSeguimiento,
                SeguimientoTrimestral = ObtenerSeguimientoTrimestral(anioSeguimiento),
                EmpleadosSinInventarioHistorico = ObtenerEmpleadosSinInventarioHistorico()
            };

            return View(model);
        }
        private List<DashboardTrimestreViewModel> ObtenerSeguimientoTrimestral(int anio)
        {
            var inicioAnio = new DateTime(anio, 1, 1);
            var finAnio = inicioAnio.AddYears(1);
            var trimestreActual = DateTime.Now.Year == anio ? ((DateTime.Now.Month - 1) / 3) + 1 : 0;
            var activosEsperados = _db.PLU_OP_Activos
                .AsNoTracking()
                .Count(a => a.Activo && a.IdEstatusActivo == 2 && a.IdResguardo != null);

            var inventariosPorTrimestre = _db.PLU_OP_InventarioFisico
                .AsNoTracking()
                .Where(i => i.Activo && i.FechaInventario >= inicioAnio && i.FechaInventario < finAnio)
                .GroupBy(i => ((i.FechaInventario.Month - 1) / 3) + 1)
                .Select(g => new
                {
                    Trimestre = g.Key,
                    TotalInventariosRealizados = g.Count(),
                    ActivosInventariadosUnicos = g.Select(i => i.IdActivo).Distinct().Count()
                })
                .ToList();

            var empleadosVisitadosPorTrimestre = (from inventario in _db.PLU_OP_InventarioFisico.AsNoTracking()
                                                   join activo in _db.PLU_OP_Activos.AsNoTracking() on inventario.IdActivo equals activo.IdActivos
                                                   join resguardo in _db.PLU_OP_Resguardo.AsNoTracking() on activo.IdResguardo equals (int?)resguardo.IdResguardo
                                                   where inventario.Activo
                                                       && inventario.FechaInventario >= inicioAnio
                                                       && inventario.FechaInventario < finAnio
                                                       && activo.IdResguardo != null
                                                   group resguardo by ((inventario.FechaInventario.Month - 1) / 3) + 1 into trimestre
                                                   select new
                                                   {
                                                       Trimestre = trimestre.Key,
                                                       EmpleadosVisitados = trimestre.Select(r => r.IdEmpleado).Distinct().Count()
                                                   })
                .ToList();

            var trimestres = new[]
            {
                new { Numero = 1, Nombre = "T1", Periodo = "Enero - Marzo" },
                new { Numero = 2, Nombre = "T2", Periodo = "Abril - Junio" },
                new { Numero = 3, Nombre = "T3", Periodo = "Julio - Septiembre" },
                new { Numero = 4, Nombre = "T4", Periodo = "Octubre - Diciembre" }
            };

            return trimestres.Select(t =>
            {
                var inventario = inventariosPorTrimestre.FirstOrDefault(i => i.Trimestre == t.Numero);
                var empleados = empleadosVisitadosPorTrimestre.FirstOrDefault(e => e.Trimestre == t.Numero);
                var activosInventariados = inventario != null ? inventario.ActivosInventariadosUnicos : 0;
                var porcentaje = activosEsperados == 0 ? 0 : Math.Round((decimal)activosInventariados * 100 / activosEsperados, 2);

                return new DashboardTrimestreViewModel
                {
                    Trimestre = t.Numero,
                    NombreTrimestre = t.Nombre,
                    Periodo = t.Periodo,
                    TotalInventariosRealizados = inventario != null ? inventario.TotalInventariosRealizados : 0,
                    ActivosInventariadosUnicos = activosInventariados,
                    EmpleadosVisitados = empleados != null ? empleados.EmpleadosVisitados : 0,
                    ActivosEsperados = activosEsperados,
                    PorcentajeAvance = porcentaje,
                    Estatus = ObtenerEstatusTrimestre(porcentaje, t.Numero, trimestreActual)
                };
            }).ToList();
        }

        private string ObtenerEstatusTrimestre(decimal porcentajeAvance, int trimestre, int trimestreActual)
        {
            if (porcentajeAvance == 0)
            {
                return "Sin iniciar";
            }

            if (porcentajeAvance >= 100)
            {
                return "Completo";
            }

            if (trimestreActual > 0 && trimestre < trimestreActual)
            {
                return "Con pendientes";
            }

            if (trimestreActual == 0 || trimestre == trimestreActual)
            {
                return "En proceso";
            }

            return "Sin iniciar";
        }

        private List<EmpleadoSinInventarioHistoricoViewModel> ObtenerEmpleadosSinInventarioHistorico()
        {
            var empleadosBase = (from activo in _db.PLU_OP_Activos.AsNoTracking()
                                 join resguardo in _db.PLU_OP_Resguardo.AsNoTracking() on activo.IdResguardo equals (int?)resguardo.IdResguardo
                                 join empleado in _db.PLU_OP_Empleados.AsNoTracking() on resguardo.IdEmpleado equals empleado.IdEmpleado
                                 where activo.Activo
                                     && activo.IdEstatusActivo == 2
                                     && activo.IdResguardo != null
                                     && resguardo.Activo
                                     && empleado.Activo
                                 group new { activo, resguardo, empleado } by new
                                 {
                                     empleado.IdEmpleado,
                                     empleado.NumeroEmpleado,
                                     empleado.NombreCompleto
                                 } into grupo
                                 let totalActivosConInventarioHistorico = grupo.Count(x => _db.PLU_OP_InventarioFisico.Any(i => i.Activo && i.IdActivo == x.activo.IdActivos))
                                 where grupo.Count() > 0 && totalActivosConInventarioHistorico == 0
                                 orderby grupo.Count() descending, grupo.Key.NombreCompleto
                                 select new
                                 {
                                     grupo.Key.IdEmpleado,
                                     grupo.Key.NumeroEmpleado,
                                     grupo.Key.NombreCompleto,
                                     TotalActivosAsignados = grupo.Count(),
                                     TotalActivosConInventarioHistorico = totalActivosConInventarioHistorico,
                                     TotalResguardos = grupo.Select(x => x.resguardo.IdResguardo).Distinct().Count(),
                                     FechaPrimerActivoAsignado = grupo.Min(x => (DateTime?)x.activo.FechaCreacion)
                                 })
                .Take(20)
                .ToList();

            var idsEmpleado = empleadosBase.Select(e => e.IdEmpleado).ToList();
            var adscripciones = _db.PLU_OP_Adscripcion
                .AsNoTracking()
                .Where(a => idsEmpleado.Contains(a.IdEmpleado))
                .OrderByDescending(a => a.FechaInicioAdscripcion)
                .ThenByDescending(a => a.FechaRegistro)
                .ThenByDescending(a => a.IdAdscripcion)
                .ToList()
                .GroupBy(a => a.IdEmpleado)
                .ToDictionary(g => g.Key, g => g.First());

            return empleadosBase.Select(e =>
            {
                PLU_OP_Adscripcion adscripcion;
                adscripciones.TryGetValue(e.IdEmpleado, out adscripcion);

                return new EmpleadoSinInventarioHistoricoViewModel
                {
                    IdEmpleado = e.IdEmpleado,
                    NumeroEmpleado = e.NumeroEmpleado.HasValue ? e.NumeroEmpleado.Value.ToString() : string.Empty,
                    NombreCompleto = e.NombreCompleto,
                    Area = adscripcion != null ? adscripcion.Area : "Sin adscripción",
                    Corporacion = adscripcion != null ? adscripcion.Corporacion : string.Empty,
                    Entidad = adscripcion != null ? adscripcion.Entidad : string.Empty,
                    PuestoFuncional = adscripcion != null ? adscripcion.PuestoFuncional : string.Empty,
                    TotalActivosAsignados = e.TotalActivosAsignados,
                    TotalResguardos = e.TotalResguardos,
                    FechaPrimerActivoAsignado = e.FechaPrimerActivoAsignado,
                    Estatus = "Sin inventario histórico"
                };
            }).ToList();
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