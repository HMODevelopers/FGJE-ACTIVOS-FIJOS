using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActivosFijos.Models.ViewModels
{
    public class ReporteInventario2025ViewModel
    {
        public string FolioInventario { get; set; }
        public string NombreEmpleado { get; set; }
        public string Adscripcion { get; set; }
        public string Municipio { get; set; }
        public DateTime FechaInventario { get; set; }
        public int TotalActivos { get; set; }
        public int Encontrados { get; set; }
        public int Pendientes { get; set; }
    }
}