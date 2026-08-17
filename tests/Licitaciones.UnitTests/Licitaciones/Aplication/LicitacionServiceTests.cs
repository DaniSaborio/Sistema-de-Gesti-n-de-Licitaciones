using Licitaciones.Application.Common;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Application.Ofertas;
using Licitaciones.Domain.Common;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.NivelesAprobacion;
using Licitaciones.Domain.Ofertas;
using Licitaciones.UnitTests.TestUtils;
using Moq;
using Xunit;

namespace Licitaciones.UnitTests.Licitaciones.Application;

public class LicitacionServiceTests
{
    private static readonly FixedClock Reloj = FixedClock.En(2026, 1, 1);

    private static LicitacionService CrearServicio(
        Mock<ILicitacionRepository> repositorio,
        Mock<IOfertaRepository>? ofertaRepositorio = null,
        Mock<INivelAprobacionRepository>? nivelRepositorio = null,
        Mock<IUnitOfWork>? unitOfWork = null)
    {
        ofertaRepositorio ??= new Mock<IOfertaRepository>();
        nivelRepositorio ??= new Mock<INivelAprobacionRepository>();
        unitOfWork ??= new Mock<IUnitOfWork>();

        unitOfWork
            .Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return new LicitacionService(
            repositorio.Object,
            ofertaRepositorio.Object,
            nivelRepositorio.Object,
            unitOfWork.Object,
            Reloj);
    }

    [Fact]
    public async Task ObtenerAsync_devuelve_la_licitacion_como_dto()
    {
        var licitacion = Licitacion.Crear(
            "LIC-001",
            "Compra de equipos",
            Reloj.UtcNow.AddDays(10),
            100000m,
            Reloj);

        var repositorio = new Mock<ILicitacionRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(licitacion);

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.ObtenerAsync(licitacion.Id);

        Assert.Equal(licitacion.Id, resultado.Id);
        Assert.Equal("LIC-001", resultado.Codigo);
        Assert.Equal("Compra de equipos", resultado.Titulo);
        Assert.Equal(EstadoLicitacion.Borrador, resultado.Estado);
        Assert.Equal(100000m, resultado.PresupuestoEstimadoCRC);
    }

