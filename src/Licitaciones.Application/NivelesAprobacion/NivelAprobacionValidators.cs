using FluentValidation;

namespace Licitaciones.Application.NivelesAprobacion;

public sealed class CrearNivelAprobacionRequestValidator : AbstractValidator<CrearNivelAprobacionRequest>
{
    public CrearNivelAprobacionRequestValidator()
    {
        RuleFor(x => x.MontoMinimoCRC).GreaterThan(0m);
        RuleFor(x => x.MontoMaximoCRC).GreaterThan(x => x.MontoMinimoCRC).When(x => x.MontoMaximoCRC is not null);
        RuleFor(x => x.Aprobador).NotEmpty().MaximumLength(150);
    }
}

public sealed class ActualizarNivelAprobacionRequestValidator : AbstractValidator<ActualizarNivelAprobacionRequest>
{
    public ActualizarNivelAprobacionRequestValidator()
    {
        RuleFor(x => x.MontoMinimoCRC).GreaterThan(0m);
        RuleFor(x => x.MontoMaximoCRC).GreaterThan(x => x.MontoMinimoCRC).When(x => x.MontoMaximoCRC is not null);
        RuleFor(x => x.Aprobador).NotEmpty().MaximumLength(150);
    }
}
