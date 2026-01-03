namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_CAT_Estados
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CLAVE_ESTADO { get; set; }

        [Required]
        [StringLength(50)]
        public string ESTADO { get; set; }

        [Required]
        [StringLength(1)]
        public string HABILITADO { get; set; }
    }
}
