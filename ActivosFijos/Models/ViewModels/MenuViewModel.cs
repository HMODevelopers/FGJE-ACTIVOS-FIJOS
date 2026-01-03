using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActivosFijos.Models.ViewModels
{
    public class MenuViewModel
    {
        public int IdMenu { get; set; }
        public string TituloMenu { get; set; }
        public string Icono { get; set; }
        public decimal Orden { get; set; }
        public bool PermisoVisualizacion { get; set; }
        public bool PermisoEspecial { get; set; }
        public List<SubMenuViewModels> SubMenus { get; set; }
    }
}