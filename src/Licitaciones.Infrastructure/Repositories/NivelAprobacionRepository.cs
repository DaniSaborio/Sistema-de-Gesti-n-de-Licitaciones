using Licitaciones.Application.Common;
using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Domain.NivelesAprobacion;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Repositories;

public sealed class NivelAprobacionRepository(LicitacionesDbContext dbContext) : INivelAprobacionRepository
{
    public Task<NivelAprobacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.NivelesAprobacion.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task<List<NivelAprobacion>> ListarTodosAsync(CancellationToken cancellationToken = default) =>
        dbContext.NivelesAprobacion.OrderBy(n => n.MontoMinimoCRC).ToListAsync(cancellationToken);

    public async Task<ResultadoPaginado<NivelAprobacion>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default)
    {
        var query = dbContext.NivelesAprobacion.OrderBy(n => n.MontoMinimoCRC).AsQueryable();
        return await query.APaginadoAsync(consulta, cancellationToken);
    }

    public void Agregar(NivelAprobacion nivel) => dbContext.NivelesAprobacion.Add(nivel);

    public void Eliminar(NivelAprobacion nivel) => dbContext.NivelesAprobacion.Remove(nivel);
}
