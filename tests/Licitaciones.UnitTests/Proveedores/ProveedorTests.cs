using Licitaciones.Domain.Proveedores;
using Licitaciones.UnitTests.TestUtils;
using Xunit;

namespace Licitaciones.UnitTests.Proveedores;

public class ProveedorTests
{
    private static readonly FixedClock Reloj = FixedClock.En(2026, 1, 1);

    [Fact]
    public void Crear_normaliza_el_nombre_para_comparaciones_de_unicidad()
    {
        var proveedor = Proveedor.Crear("  Empresa   Central  ", Reloj);

        Assert.Equal("EMPRESA CENTRAL", proveedor.NombreNormalizado);
        Assert.Equal("Empresa   Central", proveedor.Nombre);
    }

    [Fact]
    public void Crear_rechaza_nombre_vacio()
    {
        Assert.Throws<NombreProveedorInvalidoException>(() => Proveedor.Crear("   ", Reloj));
    }

    [Theory]
    [InlineData("Empresa#Central")]
    [InlineData("Empresa_Central")]
    [InlineData("Empresa@Central.com")]
    public void Crear_rechaza_caracteres_no_permitidos(string nombreInvalido)
    {
        Assert.Throws<NombreProveedorInvalidoException>(() => Proveedor.Crear(nombreInvalido, Reloj));
    }

    [Fact]
    public void ActualizarNombre_recalcula_la_forma_normalizada()
    {
        var proveedor = Proveedor.Crear("Empresa Central", Reloj);

        proveedor.ActualizarNombre("Nueva Empresa", Reloj);

        Assert.Equal("NUEVA EMPRESA", proveedor.NombreNormalizado);
    }
}
