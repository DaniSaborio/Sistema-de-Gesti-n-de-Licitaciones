# Módulo: Licitaciones

## Propósito
Administrar el ciclo de vida de una licitación: creación, publicación, recepción de
ofertas y cierre, además de exponer la mejor oferta y su aprobador.

## Responsabilidades
- CRUD completo con código único (normalizado).
- Máquina de estados: Borrador → Publicada → Cerrada, con cierre implícito cuando se
  alcanza la fecha de cierre y reapertura explícita y auditada.
- Orquestar la consulta de mejor oferta, clasificación de ahorro y aprobador
  correspondiente (delegando el cálculo puro a los servicios de dominio de Ofertas y
  Niveles de aprobación).
- Impedir reducir el presupuesto por debajo de una oferta ya registrada.

## Dependencias
- `Licitaciones.Domain.Licitaciones.Licitacion` (entidad) y servicios de dominio de
  Ofertas (`RegistroOfertaService`, `EvaluadorOfertas`) y Niveles de aprobación
  (`ResolutorNivelAprobacion`).
- `ILicitacionRepository`, `IOfertaRepository`, `INivelAprobacionRepository`
  (Application) implementados en Infrastructure.
- Expuesto tanto por `Licitaciones.Web.Controllers.LicitacionesController` (MVC) como
  por `Licitaciones.Api.Controllers.V1.LicitacionesController` (REST), ambos sobre el
  mismo `LicitacionService`.

## Entradas
`CrearLicitacionRequest { Codigo, Titulo, FechaCierre, PresupuestoEstimadoCRC }`,
`ActualizarLicitacionRequest`, `CambiarEstadoLicitacionRequest { EstadoDestino }`.

## Salidas
`LicitacionDto { Id, Codigo, Titulo, Estado, CerradaFuncionalmente, FechaCierre,
PresupuestoEstimadoCRC, CreatedAt, UpdatedAt }`, `MejorOfertaDto { Clasificacion,
OfertaId, ProveedorId, MontoOfertadoCRC, PorcentajeAhorro, Aprobador }`.

## Reglas de negocio
- Código único (normalizado), título obligatorio, presupuesto > 0, fecha de cierre
  futura al crear.
- Transiciones permitidas: Borrador→Publicada (solo si la fecha de cierre no se
  alcanzó), Borrador→Cerrada, Publicada→Cerrada. Publicada→Borrador y
  Cerrada→cualquier estado **no** están permitidas por transición directa; la única
  vía de reapertura es la acción explícita `Reabrir`.
- `EstaCerradaFuncionalmente` es verdadero si el estado es Cerrada **o** si la fecha
  de cierre ya pasó, aunque el campo Estado siga en Publicada — así una licitación
  vencida no admite nuevas ofertas aunque nadie haya presionado "Cerrar".
- El presupuesto no puede reducirse por debajo del monto de la oferta más baja ya
  registrada.

## Errores
| Situación | Excepción | HTTP |
|---|---|---|
| Código duplicado | `ConflictoDeUnicidadException` | 409 |
| Presupuesto ≤ 0 / fecha de cierre no futura | `PresupuestoInvalidoException` / `FechaCierreInvalidaException` | 422 |
| Transición de estado no permitida | `TransicionEstadoInvalidaException` | 422 |
| Id inexistente | `RecursoNoEncontradoException` | 404 |

## Pruebas
Unitarias: `LicitacionTests` (9 casos). Integración: persistencia y recuperación,
datos semilla. Funcionales: ciclo completo crear→publicar→ofertar→ver mejor oferta,
navegación desde landing page.
