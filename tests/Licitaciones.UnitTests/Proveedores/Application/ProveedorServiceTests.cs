using Licitaciones.Application.Common;
using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Proveedores;
using Licitaciones.UnitTests.TestUtils;
using Moq;
using Xunit;

namespace Licitaciones.UnitTests.Proveedores.Application;

public class ProveedorServiceTests
{
    private static readonly FixedClock Reloj = FixedClock.En(2026, 1, 1);

    [Fact]
    public async Task ObtenerAsync_devuelve_el_proveedor_como_dto()
    {
        var proveedor = Proveedor.Crear("Empresa Central", Reloj);

        var repositorio = new Mock<IProveedorRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                proveedor.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedor);

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.ObtenerAsync(proveedor.Id);

        Assert.Equal(proveedor.Id, resultado.Id);
        Assert.Equal("Empresa Central", resultado.Nombre);
        Assert.Equal(proveedor.CreatedAt, resultado.CreatedAt);
        Assert.Equal(proveedor.UpdatedAt, resultado.UpdatedAt);
    }

    [Fact]
    public async Task ObtenerAsync_lanza_excepcion_si_el_proveedor_no_existe()
    {
        var id = Guid.NewGuid();

        var repositorio = new Mock<IProveedorRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Proveedor?)null);

        var servicio = CrearServicio(repositorio);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => servicio.ObtenerAsync(id));
    }

    [Fact]
    public async Task ListarAsync_mapea_elementos_y_conserva_datos_de_paginacion()
    {
        var proveedor1 = Proveedor.Crear("Empresa Uno", Reloj);
        var proveedor2 = Proveedor.Crear("Empresa Dos", Reloj);

        var consulta = new ConsultaPaginada(
            2,
            10,
            "Empresa");

        var repositorio = new Mock<IProveedorRepository>();

        repositorio
            .Setup(x => x.ListarAsync(
                consulta,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ResultadoPaginado<Proveedor>(
                    [proveedor1, proveedor2],
                    25,
                    2,
                    10));

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.ListarAsync(consulta);

        Assert.Equal(2, resultado.Elementos.Count);
        Assert.Equal(proveedor1.Id, resultado.Elementos[0].Id);
        Assert.Equal(proveedor2.Id, resultado.Elementos[1].Id);
        Assert.Equal(25, resultado.TotalElementos);
        Assert.Equal(2, resultado.Pagina);
        Assert.Equal(10, resultado.TamanoPagina);
    }

    [Fact]
    public async Task ListarActivosAsync_devuelve_los_proveedores_mapeados()
    {
        var proveedor1 = Proveedor.Crear("Empresa Uno", Reloj);
        var proveedor2 = Proveedor.Crear("Empresa Dos", Reloj);

        var repositorio = new Mock<IProveedorRepository>();

        repositorio
            .Setup(x => x.ListarActivosAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([proveedor1, proveedor2]);

        var servicio = CrearServicio(repositorio);

        var resultado = await servicio.ListarActivosAsync();

        Assert.Collection(
            resultado,
            primero => Assert.Equal(proveedor1.Id, primero.Id),
            segundo => Assert.Equal(proveedor2.Id, segundo.Id));
    }

    [Fact]
    public async Task CrearAsync_crea_agrega_y_guarda_el_proveedor()
    {
        var repositorio = new Mock<IProveedorRepository>();

        repositorio
            .Setup(x => x.ExisteNombreNormalizadoAsync(
                "EMPRESA CENTRAL",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var unitOfWork = new Mock<IUnitOfWork>();

        unitOfWork
            .Setup(x => x.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        var resultado = await servicio.CrearAsync(
            new CrearProveedorRequest(
                "  Empresa Central  "));

        Assert.Equal(
            "Empresa Central",
            resultado.Nombre);

        Assert.Equal(
            Reloj.UtcNow,
            resultado.CreatedAt);

        repositorio.Verify(
            x => x.Agregar(
                It.Is<Proveedor>(p =>
                    p.Nombre == "Empresa Central" &&
                    p.NombreNormalizado == "EMPRESA CENTRAL")),
            Times.Once);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CrearAsync_lanza_conflicto_si_el_nombre_ya_existe()
    {
        var repositorio = new Mock<IProveedorRepository>();

        repositorio
            .Setup(x => x.ExisteNombreNormalizadoAsync(
                "EMPRESA CENTRAL",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictoDeUnicidadException>(
            () => servicio.CrearAsync(
                new CrearProveedorRequest(
                    "Empresa Central")));

        repositorio.Verify(
            x => x.Agregar(
                It.IsAny<Proveedor>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActualizarAsync_actualiza_y_guarda_el_proveedor()
    {
        var proveedor = Proveedor.Crear(
            "Empresa Antigua",
            Reloj);

        Reloj.UtcNow = new DateTimeOffset(
            2026,
            1,
            2,
            0,
            0,
            0,
            TimeSpan.Zero);

        var repositorio = new Mock<IProveedorRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                proveedor.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedor);

        repositorio
            .Setup(x => x.ExisteNombreNormalizadoAsync(
                "EMPRESA NUEVA",
                proveedor.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var unitOfWork = new Mock<IUnitOfWork>();

        unitOfWork
            .Setup(x => x.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        var resultado = await servicio.ActualizarAsync(
            proveedor.Id,
            new ActualizarProveedorRequest(
                "Empresa Nueva"));

        Assert.Equal(
            "Empresa Nueva",
            resultado.Nombre);

        Assert.Equal(
            "EMPRESA NUEVA",
            proveedor.NombreNormalizado);

        Assert.Equal(
            Reloj.UtcNow,
            resultado.UpdatedAt);

        repositorio.Verify(
            x => x.ExisteNombreNormalizadoAsync(
                "EMPRESA NUEVA",
                proveedor.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActualizarAsync_lanza_conflicto_si_el_nombre_ya_existe()
    {
        var proveedor = Proveedor.Crear(
            "Empresa Antigua",
            Reloj);

        var repositorio = new Mock<IProveedorRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                proveedor.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedor);

        repositorio
            .Setup(x => x.ExisteNombreNormalizadoAsync(
                "EMPRESA NUEVA",
                proveedor.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var unitOfWork = new Mock<IUnitOfWork>();

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictoDeUnicidadException>(
            () => servicio.ActualizarAsync(
                proveedor.Id,
                new ActualizarProveedorRequest(
                    "Empresa Nueva")));

        Assert.Equal(
            "Empresa Antigua",
            proveedor.Nombre);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EliminarAsync_marca_el_proveedor_como_eliminado_y_guarda()
    {
        var proveedor = Proveedor.Crear(
            "Empresa Central",
            Reloj);

        Reloj.UtcNow = new DateTimeOffset(
            2026,
            1,
            2,
            0,
            0,
            0,
            TimeSpan.Zero);

        var repositorio = new Mock<IProveedorRepository>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(
                proveedor.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedor);

        var unitOfWork = new Mock<IUnitOfWork>();

        unitOfWork
            .Setup(x => x.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var servicio = CrearServicio(
            repositorio,
            unitOfWork);

        await servicio.EliminarAsync(proveedor.Id);

        Assert.True(proveedor.EstaEliminado);

        Assert.Equal(
            Reloj.UtcNow,
            proveedor.DeletedAt);

        Assert.Equal(
            Reloj.UtcNow,
            proveedor.UpdatedAt);

        unitOfWork.Verify(
            x => x.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private ProveedorService CrearServicio(
        Mock<IProveedorRepository> repositorio,
        Mock<IUnitOfWork>? unitOfWork = null)
    {
        return new ProveedorService(
            repositorio.Object,
            (unitOfWork ?? new Mock<IUnitOfWork>()).Object,
            Reloj);
    }
}