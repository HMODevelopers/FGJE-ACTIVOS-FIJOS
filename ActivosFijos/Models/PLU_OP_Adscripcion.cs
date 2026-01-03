namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_OP_Adscripcion
    {
        [Key]
        public int IdAdscripcion { get; set; }

        public int IdEmpleado { get; set; }

        public int NumeroEmpleado { get; set; }

        [Required]
        [StringLength(255)]
        public string Corporacion { get; set; }

        [Required]
        [StringLength(255)]
        public string Area { get; set; }


        public DateTime Ingreso { get; set; }

        [Required]
        [StringLength(255)]
        public string PuestoFuncional { get; set; }

        [Required]
        [StringLength(255)]
        public string RangoCategoria { get; set; }

        [Required]
        [StringLength(255)]
        public string Entidad { get; set; }

        [Required]
        public int Municipio { get; set; }

        public DateTime FechaInicioAdscripcion { get; set; }

        [Required]
        [StringLength(255)]
        public string TipoMovimiento { get; set; }

        public DateTime FechaRegistro { get; set; }

        public bool MigradodRH { get; set; }

        public virtual PLU_CAT_Municipios PLU_CAT_Municipios { get; set; }

        public virtual PLU_OP_Empleados PLU_OP_Empleados { get; set; }
    }
}
