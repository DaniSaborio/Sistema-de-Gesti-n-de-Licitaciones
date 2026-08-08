using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.NivelesAprobacion;
using Licitaciones.Domain.Ofertas;
using Licitaciones.Domain.Proveedores;
using Licitaciones.Domain.TiposCambio;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence;

public sealed class LicitacionesDbContext(DbContextOptions<LicitacionesDbContext> options) : DbContext(options)
{
    public DbSet<Licitacion> Licitaciones => Set<Licitacion>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Oferta> Ofertas => Set<Oferta>();
    public DbSet<NivelAprobacion> NivelesAprobacion => Set<NivelAprobacion>();
    public DbSet<TipoCambio> TiposCambio => Set<TipoCambio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LicitacionesDbContext).Assembly);
    }
}
