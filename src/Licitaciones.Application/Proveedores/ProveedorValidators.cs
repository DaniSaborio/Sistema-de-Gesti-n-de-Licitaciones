using FluentValidation;

namespace Licitaciones.Application.Proveedores;

public sealed class CrearProveedorRequestValidator : AbstractValidator<CrearProveedorRequest>
{
    public CrearProveedorRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}

public sealed class ActualizarProveedorRequestValidator : AbstractValidator<ActualizarProveedorRequest>
{
    public ActualizarProveedorRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}
