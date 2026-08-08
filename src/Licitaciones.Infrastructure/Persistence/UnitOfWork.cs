using Licitaciones.Application.Common;

namespace Licitaciones.Infrastructure.Persistence;

public sealed class UnitOfWork(LicitacionesDbContext dbContext) : IUnitOfWork
{
    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
