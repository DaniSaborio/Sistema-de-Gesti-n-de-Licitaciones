using Licitaciones.Application.Common;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Common;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Ofertas;
using Licitaciones.Domain.Proveedores;
using Licitaciones.UnitTests.TestUtils;
using Moq;
using Xunit;

namespace Licitaciones.UnitTests.Ofertas.Application;

public class OfertaServiceTests
{
    private static readonly FixedClock Reloj = FixedClock.En(2026, 1, 1);

    private static Licitacion CrearLicitacion()
    {
        var licitacion = Licitacion.Crear(
            "LIC-001",
            "Compra de equipos",
            Reloj.UtcNow.AddDays(10),
            100000m,
            Reloj);

        licitacion.Publicar(Reloj);

        return licitacion;
    }

    private static OfertaService CrearServicio(
        Mock<IOfertaRepository> ofertaRepositorio,
        Mock<ILicitacionRepository>? licitacionRepositorio = null,
        Mock<IProveedorRepository>? proveedorRepositorio = null,
        Mock<IUnitOfWork>? unitOfWork = null)
    {
        licitacionRepositorio ??= new Mock<ILicitacionRepository>();
        proveedorRepositorio ??= new Mock<IProveedorRepository>();
        unitOfWork ??= new Mock<IUnitOfWork>();

        unitOfWork
            .Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return new OfertaService(
            ofertaRepositorio.Object,
            licitacionRepositorio.Object,
            proveedorRepositorio.Object,
            unitOfWork.Object,
            Reloj);
    }

    [Fact]
    public async Task ObtenerAsync_devuelve_la_oferta_y_el_nombre_del_proveedor()
    {
        var licitacion = CrearLicitacion();
        var proveedor = Proveedor.Crear("Proveedor Central", Reloj);

        var ofertaRepositorio = new Mock<IOfertaRepository>();

        var licitacionRepositorio = new Mock<ILicitacionRepository>();

        licitacionRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(licitacion);

        var oferta = RegistroOfertaService.Registrar(
            licitacion,
            proveedor.Id,
            80000m,
            [],
            Reloj);

        ofertaRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                oferta.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(oferta);

        var proveedorRepositorio = new Mock<IProveedorRepository>();

        proveedorRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                proveedor.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedor);

        var servicio = CrearServicio(
            ofertaRepositorio,
            licitacionRepositorio,
            proveedorRepositorio);

        var resultado = await servicio.ObtenerAsync(oferta.Id);

