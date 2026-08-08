using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Ofertas;

namespace Licitaciones.Application.Licitaciones;

public sealed record LicitacionDto(
    Guid Id,
    string Codigo,
    string Titulo,
    EstadoLicitacion Estado,
    bool CerradaFuncionalmente,
    DateTimeOffset FechaCierre,
    decimal PresupuestoEstimadoCRC,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record MejorOfertaDto(
    ClasificacionOferta Clasificacion,
    Guid? OfertaId,
    Guid? ProveedorId,
    decimal? MontoOfertadoCRC,
    decimal? PorcentajeAhorro,
    string? Aprobador);

public sealed record CrearLicitacionRequest(
    string Codigo,
    string Titulo,
    DateTimeOffset FechaCierre,
    decimal PresupuestoEstimadoCRC);

public sealed record ActualizarLicitacionRequest(
    string Titulo,
    DateTimeOffset FechaCierre,
    decimal PresupuestoEstimadoCRC);

public sealed record CambiarEstadoLicitacionRequest(EstadoLicitacion EstadoDestino);
