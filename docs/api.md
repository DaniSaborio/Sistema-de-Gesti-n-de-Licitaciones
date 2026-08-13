# API REST

Base: `/api/v1`. Documentación interactiva (Swagger UI) disponible en `/docs/api`
cuando la aplicación está corriendo; el documento OpenAPI crudo está en
`/swagger/v1/swagger.json`.

## Convenciones

- Versionado en la URL (`Asp.Versioning`); versión por defecto `1.0` si no se indica.
- Los DTO de Application (`Licitaciones.Application.*`) son el contrato HTTP; nunca se
  serializa una entidad de EF Core directamente.
- Los enums (`EstadoLicitacion`, `ClasificacionOferta`) viajan como texto
  (`"Publicada"`, `"OfertaConveniente"`), no como enteros (`JsonStringEnumConverter`
  registrado globalmente).
- Los montos son siempre `decimal` en CRC; la API no devuelve montos en USD — la
  conversión es responsabilidad de quien consume la API, usando `GET /tipos-cambio`
  para obtener el tipo de cambio activo (mismo principio que la interfaz web: CRC es
  la fuente de verdad).
- Paginación: `pagina` (1-based) y `tamanoPagina` (1-100) en query string; la
  respuesta es `{ elementos, totalElementos, pagina, tamanoPagina, totalPaginas }`.

## Endpoints

### Licitaciones

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/licitaciones` | Lista paginada, con `busqueda`, `ordenarPor` (`codigo`,`fechaCierre`,`presupuesto`,`estado`), `descendente` |
| GET | `/licitaciones/{id}` | Detalle |
| POST | `/licitaciones` | Crea (Borrador) |
| PUT | `/licitaciones/{id}` | Actualiza título/fecha/presupuesto |
| PATCH | `/licitaciones/{id}/estado` | Cambia de estado (`estadoDestino`) |
| DELETE | `/licitaciones/{id}` | Elimina (borrado lógico) |
| GET | `/licitaciones/{id}/ofertas` | Ofertas de la licitación, paginado |
| POST | `/licitaciones/{id}/ofertas` | Registra una oferta |
| GET | `/licitaciones/{id}/mejor-oferta` | Mejor oferta, clasificación y aprobador |

### Proveedores, Ofertas, Niveles de aprobación, Tipos de cambio

| Método | Ruta | Descripción |
|---|---|---|
| GET/POST | `/proveedores` | Lista paginada / crea |
| GET/PUT/DELETE | `/proveedores/{id}` | Consulta / actualiza / elimina (lógico) |
| GET | `/ofertas` | Lista paginada, filtra por `licitacionId`/`proveedorId` |
| GET/PUT/DELETE | `/ofertas/{id}` | Consulta / actualiza / elimina |
| GET/POST | `/niveles-aprobacion` | Lista paginada / crea |
| GET/PUT/DELETE | `/niveles-aprobacion/{id}` | Consulta / actualiza / elimina |
| GET/POST | `/tipos-cambio` | Lista paginada / crea |
| GET/PUT/DELETE | `/tipos-cambio/{id}` | Consulta / actualiza / elimina |
| PATCH | `/tipos-cambio/{id}/activar` | Activa este tipo de cambio (desactiva el anterior) |

## Ejemplos

Crear proveedor:

```http
POST /api/v1/proveedores
Content-Type: application/json

{ "nombre": "Empresa Central S.A." }
```

```http
201 Created
Location: /api/v1/proveedores/3622f9e4-...
{
  "id": "3622f9e4-...",
  "nombre": "Empresa Central S.A.",
  "createdAt": "2026-08-13T19:48:29.83Z",
  "updatedAt": "2026-08-13T19:48:29.83Z"
}
```

Publicar una licitación:

```http
PATCH /api/v1/licitaciones/{id}/estado
Content-Type: application/json

{ "estadoDestino": "Publicada" }
```

Consultar la mejor oferta:

```http
GET /api/v1/licitaciones/{id}/mejor-oferta
```

```json
{
  "clasificacion": "OfertaConveniente",
  "ofertaId": "640627ce-...",
  "proveedorId": "daeb1f65-...",
  "montoOfertadoCRC": 900000.0,
  "porcentajeAhorro": 10.0,
  "aprobador": "Encargado de área"
}
```

Estas tres solicitudes (y varias más: oferta duplicada rechazada, proveedor
duplicado normalizado rechazado, recurso inexistente) se ejecutaron realmente contra
una instancia con PostgreSQL real durante la Iteración 2; ver `bitacora-xp.md` para el
detalle completo y los defectos que esa verificación encontró y corrigió.

## Códigos de estado y errores

| Código | Cuándo |
|---|---|
| 200 / 201 / 204 | Éxito (consulta / creación / eliminación) |
| 400 | Solicitud mal formada o inválida (FluentValidation, o violación de integridad no cubierta por una regla de dominio) |
| 404 | Recurso no encontrado (`RecursoNoEncontradoException`) |
| 409 | Conflicto de unicidad o de concurrencia optimista |
| 422 | Regla de negocio de dominio no satisfecha (cualquier `DomainException`: estado no publicado, oferta vencida, monto inválido, rango solapado, etc.) |
| 500 | Error inesperado — nunca expone stack trace, ruta interna ni mensaje técnico crudo |

Todas las respuestas de error usan `application/problem+json` (RFC 9457) con
`title`, `status`, `detail` seguro para el usuario, `instance` (la ruta), `traceId`
(identificador de correlación para soporte) y `errorCode` (el nombre de la excepción,
útil para manejo programático). Ver `Licitaciones.Api/Middleware/ApiExceptionHandler.cs`.

## Colección de solicitudes reproducible

`assets/licitaciones.http` contiene una colección de solicitudes de ejemplo (formato
`.http`, compatible con el cliente HTTP integrado de Visual Studio / VS Code /
JetBrains Rider) que cubre el flujo completo: crear proveedor, crear licitación,
publicar, registrar oferta, intentar una oferta duplicada y consultar la mejor oferta.
