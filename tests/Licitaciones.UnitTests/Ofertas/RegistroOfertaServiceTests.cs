using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Ofertas;
using Licitaciones.UnitTests.TestUtils;
using Xunit;

namespace Licitaciones.UnitTests.Ofertas;

public class RegistroOfertaServiceTests
{
    private static readonly FixedClock Reloj = FixedClock.En(2026, 1, 1);

    private static Licitacion LicitacionPublicada(decimal presupuesto = 1_000_000m, int diasParaCierre = 10)
    {
        var licitacion = Licitacion.Crear("LIC-001", "Título", Reloj.UtcNow.AddDays(diasParaCierre), presupuesto, Reloj);
        licitacion.Publicar(Reloj);
        return licitacion;
    }

    [Fact]
    public void Registrar_acepta_una_oferta_valida()
    {
        var licitacion = LicitacionPublicada();
        var proveedorId = Guid.NewGuid();

        var oferta = RegistroOfertaService.Registrar(licitacion, proveedorId, 900_000m, [], Reloj);

        Assert.Equal(proveedorId, oferta.ProveedorId);
        Assert.Equal(900_000m, oferta.MontoOfertadoCRC);
    }

    [Fact]
    public void Registrar_acepta_una_oferta_igual_al_presupuesto()
    {
        var licitacion = LicitacionPublicada(1_000_000m);

        var oferta = RegistroOfertaService.Registrar(licitacion, Guid.NewGuid(), 1_000_000m, [], Reloj);

        Assert.Equal(1_000_000m, oferta.MontoOfertadoCRC);
    }

    [Fact]
    public void Registrar_rechaza_licitacion_no_publicada()
    {
        var licitacion = Licitacion.Crear("LIC-001", "Título", Reloj.UtcNow.AddDays(10), 1_000_000m, Reloj);

        Assert.Throws<LicitacionNoPublicadaException>(() =>
            RegistroOfertaService.Registrar(licitacion, Guid.NewGuid(), 500_000m, [], Reloj));
    }

    [Fact]
    public void Registrar_rechaza_licitacion_vencida()
    {
        var reloj = FixedClock.En(2026, 1, 1);
        var licitacion = LicitacionPublicadaConReloj(reloj);
        reloj.UtcNow = reloj.UtcNow.AddDays(20);

        Assert.Throws<LicitacionVencidaException>(() =>
            RegistroOfertaService.Registrar(licitacion, Guid.NewGuid(), 500_000m, [], reloj));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Registrar_rechaza_montos_no_positivos(decimal monto)
    {
        var licitacion = LicitacionPublicada();

        Assert.Throws<MontoOfertaInvalidoException>(() =>
            RegistroOfertaService.Registrar(licitacion, Guid.NewGuid(), monto, [], Reloj));
    }

    [Fact]
    public void Registrar_rechaza_oferta_superior_al_presupuesto()
    {
        var licitacion = LicitacionPublicada(1_000_000m);

        Assert.Throws<OfertaSuperaPresupuestoException>(() =>
            RegistroOfertaService.Registrar(licitacion, Guid.NewGuid(), 1_000_000.01m, [], Reloj));
    }

    [Fact]
    public void Registrar_rechaza_oferta_duplicada_del_mismo_proveedor()
    {
        var licitacion = LicitacionPublicada();
        var proveedorId = Guid.NewGuid();
        var ofertaPrevia = RegistroOfertaService.Registrar(licitacion, proveedorId, 500_000m, [], Reloj);

        Assert.Throws<OfertaDuplicadaException>(() =>
            RegistroOfertaService.Registrar(licitacion, proveedorId, 400_000m, [ofertaPrevia], Reloj));
    }

    private static Licitacion LicitacionPublicadaConReloj(FixedClock reloj)
    {
        var licitacion = Licitacion.Crear("LIC-001", "Título", reloj.UtcNow.AddDays(10), 1_000_000m, reloj);
        licitacion.Publicar(reloj);
        return licitacion;
    }
}
