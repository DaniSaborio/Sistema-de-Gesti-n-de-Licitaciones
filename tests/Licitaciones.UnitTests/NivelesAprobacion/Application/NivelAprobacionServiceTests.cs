using Licitaciones.Application.Common;
using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Domain.Common;
using Licitaciones.Domain.NivelesAprobacion;
using Licitaciones.UnitTests.TestUtils;
using Moq;
using Xunit;

namespace Licitaciones.UnitTests.NivelesAprobacion.Application;

public class NivelAprobacionServiceTests
{
    private static readonly FixedClock Reloj = FixedClock.En(2026, 1, 1);

    private static NivelAprobacionService CrearServicio(
        Mock<INivelAprobacionRepository> repositorio,
        Mock<IUnitOfWork>? unitOfWork = null)
    {
        unitOfWork ??= new Mock<IUnitOfWork>();

        unitOfWork
            .Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return new NivelAprobacionService(
            repositorio.Object,
            unitOfWork.Object,
            Reloj);
    }

    [Fact]
    public async Task ObtenerAsync_devuelve_el_nivel()
    {
        var nivel = NivelAprobacion.Crear(
            1m,
            100000m,
            "Gerente",
            Reloj);

        var repositorio = new Mock<INivelAprobacionRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                nivel.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(nivel);

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.ObtenerAsync(nivel.Id);

        Assert.Equal(nivel.Id, resultado.Id);
        Assert.Equal(1m, resultado.MontoMinimoCRC);
        Assert.Equal(100000m, resultado.MontoMaximoCRC);
        Assert.Equal("Gerente", resultado.Aprobador);
    }

    [Fact]
    public async Task ObtenerAsync_lanza_excepcion_si_no_existe()
    {
        var id = Guid.NewGuid();

        var repositorio = new Mock<INivelAprobacionRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((NivelAprobacion?)null);

        var servicio = CrearServicio(repositorio);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => servicio.ObtenerAsync(id));
    }

    [Fact]
    public async Task ListarAsync_mapea_los_niveles()
    {
        var nivel1 = NivelAprobacion.Crear(
            1m,
            100000m,
            "Supervisor",
            Reloj);

        var nivel2 = NivelAprobacion.Crear(
            100001m,
            500000m,
            "Gerente",
            Reloj);

        var consulta = new ConsultaPaginada(1, 10, null);

        var repositorio = new Mock<INivelAprobacionRepository>();

        repositorio
            .Setup(x => x.ListarAsync(
                consulta,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ResultadoPaginado<NivelAprobacion>(
                    [nivel1, nivel2],
                    2,
                    1,
                    10));

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.ListarAsync(consulta);

        Assert.Equal(2, resultado.Elementos.Count);
        Assert.Equal(nivel1.Id, resultado.Elementos[0].Id);
        Assert.Equal(nivel2.Id, resultado.Elementos[1].Id);
        Assert.Equal(2, resultado.TotalElementos);
    }

    [Fact]
    public async Task CrearAsync_crea_agrega_y_guarda_el_nivel()
    {
        var repositorio = new Mock<INivelAprobacionRepository>();

        repositorio
            .Setup(x => x.ListarTodosAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        var resultado = await servicio.CrearAsync(
            new CrearNivelAprobacionRequest(
                1m,
                100000m,
                "Supervisor"));

        Assert.Equal(1m, resultado.MontoMinimoCRC);
        Assert.Equal(100000m, resultado.MontoMaximoCRC);
        Assert.Equal("Supervisor", resultado.Aprobador);

        repositorio.Verify(
            x => x.Agregar(
                It.Is<NivelAprobacion>(n =>
                    n.MontoMinimoCRC == 1m &&
                    n.MontoMaximoCRC == 100000m &&
                    n.Aprobador == "Supervisor")),
            Times.Once);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CrearAsync_lanza_excepcion_si_el_rango_se_solapa()
    {
        var existente = NivelAprobacion.Crear(
            1m,
            100000m,
            "Supervisor",
            Reloj);

        var repositorio = new Mock<INivelAprobacionRepository>();

        repositorio
            .Setup(x => x.ListarTodosAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([existente]);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        await Assert.ThrowsAsync<RangoAprobacionSolapadoException>(
            () => servicio.CrearAsync(
                new CrearNivelAprobacionRequest(
                    50000m,
                    200000m,
                    "Gerente")));

        repositorio.Verify(
            x => x.Agregar(It.IsAny<NivelAprobacion>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActualizarAsync_actualiza_y_guarda_el_nivel()
    {
        var nivel = NivelAprobacion.Crear(
            1m,
            100000m,
            "Supervisor",
            Reloj);

        var repositorio = new Mock<INivelAprobacionRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                nivel.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(nivel);

        repositorio
            .Setup(x => x.ListarTodosAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([nivel]);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        var resultado = await servicio.ActualizarAsync(
            nivel.Id,
            new ActualizarNivelAprobacionRequest(
                1m,
                200000m,
                "Gerente"));

        Assert.Equal(1m, resultado.MontoMinimoCRC);
        Assert.Equal(200000m, resultado.MontoMaximoCRC);
        Assert.Equal("Gerente", resultado.Aprobador);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_elimina_y_guarda()
    {
        var nivel = NivelAprobacion.Crear(
            1m,
            100000m,
            "Supervisor",
            Reloj);

        var repositorio = new Mock<INivelAprobacionRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                nivel.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(nivel);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        await servicio.EliminarAsync(nivel.Id);

        repositorio.Verify(
            x => x.Eliminar(nivel),
            Times.Once);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResolverParaMontoAsync_devuelve_el_nivel_correspondiente()
    {
        var nivel1 = NivelAprobacion.Crear(
            1m,
            100000m,
            "Supervisor",
            Reloj);

        var nivel2 = NivelAprobacion.Crear(
            100001m,
            500000m,
            "Gerente",
            Reloj);

        var repositorio = new Mock<INivelAprobacionRepository>();

        repositorio
            .Setup(x => x.ListarTodosAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([nivel1, nivel2]);

        var servicio = CrearServicio(repositorio);

        var resultado =
            await servicio.ResolverParaMontoAsync(250000m);

        Assert.Equal(nivel2.Id, resultado.Id);
        Assert.Equal("Gerente", resultado.Aprobador);
    }

    [Fact]
    public async Task ResolverParaMontoAsync_lanza_excepcion_si_no_hay_nivel()
    {
        var nivel = NivelAprobacion.Crear(
            100000m,
            200000m,
            "Gerente",
            Reloj);

        var repositorio = new Mock<INivelAprobacionRepository>();

        repositorio
            .Setup(x => x.ListarTodosAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([nivel]);

        var servicio = CrearServicio(repositorio);

        await Assert.ThrowsAsync<NivelAprobacionNoConfiguradoException>(
            () => servicio.ResolverParaMontoAsync(500000m));
    }
}