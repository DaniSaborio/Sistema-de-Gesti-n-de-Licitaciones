using FluentValidation;

namespace Licitaciones.Application.Licitaciones;

public sealed class CrearLicitacionRequestValidator : AbstractValidator<CrearLicitacionRequest>
{
    public CrearLicitacionRequestValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PresupuestoEstimadoCRC).GreaterThan(0m);
        RuleFor(x => x.FechaCierre).NotEqual(default(DateTimeOffset));
    }
}

public sealed class ActualizarLicitacionRequestValidator : AbstractValidator<ActualizarLicitacionRequest>
{
    public ActualizarLicitacionRequestValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PresupuestoEstimadoCRC).GreaterThan(0m);
        RuleFor(x => x.FechaCierre).NotEqual(default(DateTimeOffset));
    }
}
