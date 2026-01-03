namespace ActivosFijos.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    

    public partial class PLU_OP_CambiosActivos
    {
        [Key]
        public int IdCambioActivo { get; set; }

        public string FolioCambio { get; set; }

        public int IdActivos { get; set; }

        public int? IdOficioCambio { get; set; }
        public int IdEmpleadoActual { get; set; }
        public int IdEmpleadoAnterior { get; set; }

        public int IdUsuario { get; set; }

        public bool Inventario { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual PLU_CONF_Usuario PLU_CONF_Usuario { get; set; }

        public virtual PLU_OP_Activos PLU_OP_Activos { get; set; }

        public virtual PLU_OP_OficiosCambios PLU_OP_OficiosCambios { get; set; }
    }
}
