using ActivosFijos.Models;
using ActivosFijos.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using static EDUES_ADMIN.Filters.AdminFilters;

namespace ActivosFijos.Controllers
{
    [Autenticado]
    public class HomeController : Controller
    {
        public ModelContext _db = new ModelContext();
        public ActionResult Index(string numeroEmpleadoSinInventario = "", string nombreCompletoSinInventario = "", string corporacionSinInventario = "", string areaSinInventario = "", string puestoFuncionalSinInventario = "", string municipioSinInventario = "", int iPaginaSinInventario = 1, int iPerPageSinInventario = 25)
        {
            // Filtrar activos según su estatus
            var enAlmacen = _db.PLU_OP_Activos.Count(a => a.PLU_CAT_EstatusActivo.Descripcion == "En Almacén");
            var enResguardo = _db.PLU_OP_Activos.Count(a => a.PLU_CAT_EstatusActivo.Descripcion == "En Resguardo");
            var dadosDeBaja = _db.PLU_OP_Activos.Count(a => a.PLU_CAT_EstatusActivo.Descripcion == "Baja");

            // Total de activos
            var totalActivos = enAlmacen + enResguardo + dadosDeBaja;

            int anioSeguimiento = DateTime.Now.Year;

            if (iPaginaSinInventario < 1) iPaginaSinInventario = 1;
            if (iPerPageSinInventario < 1) iPerPageSinInventario = 25;
            ViewBag.NumeroEmpleadoSinInventario = numeroEmpleadoSinInventario;
            ViewBag.NombreCompletoSinInventario = nombreCompletoSinInventario;
            ViewBag.CorporacionSinInventario = corporacionSinInventario;
            ViewBag.AreaSinInventario = areaSinInventario;
            ViewBag.PuestoFuncionalSinInventario = puestoFuncionalSinInventario;
            ViewBag.MunicipioSinInventario = municipioSinInventario;
            ViewBag.PaginaSinInventario = iPaginaSinInventario;
            ViewBag.PerPageSinInventario = iPerPageSinInventario;

            // Pasar los datos al modelo de la vista
            var model = new DashboardViewModel
            {
                ActivosEnAlmacen = enAlmacen,
                ActivosEnResguardo = enResguardo,
                ActivosDadosDeBaja = dadosDeBaja,
                TotalActivos = totalActivos,
                AnioSeguimiento = anioSeguimiento,
                SeguimientoTrimestral = ObtenerSeguimientoTrimestral(anioSeguimiento),
                EmpleadosSinInventarioHistorico = ObtenerEmpleadosSinInventarioHistorico(numeroEmpleadoSinInventario, nombreCompletoSinInventario, corporacionSinInventario, areaSinInventario, puestoFuncionalSinInventario, municipioSinInventario, iPaginaSinInventario, iPerPageSinInventario)
            };

            if (Request.IsAjaxRequest())
            {
                return PartialView("_EmpleadosSinInventarioHistorico", model.EmpleadosSinInventarioHistorico);
            }

            return View(model);
        }
        private List<DashboardTrimestreViewModel> ObtenerSeguimientoTrimestral(int anio)
        {
            var inicioAnio = new DateTime(anio, 1, 1);
            var finAnio = inicioAnio.AddYears(1);
            var anioActual = DateTime.Now.Year;
            var trimestreActual = anioActual == anio ? ObtenerTrimestre(DateTime.Now) : 0;

            var detallesInventario = _db.PLU_OP_InventarioFisico
                .AsNoTracking()
                .Where(i => i.FechaInventario >= inicioAnio && i.FechaInventario < finAnio)
                .Select(i => new
                {
                    i.IdInventario,
                    i.FolioInventario,
                    i.IdActivo,
                    i.NumeroEmpleado,
                    i.FechaInventario,
                    i.Activo
                })
                .ToList();

            var foliosPorTrimestre = detallesInventario
                .Where(i => !string.IsNullOrWhiteSpace(i.FolioInventario))
                .GroupBy(i => i.FolioInventario.Trim())
                .Select(g => new
                {
                    Trimestre = ObtenerTrimestre(g.Min(i => i.FechaInventario))
                })
                .GroupBy(f => f.Trimestre)
                .ToDictionary(g => g.Key, g => g.Count());

            var detalleDepuradoPorFolioActivo = detallesInventario
                .Where(i => !string.IsNullOrWhiteSpace(i.FolioInventario))
                .GroupBy(i => new { FolioInventario = i.FolioInventario.Trim(), i.IdActivo })
                .Select(g => new
                {
                    g.Key.FolioInventario,
                    g.Key.IdActivo,
                    FechaGrupo = g.Min(i => i.FechaInventario),
                    IdInventarioOrden = g.Min(i => i.IdInventario),
                    ResultadoEncontrado = g.Any(i => i.Activo)
                })
                .ToList();

            var encontradosPorTrimestre = detalleDepuradoPorFolioActivo
                .Where(i => i.ResultadoEncontrado)
                .GroupBy(i => i.IdActivo)
                .Select(g => g
                    .OrderBy(i => i.FechaGrupo)
                    .ThenBy(i => i.FolioInventario)
                    .ThenBy(i => i.IdInventarioOrden)
                    .First())
                .GroupBy(i => ObtenerTrimestre(i.FechaGrupo))
                .ToDictionary(g => g.Key, g => g.Count());

            var pendientesPorTrimestre = detalleDepuradoPorFolioActivo
                .Where(i => !i.ResultadoEncontrado)
                .GroupBy(i => ObtenerTrimestre(i.FechaGrupo))
                .ToDictionary(g => g.Key, g => g.Count());

            var empleadosPorTrimestre = detallesInventario
                .Where(i => i.NumeroEmpleado > 0)
                .GroupBy(i => i.NumeroEmpleado.ToString().Trim())
                .Select(g => new
                {
                    Trimestre = ObtenerTrimestre(g.Min(i => i.FechaInventario))
                })
                .GroupBy(e => e.Trimestre)
                .ToDictionary(g => g.Key, g => g.Count());

            return ObtenerMetasTrimestralesInventario().Select(metaTrimestral =>
            {
                var foliosRealizados = ObtenerConteoTrimestral(foliosPorTrimestre, metaTrimestral.Trimestre);
                var encontradosUnicosValidos = ObtenerConteoTrimestral(encontradosPorTrimestre, metaTrimestral.Trimestre);
                var pendientesDepurados = ObtenerConteoTrimestral(pendientesPorTrimestre, metaTrimestral.Trimestre);
                var empleadosVisitadosUnicos = ObtenerConteoTrimestral(empleadosPorTrimestre, metaTrimestral.Trimestre);
                var porcentajeAvance = metaTrimestral.Meta == 0
                    ? 0
                    : Math.Round((decimal)encontradosUnicosValidos * 100 / metaTrimestral.Meta, 2);
                var diferencia = encontradosUnicosValidos - metaTrimestral.Meta;

                return new DashboardTrimestreViewModel
                {
                    Trimestre = metaTrimestral.Trimestre,
                    NombreTrimestre = metaTrimestral.Nombre,
                    Periodo = metaTrimestral.Periodo,
                    Meta = metaTrimestral.Meta,
                    FoliosRealizados = foliosRealizados,
                    EncontradosUnicosValidos = encontradosUnicosValidos,
                    PendientesDepurados = pendientesDepurados,
                    EmpleadosVisitadosUnicos = empleadosVisitadosUnicos,
                    PorcentajeAvance = porcentajeAvance,
                    Diferencia = diferencia,
                    CumpleMeta = encontradosUnicosValidos >= metaTrimestral.Meta,
                    Cumplimiento = ObtenerCumplimientoTrimestre(encontradosUnicosValidos, metaTrimestral.Meta, metaTrimestral.Trimestre, anio, anioActual, trimestreActual)
                };
            }).ToList();
        }

