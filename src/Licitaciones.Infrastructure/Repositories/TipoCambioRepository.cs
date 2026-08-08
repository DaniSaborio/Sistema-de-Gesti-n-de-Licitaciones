using Licitaciones.Application.Common;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.TiposCambio;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Repositories;

public sealed class TipoCambioRepository(LicitacionesDbContext dbContext) : ITipoCambioRepository
{
    public Task<TipoCambio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.TiposCambio.FirstOrDefaultAsync(tc => tc.Id == id, cancellationToken);

    public Task<TipoCambio?> ObtenerActivoAsync(CancellationToken cancellationToken = default) =>
        dbContext.TiposCambio.FirstOrDefaultAsync(tc => tc.Activo, cancellationToken);

    public Task<List<TipoCambio>> ListarTodosAsync(CancellationToken cancellationToken = default) =>
        dbContext.TiposCambio.OrderByDescending(tc => tc.FechaVigencia).ToListAsync(cancellationToken);

    public async Task<ResultadoPaginado<TipoCambio>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default)
    {
        var query = dbContext.TiposCambio.OrderByDescending(tc => tc.FechaVigencia).AsQueryable();
        return await query.APaginadoAsync(consulta, cancellationToken);
    }

    public void Agregar(TipoCambio tipoCambio) => dbContext.TiposCambio.Add(tipoCambio);

    public void Eliminar(TipoCambio tipoCambio) => dbContext.TiposCambio.Remove(tipoCambio);
}
