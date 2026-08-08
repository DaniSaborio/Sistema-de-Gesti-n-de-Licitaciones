using Licitaciones.Application.Common;
using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Application.Licitaciones;

public interface ILicitacionRepository
{
    Task<Licitacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteCodigoNormalizadoAsync(string codigoNormalizado, Guid? excluirId, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<Licitacion>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default);
    void Agregar(Licitacion licitacion);
}