        private int ObtenerTrimestre(DateTime fecha)
        {
            return ((fecha.Month - 1) / 3) + 1;
        }

        private int ObtenerConteoTrimestral(Dictionary<int, int> conteosPorTrimestre, int trimestre)
        {
            int conteo;
            return conteosPorTrimestre.TryGetValue(trimestre, out conteo) ? conteo : 0;
        }

        private List<MetaTrimestralInventario> ObtenerMetasTrimestralesInventario()
        {
            return new List<MetaTrimestralInventario>
            {
                new MetaTrimestralInventario { Trimestre = 1, Nombre = "T1", Periodo = "Enero - Marzo", Meta = 7359 },
                new MetaTrimestralInventario { Trimestre = 2, Nombre = "T2", Periodo = "Abril - Junio", Meta = 11038 },
                new MetaTrimestralInventario { Trimestre = 3, Nombre = "T3", Periodo = "Julio - Septiembre", Meta = 11038 },
                new MetaTrimestralInventario { Trimestre = 4, Nombre = "T4", Periodo = "Octubre - Diciembre", Meta = 7359 }
            };
        }

        private string ObtenerCumplimientoTrimestre(int activosUnicosInventariados, int meta, int trimestre, int anio, int anioActual, int trimestreActual)
        {
            if (activosUnicosInventariados >= meta)
            {
                return "Cumple";
            }

            if (anio > anioActual)
            {
                return activosUnicosInventariados == 0 ? "Pendiente de iniciar" : "En proceso";
            }

            if (anio < anioActual)
            {
                return activosUnicosInventariados == 0 ? "Sin actividad" : "No cumple";
            }

            if (trimestre > trimestreActual)
            {
                return activosUnicosInventariados == 0 ? "Pendiente de iniciar" : "En proceso";
            }

            if (trimestre == trimestreActual)
            {
                return activosUnicosInventariados == 0 ? "Sin actividad" : "En proceso";
            }

            return activosUnicosInventariados == 0 ? "Sin actividad" : "No cumple";
        }

