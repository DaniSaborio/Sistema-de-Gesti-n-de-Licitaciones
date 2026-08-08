using Licitaciones.Application.Common;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Proveedores;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.Common;
using Licitaciones.Infrastructure.Clock;
using Licitaciones.Infrastructure.Persistence;
using Licitaciones.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Licitaciones.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "LicitacionesDb";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"No se encontró la cadena de conexión '{ConnectionStringName}'. Configúrela mediante variables de entorno o secretos.");

        services.AddDbContext<LicitacionesDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IClock, SystemClock>();

        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<ILicitacionRepository, LicitacionRepository>();
        services.AddScoped<IOfertaRepository, OfertaRepository>();
        services.AddScoped<INivelAprobacionRepository, NivelAprobacionRepository>();
        services.AddScoped<ITipoCambioRepository, TipoCambioRepository>();

        return services;
    }
}
