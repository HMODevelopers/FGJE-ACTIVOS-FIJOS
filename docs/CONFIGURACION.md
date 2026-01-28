# Configuración local (sin secretos)

Este archivo sirve como guía para configurar el proyecto sin exponer credenciales reales. La aplicación usa la cadena de conexión `ModelContext` definida en `ActivosFijos/Web.config` y debe apuntar a una base de datos SQL Server válida.

## Ejemplo de `connectionStrings`

> **Nota:** reemplaza los valores por los de tu entorno (servidor, base, usuario y contraseña).

```xml
<connectionStrings>
  <add name="ModelContext"
       connectionString="Data Source=SERVIDOR_SQL;Initial Catalog=PLA_INVENTARIO;User ID=USUARIO_SQL;Password=CONTRASENA;TrustServerCertificate=True;MultipleActiveResultSets=True;App=EntityFramework"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

## Buenas prácticas

- No subas credenciales reales al repositorio.
- Usa transformaciones `Web.Debug.config`/`Web.Release.config` o variables de entorno en el servidor para ajustar la cadena de conexión.
- Si necesitas compartir valores de ejemplo, utiliza un formato como el de arriba.
