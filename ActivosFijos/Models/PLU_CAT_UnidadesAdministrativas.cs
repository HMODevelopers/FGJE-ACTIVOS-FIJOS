namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CAT_UnidadesAdministrativas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_CAT_UnidadesAdministrativas()
        {
            PLU_CAT_AreasUnidadesAdministrativas = new HashSet<PLU_CAT_AreasUnidadesAdministrativas>();
        }

        [Key]
        public int IdUnidadAdministrativa { get; set; }

        public string UnidadAdministrativa { get; set; }

        public bool? Activo { get; set; }

        public DateTime? FechaCreacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_CAT_AreasUnidadesAdministrativas> PLU_CAT_AreasUnidadesAdministrativas { get; set; }
    }
}
