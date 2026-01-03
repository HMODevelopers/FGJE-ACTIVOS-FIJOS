namespace ActivosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PLU_OP_Activos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLU_OP_Activos()
        {
            PLU_OP_AltasActivos = new HashSet<PLU_OP_AltasActivos>();
            PLU_OP_BajasActivos = new HashSet<PLU_OP_BajasActivos>();
            PLU_OP_CambiosActivos = new HashSet<PLU_OP_CambiosActivos>();
            PLU_OP_FotosActivos = new HashSet<PLU_OP_FotosActivos>();
            PLU_OP_InventarioFisico = new HashSet<PLU_OP_InventarioFisico>();
        }

        [Key]
        public int IdActivos { get; set; }

        public int? IdResguardo { get; set; }

        public int? IdCategoria { get; set; }

        public int? IdMarca { get; set; }

        public int? IdProveedor { get; set; }

        public int? IdFactura { get; set; }

        public int? IdConcepto { get; set; }

        public int? IdClasificadores { get; set; }

        public int? IdUnidadAdministrativa { get; set; }

        public int? IdAreaUnidadAdministrativa { get; set; }

        public int? IdMunicipio { get; set; }

        public int? IdRecurso { get; set; }

        public int? IdAlmacen { get; set; }

        public int? IdEstadoFisico { get; set; }

        public int? IdEstatusActivo { get; set; }


        [StringLength(255)]
        [Required]
        [DisplayName("Numero Inventario")]
        public string NumeroInventario { get; set; }

        public int? NumeroResguardo { get; set; }

        public int? NumeroEmpleado { get; set; }

        //[StringLength(255)]
        //public string NombreEmpleado { get; set; }

        //public int? EmpleadoAnterior { get; set; }

        [Required]
        [DisplayName("Descripción")]
        public string Descripcion { get; set; }

        [DisplayName("Numero Serie")]
        public string NumeroSerie { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual PLU_CAT_Almacenes PLU_CAT_Almacenes { get; set; }

        public virtual PLU_CAT_AreasUnidadesAdministrativas PLU_CAT_AreasUnidadesAdministrativas { get; set; }

        public virtual PLU_CAT_CategoriaActivo PLU_CAT_CategoriaActivo { get; set; }

        public virtual PLU_CAT_Clasificadores PLU_CAT_Clasificadores { get; set; }

        public virtual PLU_CAT_Conceptos PLU_CAT_Conceptos { get; set; }

        public virtual PLU_CAT_EstadoFisicoActivo PLU_CAT_EstadoFisicoActivo { get; set; }

        public virtual PLU_CAT_EstatusActivo PLU_CAT_EstatusActivo { get; set; }

        public virtual PLU_CAT_Facturas PLU_CAT_Facturas { get; set; }

        public virtual PLU_CAT_MarcaActivo PLU_CAT_MarcaActivo { get; set; }

        public virtual PLU_CAT_Municipios PLU_CAT_Municipios { get; set; }

        public virtual PLU_CAT_Proveedores PLU_CAT_Proveedores { get; set; }

        public virtual PLU_CAT_Recurso PLU_CAT_Recurso { get; set; }

        public virtual PLU_OP_Resguardo PLU_OP_Resguardo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_AltasActivos> PLU_OP_AltasActivos { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_BajasActivos> PLU_OP_BajasActivos { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_CambiosActivos> PLU_OP_CambiosActivos { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_FotosActivos> PLU_OP_FotosActivos { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PLU_OP_InventarioFisico> PLU_OP_InventarioFisico { get; set; }
    }
}
