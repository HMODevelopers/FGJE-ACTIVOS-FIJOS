namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_OP_OficiosCambios
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_OP_OficiosCambios()
        {
            PLU_OP_CambiosActivos = new HashSet<PLU_OP_CambiosActivos>();
        }

        [Key]
        public int IdOficioCambio { get; set; }

        [Required]
        [StringLength(255)]
        public string FolioOficio { get; set; }

        [Required]
        [StringLength(255)]
        public string RutaOficio { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_CambiosActivos> PLU_OP_CambiosActivos { get; set; }
    }
}
