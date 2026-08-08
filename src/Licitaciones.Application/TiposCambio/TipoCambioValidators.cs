using FluentValidation;

namespace Licitaciones.Application.TiposCambio;

public sealed class CrearTipoCambioRequestValidator : AbstractValidator<CrearTipoCambioRequest>
{
    public CrearTipoCambioRequestValidator()
    {
        RuleFor(x => x.CRCporUSD).GreaterThan(0m);
        RuleFor(x => x.FechaVigencia).NotEqual(default(DateTimeOffset));
    }
}

public sealed class ActualizarTipoCambioRequestValidator : AbstractValidator<ActualizarTipoCambioRequest>
{
    public ActualizarTipoCambioRequestValidator()
    {
        RuleFor(x => x.CRCporUSD).GreaterThan(0m);
        RuleFor(x => x.FechaVigencia).NotEqual(default(DateTimeOffset));
    }
}
