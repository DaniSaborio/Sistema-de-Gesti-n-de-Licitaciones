using Licitaciones.Domain.Common;
using Xunit;

namespace Licitaciones.UnitTests.Common;

public class NormalizacionTextoTests
{
    [Theory]
    [InlineData("Empresa Central", "EMPRESA CENTRAL")]
    [InlineData("empresa central", "EMPRESA CENTRAL")]
    [InlineData("EMPRESA   CENTRAL", "EMPRESA CENTRAL")]
    [InlineData("  Empresa Central  ", "EMPRESA CENTRAL")]
    public void Normalizar_produce_la_misma_forma_para_variantes_equivalentes(string entrada, string esperado)
    {
        Assert.Equal(esperado, NormalizacionTexto.Normalizar(entrada));
    }

    [Theory]
    [InlineData("Empresa Central S.A. (Zona 1), Sucursal", true)]
    [InlineData("Empresa#Central", false)]
    [InlineData("Empresa_Central", false)]
    [InlineData("Empresa@Central.com", false)]
    public void TieneCaracteresPermitidosParaProveedor_valida_el_conjunto_de_caracteres_del_enunciado(
        string entrada, bool esperado)
    {
        Assert.Equal(esperado, NormalizacionTexto.TieneCaracteresPermitidosParaProveedor(entrada));
    }
}
