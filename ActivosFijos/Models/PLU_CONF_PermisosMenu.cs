namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CONF_PermisosMenu
    {
        [Key]
        public int IdPermiso { get; set; }

        public int IdRol { get; set; }

        public int? IdMenu { get; set; }

        public int? IdSubMenu { get; set; }

        public bool PermisoVisualizacion { get; set; }

        public bool PermisoEspecial { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual PLU_CAT_Roles PLU_CAT_Roles { get; set; }

        public virtual PLU_CONF_Menu PLU_CONF_Menu { get; set; }
    }
}
