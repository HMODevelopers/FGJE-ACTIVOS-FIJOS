using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace ActivosFijos.Models
{
    public partial class ModelContext : DbContext
    {
        public ModelContext()
            : base("name=ModelContext")
        {
        }

        public virtual DbSet<PLU_CAT_Almacenes> PLU_CAT_Almacenes { get; set; }
        public virtual DbSet<PLU_CAT_AreasUnidadesAdministrativas> PLU_CAT_AreasUnidadesAdministrativas { get; set; }
        public virtual DbSet<PLU_CAT_CategoriaActivo> PLU_CAT_CategoriaActivo { get; set; }
        public virtual DbSet<PLU_CAT_Clasificadores> PLU_CAT_Clasificadores { get; set; }
        public virtual DbSet<PLU_CAT_Conceptos> PLU_CAT_Conceptos { get; set; }
        public virtual DbSet<PLU_CAT_EstadoFisicoActivo> PLU_CAT_EstadoFisicoActivo { get; set; }
        public virtual DbSet<PLU_CAT_Estados> PLU_CAT_Estados { get; set; }
        public virtual DbSet<PLU_CAT_EstatusActivo> PLU_CAT_EstatusActivo { get; set; }
        public virtual DbSet<PLU_CAT_Facturas> PLU_CAT_Facturas { get; set; }
        public virtual DbSet<PLU_CAT_MarcaActivo> PLU_CAT_MarcaActivo { get; set; }
        public virtual DbSet<PLU_CAT_Municipios> PLU_CAT_Municipios { get; set; }
        public virtual DbSet<PLU_CAT_Proveedores> PLU_CAT_Proveedores { get; set; }
        public virtual DbSet<PLU_CAT_Recurso> PLU_CAT_Recurso { get; set; }
        public virtual DbSet<PLU_CAT_Roles> PLU_CAT_Roles { get; set; }
        public virtual DbSet<PLU_CAT_TipoActividad> PLU_CAT_TipoActividad { get; set; }
        public virtual DbSet<PLU_CAT_UnidadesAdministrativas> PLU_CAT_UnidadesAdministrativas { get; set; }
        public virtual DbSet<PLU_CONF_Menu> PLU_CONF_Menu { get; set; }
        public virtual DbSet<PLU_CONF_PermisosMenu> PLU_CONF_PermisosMenu { get; set; }
        public virtual DbSet<PLU_CONF_SubMenu> PLU_CONF_SubMenu { get; set; }
        public virtual DbSet<PLU_CONF_Usuario> PLU_CONF_Usuario { get; set; }
        public virtual DbSet<PLU_LOG_Actividades> PLU_LOG_Actividades { get; set; }
        public virtual DbSet<PLU_OP_Activos> PLU_OP_Activos { get; set; }
        public virtual DbSet<PLU_OP_Adscripcion> PLU_OP_Adscripcion { get; set; }
        public virtual DbSet<PLU_OP_AltasActivos> PLU_OP_AltasActivos { get; set; }
        public virtual DbSet<PLU_OP_BajasActivos> PLU_OP_BajasActivos { get; set; }
        public virtual DbSet<PLU_OP_CambiosActivos> PLU_OP_CambiosActivos { get; set; }
        public virtual DbSet<PLU_OP_Empleados> PLU_OP_Empleados { get; set; }
        public virtual DbSet<PLU_OP_FotosActivos> PLU_OP_FotosActivos { get; set; }
        public virtual DbSet<PLU_OP_InventarioFisico> PLU_OP_InventarioFisico { get; set; }
        public virtual DbSet<PLU_OP_OficiosAltas> PLU_OP_OficiosAltas { get; set; }
        public virtual DbSet<PLU_OP_OficiosBajas> PLU_OP_OficiosBajas { get; set; }
        public virtual DbSet<PLU_OP_OficiosCambios> PLU_OP_OficiosCambios { get; set; }
        public virtual DbSet<PLU_OP_Resguardo> PLU_OP_Resguardo { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PLU_CAT_Almacenes>()
                .Property(e => e.NombreAlmacen)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_AreasUnidadesAdministrativas>()
                .Property(e => e.NombreArea)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_CategoriaActivo>()
                .Property(e => e.NombreCategoria)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Clasificadores>()
                .Property(e => e.ClasificadorDescripcion)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Conceptos>()
                .Property(e => e.NombreConcepto)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_EstadoFisicoActivo>()
                .Property(e => e.Descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Estados>()
                .Property(e => e.ESTADO)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Estados>()
                .Property(e => e.HABILITADO)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_EstatusActivo>()
                .Property(e => e.Descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Facturas>()
                .Property(e => e.FolioFactura)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Facturas>()
                .Property(e => e.Total)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PLU_CAT_MarcaActivo>()
                .Property(e => e.NombreMarca)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Municipios>()
                .Property(e => e.NombreMunicipio)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Municipios>()
                .HasMany(e => e.PLU_OP_Activos)
                .WithOptional(e => e.PLU_CAT_Municipios)
                .HasForeignKey(e => e.IdMunicipio);

            modelBuilder.Entity<PLU_CAT_Municipios>()
                .HasMany(e => e.PLU_OP_Adscripcion)
                .WithRequired(e => e.PLU_CAT_Municipios)
                .HasForeignKey(e => e.Municipio)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_CAT_Proveedores>()
                .Property(e => e.RazonSocial)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Proveedores>()
                .Property(e => e.Rfc)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Recurso>()
                .Property(e => e.NombreRecurso)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Roles>()
                .Property(e => e.NombreRol)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_Roles>()
                .HasMany(e => e.PLU_CONF_PermisosMenu)
                .WithRequired(e => e.PLU_CAT_Roles)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_CAT_Roles>()
                .HasMany(e => e.PLU_CONF_Usuario)
                .WithRequired(e => e.PLU_CAT_Roles)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_CAT_TipoActividad>()
                .Property(e => e.NombreActividad)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_TipoActividad>()
                .Property(e => e.Descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CAT_TipoActividad>()
                .HasMany(e => e.PLU_LOG_Actividades)
                .WithRequired(e => e.PLU_CAT_TipoActividad)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_CAT_UnidadesAdministrativas>()
                .Property(e => e.UnidadAdministrativa)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CONF_Menu>()
                .Property(e => e.TituloMenu)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CONF_Menu>()
                .Property(e => e.Icono)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CONF_Menu>()
                .Property(e => e.Orden)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PLU_CONF_Menu>()
                .HasMany(e => e.PLU_CONF_SubMenu)
                .WithRequired(e => e.PLU_CONF_Menu)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_CONF_SubMenu>()
                .Property(e => e.TituloSubMenu)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CONF_SubMenu>()
                .Property(e => e.Controlador)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CONF_SubMenu>()
                .Property(e => e.Accion)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CONF_SubMenu>()
                .Property(e => e.Orden)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PLU_CONF_Usuario>()
                .Property(e => e.Username)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CONF_Usuario>()
                .Property(e => e.Pass)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CONF_Usuario>()
                .Property(e => e.Nombres)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CONF_Usuario>()
                .Property(e => e.Apellidos)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_CONF_Usuario>()
                .HasMany(e => e.PLU_LOG_Actividades)
                .WithRequired(e => e.PLU_CONF_Usuario)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_CONF_Usuario>()
                .HasMany(e => e.PLU_OP_AltasActivos)
                .WithRequired(e => e.PLU_CONF_Usuario)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_CONF_Usuario>()
                .HasMany(e => e.PLU_OP_BajasActivos)
                .WithRequired(e => e.PLU_CONF_Usuario)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_CONF_Usuario>()
                .HasMany(e => e.PLU_OP_CambiosActivos)
                .WithRequired(e => e.PLU_CONF_Usuario)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_CONF_Usuario>()
                .HasMany(e => e.PLU_OP_InventarioFisico)
                .WithRequired(e => e.PLU_CONF_Usuario)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_LOG_Actividades>()
                .Property(e => e.ValorAnterior)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_LOG_Actividades>()
                .Property(e => e.ValorNuevo)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_Activos>()
                .Property(e => e.NumeroInventario)
                .IsUnicode(false);

            //modelBuilder.Entity<PLU_OP_Activos>()
            //    .Property(e => e.NombreEmpleado)
            //    .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_Activos>()
                .Property(e => e.Descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_Activos>()
                .Property(e => e.NumeroSerie)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_Activos>()
                .HasMany(e => e.PLU_OP_AltasActivos)
                .WithRequired(e => e.PLU_OP_Activos)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_OP_Activos>()
                .HasMany(e => e.PLU_OP_BajasActivos)
                .WithRequired(e => e.PLU_OP_Activos)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_OP_Activos>()
                .HasMany(e => e.PLU_OP_CambiosActivos)
                .WithRequired(e => e.PLU_OP_Activos)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_OP_Activos>()
                .HasMany(e => e.PLU_OP_FotosActivos)
                .WithRequired(e => e.PLU_OP_Activos)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_OP_Activos>()
                .HasMany(e => e.PLU_OP_InventarioFisico)
                .WithRequired(e => e.PLU_OP_Activos)
                .HasForeignKey(e => e.IdActivo)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_OP_Empleados>()
                .Property(e => e.NombreCompleto)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_Empleados>()
                .Property(e => e.Nombres)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_Empleados>()
                .Property(e => e.ApellidoP)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_Empleados>()
                .Property(e => e.ApellidoM)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_Empleados>()
                .Property(e => e.Sexo)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_Empleados>()
                .Property(e => e.EstatusRH)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_Empleados>()
                .HasMany(e => e.PLU_OP_Adscripcion)
                .WithRequired(e => e.PLU_OP_Empleados)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_OP_Empleados>()
                .HasMany(e => e.PLU_OP_Resguardo)
                .WithRequired(e => e.PLU_OP_Empleados)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PLU_OP_FotosActivos>()
                .Property(e => e.RutaFoto)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_InventarioFisico>()
                .Property(e => e.Observacion)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_OficiosAltas>()
                .Property(e => e.FolioOficio)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_OficiosAltas>()
                .Property(e => e.RutaOficio)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_OficiosBajas>()
                .Property(e => e.FolioOficio)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_OficiosBajas>()
                .Property(e => e.RutaOficio)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_OficiosCambios>()
                .Property(e => e.FolioOficio)
                .IsUnicode(false);

            modelBuilder.Entity<PLU_OP_OficiosCambios>()
                .Property(e => e.RutaOficio)
                .IsUnicode(false);
        }
    }
}
