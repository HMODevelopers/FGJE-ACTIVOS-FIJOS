# Sistema de Activos Fijos · FGJE

Aplicación web para la administración integral de los activos fijos de la Fiscalía General de Justicia del Estado (FGJE). Centraliza el registro de bienes, su asignación a personal, el control de resguardos, los inventarios físicos y las bajas, manteniendo trazabilidad sobre cada movimiento.

## Propósito del sistema

La aplicación apoya el control administrativo y patrimonial de los bienes institucionales. Su objetivo es que las áreas responsables puedan conocer, en todo momento, qué activos existen, dónde se encuentran, quién los tiene bajo resguardo y cuál es su condición.

El sistema acompaña el ciclo de vida de un activo:

```text
Alta del activo → Identificación y etiquetado → Asignación / resguardo
       → Inventario físico → Cambio de responsable o ubicación → Baja definitiva
```

## Funcionalidades destacadas

- Registro individual y carga masiva de activos desde Excel.
- Administración de fotografías y documentos relacionados con los bienes.
- Impresión de etiquetas de inventario y códigos QR.
- Generación de resguardos, inventarios y otros documentos en PDF.
- Control de altas, cambios de responsable y bajas definitivas.
- Consulta de inventarios físicos y detalle de activos inventariados.
- Exportación de reportes a Excel.
- Acceso controlado por usuario, rol y permisos de menú.

## Módulos principales

| Módulo | ¿Qué permite hacer? |
| --- | --- |
| **Inicio** | Consultar indicadores de operaciones e inventarios agrupados por mes y por usuario. |
| **Activos** | Dar de alta, editar y consultar bienes; administrar fotografías, cargar activos de forma masiva, imprimir etiquetas, registrar bajas y exportar su información a Excel. |
| **Resguardos** | Asignar activos a empleados, registrar altas y cambios de resguardo, consultar movimientos y generar documentos PDF. |
| **Inventario físico** | Crear y consultar levantamientos de inventario, buscar activos, revisar el detalle de cada inventario y generar sus reportes PDF. |
| **Empleados y adscripción** | Administrar empleados, consultar y actualizar su adscripción, además de imprimir sus resguardos. |
| **Catálogos** | Mantener la información de apoyo: almacenes, áreas y unidades administrativas, categorías, clasificadores, conceptos, marcas, proveedores, facturas, estado físico y estatus del activo. |
| **Usuarios y seguridad** | Gestionar usuarios, perfiles, cambio o restablecimiento de contraseña, roles, menús, submenús y permisos. |
| **Reportes** | Consultar la actividad de usuarios y descargar reportes generales de activos, inventario y actividad en formato Excel. |

## Tecnologías utilizadas

| Capa | Tecnología |
| --- | --- |
| Aplicación web | ASP.NET MVC 5.2.9 sobre .NET Framework 4.7.2 |
| Lenguaje y vistas | C#, Razor, HTML, CSS y JavaScript |
| Datos | Entity Framework 6.4.4 y SQL Server |
| Interfaz | Bootstrap 5.2.3 y jQuery 3.4.1 |
| Archivos y documentos | EPPlus para Excel, iTextSharp para PDF y QRCoder para códigos QR |
| Utilidades | PagedList MVC, Newtonsoft.Json y validación jQuery |

## Arquitectura y estructura

Se trata de una aplicación MVC tradicional: los controladores atienden las solicitudes, los modelos representan los datos y las vistas Razor renderizan la interfaz. Entity Framework se conecta a SQL Server mediante el contexto `ModelContext`.

```text
ActivosFijos/
├── App_Start/       Configuración de rutas, filtros, Web API y bundles
├── Controllers/     Controladores de cada módulo funcional
├── Models/          Entidades, ModelContext y modelos de vista
├── Migrations/      Migraciones de Entity Framework
├── Services/Pdf/    Constructores de documentos PDF
├── Views/           Vistas Razor organizadas por módulo
├── Content/         Estilos, imágenes, plantillas y archivos adjuntos
├── Scripts/         JavaScript y librerías del lado cliente
├── Helpers/         Utilidades de sesión, autenticación y hash
└── Filters/         Filtros de autorización y acceso
```

## Requisitos de desarrollo

- Visual Studio 2022 o superior, con la carga de trabajo de ASP.NET y desarrollo web.
- .NET Framework 4.7.2 instalado.
- SQL Server local o remoto, con acceso a la base de datos del sistema.
- IIS Express para desarrollo; para ambientes publicados se requiere IIS.

## Configuración local

La aplicación utiliza una cadena de conexión llamada `ModelContext`, definida en `ActivosFijos/Web.config`. Antes de ejecutar el proyecto, reemplácela por los datos de su entorno local o de desarrollo.

