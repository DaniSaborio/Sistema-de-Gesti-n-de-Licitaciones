namespace Licitaciones.Domain.Common;

/// <summary>
/// Abstrae el reloj del sistema para permitir pruebas deterministas de reglas
/// dependientes de fecha/hora (vencimientos, cierres de licitación, etc.).
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
