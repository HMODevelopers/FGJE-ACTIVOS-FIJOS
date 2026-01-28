# FGJE Activos Fijos

## A) Visión general

**FGJE Activos Fijos** es un sistema de inventarios para la gestión de activos fijos. La solución incluye un único proyecto web ASP.NET MVC (`ActivosFijos`) dentro de la solución `ActivosFijos.sln`.【F:ActivosFijos.sln†L1-L8】

**Características principales (según módulos implementados):**
- Administración de activos (altas, bajas, cambios, resguardos e inventario físico).【F:ActivosFijos/ActivosFijos.csproj†L121-L156】
- Catálogos base: categorías, conceptos, marcas, proveedores, almacenes, estados físicos y estatus de activos.【F:ActivosFijos/ActivosFijos.csproj†L109-L121】
- Administración de usuarios, roles, menús, submenús y permisos de acceso.【F:ActivosFijos/ActivosFijos.csproj†L116-L129】
- Reportes y actividades de usuarios.【F:ActivosFijos/ActivosFijos.csproj†L120-L128】

> **Nota:** No se incluyen capturas porque el repositorio no las contiene.

## B) Stack

- **ASP.NET MVC 5.2.9** (paquetes `Microsoft.AspNet.Mvc`, `Razor`, `WebPages`).【F:ActivosFijos/packages.config†L11-L24】
- **.NET Framework 4.7.2** (`TargetFrameworkVersion` del proyecto).【F:ActivosFijos/ActivosFijos.csproj†L20-L27】
- **Entity Framework 6.4.4**.【F:ActivosFijos/packages.config†L5-L8】
- **Base de datos SQL Server** (`System.Data.SqlClient` en la cadena de conexión).【F:ActivosFijos/Web.config†L66-L69】
- **Librerías clave**:
  - EPPlus (exportación Excel).【F:ActivosFijos/packages.config†L7-L9】
  - iTextSharp (PDF).【F:ActivosFijos/packages.config†L10-L11】
  - QRCoder (QR).【F:ActivosFijos/packages.config†L25-L26】
  - Bootstrap 5.2.3 y jQuery 3.4.1 (UI).【F:ActivosFijos/packages.config†L3-L12】
  - PagedList MVC (paginación).【F:ActivosFijos/packages.config†L24-L25】

## C) Requisitos

- **Visual Studio 2022** o superior con carga de trabajo de ASP.NET y desarrollo web (la solución declara VS 17).【F:ActivosFijos.sln†L1-L5】
- **.NET Framework 4.7.2** instalado en el equipo de desarrollo.【F:ActivosFijos/ActivosFijos.csproj†L20-L27】
- **SQL Server** (local o remoto) para la base de datos de inventarios.【F:ActivosFijos/Web.config†L66-L69】
- **IIS Express** para desarrollo (configurado en el proyecto).【F:ActivosFijos/ActivosFijos.csproj†L26-L33】

## D) Configuración de entorno

- El archivo principal es `ActivosFijos/Web.config`, donde vive la cadena de conexión `ModelContext`.【F:ActivosFijos/Web.config†L1-L69】
- Existen transformaciones `Web.Debug.config` y `Web.Release.config` para ajustar settings por ambiente.【F:ActivosFijos/Web.Debug.config†L1-L27】【F:ActivosFijos/Web.Release.config†L1-L19】
- **Cadena de conexión**:
  - **Nombre:** `ModelContext`.
  - **Proveedor:** `System.Data.SqlClient`.
  - **Ejemplo seguro:** ver [`docs/CONFIGURACION.md`](docs/CONFIGURACION.md).【F:docs/CONFIGURACION.md†L1-L21】
- **Variables sensibles:** credenciales SQL no deben ser versionadas (en el repo hay un ejemplo que debe reemplazarse localmente).【F:ActivosFijos/Web.config†L66-L69】

## E) Base de datos (Entity Framework)

