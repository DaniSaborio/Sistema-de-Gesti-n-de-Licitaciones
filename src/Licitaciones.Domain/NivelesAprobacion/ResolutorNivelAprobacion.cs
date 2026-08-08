namespace Licitaciones.Domain.NivelesAprobacion;

/// <summary>
/// Resuelve el aprobador desde la tabla parametrizable de niveles, sin
/// codificar rangos mediante condicionales fijos (sección 8.7 del enunciado).
/// </summary>
public static class ResolutorNivelAprobacion
{
    public static NivelAprobacion Resolver(decimal montoCRC, IEnumerable<NivelAprobacion> niveles)
    {
        var nivel = niveles.FirstOrDefault(n => n.Cubre(montoCRC));
        return nivel ?? throw new NivelAprobacionNoConfiguradoException();
    }

    public static void ValidarNuevoRango(NivelAprobacion candidato, IEnumerable<NivelAprobacion> nivelesExistentes)
    {
        var existentes = nivelesExistentes.Where(n => n.Id != candidato.Id).ToList();

        // Se valida primero la cardinalidad de rangos abiertos: dos rangos sin
        // monto máximo siempre se solapan en su cola, así que sin este orden
        // el error genérico de solape ocultaría el diagnóstico más específico.
        var totalAbiertos = existentes.Count(n => n.MontoMaximoCRC is null) + (candidato.MontoMaximoCRC is null ? 1 : 0);
        if (totalAbiertos > 1)
        {
            throw new MultiplesRangosAbiertosException();
        }

        if (existentes.Any(candidato.SeSolapaCon))
        {
            throw new RangoAprobacionSolapadoException();
        }
    }
}
