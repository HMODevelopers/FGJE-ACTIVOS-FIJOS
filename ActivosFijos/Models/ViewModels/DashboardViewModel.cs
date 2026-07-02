using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace ActivosFijos.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int ActivosEnAlmacen { get; set; }
        public int ActivosEnResguardo { get; set; }
        public int ActivosDadosDeBaja { get; set; }
        public int TotalActivos { get; set; }  // Total de activos
        public int AnioSeguimiento { get; set; }
        public List<DashboardTrimestreViewModel> SeguimientoTrimestral { get; set; }
        public IPagedList<EmpleadoSinInventarioHistoricoViewModel> EmpleadosSinInventarioHistorico { get; set; }
    }

    public class DashboardTrimestreViewModel
    {
        public int Trimestre { get; set; }
        public string NombreTrimestre { get; set; }
        public string Periodo { get; set; }
        public int Meta { get; set; }
        public int FoliosRealizados { get; set; }
        public int EncontradosUnicosValidos { get; set; }
        public int PendientesDepurados { get; set; }
        public int EmpleadosVisitadosUnicos { get; set; }
        public decimal PorcentajeAvance { get; set; }
        public int Diferencia { get; set; }
        public string Cumplimiento { get; set; }
        public bool CumpleMeta { get; set; }
    }

    public class MetaTrimestralInventario
    {
        public int Trimestre { get; set; }
        public string Nombre { get; set; }
        public string Periodo { get; set; }
        public int Meta { get; set; }
    }

    public class EmpleadoSinInventarioHistoricoViewModel
    {
        public int IdEmpleado { get; set; }
        public string NumeroEmpleado { get; set; }
        public string NombreCompleto { get; set; }
        public string Area { get; set; }
        public string Corporacion { get; set; }
        public string Entidad { get; set; }
        public string PuestoFuncional { get; set; }
        public string Municipio { get; set; }
        public int TotalActivosAsignados { get; set; }
        public int TotalResguardos { get; set; }
        public DateTime? FechaPrimerActivoAsignado { get; set; }
        public string Estatus { get; set; }
    }
}