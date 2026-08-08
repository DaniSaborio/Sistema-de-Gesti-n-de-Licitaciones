using Licitaciones.Domain.TiposCambio;
using Licitaciones.UnitTests.TestUtils;
using Xunit;

namespace Licitaciones.UnitTests.TiposCambio;

public class ConversorMonedaTests
{
    private static readonly FixedClock Reloj = FixedClock.En(2026, 1, 1);

    [Fact]
    public void ConvertirCrcAUsd_divide_el_monto_por_el_tipo_de_cambio_activo()
    {
        var tipoCambio = TipoCambio.Crear(520m, Reloj.UtcNow, Reloj);

        var usd = ConversorMoneda.ConvertirCrcAUsd(1_040_000m, tipoCambio);

        Assert.Equal(2000m, usd);
    }

    [Fact]
    public void ConvertirCrcAUsd_no_modifica_el_monto_original_en_crc()
    {
        var tipoCambio = TipoCambio.Crear(500m, Reloj.UtcNow, Reloj);
        const decimal montoOriginalCRC = 750_000m;

        ConversorMoneda.ConvertirCrcAUsd(montoOriginalCRC, tipoCambio);

        Assert.Equal(750_000m, montoOriginalCRC);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void Crear_rechaza_tipo_de_cambio_no_positivo(decimal valor)
    {
        Assert.Throws<TipoCambioInvalidoException>(() => TipoCambio.Crear(valor, Reloj.UtcNow, Reloj));
    }

    [Fact]
    public void Activar_marca_el_tipo_de_cambio_como_activo()
    {
        var tipoCambio = TipoCambio.Crear(500m, Reloj.UtcNow, Reloj);

        tipoCambio.Activar(Reloj);

        Assert.True(tipoCambio.Activo);
    }
}