- El proyecto usa **Entity Framework 6** con un `DbContext` llamado `ModelContext`.【F:ActivosFijos/Models/ModelContext.cs†L7-L19】
- **No se encontraron migraciones ni archivo `.edmx`** en el repositorio, por lo que la base debe existir previamente y corresponder al modelo de entidades en `Models/`.【F:ActivosFijos/Models/ModelContext.cs†L7-L54】

### Escenario actual (sin migraciones)

1. Crear la base de datos en SQL Server.
2. Ajustar la cadena de conexión `ModelContext` en `Web.config`.
3. Validar que las tablas existentes coincidan con las entidades del proyecto (prefijos `PLU_*`).【F:ActivosFijos/Models/ModelContext.cs†L7-L54】

### Recomendaciones

- Si se desea usar migraciones Code First, habilitarlo con `Enable-Migrations` en la Package Manager Console y generar una migración inicial.
- Documentar scripts SQL o un `edmx` si se trabaja en modo Database First.

## F) Cómo correr (desarrollo)

1. Abrir `ActivosFijos.sln` en Visual Studio.【F:ActivosFijos.sln†L1-L8】
2. Restaurar paquetes NuGet (`packages.config`).【F:ActivosFijos/packages.config†L1-L35】
3. Establecer `ActivosFijos` como **Startup Project**.
4. Compilar la solución.
5. Ejecutar con IIS Express. El proyecto define la URL local en `https://localhost:44312/`.【F:ActivosFijos/ActivosFijos.csproj†L760-L768】

> **Credenciales:** No se encontraron usuarios/credenciales por defecto en el código; la autenticación se basa en registros de la tabla `PLU_CONF_Usuario` y contraseñas hasheadas con SHA-256.【F:ActivosFijos/Helpers/AuthHelper.cs†L13-L42】【F:ActivosFijos/Helpers/HashHelper.cs†L49-L60】

## G) Cómo desplegar (producción)

Escenario típico en IIS:

1. Publicar el proyecto desde Visual Studio (carpeta o perfil IIS).
2. Configurar un **Application Pool** con **.NET CLR v4.0** y pipeline **Integrated**.
3. Ajustar `Web.config` en el servidor con la cadena de conexión real.
4. Otorgar permisos a carpetas de uploads si se usan (por ejemplo, `Content/FotosActivos`, `Content/Oficios*`).【F:ActivosFijos/ActivosFijos.csproj†L636-L646】

No se encontraron perfiles de publicación (`.pubxml`) en el repositorio.

## H) Estructura del proyecto

```
ActivosFijos/
├─ App_Start/        # BundleConfig, FilterConfig, RouteConfig, WebApiConfig
├─ Controllers/      # Controladores MVC (módulos funcionales)
├─ Models/           # DbContext + entidades EF
├─ Models/ViewModels # ViewModels
├─ Views/            # Vistas Razor
├─ Helpers/          # Utilidades (auth, hash, sesiones, etc.)
├─ Filters/          # Filtros de autenticación
├─ Content/          # CSS, assets, archivos adjuntos
└─ Scripts/          # JS y librerías
```

Referencias:
- `App_Start` contiene la configuración de rutas, bundles y Web API.【F:ActivosFijos/App_Start/RouteConfig.cs†L8-L19】【F:ActivosFijos/App_Start/BundleConfig.cs†L8-L28】【F:ActivosFijos/App_Start/WebApiConfig.cs†L8-L24】
- `Models` expone `ModelContext` y las entidades EF.【F:ActivosFijos/Models/ModelContext.cs†L7-L54】
- `Filters` incluye filtros de autenticación para login y sesiones.【F:ActivosFijos/Filters/AdminFilters.cs†L8-L44】

## I) Módulos funcionales

Basado en los controladores existentes:

