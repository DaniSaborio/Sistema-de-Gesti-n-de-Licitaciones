using Licitaciones.Domain.Common;

namespace Licitaciones.Domain.Ofertas;

public sealed class MontoOfertaInvalidoException : DomainException
{
    public MontoOfertaInvalidoException() : base("El monto ofertado debe ser mayor que cero.") { }
}

public sealed class OfertaSuperaPresupuestoException : DomainException
{
    public OfertaSuperaPresupuestoException()
        : base("El monto ofertado no puede superar el presupuesto estimado de la licitación.") { }
}

public sealed class OfertaDuplicadaException : DomainException
{
    public OfertaDuplicadaException()
        : base("El proveedor ya registró una oferta para esta licitación.") { }
}
