using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Playwright;
using Testcontainers.PostgreSql;
using Xunit;

namespace Licitaciones.FunctionalTests;

/// <summary>
/// Levanta la aplicación real como un proceso independiente (exactamente como se
/// ejecutaría en producción o en el Dockerfile), contra un PostgreSQL de
/// Testcontainers, y un navegador Chromium de Playwright headless — el flujo
/// funcional de extremo a extremo exigido por la sección 12.3 del enunciado.
/// Se prefirió esto a WebApplicationFactory + Kestrel embebido: en dos intentos
/// reales contra el runner de GitHub Actions, WebApplicationFactory nunca llegó a
/// enlazar un puerto real (el servidor quedaba escuchando en "http://127.0.0.1:0",
/// el puerto configurado, en vez del puerto real asignado por el sistema operativo),
/// mientras que un proceso real de `dotnet` es exactamente lo mismo que ejecuta
/// `docker compose up` o Kubernetes — más simple y más fiel a un escenario real.
/// Requiere Docker (para Testcontainers); se ejecuta en GitHub Actions.
/// </summary>
public sealed class AplicacionWebFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("licitaciones_e2e")
        .WithUsername("licitaciones")
        .WithPassword("licitaciones")
        .Build();

    private readonly HttpClient _httpClient = new();
    private Process? _proceso;
    private IPlaywright? _playwright;

    public string BaseUrl { get; private set; } = string.Empty;
    public IBrowser Browser { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var puerto = ObtenerPuertoLibre();
        BaseUrl = $"http://127.0.0.1:{puerto}";

        var rutaEnsamblado = typeof(Program).Assembly.Location;

        _proceso = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                ArgumentList = { rutaEnsamblado },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment =
                {
                    ["ASPNETCORE_URLS"] = BaseUrl,
                    ["ASPNETCORE_ENVIRONMENT"] = "Development",
                    ["ConnectionStrings__LicitacionesDb"] = _postgres.GetConnectionString(),
                },
            },
        };
        _proceso.Start();

        await EsperarListoAsync();

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    private static int ObtenerPuertoLibre()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var puerto = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return puerto;
    }

    private async Task EsperarListoAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        Exception? ultimoError = null;

        while (!cts.IsCancellationRequested)
        {
            if (_proceso!.HasExited)
            {
                var salida = await _proceso.StandardOutput.ReadToEndAsync();
                var error = await _proceso.StandardError.ReadToEndAsync();
                throw new InvalidOperationException(
                    $"El proceso de la aplicación terminó antes de tiempo (código {_proceso.ExitCode}).\nSTDOUT: {salida}\nSTDERR: {error}");
            }

            try
            {
                var respuesta = await _httpClient.GetAsync($"{BaseUrl}/health/live", cts.Token);
                if (respuesta.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                ultimoError = ex;
            }

            await Task.Delay(500, CancellationToken.None);
        }

        throw new TimeoutException($"La aplicación no respondió en {BaseUrl}/health/live a tiempo.", ultimoError);
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        _playwright?.Dispose();
        _httpClient.Dispose();

        if (_proceso is not null && !_proceso.HasExited)
        {
            _proceso.Kill(entireProcessTree: true);
        }

        _proceso?.Dispose();

        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition("AplicacionWeb")]
public sealed class AplicacionWebCollection : ICollectionFixture<AplicacionWebFixture>;
