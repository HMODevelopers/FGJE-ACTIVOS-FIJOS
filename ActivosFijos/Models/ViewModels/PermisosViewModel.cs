using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActivosFijos.Models.ViewModels
{
    public class PermisosViewModel
    {
        public int IdPermiso { get; set; }
        public int IdRol { get; set; }
        public int? IdMenu { get; set; }
        public int? IdSubMenu { get; set; }
        public bool PermisoVisualizacion { get; set; }
        public bool PermisoEspecial { get; set; }
        public string Titulo { get; set; }
    }


}