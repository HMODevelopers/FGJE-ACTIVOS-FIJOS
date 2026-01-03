namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CAT_Clasificadores
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_CAT_Clasificadores()
        {
            PLU_OP_Activos = new HashSet<PLU_OP_Activos>();
        }

        [Key]
        public int IdClasificadores { get; set; }

        public int? IdPartEspeci { get; set; }

        public int? IdCapitulo { get; set; }

        public int? IdConcepto { get; set; }

        public int? IdPartidaGenerica { get; set; }

        public int? idPartidaEspecifica { get; set; }

        public int? IdClasificador { get; set; }

        [StringLength(100)]
        public string ClasificadorDescripcion { get; set; }

        public bool? Activo { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public virtual PLU_CAT_Conceptos PLU_CAT_Conceptos { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_Activos> PLU_OP_Activos { get; set; }
    }
}
