namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CAT_Facturas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_CAT_Facturas()
        {
            PLU_OP_Activos = new HashSet<PLU_OP_Activos>();
        }

        [Key]
        public int IdFactura { get; set; }

        [Required]
        [DisplayName("Proveedor")]
        public int? IdProveedor { get; set; }

        [Required]
        [DisplayName("Recurso")]
        public int? IdRecurso { get; set; }

        [StringLength(100)]
        [Required]
        [DisplayName("Folio Factura")]
        public string FolioFactura { get; set; }

        [Required]
        [DisplayName("Fecha Factura")]
        public DateTime? FechaFactura { get; set; }

        [Required]
        [DisplayName("Total")]
        public decimal? Total { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_Activos> PLU_OP_Activos { get; set; }

        public virtual PLU_CAT_Proveedores PLU_CAT_Proveedores { get; set; }   

        public virtual PLU_CAT_Recurso PLU_CAT_Recursos { get; set; }
    }
}
