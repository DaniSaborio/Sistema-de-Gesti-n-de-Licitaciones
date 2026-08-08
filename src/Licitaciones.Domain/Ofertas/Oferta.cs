using Licitaciones.Domain.Common;

namespace Licitaciones.Domain.Ofertas;

public sealed class Oferta : Entity
{
    public Guid LicitacionId { get; private set; }
    public Guid ProveedorId { get; private set; }
    public decimal MontoOfertadoCRC { get; private set; }
    public DateTimeOffset FechaRegistro { get; private set; }

    private Oferta() { }

    internal static Oferta Crear(Guid licitacionId, Guid proveedorId, decimal montoOfertadoCRC, IClock clock)
    {
        var ahora = clock.UtcNow;
        return new Oferta
        {
            LicitacionId = licitacionId,
            ProveedorId = proveedorId,
            MontoOfertadoCRC = montoOfertadoCRC,
            FechaRegistro = ahora,
            CreatedAt = ahora,
            UpdatedAt = ahora,
        };
    }
}
