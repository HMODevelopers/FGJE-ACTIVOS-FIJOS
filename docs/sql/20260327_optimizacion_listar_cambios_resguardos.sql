/*
    Script de apoyo para optimizar Resguardos/ListarCambios
    Motor: SQL Server
*/

/* ============================================================
   PLU_OP_CambiosActivos
   Índice principal para filtros, orden y joins de ListarCambios
   ============================================================ */
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PLU_OP_CambiosActivos_ListarCambios_Main'
      AND object_id = OBJECT_ID('dbo.PLU_OP_CambiosActivos')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_PLU_OP_CambiosActivos_ListarCambios_Main
        ON dbo.PLU_OP_CambiosActivos (Activo, FechaCreacion DESC, FolioCambio)
        INCLUDE (IdCambioActivo, IdActivos, IdEmpleadoAnterior, IdEmpleadoActual, IdOficioCambio, IdUsuario, Inventario);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PLU_OP_CambiosActivos_FolioCambio'
      AND object_id = OBJECT_ID('dbo.PLU_OP_CambiosActivos')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_PLU_OP_CambiosActivos_FolioCambio
        ON dbo.PLU_OP_CambiosActivos (FolioCambio)
        INCLUDE (IdCambioActivo, IdActivos, FechaCreacion, Activo, IdEmpleadoAnterior, IdEmpleadoActual, IdOficioCambio);
END
GO

/* ============================================================
   PLU_OP_Activos
   Índices para filtros por inventario/descripcion/serie y join por IdActivos
   ============================================================ */
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PLU_OP_Activos_ListarCambios_Main'
      AND object_id = OBJECT_ID('dbo.PLU_OP_Activos')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_PLU_OP_Activos_ListarCambios_Main
        ON dbo.PLU_OP_Activos (IdActivos)
        INCLUDE (NumeroInventario, Descripcion, NumeroSerie, Activo);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PLU_OP_Activos_NumeroInventario'
      AND object_id = OBJECT_ID('dbo.PLU_OP_Activos')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_PLU_OP_Activos_NumeroInventario
        ON dbo.PLU_OP_Activos (NumeroInventario)
        INCLUDE (IdActivos, Descripcion, NumeroSerie, Activo);
END
GO

/* ============================================================
   PLU_OP_Empleados
   Índices para joins y filtros por Número/Nombre
   ============================================================ */
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PLU_OP_Empleados_IdEmpleado_ListarCambios'
      AND object_id = OBJECT_ID('dbo.PLU_OP_Empleados')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_PLU_OP_Empleados_IdEmpleado_ListarCambios
        ON dbo.PLU_OP_Empleados (IdEmpleado)
        INCLUDE (NumeroEmpleado, NombreCompleto, Nombres, ApellidoP, ApellidoM, Activo);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PLU_OP_Empleados_NumeroEmpleado'
      AND object_id = OBJECT_ID('dbo.PLU_OP_Empleados')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_PLU_OP_Empleados_NumeroEmpleado
        ON dbo.PLU_OP_Empleados (NumeroEmpleado)
        INCLUDE (IdEmpleado, NombreCompleto, Nombres, ApellidoP, ApellidoM, Activo);
END
GO

/* ============================================================
   PLU_OP_OficiosCambios
   Índices para join y filtro por folio de oficio
   ============================================================ */
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PLU_OP_OficiosCambios_IdOficioCambio_Listar'
      AND object_id = OBJECT_ID('dbo.PLU_OP_OficiosCambios')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_PLU_OP_OficiosCambios_IdOficioCambio_Listar
        ON dbo.PLU_OP_OficiosCambios (IdOficioCambio)
        INCLUDE (FolioOficio, RutaOficio, Activo, FechaCreacion);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PLU_OP_OficiosCambios_FolioOficio'
      AND object_id = OBJECT_ID('dbo.PLU_OP_OficiosCambios')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_PLU_OP_OficiosCambios_FolioOficio
        ON dbo.PLU_OP_OficiosCambios (FolioOficio)
        INCLUDE (IdOficioCambio, RutaOficio, Activo, FechaCreacion);
END
GO
