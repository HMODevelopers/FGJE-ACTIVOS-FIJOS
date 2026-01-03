namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_OP_BajasActivos
    {
        [Key]
        public int IdBaja { get; set; }
        public string FolioBaja { get; set; }   
        public int IdActivos { get; set; }

        public int? IdOficioBaja { get; set; }

        public int? NumeroLote { get; set; }    

        public int IdUsuario { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual PLU_CONF_Usuario PLU_CONF_Usuario { get; set; }

        public virtual PLU_OP_Activos PLU_OP_Activos { get; set; }

        public virtual PLU_OP_OficiosBajas PLU_OP_OficiosBajas { get; set; }
    }
}
