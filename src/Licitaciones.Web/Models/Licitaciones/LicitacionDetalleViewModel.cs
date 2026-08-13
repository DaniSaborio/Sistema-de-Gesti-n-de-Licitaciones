using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Proveedores;

namespace Licitaciones.Web.Models.Licitaciones;

public sealed class LicitacionDetalleViewModel
{
    public required LicitacionDto Licitacion { get; init; }
    public required MejorOfertaDto MejorOferta { get; init; }
    public required IReadOnlyList<OfertaDto> Ofertas { get; init; }
    public required IReadOnlyList<ProveedorDto> ProveedoresDisponibles { get; init; }
}
