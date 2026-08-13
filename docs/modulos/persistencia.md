# Módulo: Persistencia (Licitaciones.Infrastructure)

## Propósito
Implementar el acceso a datos con Entity Framework Core 9 + PostgreSQL, sin filtrar
detalles de EF Core hacia Application o Domain.

## Responsabilidades
- `LicitacionesDbContext` y las cinco configuraciones Fluent API (una por entidad),
  con índices únicos, restricciones `CHECK`, columnas `numeric` explícitas y
  concurrencia optimista vía `xmin`.
- Repositorios (`ProveedorRepository`, `LicitacionRepository`, `OfertaRepository`,
  `NivelAprobacionRepository`, `TipoCambioRepository`) que implementan las interfaces
  definidas en Application.
- `UnitOfWork`: envuelve `DbContext.SaveChangesAsync`, traduciendo
  `DbUpdateConcurrencyException`/`DbUpdateException` a excepciones propias de
  Application (`ConflictoDeConcurrenciaException`/`ErrorDeIntegridadDeDatosException`)
  para que ni Application ni Api necesiten depender de EF Core.
- `SystemClock`: implementación de producción de `IClock` (Domain.Common), usando
  `DateTimeOffset.UtcNow`.
- Migraciones versionadas y datos semilla (niveles de aprobación, tipo de cambio
  inicial).

## Dependencias
`Microsoft.EntityFrameworkCore` 9.0.9, `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4
(fijados explícitamente porque las versiones "latest" de NuGet en el momento de
construir el proyecto apuntaban a .NET 10). Referencia a `Licitaciones.Application`
(para implementar sus interfaces) y `Licitaciones.Domain` (para las entidades).

## Ver también
`modelo-datos.md` para el diagrama entidad-relación completo y el detalle de cada
decisión de modelado (índices, restricciones, concurrencia, borrado lógico).

## Piezas técnicas propias de este módulo
- **`PaginacionExtensions.APaginadoAsync`**: helper compartido por los cinco
  repositorios para no repetir `Skip`/`Take`/`Count` cinco veces.
- **`LicitacionesDbContextFactory`** (`IDesignTimeDbContextFactory`): permite que
  `dotnet ef migrations add`/`database update` funcionen sin necesitar levantar todo
  el host de ASP.NET Core, leyendo la cadena de conexión de una variable de entorno
  con un valor de reserva para desarrollo local.

## Nota de diseño (API real vs. recordada)
El primer intento de configurar concurrencia optimista usó
`builder.UseXminAsConcurrencyToken()`, un método que se recordaba de versiones
anteriores de `Npgsql.EntityFrameworkCore.PostgreSQL` pero que **no existe** en la
versión 9.0.4 instalada (se verificó inspeccionando el ensamblado y la documentación
XML embebida: no hay ningún método con ese nombre). El mecanismo real y soportado es
mapear una propiedad sombra `uint` llamada `xmin` con `.IsRowVersion()`, que una
convención específica de Npgsql detecta y mapea a la columna interna `xmin` de
PostgreSQL automáticamente. Se corrigió en las cinco configuraciones.

## Errores
`DbUpdateConcurrencyException` → `ConflictoDeConcurrenciaException` (409 en la API).
`DbUpdateException` (violación de restricción/FK/índice único) →
`ErrorDeIntegridadDeDatosException` (400 en la API), sin exponer el mensaje SQL crudo.

## Pruebas
Integración (`Licitaciones.IntegrationTests`, PostgreSQL real vía Testcontainers):
migraciones, índices únicos forzados a nivel de base de datos, `FK Restrict`,
restricción `CHECK`, concurrencia optimista, persistencia/recuperación, datos semilla.
Verificado además manualmente contra una instancia PostgreSQL nativa real durante el
desarrollo (`dotnet ef database update` + inspección con `psql`), documentado en
`bitacora-xp.md`.
