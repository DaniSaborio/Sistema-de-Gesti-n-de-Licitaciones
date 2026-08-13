using Licitaciones.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence;

public sealed class UnitOfWork(LicitacionesDbContext dbContext) : IUnitOfWork
{
    public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictoDeConcurrenciaException(
                "El recurso fue modificado por otra operación antes de guardar los cambios.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new ErrorDeIntegridadDeDatosException(
                "La operación viola una restricción de integridad de datos.", ex);
        }
    }
}
