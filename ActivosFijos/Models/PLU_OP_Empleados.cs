namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_OP_Empleados
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_OP_Empleados()
        {
            PLU_OP_Adscripcion = new HashSet<PLU_OP_Adscripcion>();
            PLU_OP_Resguardo = new HashSet<PLU_OP_Resguardo>();
        }

        [Key]
        public int IdEmpleado { get; set; }

        public int? NumeroEmpleado { get; set; }

        [StringLength(100)]
        public string NombreCompleto { get; set; }

        [StringLength(100)]
        public string Nombres { get; set; }

        [StringLength(100)]
        public string ApellidoP { get; set; }

        [StringLength(100)]
        public string ApellidoM { get; set; }

        [StringLength(50)]
        public string Sexo { get; set; }

        [StringLength(50)]
        public string EstatusRH { get; set; }

        public DateTime? FechaUAC { get; set; }

        public DateTime? FechaEstatus { get; set; }

        
        public bool TieneResguardo { get; set; } // Nueva propiedad

        public bool MigradodRH { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_Adscripcion> PLU_OP_Adscripcion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_Resguardo> PLU_OP_Resguardo { get; set; }
    }
}
