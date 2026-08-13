using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Licitaciones.IntegrationTests;

/// <summary>
/// Levanta un contenedor PostgreSQL 16 real (Testcontainers) compartido entre las
/// pruebas de la colección "Postgres" y aplica las migraciones una sola vez
/// (sección 12.2 del enunciado: persistencia contra PostgreSQL real, nunca SQLite).
/// Requiere un daemon de Docker disponible; se ejecuta en GitHub Actions y en
/// cualquier máquina de desarrollo con Docker, no en este entorno de generación.
/// </summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _contenedor = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("licitaciones_test")
        .WithUsername("licitaciones")
        .WithPassword("licitaciones")
        .Build();

    public string ConnectionString => _contenedor.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _contenedor.StartAsync();

        var options = new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var dbContext = new LicitacionesDbContext(options);
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _contenedor.DisposeAsync();

    public LicitacionesDbContext CrearDbContext()
    {
        var options = new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new LicitacionesDbContext(options);
    }
}

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresContainerFixture>;
