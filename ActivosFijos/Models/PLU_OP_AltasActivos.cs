namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_OP_AltasActivos
    {
        [Key]
        public int IdAltaActivos { get; set; }

        public string FolioAlta { get; set; }
        public int IdEmpleado { get; set; }

        public int IdActivos { get; set; }

        public int? IdOficioAlta { get; set; }

        public int IdUsuario { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual PLU_CONF_Usuario PLU_CONF_Usuario { get; set; }

        public virtual PLU_OP_Activos PLU_OP_Activos { get; set; }

        public virtual PLU_OP_OficiosAltas PLU_OP_OficiosAltas { get; set; }
    }
}
