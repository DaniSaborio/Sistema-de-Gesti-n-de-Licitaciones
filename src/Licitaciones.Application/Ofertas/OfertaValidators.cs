using FluentValidation;

namespace Licitaciones.Application.Ofertas;

public sealed class RegistrarOfertaRequestValidator : AbstractValidator<RegistrarOfertaRequest>
{
    public RegistrarOfertaRequestValidator()
    {
        RuleFor(x => x.ProveedorId).NotEmpty();
        RuleFor(x => x.MontoOfertadoCRC).GreaterThan(0m);
    }
}

public sealed class ActualizarOfertaRequestValidator : AbstractValidator<ActualizarOfertaRequest>
{
    public ActualizarOfertaRequestValidator()
    {
        RuleFor(x => x.MontoOfertadoCRC).GreaterThan(0m);
    }
}
