using Licitaciones.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using Testcontainers.PostgreSql;
using Xunit;

namespace Licitaciones.FunctionalTests;

/// <summary>
/// WebApplicationFactory arranca por defecto un TestServer en memoria (sin socket
/// real), inútil para Playwright, que corre en un proceso de navegador aparte y
/// necesita conectarse por HTTP real. Este factory fuerza a Kestrel a escuchar en un
/// puerto real (patrón documentado por Microsoft para combinar WebApplicationFactory
/// con pruebas de navegador) y expone la dirección efectivamente asignada por el SO.
/// </summary>
internal sealed class KestrelWebApplicationFactory : WebApplicationFactory<Program>
{
    public string BaseUrl { get; private set; } = string.Empty;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureWebHost(webHost => webHost.UseUrls("http://127.0.0.1:0"));

        var host = builder.Build();
        host.Start();

        var direcciones = host.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses;
        BaseUrl = direcciones.First();

        return host;
    }
}

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

    private KestrelWebApplicationFactory? _factory;
    private IPlaywright? _playwright;

    public string BaseUrl => _factory?.BaseUrl ?? string.Empty;
    public IBrowser Browser { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Program.cs lee la cadena de conexión de forma síncrona y temprana (antes de
        // builder.Build()), así que un ConfigureAppConfiguration inyectado vía
        // WithWebHostBuilder llega demasiado tarde para hosting mínimo (Program.cs de
        // top-level statements). Las variables de entorno sí las lee
        // WebApplicationBuilder.CreateBuilder() desde la primera línea.
        Environment.SetEnvironmentVariable("ConnectionStrings__LicitacionesDb", _postgres.GetConnectionString());

        _factory = new KestrelWebApplicationFactory();
        // Forzar la creación del host (CreateHost ya deja BaseUrl con el puerto real).
        _ = _factory.Services;

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
