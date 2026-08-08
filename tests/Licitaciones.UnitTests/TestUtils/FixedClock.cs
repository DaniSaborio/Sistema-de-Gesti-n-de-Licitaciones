using Licitaciones.Domain.Common;

namespace Licitaciones.UnitTests.TestUtils;

/// <summary>Reloj determinista para pruebas: IClock inyectable (sección 8.2 del enunciado).</summary>
public sealed class FixedClock(DateTimeOffset momento) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = momento;

    public static FixedClock En(int anio, int mes, int dia, int hora = 0, int minuto = 0) =>
        new(new DateTimeOffset(anio, mes, dia, hora, minuto, 0, TimeSpan.Zero));
}