    [Fact]
    public async Task ObtenerAsync_lanza_excepcion_si_no_existe()
    {
        var id = Guid.NewGuid();

        var repositorio = new Mock<ILicitacionRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Licitacion?)null);

        var servicio = CrearServicio(repositorio);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => servicio.ObtenerAsync(id));
    }

    [Fact]
    public async Task ListarAsync_mapea_las_licitaciones_y_conserva_paginacion()
    {
        var licitacion1 = Licitacion.Crear(
            "LIC-001",
            "Compra uno",
            Reloj.UtcNow.AddDays(10),
            100000m,
            Reloj);

        var licitacion2 = Licitacion.Crear(
            "LIC-002",
            "Compra dos",
            Reloj.UtcNow.AddDays(15),
            200000m,
            Reloj);

        var consulta = new ConsultaPaginada(1, 10, null);

        var repositorio = new Mock<ILicitacionRepository>();

        repositorio
            .Setup(x => x.ListarAsync(
                consulta,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ResultadoPaginado<Licitacion>(
                    [licitacion1, licitacion2],
                    2,
                    1,
                    10));

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.ListarAsync(consulta);

        Assert.Equal(2, resultado.Elementos.Count);
        Assert.Equal(licitacion1.Id, resultado.Elementos[0].Id);
        Assert.Equal(licitacion2.Id, resultado.Elementos[1].Id);
        Assert.Equal(2, resultado.TotalElementos);
        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(10, resultado.TamanoPagina);
    }

    [Fact]
    public async Task CrearAsync_crea_agrega_y_guarda_la_licitacion()
    {
        var repositorio = new Mock<ILicitacionRepository>();

        repositorio
            .Setup(x => x.ExisteCodigoNormalizadoAsync(
                "LIC-001",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork: unitOfWork);

        var resultado = await servicio.CrearAsync(
            new CrearLicitacionRequest(
                " LIC-001 ",
                "Compra de equipos",
                Reloj.UtcNow.AddDays(10),
                100000m));

        Assert.Equal("LIC-001", resultado.Codigo);
        Assert.Equal("Compra de equipos", resultado.Titulo);
        Assert.Equal(100000m, resultado.PresupuestoEstimadoCRC);
        Assert.Equal(EstadoLicitacion.Borrador, resultado.Estado);

        repositorio.Verify(
            x => x.Agregar(
                It.Is<Licitacion>(l =>
                    l.Codigo == "LIC-001" &&
                    l.Titulo == "Compra de equipos" &&
                    l.PresupuestoEstimadoCRC == 100000m)),
            Times.Once);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CrearAsync_lanza_conflicto_si_el_codigo_ya_existe()
    {
        var repositorio = new Mock<ILicitacionRepository>();

        repositorio
            .Setup(x => x.ExisteCodigoNormalizadoAsync(
                "LIC-001",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork: unitOfWork);

        await Assert.ThrowsAsync<ConflictoDeUnicidadException>(
            () => servicio.CrearAsync(
                new CrearLicitacionRequest(
                    "LIC-001",
                    "Compra",
                    Reloj.UtcNow.AddDays(10),
                    100000m)));

        repositorio.Verify(
            x => x.Agregar(It.IsAny<Licitacion>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActualizarAsync_actualiza_y_guarda_la_licitacion()
    {
        var licitacion = Licitacion.Crear(
            "LIC-001",
            "Titulo anterior",
            Reloj.UtcNow.AddDays(10),
            100000m,
            Reloj);

        var repositorio = new Mock<ILicitacionRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(licitacion);

        var ofertaRepositorio = new Mock<IOfertaRepository>();

        ofertaRepositorio
            .Setup(x => x.ObtenerMontoMinimoAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((decimal?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            ofertaRepositorio,
            unitOfWork: unitOfWork);

        var resultado = await servicio.ActualizarAsync(
            licitacion.Id,
            new ActualizarLicitacionRequest(
                "Titulo actualizado",
                Reloj.UtcNow.AddDays(20),
                150000m));

        Assert.Equal("Titulo actualizado", resultado.Titulo);
        Assert.Equal(150000m, resultado.PresupuestoEstimadoCRC);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoAsync_publica_una_licitacion_en_borrador()
    {
        var licitacion = Licitacion.Crear(
            "LIC-001",
            "Compra",
            Reloj.UtcNow.AddDays(10),
            100000m,
            Reloj);

        var repositorio = new Mock<ILicitacionRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(licitacion);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork: unitOfWork);

        var resultado = await servicio.CambiarEstadoAsync(
            licitacion.Id,
            new CambiarEstadoLicitacionRequest(
                EstadoLicitacion.Publicada));

        Assert.Equal(EstadoLicitacion.Publicada, resultado.Estado);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoAsync_cierra_la_licitacion()
    {
        var licitacion = Licitacion.Crear(
            "LIC-001",
            "Compra",
            Reloj.UtcNow.AddDays(10),
            100000m,
            Reloj);

        licitacion.Publicar(Reloj);

        var repositorio = new Mock<ILicitacionRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(licitacion);

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.CambiarEstadoAsync(
            licitacion.Id,
            new CambiarEstadoLicitacionRequest(
                EstadoLicitacion.Cerrada));

        Assert.Equal(EstadoLicitacion.Cerrada, resultado.Estado);
    }

    [Fact]
    public async Task EliminarAsync_elimina_logicamente_y_guarda()
    {
        var licitacion = Licitacion.Crear(
            "LIC-001",
            "Compra",
            Reloj.UtcNow.AddDays(10),
            100000m,
            Reloj);

        var repositorio = new Mock<ILicitacionRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(licitacion);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork: unitOfWork);

        await servicio.EliminarAsync(licitacion.Id);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerMejorOfertaAsync_sin_ofertas_devuelve_sin_ofertas_validas()
    {
        var licitacion = Licitacion.Crear(
            "LIC-001",
            "Compra",
            Reloj.UtcNow.AddDays(10),
            100000m,
            Reloj);

        var repositorio = new Mock<ILicitacionRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(licitacion);

        var ofertaRepositorio = new Mock<IOfertaRepository>();

        ofertaRepositorio
            .Setup(x => x.ListarPorLicitacionAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var servicio = CrearServicio(
            repositorio,
            ofertaRepositorio);

        var resultado =
            await servicio.ObtenerMejorOfertaAsync(licitacion.Id);

        Assert.Equal(
            ClasificacionOferta.SinOfertasValidas,
            resultado.Clasificacion);

        Assert.Null(resultado.OfertaId);
        Assert.Null(resultado.ProveedorId);
        Assert.Null(resultado.MontoOfertadoCRC);
        Assert.Null(resultado.PorcentajeAhorro);
        Assert.Null(resultado.Aprobador);
    }
}