        Assert.Equal(oferta.Id, resultado.Id);
        Assert.Equal(licitacion.Id, resultado.LicitacionId);
        Assert.Equal(proveedor.Id, resultado.ProveedorId);
        Assert.Equal("Proveedor Central", resultado.ProveedorNombre);
        Assert.Equal(80000m, resultado.MontoOfertadoCRC);
    }

    [Fact]
    public async Task ObtenerAsync_lanza_excepcion_si_la_oferta_no_existe()
    {
        var id = Guid.NewGuid();

        var ofertaRepositorio = new Mock<IOfertaRepository>();

        ofertaRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Oferta?)null);

        var servicio = CrearServicio(ofertaRepositorio);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => servicio.ObtenerAsync(id));
    }

    [Fact]
    public async Task ListarAsync_mapea_las_ofertas()
    {
        var licitacion = CrearLicitacion();

        var proveedor = Proveedor.Crear(
            "Proveedor Central",
            Reloj);

        var oferta = RegistroOfertaService.Registrar(
            licitacion,
            proveedor.Id,
            75000m,
            [],
            Reloj);

        var consulta = new ConsultaPaginada(1, 10, null);

        var ofertaRepositorio = new Mock<IOfertaRepository>();

        ofertaRepositorio
            .Setup(x => x.ListarAsync(
                consulta,
                licitacion.Id,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ResultadoPaginado<Oferta>(
                    [oferta],
                    1,
                    1,
                    10));

        var proveedorRepositorio = new Mock<IProveedorRepository>();

        proveedorRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                proveedor.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedor);

        var servicio = CrearServicio(
            ofertaRepositorio,
            proveedorRepositorio: proveedorRepositorio);

        var resultado = await servicio.ListarAsync(
            consulta,
            licitacion.Id,
            null);

        Assert.Single(resultado.Elementos);
        Assert.Equal(oferta.Id, resultado.Elementos[0].Id);
        Assert.Equal("Proveedor Central", resultado.Elementos[0].ProveedorNombre);
        Assert.Equal(75000m, resultado.Elementos[0].MontoOfertadoCRC);
        Assert.Equal(1, resultado.TotalElementos);
    }

    [Fact]
    public async Task RegistrarAsync_crea_agrega_y_guarda_la_oferta()
    {
        var licitacion = CrearLicitacion();

        var proveedor = Proveedor.Crear(
            "Proveedor Central",
            Reloj);

        var ofertaRepositorio = new Mock<IOfertaRepository>();

        ofertaRepositorio
            .Setup(x => x.ListarPorLicitacionAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var licitacionRepositorio = new Mock<ILicitacionRepository>();

        licitacionRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(licitacion);

        var proveedorRepositorio = new Mock<IProveedorRepository>();

        proveedorRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                proveedor.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedor);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            ofertaRepositorio,
            licitacionRepositorio,
            proveedorRepositorio,
            unitOfWork);

        var resultado = await servicio.RegistrarAsync(
            licitacion.Id,
            new RegistrarOfertaRequest(
                proveedor.Id,
                80000m));

        Assert.Equal(licitacion.Id, resultado.LicitacionId);
        Assert.Equal(proveedor.Id, resultado.ProveedorId);
        Assert.Equal(80000m, resultado.MontoOfertadoCRC);

        ofertaRepositorio.Verify(
            x => x.Agregar(
                It.Is<Oferta>(o =>
                    o.LicitacionId == licitacion.Id &&
                    o.ProveedorId == proveedor.Id &&
                    o.MontoOfertadoCRC == 80000m)),
            Times.Once);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegistrarAsync_lanza_excepcion_si_la_licitacion_no_existe()
    {
        var licitacionId = Guid.NewGuid();

        var ofertaRepositorio = new Mock<IOfertaRepository>();
        var licitacionRepositorio = new Mock<ILicitacionRepository>();

        licitacionRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                licitacionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Licitacion?)null);

        var servicio = CrearServicio(
            ofertaRepositorio,
            licitacionRepositorio);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => servicio.RegistrarAsync(
                licitacionId,
                new RegistrarOfertaRequest(
                    Guid.NewGuid(),
                    50000m)));
    }

    [Fact]
    public async Task RegistrarAsync_lanza_excepcion_si_el_proveedor_no_existe()
    {
        var licitacion = CrearLicitacion();

        var ofertaRepositorio = new Mock<IOfertaRepository>();

        var licitacionRepositorio = new Mock<ILicitacionRepository>();

        licitacionRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(licitacion);

        var proveedorId = Guid.NewGuid();

        var proveedorRepositorio = new Mock<IProveedorRepository>();

        proveedorRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                proveedorId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Proveedor?)null);

        var servicio = CrearServicio(
            ofertaRepositorio,
            licitacionRepositorio,
            proveedorRepositorio);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => servicio.RegistrarAsync(
                licitacion.Id,
                new RegistrarOfertaRequest(
                    proveedorId,
                    50000m)));
    }

    [Fact]
    public async Task EliminarAsync_elimina_y_guarda_si_la_licitacion_esta_abierta()
    {
        var licitacion = CrearLicitacion();

        var proveedor = Proveedor.Crear(
            "Proveedor Central",
            Reloj);

        var oferta = RegistroOfertaService.Registrar(
            licitacion,
            proveedor.Id,
            70000m,
            [],
            Reloj);

        var ofertaRepositorio = new Mock<IOfertaRepository>();

        ofertaRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                oferta.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(oferta);

        var licitacionRepositorio = new Mock<ILicitacionRepository>();

        licitacionRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                licitacion.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(licitacion);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            ofertaRepositorio,
            licitacionRepositorio,
            unitOfWork: unitOfWork);

        await servicio.EliminarAsync(oferta.Id);

        ofertaRepositorio.Verify(
            x => x.Eliminar(oferta),
            Times.Once);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_no_permite_eliminar_oferta_de_licitacion_cerrada()
    {
        var licitacion = CrearLicitacion();
        licitacion.Cerrar(Reloj);

        var proveedor = Proveedor.Crear(
            "Proveedor Central",
            Reloj);

        var oferta = RegistroOfertaService.Registrar(
            CrearLicitacion(),
            proveedor.Id,
            70000m,
            [],
            Reloj);

        var ofertaRepositorio = new Mock<IOfertaRepository>();

        ofertaRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                oferta.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(oferta);

        var licitacionRepositorio = new Mock<ILicitacionRepository>();

        licitacionRepositorio
            .Setup(x => x.ObtenerPorIdAsync(
                oferta.LicitacionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(licitacion);

        var servicio = CrearServicio(
            ofertaRepositorio,
            licitacionRepositorio);

        await Assert.ThrowsAsync<LicitacionVencidaException>(
            () => servicio.EliminarAsync(oferta.Id));

        ofertaRepositorio.Verify(
            x => x.Eliminar(It.IsAny<Oferta>()),
            Times.Never);
    }
}