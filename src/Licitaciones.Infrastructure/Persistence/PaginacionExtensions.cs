using Licitaciones.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence;

internal static class PaginacionExtensions
{
    public static async Task<ResultadoPaginado<T>> APaginadoAsync<T>(
        this IQueryable<T> consulta, ConsultaPaginada parametros, CancellationToken cancellationToken)
    {
        var total = await consulta.CountAsync(cancellationToken);
        var elementos = await consulta
            .Skip((parametros.Pagina - 1) * parametros.TamanoPagina)
            .Take(parametros.TamanoPagina)
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<T>(elementos, total, parametros.Pagina, parametros.TamanoPagina);
    }
}
