using Licitaciones.Application.Common;
using Licitaciones.Domain.Ofertas;

namespace Licitaciones.Application.Ofertas;

public interface IOfertaRepository
{
    Task<Oferta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Oferta>> ListarPorLicitacionAsync(Guid licitacionId, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<Oferta>> ListarAsync(
        ConsultaPaginada consulta, Guid? licitacionId, Guid? proveedorId, CancellationToken cancellationToken = default);
    Task<decimal?> ObtenerMontoMinimoAsync(Guid licitacionId, CancellationToken cancellationToken = default);
    void Agregar(Oferta oferta);
    void Eliminar(Oferta oferta);
}
