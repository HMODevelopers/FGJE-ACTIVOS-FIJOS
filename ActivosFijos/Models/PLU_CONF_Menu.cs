namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CONF_Menu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_CONF_Menu()
        {
            PLU_CONF_PermisosMenu = new HashSet<PLU_CONF_PermisosMenu>();
            PLU_CONF_SubMenu = new HashSet<PLU_CONF_SubMenu>();
        }

        [Key]
        public int IdMenu { get; set; }

        [Required]
        [StringLength(255)]
        public string TituloMenu { get; set; }

        [StringLength(50)]
        public string Icono { get; set; }

        public decimal Orden { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_CONF_PermisosMenu> PLU_CONF_PermisosMenu { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_CONF_SubMenu> PLU_CONF_SubMenu { get; set; }
    }
}
