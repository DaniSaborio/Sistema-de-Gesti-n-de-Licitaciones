using FluentValidation;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Proveedores;
using Licitaciones.Application.TiposCambio;
using Microsoft.Extensions.DependencyInjection;

namespace Licitaciones.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProveedorService, ProveedorService>();
        services.AddScoped<ILicitacionService, LicitacionService>();
        services.AddScoped<IOfertaService, OfertaService>();
        services.AddScoped<INivelAprobacionService, NivelAprobacionService>();
        services.AddScoped<ITipoCambioService, TipoCambioService>();

        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        return services;
    }
}
