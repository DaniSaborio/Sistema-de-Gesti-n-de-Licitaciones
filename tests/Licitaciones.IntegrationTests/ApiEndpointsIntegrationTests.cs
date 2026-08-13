using System.Net;
using System.Net.Http.Json;
using Licitaciones.Application.Proveedores;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Licitaciones.IntegrationTests;

/// <summary>
/// Prueba los endpoints REST de extremo a extremo (aplicación + Infrastructure +
/// PostgreSQL real), sin mocks (sección 12.2: "pruebas de endpoints con
/// infraestructura real").
/// </summary>
[Collection("Postgres")]
public class ApiEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiEndpointsIntegrationTests(PostgresContainerFixture fixture, WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:LicitacionesDb"] = fixture.ConnectionString,
                });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task Crear_proveedor_persiste_y_se_puede_consultar_por_id()
    {
        var respuestaCreacion = await _client.PostAsJsonAsync("/api/v1/proveedores", new CrearProveedorRequest("Proveedor API Integración"));
        Assert.Equal(HttpStatusCode.Created, respuestaCreacion.StatusCode);

        var creado = await respuestaCreacion.Content.ReadFromJsonAsync<ProveedorDto>();
        Assert.NotNull(creado);

        var respuestaConsulta = await _client.GetAsync($"/api/v1/proveedores/{creado!.Id}");
        Assert.Equal(HttpStatusCode.OK, respuestaConsulta.StatusCode);

        var recuperado = await respuestaConsulta.Content.ReadFromJsonAsync<ProveedorDto>();
        Assert.Equal("Proveedor API Integración", recuperado!.Nombre);
    }

    [Fact]
    public async Task Crear_proveedor_duplicado_normalizado_responde_409_con_problem_details()
    {
        await _client.PostAsJsonAsync("/api/v1/proveedores", new CrearProveedorRequest("Duplicado Integración"));

        var respuesta = await _client.PostAsJsonAsync("/api/v1/proveedores", new CrearProveedorRequest("duplicado   integración"));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Consultar_un_proveedor_inexistente_responde_404()
    {
        var respuesta = await _client.GetAsync($"/api/v1/proveedores/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}
