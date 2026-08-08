using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Licitaciones.Infrastructure.Persistence;

/// <summary>
/// Fábrica usada exclusivamente por las herramientas de `dotnet ef` en tiempo de
/// diseño (creación de migraciones); en tiempo de ejecución la aplicación
/// configura el DbContext mediante <see cref="DependencyInjection.AddInfrastructure"/>.
/// </summary>
public sealed class LicitacionesDbContextFactory : IDesignTimeDbContextFactory<LicitacionesDbContext>
{
    public LicitacionesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__LICITACIONESDB")
            ?? "Host=localhost;Port=5432;Database=licitaciones;Username=licitaciones;Password=licitaciones";

        var optionsBuilder = new DbContextOptionsBuilder<LicitacionesDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LicitacionesDbContext(optionsBuilder.Options);
    }
}
