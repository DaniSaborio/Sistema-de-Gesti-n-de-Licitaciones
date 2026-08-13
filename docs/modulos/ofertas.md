# Módulo: Ofertas

## Propósito
Registrar y administrar las ofertas económicas de los proveedores sobre licitaciones
publicadas, garantizando las cinco reglas de aceptación del enunciado y calculando la
mejor oferta.

## Responsabilidades
- Registrar una oferta validando: licitación publicada, no vencida, monto positivo, no
  superior al presupuesto, y no duplicada por proveedor.
- Editar (revalida todas las reglas anteriores contra el nuevo monto) y eliminar
  (bloqueado si la licitación ya está cerrada o vencida) ofertas existentes.
- Listar y filtrar por licitación y/o proveedor, con paginación.
- Determinar la mejor oferta de una licitación (delegado a `EvaluadorOfertas`, usado
  por el módulo de Licitaciones).

## Dependencias
- `Licitaciones.Domain.Ofertas.Oferta`, `RegistroOfertaService`, `EvaluadorOfertas`.
- `IOfertaRepository` (Application) implementado por `OfertaRepository`
  (Infrastructure), con índice único compuesto `(licitacion_id, proveedor_id)`.
- Depende de `ILicitacionRepository` e `IProveedorRepository` para cargar las
  entidades relacionadas antes de aplicar las reglas de dominio.

## Entradas
`RegistrarOfertaRequest { ProveedorId, MontoOfertadoCRC }` (dentro del contexto de una
licitación), `ActualizarOfertaRequest { MontoOfertadoCRC }`.

## Salidas
`OfertaDto { Id, LicitacionId, ProveedorId, ProveedorNombre, MontoOfertadoCRC,
FechaRegistro }`, `ResultadoPaginado<OfertaDto>`.

## Reglas de negocio (las cinco de la sección 8 del enunciado)
1. Monto ofertado > 0.
2. Monto ofertado ≤ presupuesto estimado de la licitación (igual al presupuesto es
   válido).
3. La licitación debe estar en estado Publicada.
4. La licitación no debe estar vencida (fecha/hora actual ≥ fecha de cierre).
5. Un proveedor no puede registrar más de una oferta para la misma licitación.

Todas se aplican tanto al **registrar** como al **editar** una oferta (editar se
implementa como "validar como si fuera una oferta nueva del mismo proveedor,
descartando la anterior solo si la nueva es válida", para no dejar el sistema en un
estado inconsistente si la validación falla a mitad de camino).

## Errores
| Situación | Excepción | HTTP |
|---|---|---|
| Licitación no publicada | `LicitacionNoPublicadaException` | 422 |
| Licitación vencida | `LicitacionVencidaException` | 422 |
| Monto ≤ 0 | `MontoOfertaInvalidoException` | 422 |
| Monto > presupuesto | `OfertaSuperaPresupuestoException` | 422 |
| Oferta duplicada del proveedor | `OfertaDuplicadaException` | 422 |
| Id inexistente (oferta, licitación o proveedor) | `RecursoNoEncontradoException` | 404 |

## Pruebas
Unitarias: `RegistroOfertaServiceTests` (7 casos) y `EvaluadorOfertasTests` (5 casos).
Integración: índice único compuesto forzado ante una condición de carrera simulada
entre dos `DbContext`. Funcionales: registro exitoso reflejado en la mejor oferta,
rechazo visible de oferta duplicada.
