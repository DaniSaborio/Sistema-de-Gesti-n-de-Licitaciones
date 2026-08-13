# Módulo: Niveles de aprobación

## Propósito
Configurar rangos de monto (CRC) con su aprobador correspondiente, y resolver
automáticamente quién debe aprobar una adjudicación según el monto de la mejor
oferta — sin codificar los rangos como condicionales fijos en el código.

## Responsabilidades
- CRUD de rangos: monto mínimo, monto máximo (opcional = rango abierto) y aprobador.
- Validar que los rangos no se solapen entre sí.
- Validar que exista **como máximo un** rango abierto (sin monto máximo) a la vez.
- Resolver, dado un monto, el aprobador correspondiente mediante búsqueda
  parametrizada sobre la tabla de niveles.

## Dependencias
- `Licitaciones.Domain.NivelesAprobacion.NivelAprobacion` y
  `ResolutorNivelAprobacion` (búsqueda y validación de rangos).
- `INivelAprobacionRepository` (Application) implementado en Infrastructure.
- Consumido por `LicitacionService.ObtenerMejorOfertaAsync` para resolver el
  aprobador de la mejor oferta de una licitación.

## Entradas
`CrearNivelAprobacionRequest { MontoMinimoCRC, MontoMaximoCRC?, Aprobador }`,
`ActualizarNivelAprobacionRequest` (misma forma).

## Salidas
`NivelAprobacionDto { Id, MontoMinimoCRC, MontoMaximoCRC, Aprobador }`.

## Reglas de negocio
- Monto mínimo > 0; si hay monto máximo, debe ser mayor que el mínimo.
- **Cardinalidad de rangos abiertos primero, solape después**: dos rangos sin monto
  máximo siempre se solapan matemáticamente en su cola (ambos se extienden a
  infinito), así que si se validara el solape genérico primero, el mensaje de error
  específico ("solo puede existir un rango abierto") quedaría oculto detrás del
  genérico ("el rango se solapa"). Este orden de validación se descubrió mediante un
  test en rojo real durante el desarrollo (ver `bitacora-xp.md`, iteración 0) y quedó
  documentado como comentario en el propio código
  (`ResolutorNivelAprobacion.ValidarNuevoRango`).
- La resolución del aprobador (`Resolver`) es una búsqueda `FirstOrDefault` sobre la
  tabla de niveles, nunca una cadena de `if`/`else` con montos fijos, tal como exige
  el enunciado.

## Errores
| Situación | Excepción | HTTP |
|---|---|---|
| Monto mínimo ≤ 0 / máximo ≤ mínimo / aprobador vacío | `RangoAprobacionInvalidoException` | 422 |
| Rango solapado con uno existente | `RangoAprobacionSolapadoException` | 422 |
| Segundo rango abierto | `MultiplesRangosAbiertosException` | 422 |
| Sin nivel configurado para el monto | `NivelAprobacionNoConfiguradoException` | 422 |

## Pruebas
Unitarias: `ResolutorNivelAprobacionTests` (7 casos, incluyendo el caso que documenta
el ciclo rojo-verde-refactor real del proyecto).
