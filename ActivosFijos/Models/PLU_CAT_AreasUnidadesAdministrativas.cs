namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CAT_AreasUnidadesAdministrativas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_CAT_AreasUnidadesAdministrativas()
        {
            PLU_OP_Activos = new HashSet<PLU_OP_Activos>();
        }

        [Key]
        public int IdAreaUnidadAdministrativa { get; set; }

        public int? IdUnidadAdministrativa { get; set; }

        public string NombreArea { get; set; }

        public bool? Activo { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public virtual PLU_CAT_UnidadesAdministrativas PLU_CAT_UnidadesAdministrativas { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_Activos> PLU_OP_Activos { get; set; }
    }
}
