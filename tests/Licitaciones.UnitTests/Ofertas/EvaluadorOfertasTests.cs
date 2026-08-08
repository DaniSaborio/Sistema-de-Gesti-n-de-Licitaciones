using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Ofertas;
using Licitaciones.UnitTests.TestUtils;
using Xunit;

namespace Licitaciones.UnitTests.Ofertas;

public class EvaluadorOfertasTests
{
    private static readonly FixedClock Reloj = FixedClock.En(2026, 1, 1);

    private static Licitacion LicitacionPublicada(decimal presupuesto) =>
        CrearYPublicar(presupuesto);

    private static Licitacion CrearYPublicar(decimal presupuesto)
    {
        var licitacion = Licitacion.Crear("LIC-001", "Título", Reloj.UtcNow.AddDays(10), presupuesto, Reloj);
        licitacion.Publicar(Reloj);
        return licitacion;
    }

    [Fact]
    public void MejorOferta_sin_ofertas_es_nula_y_clasificacion_es_sin_ofertas_validas()
    {
        var licitacion = LicitacionPublicada(1_000_000m);

        var mejor = EvaluadorOfertas.MejorOferta([]);

        Assert.Null(mejor);
        Assert.Equal(ClasificacionOferta.SinOfertasValidas, EvaluadorOfertas.Clasificar(licitacion, mejor));
    }

    [Fact]
    public void MejorOferta_selecciona_el_menor_monto()
    {
        var licitacion = LicitacionPublicada(1_000_000m);
        var mayor = RegistroOfertaService.Registrar(licitacion, Guid.NewGuid(), 900_000m, [], Reloj);
        var menor = RegistroOfertaService.Registrar(licitacion, Guid.NewGuid(), 700_000m, [mayor], Reloj);

        var mejor = EvaluadorOfertas.MejorOferta([mayor, menor]);

        Assert.Equal(menor.Id, mejor!.Id);
    }

    [Fact]
    public void MejorOferta_en_empate_selecciona_la_registrada_primero()
    {
        var licitacion = LicitacionPublicada(1_000_000m);
        var reloj = FixedClock.En(2026, 1, 1);
        var primera = RegistroOfertaService.Registrar(licitacion, Guid.NewGuid(), 700_000m, [], reloj);
        reloj.UtcNow = reloj.UtcNow.AddMinutes(5);
        var segunda = RegistroOfertaService.Registrar(licitacion, Guid.NewGuid(), 700_000m, [primera], reloj);

        var mejor = EvaluadorOfertas.MejorOferta([segunda, primera]);

        Assert.Equal(primera.Id, mejor!.Id);
    }

    [Theory]
    [InlineData(900_000, ClasificacionOferta.OfertaConveniente)] // 10% de ahorro exacto
    [InlineData(950_000, ClasificacionOferta.OfertaAceptable)]   // 5% de ahorro
    [InlineData(1_000_000, ClasificacionOferta.OfertaValidaSinAhorro)] // 0% de ahorro
    public void Clasificar_aplica_los_umbrales_de_ahorro_del_enunciado(decimal montoMejorOferta, ClasificacionOferta esperado)
    {
        var licitacion = LicitacionPublicada(1_000_000m);
        var oferta = RegistroOfertaService.Registrar(licitacion, Guid.NewGuid(), montoMejorOferta, [], Reloj);

        var clasificacion = EvaluadorOfertas.Clasificar(licitacion, oferta);

        Assert.Equal(esperado, clasificacion);
    }
}
