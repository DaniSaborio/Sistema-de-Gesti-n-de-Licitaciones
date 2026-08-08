namespace Licitaciones.Application.Ofertas;

public sealed record OfertaDto(
    Guid Id,
    Guid LicitacionId,
    Guid ProveedorId,
    string ProveedorNombre,
    decimal MontoOfertadoCRC,
    DateTimeOffset FechaRegistro);

public sealed record RegistrarOfertaRequest(Guid ProveedorId, decimal MontoOfertadoCRC);

public sealed record ActualizarOfertaRequest(decimal MontoOfertadoCRC);