- **Autenticación:** `AuthController` (login y logout).【F:ActivosFijos/ActivosFijos.csproj†L114-L118】
- **Activos y resguardos:** `ActivosController`, `ResguardosController` y `InventarioController`.【F:ActivosFijos/ActivosFijos.csproj†L109-L122】
- **Catálogos:** `Almacenes`, `AreasUnidadesAdmin`, `Categorias`, `Clasificadores`, `Conceptos`, `EstadoFisico`, `EstatusActivo`, `Facturas`, `Marcas`, `Proveedores`.【F:ActivosFijos/ActivosFijos.csproj†L109-L121】
- **Administración y seguridad:** `Usuarios`, `Roles`, `Permisos`, `Menu`, `SubMenu`.【F:ActivosFijos/ActivosFijos.csproj†L115-L129】
- **Reportes:** `ReportesController`.【F:ActivosFijos/ActivosFijos.csproj†L121-L123】

## J) Seguridad y roles

- **Autenticación:** Forms Authentication con cookie de sesión; redirección a `Auth/Index` cuando no hay sesión.【F:ActivosFijos/Web.config†L16-L23】【F:ActivosFijos/Filters/AdminFilters.cs†L10-L29】
- **Gestión de sesión:** `SessionHelper` administra la cookie y obtiene el ID del usuario autenticado.【F:ActivosFijos/Helpers/SessionHelper.cs†L10-L66】
- **Roles y permisos:** se basan en tablas `PLU_CAT_Roles` y `PLU_CONF_PermisosMenu`.【F:ActivosFijos/Models/PLU_CAT_Roles.cs†L1-L36】【F:ActivosFijos/Models/PLU_CONF_PermisosMenu.cs†L1-L36】
- **Password hashing:** SHA-256 en `AuthHelper`/`HashHelper`.【F:ActivosFijos/Helpers/AuthHelper.cs†L13-L42】【F:ActivosFijos/Helpers/HashHelper.cs†L49-L60】

## K) Troubleshooting

- **Error de conexión a BD:** verificar que `ModelContext` apunte al servidor correcto y que el usuario tenga permisos en la BD.【F:ActivosFijos/Web.config†L66-L69】
- **Credenciales inválidas:** las contraseñas se comparan con SHA-256; revisar valores de `PLU_CONF_Usuario.Pass`.【F:ActivosFijos/Helpers/AuthHelper.cs†L18-L36】【F:ActivosFijos/Helpers/HashHelper.cs†L49-L60】
- **Paquetes faltantes:** restaurar NuGet cuando el build lo indique (referencias en `packages.config`).【F:ActivosFijos/packages.config†L1-L35】
- **Errores sin logging estructurado:** actualmente se usa `Trace.TraceError` en autenticación; revisar output de logs de IIS/Trace si hay fallas de login.【F:ActivosFijos/Helpers/AuthHelper.cs†L35-L41】

## L) Contribución y convenciones

- No hay guías de contribución específicas en el repositorio. Se recomienda acordar convención de ramas/commits si el equipo lo requiere.
- No se encontraron proyectos de pruebas automatizadas en la solución.

## M) Licencia

No se encontró archivo `LICENSE` en el repositorio.

## Hallazgos rápidos (stack/EF/BD/autenticación)

- Proyecto ASP.NET MVC 5.2.9 sobre .NET Framework 4.7.2.【F:ActivosFijos/ActivosFijos.csproj†L20-L27】【F:ActivosFijos/packages.config†L11-L24】
- Entity Framework 6.4.4 con `ModelContext` y entidades `PLU_*`; sin migraciones/EDMX detectados.【F:ActivosFijos/packages.config†L5-L8】【F:ActivosFijos/Models/ModelContext.cs†L7-L54】
- Base de datos SQL Server definida por `ModelContext` en `Web.config`.【F:ActivosFijos/Web.config†L66-L69】
- Autenticación basada en Forms Authentication con cookies y login en `Auth/Index`.【F:ActivosFijos/Web.config†L16-L23】【F:ActivosFijos/Filters/AdminFilters.cs†L10-L29】
- Contraseñas hasheadas con SHA-256 (no hay credenciales hardcodeadas).【F:ActivosFijos/Helpers/AuthHelper.cs†L18-L36】【F:ActivosFijos/Helpers/HashHelper.cs†L49-L60】
