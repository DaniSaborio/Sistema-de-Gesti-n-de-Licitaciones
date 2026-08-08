using Licitaciones.Application.Common;
using Licitaciones.Domain.NivelesAprobacion;

namespace Licitaciones.Application.NivelesAprobacion;

public interface INivelAprobacionRepository
{
    Task<NivelAprobacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<NivelAprobacion>> ListarTodosAsync(CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<NivelAprobacion>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default);
    void Agregar(NivelAprobacion nivel);
    void Eliminar(NivelAprobacion nivel);
}
