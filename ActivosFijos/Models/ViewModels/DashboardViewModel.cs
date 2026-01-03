using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActivosFijos.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int ActivosEnAlmacen { get; set; }
        public int ActivosEnResguardo { get; set; }
        public int ActivosDadosDeBaja { get; set; }
        public int TotalActivos { get; set; }  // Total de activos
    }
}