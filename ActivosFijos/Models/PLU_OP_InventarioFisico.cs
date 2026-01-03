namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_OP_InventarioFisico
    {
        [Key]
        public int IdInventario { get; set; }

        public string FolioInventario { get; set; }
        public int IdActivo { get; set; }

        public int IdUsuario { get; set; }

        public int NumeroEmpleado { get; set; }

        [Column(TypeName = "date")]
        public DateTime FechaInventario { get; set; }

        public string Observacion { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual PLU_CONF_Usuario PLU_CONF_Usuario { get; set; }

        public virtual PLU_OP_Activos PLU_OP_Activos { get; set; }
    }
}
