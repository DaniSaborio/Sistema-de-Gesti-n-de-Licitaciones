using Licitaciones.Application.Common;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.Common;
using Licitaciones.Domain.TiposCambio;
using Licitaciones.UnitTests.TestUtils;
using Moq;
using Xunit;

namespace Licitaciones.UnitTests.TiposCambio.Application;

public class TipoCambioServiceTests
{
    private static readonly FixedClock Reloj = FixedClock.En(2026, 1, 1);

    private static TipoCambioService CrearServicio(
        Mock<ITipoCambioRepository> repositorio,
        Mock<IUnitOfWork>? unitOfWork = null)
    {
        unitOfWork ??= new Mock<IUnitOfWork>();

        unitOfWork
            .Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return new TipoCambioService(
            repositorio.Object,
            unitOfWork.Object,
            Reloj);
    }

    [Fact]
    public async Task ObtenerAsync_devuelve_el_tipo_de_cambio()
    {
        var tipoCambio = TipoCambio.Crear(
            520m,
            Reloj.UtcNow,
            Reloj);

        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                tipoCambio.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipoCambio);

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.ObtenerAsync(tipoCambio.Id);

        Assert.Equal(tipoCambio.Id, resultado.Id);
        Assert.Equal(520m, resultado.CRCporUSD);
        Assert.Equal(Reloj.UtcNow, resultado.FechaVigencia);
        Assert.False(resultado.Activo);
    }

    [Fact]
    public async Task ObtenerAsync_lanza_excepcion_si_no_existe()
    {
        var id = Guid.NewGuid();

        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TipoCambio?)null);

        var servicio = CrearServicio(repositorio);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => servicio.ObtenerAsync(id));
    }

    [Fact]
    public async Task ObtenerActivoAsync_devuelve_el_tipo_de_cambio_activo()
    {
        var tipoCambio = TipoCambio.Crear(
            520m,
            Reloj.UtcNow,
            Reloj);

        tipoCambio.Activar(Reloj);

        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerActivoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipoCambio);

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.ObtenerActivoAsync();

        Assert.NotNull(resultado);
        Assert.Equal(tipoCambio.Id, resultado!.Id);
        Assert.Equal(520m, resultado.CRCporUSD);
        Assert.True(resultado.Activo);
    }

    [Fact]
    public async Task ObtenerActivoAsync_devuelve_null_si_no_hay_activo()
    {
        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerActivoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TipoCambio?)null);

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.ObtenerActivoAsync();

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ListarAsync_mapea_los_tipos_de_cambio()
    {
        var tipo1 = TipoCambio.Crear(
            500m,
            Reloj.UtcNow,
            Reloj);

        var tipo2 = TipoCambio.Crear(
            520m,
            Reloj.UtcNow.AddDays(1),
            Reloj);

        var consulta = new ConsultaPaginada(1, 10, null);

        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ListarAsync(
                consulta,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ResultadoPaginado<TipoCambio>(
                    [tipo1, tipo2],
                    2,
                    1,
                    10));

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.ListarAsync(consulta);

        Assert.Equal(2, resultado.Elementos.Count);
        Assert.Equal(tipo1.Id, resultado.Elementos[0].Id);
        Assert.Equal(tipo2.Id, resultado.Elementos[1].Id);
        Assert.Equal(2, resultado.TotalElementos);
    }

    [Fact]
    public async Task CrearAsync_crea_y_activa_el_primer_tipo_de_cambio()
    {
        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerActivoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TipoCambio?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        var resultado = await servicio.CrearAsync(
            new CrearTipoCambioRequest(
                520m,
                Reloj.UtcNow));

        Assert.Equal(520m, resultado.CRCporUSD);
        Assert.True(resultado.Activo);

        repositorio.Verify(
            x => x.Agregar(
                It.Is<TipoCambio>(t =>
                    t.CRCporUSD == 520m &&
                    t.Activo)),
            Times.Once);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CrearAsync_no_activa_un_nuevo_tipo_si_ya_existe_un_activo()
    {
        var activo = TipoCambio.Crear(
            500m,
            Reloj.UtcNow,
            Reloj);

        activo.Activar(Reloj);

        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerActivoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(activo);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        var resultado = await servicio.CrearAsync(
            new CrearTipoCambioRequest(
                520m,
                Reloj.UtcNow.AddDays(1)));

        Assert.Equal(520m, resultado.CRCporUSD);
        Assert.False(resultado.Activo);

        repositorio.Verify(
            x => x.Agregar(
                It.Is<TipoCambio>(t =>
                    t.CRCporUSD == 520m &&
                    !t.Activo)),
            Times.Once);
    }

    [Fact]
    public async Task ActualizarAsync_actualiza_y_guarda()
    {
        var tipoCambio = TipoCambio.Crear(
            500m,
            Reloj.UtcNow,
            Reloj);

        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                tipoCambio.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipoCambio);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        var resultado = await servicio.ActualizarAsync(
            tipoCambio.Id,
            new ActualizarTipoCambioRequest(
                530m,
                Reloj.UtcNow.AddDays(2)));

        Assert.Equal(530m, resultado.CRCporUSD);
        Assert.Equal(
            Reloj.UtcNow.AddDays(2),
            resultado.FechaVigencia);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_no_permite_eliminar_el_tipo_activo()
    {
        var tipoCambio = TipoCambio.Crear(
            520m,
            Reloj.UtcNow,
            Reloj);

        tipoCambio.Activar(Reloj);

        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                tipoCambio.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipoCambio);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictoDeUnicidadException>(
            () => servicio.EliminarAsync(tipoCambio.Id));

        repositorio.Verify(
            x => x.Eliminar(It.IsAny<TipoCambio>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EliminarAsync_elimina_un_tipo_inactivo()
    {
        var tipoCambio = TipoCambio.Crear(
            520m,
            Reloj.UtcNow,
            Reloj);

        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                tipoCambio.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipoCambio);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        await servicio.EliminarAsync(tipoCambio.Id);

        repositorio.Verify(
            x => x.Eliminar(tipoCambio),
            Times.Once);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActivarAsync_desactiva_el_anterior_y_activa_el_nuevo()
    {
        var anterior = TipoCambio.Crear(
            500m,
            Reloj.UtcNow,
            Reloj);

        anterior.Activar(Reloj);

        var nuevo = TipoCambio.Crear(
            520m,
            Reloj.UtcNow.AddDays(1),
            Reloj);

        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                nuevo.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(nuevo);

        repositorio
            .Setup(x => x.ObtenerActivoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(anterior);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        var resultado =
            await servicio.ActivarAsync(nuevo.Id);

        Assert.True(resultado.Activo);
        Assert.False(anterior.Activo);
        Assert.True(nuevo.Activo);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConvertirCrcAUsdAsync_convierte_usando_el_tipo_activo()
    {
        var tipoCambio = TipoCambio.Crear(
            500m,
            Reloj.UtcNow,
            Reloj);

        tipoCambio.Activar(Reloj);

        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerActivoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tipoCambio);

        var servicio = CrearServicio(repositorio);

        var resultado =
            await servicio.ConvertirCrcAUsdAsync(10000m);

        Assert.Equal(20m, resultado);
    }

    [Fact]
    public async Task ConvertirCrcAUsdAsync_lanza_excepcion_si_no_hay_tipo_activo()
    {
        var repositorio = new Mock<ITipoCambioRepository>();

        repositorio
            .Setup(x => x.ObtenerActivoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TipoCambio?)null);

        var servicio = CrearServicio(repositorio);

        await Assert.ThrowsAsync<TipoCambioNoConfiguradoException>(
            () => servicio.ConvertirCrcAUsdAsync(10000m));
    }
}