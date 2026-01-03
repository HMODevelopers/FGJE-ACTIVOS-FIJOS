namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CAT_MarcaActivo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_CAT_MarcaActivo()
        {
            PLU_OP_Activos = new HashSet<PLU_OP_Activos>();
        }

        [Key]
        public int IdMarca { get; set; }

        [Required]
        [StringLength(255)]
        public string NombreMarca { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_Activos> PLU_OP_Activos { get; set; }
    }
}
