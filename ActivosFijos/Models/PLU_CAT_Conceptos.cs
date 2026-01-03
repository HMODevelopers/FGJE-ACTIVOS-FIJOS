namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CAT_Conceptos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_CAT_Conceptos()
        {
            PLU_CAT_Clasificadores = new HashSet<PLU_CAT_Clasificadores>();
            PLU_OP_Activos = new HashSet<PLU_OP_Activos>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdConceptos { get; set; }

        public int? IdCapitulo { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Concepto")]
        public int IdConcepto { get; set; }

        [StringLength(100)]
        [Required]
        [DisplayName("Nombre Concepto")]
        public string NombreConcepto { get; set; }

        public bool Activo { get; set; }

        public DateTime? FechaCreacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_CAT_Clasificadores> PLU_CAT_Clasificadores { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_Activos> PLU_OP_Activos { get; set; }
    }
}
