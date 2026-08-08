using Licitaciones.Domain.NivelesAprobacion;
using Licitaciones.UnitTests.TestUtils;
using Xunit;

namespace Licitaciones.UnitTests.NivelesAprobacion;

public class ResolutorNivelAprobacionTests
{
    private static readonly FixedClock Reloj = FixedClock.En(2026, 1, 1);

    private static List<NivelAprobacion> NivelesDelEnunciado() =>
    [
        NivelAprobacion.Crear(0.01m, 999_999.99m, "Encargado de área", Reloj),
        NivelAprobacion.Crear(1_000_000.00m, 9_999_999.99m, "Gerencia", Reloj),
        NivelAprobacion.Crear(10_000_000.00m, null, "Junta Directiva", Reloj),
    ];

    [Theory]
    [InlineData(500_000, "Encargado de área")]
    [InlineData(5_000_000, "Gerencia")]
    [InlineData(50_000_000, "Junta Directiva")]
    public void Resolver_encuentra_el_aprobador_segun_el_monto(decimal monto, string aprobadorEsperado)
    {
        var nivel = ResolutorNivelAprobacion.Resolver(monto, NivelesDelEnunciado());

        Assert.Equal(aprobadorEsperado, nivel.Aprobador);
    }

    [Fact]
    public void Resolver_lanza_excepcion_si_no_hay_nivel_configurado_para_el_monto()
    {
        Assert.Throws<NivelAprobacionNoConfiguradoException>(() =>
            ResolutorNivelAprobacion.Resolver(100m, []));
    }

    [Fact]
    public void ValidarNuevoRango_rechaza_rangos_solapados()
    {
        var existentes = new List<NivelAprobacion>
        {
            NivelAprobacion.Crear(0.01m, 999_999.99m, "Encargado de área", Reloj),
        };
        var candidato = NivelAprobacion.Crear(500_000m, 1_500_000m, "Gerencia", Reloj);

        Assert.Throws<RangoAprobacionSolapadoException>(() =>
            ResolutorNivelAprobacion.ValidarNuevoRango(candidato, existentes));
    }

    [Fact]
    public void ValidarNuevoRango_rechaza_un_segundo_rango_abierto()
    {
        var existentes = new List<NivelAprobacion>
        {
            NivelAprobacion.Crear(10_000_000m, null, "Junta Directiva", Reloj),
        };
        var candidato = NivelAprobacion.Crear(20_000_000m, null, "Presidencia", Reloj);

        Assert.Throws<MultiplesRangosAbiertosException>(() =>
            ResolutorNivelAprobacion.ValidarNuevoRango(candidato, existentes));
    }

    [Fact]
    public void ValidarNuevoRango_acepta_rangos_contiguos_sin_solape()
    {
        var existentes = new List<NivelAprobacion>
        {
            NivelAprobacion.Crear(0.01m, 999_999.99m, "Encargado de área", Reloj),
        };
        var candidato = NivelAprobacion.Crear(1_000_000m, 9_999_999.99m, "Gerencia", Reloj);

        var excepcion = Record.Exception(() => ResolutorNivelAprobacion.ValidarNuevoRango(candidato, existentes));

        Assert.Null(excepcion);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Crear_rechaza_monto_minimo_no_positivo(decimal montoMinimo)
    {
        Assert.Throws<RangoAprobacionInvalidoException>(() =>
            NivelAprobacion.Crear(montoMinimo, 100m, "Aprobador", Reloj));
    }

    [Fact]
    public void Crear_rechaza_monto_maximo_menor_o_igual_al_minimo()
    {
        Assert.Throws<RangoAprobacionInvalidoException>(() =>
            NivelAprobacion.Crear(100m, 100m, "Aprobador", Reloj));
    }
}
