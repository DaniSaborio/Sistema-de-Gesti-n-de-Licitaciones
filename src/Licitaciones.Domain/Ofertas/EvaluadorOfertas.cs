using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Domain.Ofertas;

/// <summary>
/// Determina la mejor oferta (menor monto válido en CRC; en empate gana la
/// registrada primero) y su clasificación de ahorro respecto al presupuesto
/// estimado de la licitación (sección 8.6 del enunciado).
/// </summary>
public static class EvaluadorOfertas
{
    private const decimal UmbralOfertaConvenientePorcentaje = 10m;

    public static Oferta? MejorOferta(IEnumerable<Oferta> ofertas) =>
        ofertas
            .OrderBy(o => o.MontoOfertadoCRC)
            .ThenBy(o => o.FechaRegistro)
            .FirstOrDefault();

    public static ClasificacionOferta Clasificar(Licitacion licitacion, Oferta? mejorOferta)
    {
        if (mejorOferta is null)
        {
            return ClasificacionOferta.SinOfertasValidas;
        }

        var porcentajeAhorro =
            (licitacion.PresupuestoEstimadoCRC - mejorOferta.MontoOfertadoCRC)
            / licitacion.PresupuestoEstimadoCRC
            * 100m;

        return porcentajeAhorro switch
        {
            >= UmbralOfertaConvenientePorcentaje => ClasificacionOferta.OfertaConveniente,
            > 0m => ClasificacionOferta.OfertaAceptable,
            _ => ClasificacionOferta.OfertaValidaSinAhorro,
        };
    }
}
