using Licitaciones.Domain.Licitaciones;
using Licitaciones.UnitTests.TestUtils;
using Xunit;

namespace Licitaciones.UnitTests.Licitaciones;

public class LicitacionTests
{
    private static readonly FixedClock Reloj = FixedClock.En(2026, 1, 1);

    private static Licitacion CrearLicitacionValida(decimal presupuesto = 1_000_000m) =>
        Licitacion.Crear("LIC-001", "Compra de equipo", Reloj.UtcNow.AddDays(10), presupuesto, Reloj);

    [Fact]
    public void Crear_normaliza_el_codigo_para_comparaciones_de_unicidad()
    {
        var licitacion = Licitacion.Crear("  lic-001  ", "Título", Reloj.UtcNow.AddDays(1), 100m, Reloj);

        Assert.Equal("LIC-001", licitacion.CodigoNormalizado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Crear_rechaza_presupuesto_no_positivo(decimal presupuesto)
    {
        Assert.Throws<PresupuestoInvalidoException>(() =>
            Licitacion.Crear("LIC-001", "Título", Reloj.UtcNow.AddDays(1), presupuesto, Reloj));
    }

    [Fact]
    public void Crear_rechaza_fecha_de_cierre_no_futura()
    {
        Assert.Throws<FechaCierreInvalidaException>(() =>
            Licitacion.Crear("LIC-001", "Título", Reloj.UtcNow, 100m, Reloj));
    }

    [Fact]
    public void Publicar_desde_borrador_cambia_el_estado_a_publicada()
    {
        var licitacion = CrearLicitacionValida();

        licitacion.Publicar(Reloj);

        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
    }

    [Fact]
    public void Publicar_una_licitacion_ya_publicada_lanza_excepcion()
    {
        var licitacion = CrearLicitacionValida();
        licitacion.Publicar(Reloj);

        Assert.Throws<TransicionEstadoInvalidaException>(() => licitacion.Publicar(Reloj));
    }

    [Fact]
    public void Publicada_a_borrador_no_esta_permitida()
    {
        var licitacion = CrearLicitacionValida();
        licitacion.Publicar(Reloj);
        licitacion.Cerrar(Reloj);

        // Cerrada -> Borrador solo mediante Reabrir explícito, nunca vía Publicar/Cerrar directos.
        Assert.Throws<TransicionEstadoInvalidaException>(() => licitacion.Cerrar(Reloj));
    }

    [Fact]
    public void EstaCerradaFuncionalmente_es_verdadero_cuando_se_alcanzo_la_fecha_de_cierre_aunque_el_estado_no_se_actualizo()
    {
        var reloj = FixedClock.En(2026, 1, 1);
        var licitacion = Licitacion.Crear("LIC-001", "Título", reloj.UtcNow.AddDays(1), 100m, reloj);
        licitacion.Publicar(reloj);

        reloj.UtcNow = reloj.UtcNow.AddDays(2);

        Assert.True(licitacion.EstaCerradaFuncionalmente(reloj));
        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
    }

    [Fact]
    public void Reabrir_una_licitacion_cerrada_permite_volver_a_publicada()
    {
        var licitacion = CrearLicitacionValida();
        licitacion.Publicar(Reloj);
        licitacion.Cerrar(Reloj);

        licitacion.Reabrir(EstadoLicitacion.Publicada, Reloj);

        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
    }

    [Fact]
    public void ActualizarDatos_rechaza_reducir_el_presupuesto_por_debajo_de_una_oferta_existente()
    {
        var licitacion = CrearLicitacionValida(1_000_000m);

        Assert.Throws<PresupuestoInvalidoException>(() =>
            licitacion.ActualizarDatos("Título", Reloj.UtcNow.AddDays(5), 500_000m, mejorOfertaExistenteCRC: 800_000m, Reloj));
    }
}
