using Licitaciones.Application.Common;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Common;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Ofertas;
using Licitaciones.Domain.Proveedores;

namespace Licitaciones.Application.Ofertas;

public interface IOfertaService
{
    Task<OfertaDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<OfertaDto>> ListarAsync(
        ConsultaPaginada consulta, Guid? licitacionId, Guid? proveedorId, CancellationToken cancellationToken = default);
    Task<OfertaDto> RegistrarAsync(Guid licitacionId, RegistrarOfertaRequest request, CancellationToken cancellationToken = default);
    Task<OfertaDto> ActualizarAsync(Guid id, ActualizarOfertaRequest request, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class OfertaService(
    IOfertaRepository ofertaRepositorio,
    ILicitacionRepository licitacionRepositorio,
    IProveedorRepository proveedorRepositorio,
    IUnitOfWork unitOfWork,
    IClock clock) : IOfertaService
{
    public async Task<OfertaDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var oferta = await ofertaRepositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Oferta), id);
        var proveedor = await proveedorRepositorio.ObtenerPorIdAsync(oferta.ProveedorId, cancellationToken);
        return AOfertaDto(oferta, proveedor);
    }

    public async Task<ResultadoPaginado<OfertaDto>> ListarAsync(
        ConsultaPaginada consulta, Guid? licitacionId, Guid? proveedorId, CancellationToken cancellationToken = default)
    {
        var resultado = await ofertaRepositorio.ListarAsync(consulta, licitacionId, proveedorId, cancellationToken);
        var dtos = new List<OfertaDto>(resultado.Elementos.Count);
        foreach (var oferta in resultado.Elementos)
        {
            var proveedor = await proveedorRepositorio.ObtenerPorIdAsync(oferta.ProveedorId, cancellationToken);
            dtos.Add(AOfertaDto(oferta, proveedor));
        }

        return new ResultadoPaginado<OfertaDto>(dtos, resultado.TotalElementos, resultado.Pagina, resultado.TamanoPagina);
    }

    public async Task<OfertaDto> RegistrarAsync(Guid licitacionId, RegistrarOfertaRequest request, CancellationToken cancellationToken = default)
    {
        var licitacion = await licitacionRepositorio.ObtenerPorIdAsync(licitacionId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Licitacion), licitacionId);
        var proveedor = await proveedorRepositorio.ObtenerPorIdAsync(request.ProveedorId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Proveedor), request.ProveedorId);
        var ofertasExistentes = await ofertaRepositorio.ListarPorLicitacionAsync(licitacionId, cancellationToken);

        var oferta = RegistroOfertaService.Registrar(licitacion, proveedor.Id, request.MontoOfertadoCRC, ofertasExistentes, clock);

        ofertaRepositorio.Agregar(oferta);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return AOfertaDto(oferta, proveedor);
    }

    public async Task<OfertaDto> ActualizarAsync(Guid id, ActualizarOfertaRequest request, CancellationToken cancellationToken = default)
    {
        var oferta = await ofertaRepositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Oferta), id);
        var licitacion = await licitacionRepositorio.ObtenerPorIdAsync(oferta.LicitacionId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Licitacion), oferta.LicitacionId);
        var ofertasExistentes = (await ofertaRepositorio.ListarPorLicitacionAsync(oferta.LicitacionId, cancellationToken))
            .Where(o => o.Id != id)
            .ToList();

        // Reutiliza las reglas de aceptación: registra una oferta "reemplazo" con
        // el mismo proveedor y descarta la anterior solo si la nueva es válida.
        var ofertaValidada = RegistroOfertaService.Registrar(licitacion, oferta.ProveedorId, request.MontoOfertadoCRC, ofertasExistentes, clock);

        ofertaRepositorio.Eliminar(oferta);
        ofertaRepositorio.Agregar(ofertaValidada);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        var proveedor = await proveedorRepositorio.ObtenerPorIdAsync(oferta.ProveedorId, cancellationToken);
        return AOfertaDto(ofertaValidada, proveedor);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var oferta = await ofertaRepositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Oferta), id);
        var licitacion = await licitacionRepositorio.ObtenerPorIdAsync(oferta.LicitacionId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Licitacion), oferta.LicitacionId);

        if (licitacion.EstaCerradaFuncionalmente(clock))
        {
            throw new LicitacionVencidaException();
        }

        ofertaRepositorio.Eliminar(oferta);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
    }

    private static OfertaDto AOfertaDto(Oferta oferta, Proveedor? proveedor) => new(
        oferta.Id,
        oferta.LicitacionId,
        oferta.ProveedorId,
        proveedor?.Nombre ?? string.Empty,
        oferta.MontoOfertadoCRC,
        oferta.FechaRegistro);
}
