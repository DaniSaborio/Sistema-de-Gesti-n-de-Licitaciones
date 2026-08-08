namespace Licitaciones.Application.Proveedores;

public sealed record ProveedorDto(
    Guid Id,
    string Nombre,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CrearProveedorRequest(string Nombre);

public sealed record ActualizarProveedorRequest(string Nombre);
