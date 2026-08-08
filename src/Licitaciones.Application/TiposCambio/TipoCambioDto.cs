namespace Licitaciones.Application.TiposCambio;

public sealed record TipoCambioDto(
    Guid Id,
    decimal CRCporUSD,
    DateTimeOffset FechaVigencia,
    bool Activo);

public sealed record CrearTipoCambioRequest(decimal CRCporUSD, DateTimeOffset FechaVigencia);

public sealed record ActualizarTipoCambioRequest(decimal CRCporUSD, DateTimeOffset FechaVigencia);
