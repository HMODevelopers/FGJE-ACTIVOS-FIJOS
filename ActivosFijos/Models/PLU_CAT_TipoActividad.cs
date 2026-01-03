namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CAT_TipoActividad
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_CAT_TipoActividad()
        {
            PLU_LOG_Actividades = new HashSet<PLU_LOG_Actividades>();
        }

        [Key]
        public int IdTipoActividad { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreActividad { get; set; }

        [Column(TypeName = "text")]
        public string Descripcion { get; set; }

        public bool? Activo { get; set; }

        public DateTime? FechaCreacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_LOG_Actividades> PLU_LOG_Actividades { get; set; }
    }
}
