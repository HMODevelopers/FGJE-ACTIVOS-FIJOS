# Nota técnica: Optimización `Resguardos/ListarCambios`

## Diagnóstico del problema
El listado original agrupaba por `FolioCambio` y dentro de la proyección ejecutaba múltiples accesos repetidos a `FirstOrDefault()` y navegaciones profundas (activo/resguardo/empleado/usuario/oficio). Esa forma de construir la consulta generaba SQL complejo, costoso y frágil para históricos grandes, elevando la probabilidad de `EntityCommandExecutionException` por timeout o planes de ejecución subóptimos.

## Qué se optimizó
1. Se eliminó la dependencia de `FirstOrDefault()` repetido sobre cada grupo.
2. Se movió toda la lógica de filtros al `IQueryable` para que ejecute en SQL Server (sin filtrado en memoria).
3. Se incorporó `AsNoTracking()` para lectura de listado.
4. Se separó la consulta en dos fases SQL:
   - detalle filtrado con joins explícitos (`CambiosActivos`, `Activos`, `Empleados`, `Oficios`),
   - resumen por `FolioCambio` con `MAX(IdCambioActivo)` + `COUNT DISTINCT` y join al detalle representativo.
5. Se añadió paginación real con `ToPagedList` sobre consulta ordenada.
6. Se agregaron filtros específicos por campos del módulo (folio, inventario, descripción, serie, empleados anterior/actual, oficio, rango de fechas, estatus).

## Parte más propensa a disparar el error
La sección más riesgosa era la proyección agrupada con muchas llamadas a `g.FirstOrDefault().<navegación>`, porque multiplicaba joins/subconsultas implícitas y podía degradar severamente el plan cuando la tabla `PLU_OP_CambiosActivos` crecía.

## Script SQL de apoyo
Se agregó un script de índices no cluster para reforzar joins, filtros y orden del listado:

- `docs/sql/20260327_optimizacion_listar_cambios_resguardos.sql`
