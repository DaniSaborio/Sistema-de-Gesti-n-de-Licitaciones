using Licitaciones.Application.Common;
using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Proveedores;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Repositories;

public sealed class ProveedorRepository(LicitacionesDbContext dbContext) : IProveedorRepository
{
    public Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Proveedores.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExisteNombreNormalizadoAsync(string nombreNormalizado, Guid? excluirId, CancellationToken cancellationToken = default) =>
        dbContext.Proveedores.AnyAsync(
            p => p.NombreNormalizado == nombreNormalizado && (excluirId == null || p.Id != excluirId),
            cancellationToken);

    public async Task<ResultadoPaginado<Proveedor>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Proveedores.AsQueryable();

        if (!string.IsNullOrWhiteSpace(consulta.Busqueda))
        {
            var termino = consulta.Busqueda.Trim().ToUpperInvariant();
            query = query.Where(p => p.NombreNormalizado.Contains(termino));
        }

        query = consulta.OrdenarPor?.ToLowerInvariant() switch
        {
            "nombre" when consulta.Descendente => query.OrderByDescending(p => p.Nombre),
            "nombre" => query.OrderBy(p => p.Nombre),
            "createdat" when consulta.Descendente => query.OrderByDescending(p => p.CreatedAt),
            "createdat" => query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Nombre),
        };

        return await query.APaginadoAsync(consulta, cancellationToken);
    }

    public Task<List<Proveedor>> ListarActivosAsync(CancellationToken cancellationToken = default) =>
        dbContext.Proveedores.OrderBy(p => p.Nombre).ToListAsync(cancellationToken);

    public void Agregar(Proveedor proveedor) => dbContext.Proveedores.Add(proveedor);
}
