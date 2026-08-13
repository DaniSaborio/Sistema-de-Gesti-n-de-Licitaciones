using Licitaciones.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Testcontainers.PostgreSql;
using Xunit;

namespace Licitaciones.FunctionalTests;

/// <summary>
/// Levanta la aplicación real (Web + Api en un solo proceso) sobre Kestrel con un
/// puerto real, contra un PostgreSQL de Testcontainers, y un navegador Chromium de
/// Playwright headless — el flujo funcional de extremo a extremo exigido por la
/// sección 12.3 del enunciado. Requiere Docker; se ejecuta en GitHub Actions.
/// </summary>
public sealed class AplicacionWebFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("licitaciones_e2e")
        .WithUsername("licitaciones")
        .WithPassword("licitaciones")
        .Build();

    private WebApplicationFactory<Program>? _factory;
    private IPlaywright? _playwright;

    public string BaseUrl { get; private set; } = string.Empty;
    public IBrowser Browser { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseKestrel();
            builder.UseUrls("http://127.0.0.1:0");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:LicitacionesDb"] = _postgres.GetConnectionString(),
                });
            });
        });

        // Fuerza la creación del servidor Kestrel real (no el TestServer en memoria)
        // para que Playwright, que corre en un proceso de navegador aparte, pueda
        // conectarse por HTTP igual que un usuario real.
        var servidor = _factory.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
        var direccion = servidor.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()!
            .Addresses.First();
        BaseUrl = direccion;

        using (var scope = _factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<LicitacionesDbContext>().Database.MigrateAsync();
        }

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        _playwright?.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition("AplicacionWeb")]
public sealed class AplicacionWebCollection : ICollectionFixture<AplicacionWebFixture>;
