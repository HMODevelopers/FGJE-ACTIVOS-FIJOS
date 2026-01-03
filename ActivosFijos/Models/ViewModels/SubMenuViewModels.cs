using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActivosFijos.Models.ViewModels
{
    public class SubMenuViewModels
    {
        public int IdSubMenu { get; set; }
        public int IdMenu { get; set; } // ID del menú al que pertenece este submenú
        public string TituloSubMenu { get; set; }
        public string Controlador { get; set; }
        public string Accion { get; set; }
        public decimal Orden { get; set; }
        public bool PermisoVisualizacion { get; set; }
        public bool PermisoEspecial { get; set; }
    }
}