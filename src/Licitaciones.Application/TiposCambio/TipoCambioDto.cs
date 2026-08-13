using System.Text.Json.Serialization;

namespace Licitaciones.Application.TiposCambio;

public sealed record TipoCambioDto(
    Guid Id,
    [property: JsonPropertyName("crcPorUsd")] decimal CRCporUSD,
    DateTimeOffset FechaVigencia,
    bool Activo);

public sealed record CrearTipoCambioRequest([property: JsonPropertyName("crcPorUsd")] decimal CRCporUSD, DateTimeOffset FechaVigencia);

public sealed record ActualizarTipoCambioRequest([property: JsonPropertyName("crcPorUsd")] decimal CRCporUSD, DateTimeOffset FechaVigencia);
