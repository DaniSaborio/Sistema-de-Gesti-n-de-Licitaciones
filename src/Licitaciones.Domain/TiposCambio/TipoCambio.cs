using Licitaciones.Domain.Common;

namespace Licitaciones.Domain.TiposCambio;

public sealed class TipoCambio : Entity
{
    public decimal CRCporUSD { get; private set; }
    public DateTimeOffset FechaVigencia { get; private set; }
    public bool Activo { get; private set; }

    private TipoCambio() { }

    public static TipoCambio Crear(decimal crCporUsd, DateTimeOffset fechaVigencia, IClock clock)
    {
        Validar(crCporUsd);
        var ahora = clock.UtcNow;
        return new TipoCambio
        {
            CRCporUSD = crCporUsd,
            FechaVigencia = fechaVigencia,
            Activo = false,
            CreatedAt = ahora,
            UpdatedAt = ahora,
        };
    }

    public void Actualizar(decimal crCporUsd, DateTimeOffset fechaVigencia, IClock clock)
    {
        Validar(crCporUsd);
        CRCporUSD = crCporUsd;
        FechaVigencia = fechaVigencia;
        Touch(clock);
    }

    public void Activar(IClock clock)
    {
        Activo = true;
        Touch(clock);
    }

    public void Desactivar(IClock clock)
    {
        Activo = false;
        Touch(clock);
    }

    private static void Validar(decimal crCporUsd)
    {
        if (crCporUsd <= 0)
        {
            throw new TipoCambioInvalidoException("El tipo de cambio debe ser mayor que cero.");
        }
    }
}

public sealed class TipoCambioInvalidoException : DomainException
{
    public TipoCambioInvalidoException(string mensaje) : base(mensaje) { }
}

public sealed class TipoCambioNoConfiguradoException : DomainException
{
    public TipoCambioNoConfiguradoException()
        : base("No existe un tipo de cambio activo configurado.") { }
}
