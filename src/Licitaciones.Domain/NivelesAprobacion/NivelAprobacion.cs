using Licitaciones.Domain.Common;

namespace Licitaciones.Domain.NivelesAprobacion;

public sealed class NivelAprobacion : Entity
{
    public decimal MontoMinimoCRC { get; private set; }
    public decimal? MontoMaximoCRC { get; private set; }
    public string Aprobador { get; private set; } = default!;

    private NivelAprobacion() { }

    public static NivelAprobacion Crear(decimal montoMinimoCRC, decimal? montoMaximoCRC, string aprobador, IClock clock)
    {
        Validar(montoMinimoCRC, montoMaximoCRC, aprobador);
        var ahora = clock.UtcNow;
        return new NivelAprobacion
        {
            MontoMinimoCRC = montoMinimoCRC,
            MontoMaximoCRC = montoMaximoCRC,
            Aprobador = aprobador.Trim(),
            CreatedAt = ahora,
            UpdatedAt = ahora,
        };
    }

    public void Actualizar(decimal montoMinimoCRC, decimal? montoMaximoCRC, string aprobador, IClock clock)
    {
        Validar(montoMinimoCRC, montoMaximoCRC, aprobador);
        MontoMinimoCRC = montoMinimoCRC;
        MontoMaximoCRC = montoMaximoCRC;
        Aprobador = aprobador.Trim();
        Touch(clock);
    }

    public bool Cubre(decimal montoCRC) =>
        montoCRC >= MontoMinimoCRC && (MontoMaximoCRC is null || montoCRC <= MontoMaximoCRC);

    public bool SeSolapaCon(NivelAprobacion otro)
    {
        var finPropio = MontoMaximoCRC ?? decimal.MaxValue;
        var finOtro = otro.MontoMaximoCRC ?? decimal.MaxValue;
        return MontoMinimoCRC <= finOtro && otro.MontoMinimoCRC <= finPropio;
    }

    private static void Validar(decimal montoMinimoCRC, decimal? montoMaximoCRC, string aprobador)
    {
        if (montoMinimoCRC <= 0)
        {
            throw new RangoAprobacionInvalidoException("El monto mínimo debe ser mayor que cero.");
        }

        if (montoMaximoCRC is not null && montoMaximoCRC <= montoMinimoCRC)
        {
            throw new RangoAprobacionInvalidoException("El monto máximo debe ser mayor que el monto mínimo.");
        }

        if (string.IsNullOrWhiteSpace(aprobador))
        {
            throw new RangoAprobacionInvalidoException("El aprobador es obligatorio.");
        }
    }
}

public sealed class RangoAprobacionInvalidoException : DomainException
{
    public RangoAprobacionInvalidoException(string mensaje) : base(mensaje) { }
}

public sealed class RangoAprobacionSolapadoException : DomainException
{
    public RangoAprobacionSolapadoException()
        : base("El rango de aprobación se solapa con uno existente.") { }
}

public sealed class MultiplesRangosAbiertosException : DomainException
{
    public MultiplesRangosAbiertosException()
        : base("Solo puede existir un rango de aprobación abierto sin monto máximo.") { }
}

public sealed class NivelAprobacionNoConfiguradoException : DomainException
{
    public NivelAprobacionNoConfiguradoException()
        : base("No existe un nivel de aprobación configurado para el monto indicado.") { }
}