```xml
<connectionStrings>
  <add name="ModelContext"
       connectionString="Data Source=SERVIDOR_SQL;Initial Catalog=PLA_INVENTARIO;User ID=USUARIO_SQL;Password=CONTRASENA;TrustServerCertificate=True;MultipleActiveResultSets=True;App=EntityFramework"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

> Nunca almacene credenciales reales en el repositorio. Para despliegues, use las transformaciones `Web.Debug.config` y `Web.Release.config`, o bien el mecanismo de secretos definido por su infraestructura.

Para más información, consulte [Configuración local](docs/CONFIGURACION.md).

## Base de datos y migraciones

- El acceso a datos se implementa con Entity Framework 6 mediante `ModelContext`.
- El proyecto contiene migraciones de Entity Framework; actualmente incluye una migración para el campo `ForcePasswordChange` de usuarios.
- La estructura base de la base de datos debe estar disponible antes de ejecutar la aplicación. Confirme que las tablas y procedimientos requeridos existan en el entorno SQL Server.
- Antes de aplicar cambios en una base compartida, respalde la información y valide las migraciones en un entorno de pruebas.

## Cómo ejecutar el proyecto

1. Clone el repositorio y abra `ActivosFijos.sln` en Visual Studio.
2. Restaure los paquetes NuGet de la solución.
3. Ajuste la cadena de conexión `ModelContext` en `ActivosFijos/Web.config`.
4. Establezca `ActivosFijos` como proyecto de inicio.
5. Compile la solución para validar dependencias y configuración.
6. Ejecute la aplicación con IIS Express.

La URL configurada para desarrollo es `https://localhost:44312/`.

## Autenticación y permisos

El acceso se basa en autenticación por formularios y sesiones. Una vez iniciada la sesión, el sistema identifica al usuario autenticado y aplica sus permisos según los roles, menús y submenús que tenga asignados.

- El inicio de sesión se encuentra en `Auth/Index`.
- La sesión de autenticación tiene una duración configurada de 60 minutos.
- Los módulos protegidos utilizan filtros de acceso.
- Las contraseñas se comparan mediante hash SHA-256.
- El sistema contempla el cambio forzoso de contraseña para usuarios que lo requieran.

Consulte el [flujo de contraseñas](docs/AUTH_PASSWORD_FLOW.md) para detalles del proceso.

## Archivos generados y adjuntos

La aplicación maneja archivos asociados a la operación, como fotografías de activos y oficios de resguardo o baja. En un despliegue en IIS, la identidad del Application Pool debe tener permisos de escritura sobre las carpetas de contenido utilizadas para esos adjuntos.

Los documentos PDF se generan desde servicios especializados para resguardos, empleados e inventarios. Las exportaciones de Excel se producen desde los módulos de activos y reportes.

## Despliegue en IIS

1. Publique el proyecto desde Visual Studio a una carpeta o sitio IIS.
2. Configure el Application Pool con **.NET CLR v4.0** y pipeline **Integrated**.
3. Actualice la cadena de conexión para el servidor de producción mediante una transformación o configuración segura.
4. Verifique los permisos de escritura para fotografías, oficios y otros adjuntos.
5. Confirme que el servidor tenga acceso a SQL Server y que la base de datos esté preparada.
6. Realice una prueba de inicio de sesión, consulta de activos y generación de documentos antes de liberar el sitio.

## Solución de problemas frecuentes

| Situación | Revisión recomendada |
| --- | --- |
| No es posible conectarse a la base de datos | Revise el servidor, nombre de base, credenciales y permisos de `ModelContext`. |
| Faltan referencias al compilar | Restaure los paquetes NuGet y vuelva a compilar la solución. |
| No se pueden cargar imágenes u oficios | Compruebe que las carpetas de destino existan y que IIS tenga permiso de escritura. |
| Un usuario no puede acceder a un módulo | Verifique que tenga un rol y permisos asignados para el menú o submenú correspondiente. |
| El inicio de sesión falla | Confirme que el usuario esté activo y que su contraseña coincida con el hash almacenado. |
| No se generan PDFs o Excel | Verifique permisos de escritura, dependencias restauradas y la información requerida por el reporte. |

## Documentación complementaria

- [Configuración local](docs/CONFIGURACION.md)
- [Flujo de contraseñas](docs/AUTH_PASSWORD_FLOW.md)
- [Nota técnica: optimización de listado de cambios de resguardo](docs/NOTA_TECNICA_OPTIMIZACION_LISTARCAMBIOS.md)
- [Script SQL de optimización](docs/sql/20260327_optimizacion_listar_cambios_resguardos.sql)

## Consideraciones para contribución

No hay una guía de contribución formal en el repositorio. Para cambios funcionales se recomienda:

1. Trabajar en una rama independiente.
2. Probar los flujos afectados con una base de datos de desarrollo.
3. No incluir credenciales, archivos de usuarios ni datos sensibles.
4. Documentar scripts SQL, cambios de configuración o migraciones requeridas.
5. Validar la generación de PDFs, Excel y adjuntos cuando el cambio los involucre.

## Licencia

Este repositorio no incluye un archivo de licencia.
