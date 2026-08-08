using Licitaciones.Application.Common;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Repositories;

public sealed class LicitacionRepository(LicitacionesDbContext dbContext) : ILicitacionRepository
{
    public Task<Licitacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Licitaciones.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<bool> ExisteCodigoNormalizadoAsync(string codigoNormalizado, Guid? excluirId, CancellationToken cancellationToken = default) =>
        dbContext.Licitaciones.AnyAsync(
            l => l.CodigoNormalizado == codigoNormalizado && (excluirId == null || l.Id != excluirId),
            cancellationToken);

    public async Task<ResultadoPaginado<Licitacion>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Licitaciones.AsQueryable();

        if (!string.IsNullOrWhiteSpace(consulta.Busqueda))
        {
            var termino = consulta.Busqueda.Trim().ToUpperInvariant();
            query = query.Where(l => l.CodigoNormalizado.Contains(termino) || l.Titulo.ToUpper().Contains(termino));
        }

        query = consulta.OrdenarPor?.ToLowerInvariant() switch
        {
            "codigo" when consulta.Descendente => query.OrderByDescending(l => l.Codigo),
            "codigo" => query.OrderBy(l => l.Codigo),
            "fechacierre" when consulta.Descendente => query.OrderByDescending(l => l.FechaCierre),
            "fechacierre" => query.OrderBy(l => l.FechaCierre),
            "presupuesto" when consulta.Descendente => query.OrderByDescending(l => l.PresupuestoEstimadoCRC),
            "presupuesto" => query.OrderBy(l => l.PresupuestoEstimadoCRC),
            "estado" when consulta.Descendente => query.OrderByDescending(l => l.Estado),
            "estado" => query.OrderBy(l => l.Estado),
            _ => query.OrderByDescending(l => l.CreatedAt),
        };

        return await query.APaginadoAsync(consulta, cancellationToken);
    }

    public void Agregar(Licitacion licitacion) => dbContext.Licitaciones.Add(licitacion);
}
