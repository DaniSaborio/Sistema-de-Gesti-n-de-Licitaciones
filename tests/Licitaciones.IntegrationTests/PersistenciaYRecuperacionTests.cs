using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.NivelesAprobacion;
using Licitaciones.Domain.Proveedores;
using Licitaciones.Domain.TiposCambio;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Licitaciones.IntegrationTests;

[Collection("Postgres")]
public class PersistenciaYRecuperacionTests(PostgresContainerFixture fixture)
{
    private static readonly ClockDePrueba Reloj = new(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Una_licitacion_persistida_se_recupera_con_los_mismos_valores()
    {
        await using var dbEscritura = fixture.CrearDbContext();
        var licitacion = Licitacion.Crear("LIC-PERSIST-001", "Compra de prueba", Reloj.UtcNow.AddDays(15), 2_500_000.50m, Reloj);
        dbEscritura.Licitaciones.Add(licitacion);
        await dbEscritura.SaveChangesAsync();

        await using var dbLectura = fixture.CrearDbContext();
        var recuperada = await dbLectura.Licitaciones.SingleAsync(l => l.Id == licitacion.Id);

        Assert.Equal(licitacion.Codigo, recuperada.Codigo);
        Assert.Equal(licitacion.CodigoNormalizado, recuperada.CodigoNormalizado);
        Assert.Equal(licitacion.PresupuestoEstimadoCRC, recuperada.PresupuestoEstimadoCRC);
        Assert.Equal(EstadoLicitacion.Borrador, recuperada.Estado);
    }

    [Fact]
    public async Task El_borrado_logico_de_un_proveedor_lo_excluye_de_las_consultas_por_defecto()
    {
        await using var dbEscritura = fixture.CrearDbContext();
        var proveedor = Proveedor.Crear("Proveedor Borrado Lógico", Reloj);
        dbEscritura.Proveedores.Add(proveedor);
        await dbEscritura.SaveChangesAsync();

        proveedor.EliminarLogicamente(Reloj);
        await dbEscritura.SaveChangesAsync();

        await using var dbLectura = fixture.CrearDbContext();
        var visible = await dbLectura.Proveedores.SingleOrDefaultAsync(p => p.Id == proveedor.Id);
        var visibleIgnorandoFiltro = await dbLectura.Proveedores.IgnoreQueryFilters().SingleOrDefaultAsync(p => p.Id == proveedor.Id);

        Assert.Null(visible);
        Assert.NotNull(visibleIgnorandoFiltro);
        Assert.NotNull(visibleIgnorandoFiltro!.DeletedAt);
    }

    [Fact]
    public async Task Los_datos_semilla_de_niveles_de_aprobacion_y_tipo_de_cambio_existen_tras_migrar()
    {
        await using var db = fixture.CrearDbContext();

        var niveles = await db.NivelesAprobacion.OrderBy(n => n.MontoMinimoCRC).ToListAsync();
        Assert.Equal(3, niveles.Count);
        Assert.Equal("Encargado de área", niveles[0].Aprobador);
        Assert.Equal("Gerencia", niveles[1].Aprobador);
        Assert.Equal("Junta Directiva", niveles[2].Aprobador);
        Assert.Null(niveles[2].MontoMaximoCRC);

        var tipoCambioActivo = await db.TiposCambio.SingleAsync(tc => tc.Activo);
        Assert.True(tipoCambioActivo.CRCporUSD > 0);
    }
}
