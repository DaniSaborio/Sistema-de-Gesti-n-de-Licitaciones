using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Licitaciones.Api.Swagger;

/// <summary>Genera un documento OpenAPI por cada versión de API descubierta (sección 10).</summary>
public sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Sistema de Gestión de Licitaciones — API REST",
                Version = description.ApiVersion.ToString(),
                Description = "Licitaciones, proveedores, ofertas, niveles de aprobación y tipos de cambio (CRC como fuente de verdad).",
            });
        }
    }
}
