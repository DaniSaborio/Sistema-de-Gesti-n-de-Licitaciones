namespace Licitaciones.Application.NivelesAprobacion;

public sealed record NivelAprobacionDto(
    Guid Id,
    decimal MontoMinimoCRC,
    decimal? MontoMaximoCRC,
    string Aprobador);

public sealed record CrearNivelAprobacionRequest(decimal MontoMinimoCRC, decimal? MontoMaximoCRC, string Aprobador);

public sealed record ActualizarNivelAprobacionRequest(decimal MontoMinimoCRC, decimal? MontoMaximoCRC, string Aprobador);
