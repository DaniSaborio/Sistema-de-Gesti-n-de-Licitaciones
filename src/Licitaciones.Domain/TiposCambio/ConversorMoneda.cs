namespace Licitaciones.Domain.TiposCambio;

/// <summary>
/// Convierte montos de CRC (fuente de verdad persistida) a USD como valor
/// calculado de presentación; nunca modifica los valores originales (8.8).
/// </summary>
public static class ConversorMoneda
{
    public static decimal ConvertirCrcAUsd(decimal montoCRC, TipoCambio tipoCambioActivo)
    {
        ArgumentNullException.ThrowIfNull(tipoCambioActivo);
        return Math.Round(montoCRC / tipoCambioActivo.CRCporUSD, 2, MidpointRounding.AwayFromZero);
    }
}