        private IPagedList<EmpleadoSinInventarioHistoricoViewModel> ObtenerEmpleadosSinInventarioHistorico(string numeroEmpleado, string nombreCompleto, string corporacion, string area, string puestoFuncional, string municipio, int iPagina, int iPerPage)
        {
            numeroEmpleado = (numeroEmpleado ?? string.Empty).Trim();
            nombreCompleto = (nombreCompleto ?? string.Empty).Trim();
            corporacion = (corporacion ?? string.Empty).Trim();
            area = (area ?? string.Empty).Trim();
            puestoFuncional = (puestoFuncional ?? string.Empty).Trim();
            municipio = (municipio ?? string.Empty).Trim();

            var empleadosBase = from activo in _db.PLU_OP_Activos.AsNoTracking()
                                join resguardo in _db.PLU_OP_Resguardo.AsNoTracking()
                                    on activo.IdResguardo equals (int?)resguardo.IdResguardo
                                join empleado in _db.PLU_OP_Empleados.AsNoTracking()
                                    on resguardo.IdEmpleado equals empleado.IdEmpleado
                                where activo.Activo
                                    && activo.IdEstatusActivo == 2
                                    && activo.IdResguardo != null
                                    && resguardo.Activo
                                    && empleado.Activo
                                    && empleado.NumeroEmpleado.HasValue
                                    && !_db.PLU_OP_InventarioFisico.Any(i => i.NumeroEmpleado == empleado.NumeroEmpleado.Value)
                                group new { activo, resguardo, empleado } by new
                                {
                                    empleado.IdEmpleado,
                                    empleado.NumeroEmpleado,
                                    empleado.NombreCompleto
                                } into grupo
                                select new
                                {
                                    grupo.Key.IdEmpleado,
                                    grupo.Key.NumeroEmpleado,
                                    grupo.Key.NombreCompleto,
                                    TotalActivosAsignados = grupo.Select(x => x.activo.IdActivos).Distinct().Count(),
                                    TotalResguardos = grupo.Select(x => x.resguardo.IdResguardo).Distinct().Count(),
                                    FechaPrimerActivoAsignado = grupo.Min(x => (DateTime?)x.activo.FechaCreacion)
                                };

            var consulta = empleadosBase.Select(e => new EmpleadoSinInventarioHistoricoViewModel
            {
                IdEmpleado = e.IdEmpleado,
                NumeroEmpleado = SqlFunctions.StringConvert((double)e.NumeroEmpleado.Value).Trim(),
                NombreCompleto = e.NombreCompleto,

                Corporacion = _db.PLU_OP_Adscripcion
                    .Where(a => a.IdEmpleado == e.IdEmpleado)
                    .OrderByDescending(a => a.FechaInicioAdscripcion)
                    .ThenByDescending(a => a.FechaRegistro)
                    .ThenByDescending(a => a.IdAdscripcion)
                    .Select(a => a.Corporacion)
                    .FirstOrDefault(),

                Area = _db.PLU_OP_Adscripcion
                    .Where(a => a.IdEmpleado == e.IdEmpleado)
                    .OrderByDescending(a => a.FechaInicioAdscripcion)
                    .ThenByDescending(a => a.FechaRegistro)
                    .ThenByDescending(a => a.IdAdscripcion)
                    .Select(a => a.Area)
                    .FirstOrDefault(),

                PuestoFuncional = _db.PLU_OP_Adscripcion
                    .Where(a => a.IdEmpleado == e.IdEmpleado)
                    .OrderByDescending(a => a.FechaInicioAdscripcion)
                    .ThenByDescending(a => a.FechaRegistro)
                    .ThenByDescending(a => a.IdAdscripcion)
                    .Select(a => a.PuestoFuncional)
                    .FirstOrDefault(),

                Municipio = _db.PLU_OP_Adscripcion
                    .Where(a => a.IdEmpleado == e.IdEmpleado)
                    .OrderByDescending(a => a.FechaInicioAdscripcion)
                    .ThenByDescending(a => a.FechaRegistro)
                    .ThenByDescending(a => a.IdAdscripcion)
                    .Select(a => a.PLU_CAT_Municipios.NombreMunicipio)
                    .FirstOrDefault(),

                TotalActivosAsignados = e.TotalActivosAsignados,
                TotalResguardos = e.TotalResguardos,
                FechaPrimerActivoAsignado = e.FechaPrimerActivoAsignado,
                Estatus = "Sin inventario histórico"
            });

            if (!string.IsNullOrWhiteSpace(numeroEmpleado))
            {
                consulta = consulta.Where(e => e.NumeroEmpleado.Contains(numeroEmpleado));
            }

            if (!string.IsNullOrWhiteSpace(nombreCompleto))
            {
                consulta = consulta.Where(e => e.NombreCompleto.Contains(nombreCompleto));
            }

            if (!string.IsNullOrWhiteSpace(corporacion))
            {
                consulta = consulta.Where(e => e.Corporacion.Contains(corporacion));
            }

            if (!string.IsNullOrWhiteSpace(area))
            {
                consulta = consulta.Where(e => e.Area.Contains(area));
            }

            if (!string.IsNullOrWhiteSpace(puestoFuncional))
            {
                consulta = consulta.Where(e => e.PuestoFuncional.Contains(puestoFuncional));
            }

            if (!string.IsNullOrWhiteSpace(municipio))
            {
                consulta = consulta.Where(e => e.Municipio.Contains(municipio));
            }

            return consulta
                .OrderBy(e => e.Corporacion)
                .ThenByDescending(e => e.TotalActivosAsignados)
                .ThenBy(e => e.NombreCompleto)
                .ToPagedList(iPagina, iPerPage);
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