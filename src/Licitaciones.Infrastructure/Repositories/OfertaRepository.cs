using Licitaciones.Application.Common;
using Licitaciones.Application.Ofertas;
using Licitaciones.Domain.Ofertas;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Repositories;

public sealed class OfertaRepository(LicitacionesDbContext dbContext) : IOfertaRepository
{
    public Task<Oferta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Ofertas.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<List<Oferta>> ListarPorLicitacionAsync(Guid licitacionId, CancellationToken cancellationToken = default) =>
        dbContext.Ofertas.Where(o => o.LicitacionId == licitacionId).ToListAsync(cancellationToken);

    public async Task<ResultadoPaginado<Oferta>> ListarAsync(
        ConsultaPaginada consulta, Guid? licitacionId, Guid? proveedorId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Ofertas.AsQueryable();

        if (licitacionId is not null)
        {
            query = query.Where(o => o.LicitacionId == licitacionId);
        }

        if (proveedorId is not null)
        {
            query = query.Where(o => o.ProveedorId == proveedorId);
        }

        query = consulta.OrdenarPor?.ToLowerInvariant() switch
        {
            "monto" when consulta.Descendente => query.OrderByDescending(o => o.MontoOfertadoCRC),
            "monto" => query.OrderBy(o => o.MontoOfertadoCRC),
            _ when consulta.Descendente => query.OrderByDescending(o => o.FechaRegistro),
            _ => query.OrderBy(o => o.FechaRegistro),
        };

        return await query.APaginadoAsync(consulta, cancellationToken);
    }

    public Task<decimal?> ObtenerMontoMinimoAsync(Guid licitacionId, CancellationToken cancellationToken = default) =>
        dbContext.Ofertas.Where(o => o.LicitacionId == licitacionId)
            .Select(o => (decimal?)o.MontoOfertadoCRC)
            .OrderBy(m => m)
            .FirstOrDefaultAsync(cancellationToken);

    public void Agregar(Oferta oferta) => dbContext.Ofertas.Add(oferta);

    public void Eliminar(Oferta oferta) => dbContext.Ofertas.Remove(oferta);
}
