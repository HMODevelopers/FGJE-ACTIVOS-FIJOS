namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CONF_SubMenu
    {
        [Key]
        public int IdSubMenu { get; set; }

        public int IdMenu { get; set; }

        [Required]
        [StringLength(255)]
        public string TituloSubMenu { get; set; }

        [Required]
        [StringLength(50)]
        public string Controlador { get; set; }

        [Required]
        [StringLength(50)]
        public string Accion { get; set; }

        public decimal Orden { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual PLU_CONF_Menu PLU_CONF_Menu { get; set; }
    }
}
