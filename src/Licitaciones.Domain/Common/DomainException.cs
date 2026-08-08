namespace Licitaciones.Domain.Common;

/// <summary>
/// Excepción base para violaciones de reglas de negocio del dominio.
/// La capa Application/Api la traduce a respuestas HTTP controladas (ProblemDetails).
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
