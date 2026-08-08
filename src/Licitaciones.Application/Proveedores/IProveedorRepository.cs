using Licitaciones.Application.Common;
using Licitaciones.Domain.Proveedores;

namespace Licitaciones.Application.Proveedores;

public interface IProveedorRepository
{
    Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreNormalizadoAsync(string nombreNormalizado, Guid? excluirId, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<Proveedor>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default);
    Task<List<Proveedor>> ListarActivosAsync(CancellationToken cancellationToken = default);
    void Agregar(Proveedor proveedor);
}
