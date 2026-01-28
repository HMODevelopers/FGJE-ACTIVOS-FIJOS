namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CONF_Usuario
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_CONF_Usuario()
        {
            PLU_LOG_Actividades = new HashSet<PLU_LOG_Actividades>();
            PLU_OP_AltasActivos = new HashSet<PLU_OP_AltasActivos>();
            PLU_OP_BajasActivos = new HashSet<PLU_OP_BajasActivos>();
            PLU_OP_CambiosActivos = new HashSet<PLU_OP_CambiosActivos>();
            PLU_OP_InventarioFisico = new HashSet<PLU_OP_InventarioFisico>();
        }

        [Key]
        public int IdUsuario { get; set; }

        [Required]
        [DisplayName("Rol")]
        public int IdRol { get; set; }

        [Required]
        [StringLength(100)]
        [DisplayName("Nombre Usuario")]
        public string Username { get; set; }

        [Required]
        [DisplayName("Password")]
        public string Pass { get; set; }

        [DisplayName("Requiere cambio de contraseña")]
        public bool ForcePasswordChange { get; set; }

        [Required]
        [StringLength(100)]
        [DisplayName("Nombres")]
        public string Nombres { get; set; }

        [Required]
        [StringLength(100)]
        [DisplayName("Apellidos")]
        public string Apellidos { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual PLU_CAT_Roles PLU_CAT_Roles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_LOG_Actividades> PLU_LOG_Actividades { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_AltasActivos> PLU_OP_AltasActivos { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_BajasActivos> PLU_OP_BajasActivos { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_CambiosActivos> PLU_OP_CambiosActivos { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_InventarioFisico> PLU_OP_InventarioFisico { get; set; }
    }
}
