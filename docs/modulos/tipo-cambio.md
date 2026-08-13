# Módulo: Tipo de cambio

## Propósito
Mantener el tipo de cambio CRC/USD que la interfaz usa para mostrar montos en dólares
como valor calculado, sin depender de una API externa y sin modificar los valores
oficiales almacenados en colones.

## Responsabilidades
- CRUD de registros de tipo de cambio (valor, fecha de vigencia).
- Garantizar que solo exista **un** tipo de cambio activo a la vez (activar uno
  desactiva automáticamente el anterior).
- Convertir un monto de CRC a USD usando el tipo de cambio activo, redondeado a dos
  decimales.
- Impedir eliminar el tipo de cambio actualmente activo (para no dejar el sistema sin
  ninguno configurado).

## Dependencias
- `Licitaciones.Domain.TiposCambio.TipoCambio` y `ConversorMoneda`.
- `ITipoCambioRepository` (Application) implementado en Infrastructure, con un índice
  único **parcial** en PostgreSQL (`WHERE activo = true`) como defensa adicional a
  nivel de base de datos.
- Consumido por `LicitacionesWebControllerBase` (precarga el activo una vez por
  solicitud MVC) y por el TagHelper `<monto>` (Web) para renderizar el equivalente en
  USD junto al valor en CRC.

## Entradas
`CrearTipoCambioRequest { crcPorUsd, FechaVigencia }`, `ActualizarTipoCambioRequest`
(misma forma; el campo se expone en JSON como `crcPorUsd` — ver nota de nomenclatura
más abajo).

## Salidas
`TipoCambioDto { Id, crcPorUsd, FechaVigencia, Activo }`.

## Reglas de negocio
- El tipo de cambio debe ser mayor que cero.
- Los valores oficiales se almacenan **únicamente** en CRC; la conversión a USD es
  siempre una representación calculada en el momento de mostrarse
  (`ConversorMoneda.ConvertirCrcAUsd`), nunca una operación que persista o modifique
  un valor ya guardado.
- Al crear el primer tipo de cambio (si no hay ninguno activo), se activa
  automáticamente; los siguientes se crean inactivos hasta que se activen
  explícitamente.

## Nota de nomenclatura JSON
La propiedad de dominio `CRCporUSD` se expone en la API como `crcPorUsd`
(`[JsonPropertyName]` explícito): la heurística de acrónimos de `System.Text.Json`
camelizaba `CRCporUSD` a `"crCporUsd"` (partiendo el acrónimo "CRC" a la mitad), un
resultado poco profesional en la documentación de Swagger que se corrigió
explícitamente — ver `bitacora-xp.md`, verificación en vivo, hallazgo #3.

## Errores
| Situación | Excepción | HTTP |
|---|---|---|
| Valor ≤ 0 | `TipoCambioInvalidoException` | 422 |
| Ningún tipo de cambio activo al intentar convertir | `TipoCambioNoConfiguradoException` | 422 |
| Eliminar el tipo de cambio activo | `ConflictoDeUnicidadException` | 409 |

## Pruebas
Unitarias: `ConversorMonedaTests` (4 casos). Integración: dato semilla activo presente
tras migrar. Funcionales: alternancia visual CRC/USD sin recargar la página.
