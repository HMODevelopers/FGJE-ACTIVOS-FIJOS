# Flujo actual de autenticación y contraseñas

## Resumen del flujo

### Dónde está el login (Controller/Action y View)
- Controller/Action: `AuthController.Index` (GET) y `AuthController.Login` (POST).【F:ActivosFijos/Controllers/AuthController.cs†L9-L30】
- View: `Views/Auth/Index.cshtml`.【F:ActivosFijos/Views/Auth/Index.cshtml†L1-L98】

### Dónde se valida la contraseña
- Validación en `AuthHelper.Auth` mediante consulta EF a `PLU_CONF_Usuario` con `Username`, `Pass` (hash) y `Activo`.【F:ActivosFijos/Helpers/AuthHelper.cs†L10-L47】

### Cómo se guarda la contraseña (hash/salt/iteraciones)
- Hash con SHA256 sin salt (helper `HashHelper.SHA256`).【F:ActivosFijos/Helpers/HashHelper.cs†L45-L55】
- Se usa al crear usuarios (`UsuariosController.Guardar` cuando `IdUsuario == 0`).【F:ActivosFijos/Controllers/UsuariosController.cs†L58-L77】

### Tabla/Entidad de usuario
- Entidad `PLU_CONF_Usuario` con campos relevantes: `IdUsuario`, `IdRol`, `Username`, `Pass`, `Activo`, `FechaCreacion`, etc.【F:ActivosFijos/Models/PLU_CONF_Usuario.cs†L9-L63】
- Se agrega bandera `ForcePasswordChange` para exigir cambio en el próximo login.【F:ActivosFijos/Models/PLU_CONF_Usuario.cs†L29-L43】

### Manejo de roles/permisos
- Roles asociados por `IdRol` y relación con `PLU_CAT_Roles`.【F:ActivosFijos/Models/PLU_CONF_Usuario.cs†L16-L54】
- Menús y permisos se resuelven fuera del login; el módulo Usuarios ya está protegido por `[Autenticado]`.【F:ActivosFijos/Controllers/UsuariosController.cs†L13-L17】

### Manejo de sesión/cookie
- Autenticación vía `FormsAuthentication` y cookie con `UserData` (IdUsuario).【F:ActivosFijos/Helpers/SessionHelper.cs†L14-L67】
- Filtro `Autenticado` valida la sesión; `NoLogin` impide acceso a login cuando ya hay sesión.【F:ActivosFijos/Filters/AdminFilters.cs†L9-L54】

### Funcionalidad parcial existente de cambio/reset
- No había pantalla ni endpoint específico para cambiar/restablecer contraseñas.
- En Perfil existía un botón/modal placeholder sin lógica real de cambio.【F:ActivosFijos/Views/Usuarios/Perfil.cshtml†L12-L44】

## Archivos clave encontrados
- `ActivosFijos/Controllers/AuthController.cs`
- `ActivosFijos/Helpers/AuthHelper.cs`
- `ActivosFijos/Helpers/HashHelper.cs`
- `ActivosFijos/Helpers/SessionHelper.cs`
- `ActivosFijos/Filters/AdminFilters.cs`
- `ActivosFijos/Models/PLU_CONF_Usuario.cs`
- `ActivosFijos/Controllers/UsuariosController.cs`
- `ActivosFijos/Views/Auth/Index.cshtml`
- `ActivosFijos/Views/Usuarios/Perfil.cshtml`
