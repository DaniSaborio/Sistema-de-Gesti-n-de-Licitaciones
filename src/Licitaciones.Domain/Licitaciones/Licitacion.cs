using Licitaciones.Domain.Common;

namespace Licitaciones.Domain.Licitaciones;

public sealed class Licitacion : SoftDeletableEntity
{
    public string Codigo { get; private set; } = default!;
    public string CodigoNormalizado { get; private set; } = default!;
    public string Titulo { get; private set; } = default!;
    public EstadoLicitacion Estado { get; private set; }
    public DateTimeOffset FechaCierre { get; private set; }
    public decimal PresupuestoEstimadoCRC { get; private set; }

    private Licitacion() { }

    public static Licitacion Crear(
        string codigo,
        string titulo,
        DateTimeOffset fechaCierre,
        decimal presupuestoEstimadoCRC,
        IClock clock)
    {
        ValidarCodigo(codigo);
        ValidarTitulo(titulo);
        ValidarPresupuesto(presupuestoEstimadoCRC);
        if (fechaCierre <= clock.UtcNow)
        {
            throw new FechaCierreInvalidaException("La fecha de cierre debe ser futura.");
        }

        var ahora = clock.UtcNow;
        return new Licitacion
        {
            Codigo = codigo.Trim(),
            CodigoNormalizado = NormalizacionTexto.Normalizar(codigo),
            Titulo = titulo.Trim(),
            Estado = EstadoLicitacion.Borrador,
            FechaCierre = fechaCierre,
            PresupuestoEstimadoCRC = presupuestoEstimadoCRC,
            CreatedAt = ahora,
            UpdatedAt = ahora,
        };
    }

    public void ActualizarDatos(
        string titulo,
        DateTimeOffset fechaCierre,
        decimal presupuestoEstimadoCRC,
        decimal? mejorOfertaExistenteCRC,
        IClock clock)
    {
        ValidarTitulo(titulo);
        ValidarPresupuesto(presupuestoEstimadoCRC);
        if (fechaCierre <= clock.UtcNow)
        {
            throw new FechaCierreInvalidaException("La fecha de cierre debe ser futura.");
        }

        if (mejorOfertaExistenteCRC is not null && presupuestoEstimadoCRC < mejorOfertaExistenteCRC)
        {
            throw new PresupuestoInvalidoException(
                "El presupuesto no puede reducirse por debajo de una oferta ya registrada.");
        }

        Titulo = titulo.Trim();
        FechaCierre = fechaCierre;
        PresupuestoEstimadoCRC = presupuestoEstimadoCRC;
        Touch(clock);
    }

    public bool EstaCerradaFuncionalmente(IClock clock) =>
        Estado == EstadoLicitacion.Cerrada || clock.UtcNow >= FechaCierre;

    public void Publicar(IClock clock)
    {
        if (Estado != EstadoLicitacion.Borrador)
        {
            throw new TransicionEstadoInvalidaException(Estado, EstadoLicitacion.Publicada);
        }

        if (clock.UtcNow >= FechaCierre)
        {
            throw new FechaCierreInvalidaException("No se puede publicar una licitación con fecha de cierre ya alcanzada.");
        }

        Estado = EstadoLicitacion.Publicada;
        Touch(clock);
    }

    public void Cerrar(IClock clock)
    {
        if (Estado == EstadoLicitacion.Cerrada)
        {
            throw new TransicionEstadoInvalidaException(Estado, EstadoLicitacion.Cerrada);
        }

        Estado = EstadoLicitacion.Cerrada;
        Touch(clock);
    }

    /// <summary>
    /// Única forma de reapertura permitida por el enunciado (8.1): una acción
    /// explícita y auditada, no una transición libre.
    /// </summary>
    public void Reabrir(EstadoLicitacion destino, IClock clock)
    {
        if (Estado != EstadoLicitacion.Cerrada || destino == EstadoLicitacion.Cerrada)
        {
            throw new TransicionEstadoInvalidaException(Estado, destino);
        }

        Estado = destino;
        Touch(clock);
    }

    private static void ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new CodigoLicitacionInvalidoException("El código de la licitación es obligatorio.");
        }
    }

    private static void ValidarTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new CodigoLicitacionInvalidoException("El título de la licitación es obligatorio.");
        }
    }

    private static void ValidarPresupuesto(decimal presupuesto)
    {
        if (presupuesto <= 0)
        {
            throw new PresupuestoInvalidoException("El presupuesto estimado debe ser mayor que cero.");
        }
    }
}
