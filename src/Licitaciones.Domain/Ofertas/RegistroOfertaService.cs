using Licitaciones.Domain.Common;
using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Domain.Ofertas;

/// <summary>
/// Concentra las reglas de aceptación de una oferta (sección 8 del enunciado).
/// Es un servicio de dominio puro: recibe el estado ya cargado por la capa
/// Application (licitación y ofertas existentes) y no depende de infraestructura,
/// lo que permite probarlo íntegramente con xUnit sin base de datos.
/// </summary>
public static class RegistroOfertaService
{
    public static Oferta Registrar(
        Licitacion licitacion,
        Guid proveedorId,
        decimal montoOfertadoCRC,
        IReadOnlyCollection<Oferta> ofertasExistentes,
        IClock clock)
    {
        if (licitacion.Estado != EstadoLicitacion.Publicada)
        {
            throw new LicitacionNoPublicadaException();
        }

        if (licitacion.EstaCerradaFuncionalmente(clock))
        {
            throw new LicitacionVencidaException();
        }

        if (montoOfertadoCRC <= 0)
        {
            throw new MontoOfertaInvalidoException();
        }

        if (montoOfertadoCRC > licitacion.PresupuestoEstimadoCRC)
        {
            throw new OfertaSuperaPresupuestoException();
        }

        if (ofertasExistentes.Any(o => o.ProveedorId == proveedorId))
        {
            throw new OfertaDuplicadaException();
        }

        return Oferta.Crear(licitacion.Id, proveedorId, montoOfertadoCRC, clock);
    }
}
