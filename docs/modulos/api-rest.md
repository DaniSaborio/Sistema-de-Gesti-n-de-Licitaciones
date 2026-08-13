# Módulo: API REST (Licitaciones.Api)

## Propósito
Exponer todas las operaciones del sistema como una API REST versionada, documentada e
independiente de la interfaz web, para permitir integraciones externas.

## Responsabilidades
- Controladores REST (`/api/v1/...`) para los cinco módulos, cada uno delegando en el
  `*Service` de Application correspondiente.
- Versionado explícito (`Asp.Versioning`), documentación OpenAPI/Swagger por versión.
- Traducción uniforme de excepciones a `ProblemDetails` (`ApiExceptionHandler`),
  nunca exponer stack traces, rutas internas ni mensajes técnicos.
- Contratos HTTP propios (DTOs de Application): nunca se serializa una entidad de EF
  Core directamente.

## Dependencias
`Licitaciones.Application` (todos los `*Service`, DTOs), `Asp.Versioning.Mvc` +
`.ApiExplorer`, `Swashbuckle.AspNetCore`. Es una biblioteca de clases (sin
`Program.cs` propio) montada por `Licitaciones.Web` como Application Part — ver
`arquitectura-general.md` para la justificación de esta decisión.

## Entradas / Salidas
Ver `api.md` para el listado completo de endpoints, ejemplos de solicitud/respuesta y
la colección reproducible en `assets/licitaciones.http`.

## Piezas técnicas propias de este módulo
- **`ApiExceptionHandler`** (`IExceptionHandler`): clasifica cada excepción a un
  `(status, title, detail)` y construye el `ProblemDetails` con `traceId` (para
  correlacionar con los logs del servidor) y `errorCode` (el nombre de la excepción,
  útil para manejo programático por quien consume la API). Solo actúa sobre rutas que
  empiezan con `/api` — el resto del sitio usa la página de error MVC.
- **`ConfigureSwaggerOptions`**: genera un documento OpenAPI por cada versión de API
  descubierta automáticamente.
- **`JsonStringEnumConverter`** registrado globalmente: los enums (`EstadoLicitacion`,
  `ClasificacionOferta`) viajan como texto legible, no como números.

## Nota de diseño (defecto real corregido)
El primer intento de configurar el manejo de excepciones usó
`app.UseExceptionHandler("/Home/Error")` de forma incondicional. Ese *overload* con
ruta reejecuta el pipeline en esa ruta cuando ningún `IExceptionHandler` maneja la
excepción — pero además, en la configuración que se probó en vivo, las rutas `/api`
terminaban devolviendo la página HTML de error en vez de `ProblemDetails`. Se separó
el pipeline con `UseWhen`: las rutas `/api` usan exclusivamente `ApiExceptionHandler`;
el resto del sitio usa la página de error MVC. Ver `bitacora-xp.md` para el detalle
completo de cómo se encontró este defecto (probando contra PostgreSQL real, no solo
compilando).

## Errores
Ver la tabla de códigos de estado en `api.md`. Todas las respuestas de error son
`application/problem+json`.

## Pruebas
Integración: `ApiEndpointsIntegrationTests` (alta y consulta de proveedor vía HTTP
real, conflicto 409 con `ProblemDetails`, 404 en recurso inexistente) contra
PostgreSQL real (Testcontainers). Verificado manualmente en vivo durante la Iteración
2 contra una instancia PostgreSQL real (ver `bitacora-xp.md`).
