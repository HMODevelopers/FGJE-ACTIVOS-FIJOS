namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CAT_Roles
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_CAT_Roles()
        {
            PLU_CONF_PermisosMenu = new HashSet<PLU_CONF_PermisosMenu>();
            PLU_CONF_Usuario = new HashSet<PLU_CONF_Usuario>();
        }

        [Key]
        public int IdRol { get; set; }

        [Required]
        [StringLength(255)]
        public string NombreRol { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_CONF_PermisosMenu> PLU_CONF_PermisosMenu { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_CONF_Usuario> PLU_CONF_Usuario { get; set; }
    }
}
