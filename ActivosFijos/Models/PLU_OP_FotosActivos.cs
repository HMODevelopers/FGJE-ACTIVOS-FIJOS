namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_OP_FotosActivos
    {
        [Key]
        public int IdFoto { get; set; }

        public int IdActivos { get; set; }

        [Required]
        public string RutaFoto { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual PLU_OP_Activos PLU_OP_Activos { get; set; }
    }
}
