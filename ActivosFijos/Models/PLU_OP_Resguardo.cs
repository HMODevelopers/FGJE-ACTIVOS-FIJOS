namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_OP_Resguardo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_OP_Resguardo()
        {
            PLU_OP_Activos = new HashSet<PLU_OP_Activos>();
        }

        [Key]
        public int IdResguardo { get; set; }

        public int IdEmpleado { get; set; }

        public int NumeroEmpleado { get; set; }

        public int NumeroResguardo { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_Activos> PLU_OP_Activos { get; set; }

        public virtual PLU_OP_Empleados PLU_OP_Empleados { get; set; }
    }
}
