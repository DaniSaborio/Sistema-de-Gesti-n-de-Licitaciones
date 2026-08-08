using Licitaciones.Application.Common;
using Licitaciones.Domain.TiposCambio;

namespace Licitaciones.Application.TiposCambio;

public interface ITipoCambioRepository
{
    Task<TipoCambio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TipoCambio?> ObtenerActivoAsync(CancellationToken cancellationToken = default);
    Task<List<TipoCambio>> ListarTodosAsync(CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<TipoCambio>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default);
    void Agregar(TipoCambio tipoCambio);
    void Eliminar(TipoCambio tipoCambio);
}
