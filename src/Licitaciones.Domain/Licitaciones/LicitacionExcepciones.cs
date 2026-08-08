using Licitaciones.Domain.Common;

namespace Licitaciones.Domain.Licitaciones;

public sealed class TransicionEstadoInvalidaException : DomainException
{
    public TransicionEstadoInvalidaException(EstadoLicitacion origen, EstadoLicitacion destino)
        : base($"No se permite la transición de estado de '{origen}' a '{destino}'.") { }
}

public sealed class PresupuestoInvalidoException : DomainException
{
    public PresupuestoInvalidoException(string mensaje) : base(mensaje) { }
}

public sealed class FechaCierreInvalidaException : DomainException
{
    public FechaCierreInvalidaException(string mensaje) : base(mensaje) { }
}

public sealed class LicitacionNoPublicadaException : DomainException
{
    public LicitacionNoPublicadaException()
        : base("Solo se pueden registrar ofertas sobre licitaciones publicadas.") { }
}

public sealed class LicitacionVencidaException : DomainException
{
    public LicitacionVencidaException()
        : base("La licitación se encuentra vencida o cerrada; no admite nuevas ofertas.") { }
}

public sealed class CodigoLicitacionInvalidoException : DomainException
{
    public CodigoLicitacionInvalidoException(string mensaje) : base(mensaje) { }
}
