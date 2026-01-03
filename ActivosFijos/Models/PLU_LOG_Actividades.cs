namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_LOG_Actividades
    {
        [Key]
        public int IdLogActividad { get; set; }

        public int IdUsuario { get; set; }

        public int IdTipoActividad { get; set; }

        [Column(TypeName = "text")]
        public string ValorAnterior { get; set; }

        [Column(TypeName = "text")]
        public string ValorNuevo { get; set; }

        public DateTime? timestamp { get; set; }

        public bool? Activo { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public virtual PLU_CAT_TipoActividad PLU_CAT_TipoActividad { get; set; }

        public virtual PLU_CONF_Usuario PLU_CONF_Usuario { get; set; }
    }
}
