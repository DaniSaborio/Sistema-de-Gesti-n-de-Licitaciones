using Licitaciones.Application.Common;
using Licitaciones.Domain.Common;
using Licitaciones.Domain.Proveedores;

namespace Licitaciones.Application.Proveedores;

public interface IProveedorService
{
    Task<ProveedorDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<ProveedorDto>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default);
    Task<List<ProveedorDto>> ListarActivosAsync(CancellationToken cancellationToken = default);
    Task<ProveedorDto> CrearAsync(CrearProveedorRequest request, CancellationToken cancellationToken = default);
    Task<ProveedorDto> ActualizarAsync(Guid id, ActualizarProveedorRequest request, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class ProveedorService(
    IProveedorRepository repositorio,
    IUnitOfWork unitOfWork,
    IClock clock) : IProveedorService
{
    public async Task<ProveedorDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proveedor = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Proveedor), id);
        return AProveedorDto(proveedor);
    }

    public async Task<ResultadoPaginado<ProveedorDto>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default)
    {
        var resultado = await repositorio.ListarAsync(consulta, cancellationToken);
        return new ResultadoPaginado<ProveedorDto>(
            resultado.Elementos.Select(AProveedorDto).ToList(),
            resultado.TotalElementos,
            resultado.Pagina,
            resultado.TamanoPagina);
    }

    public async Task<List<ProveedorDto>> ListarActivosAsync(CancellationToken cancellationToken = default) =>
        (await repositorio.ListarActivosAsync(cancellationToken)).Select(AProveedorDto).ToList();

    public async Task<ProveedorDto> CrearAsync(CrearProveedorRequest request, CancellationToken cancellationToken = default)
    {
        var normalizado = NormalizacionTexto.Normalizar(request.Nombre);
        if (await repositorio.ExisteNombreNormalizadoAsync(normalizado, excluirId: null, cancellationToken))
        {
            throw new ConflictoDeUnicidadException($"Ya existe un proveedor con el nombre '{request.Nombre}'.");
        }

        var proveedor = Proveedor.Crear(request.Nombre, clock);
        repositorio.Agregar(proveedor);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return AProveedorDto(proveedor);
    }

    public async Task<ProveedorDto> ActualizarAsync(Guid id, ActualizarProveedorRequest request, CancellationToken cancellationToken = default)
    {
        var proveedor = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Proveedor), id);

        var normalizado = NormalizacionTexto.Normalizar(request.Nombre);
        if (await repositorio.ExisteNombreNormalizadoAsync(normalizado, excluirId: id, cancellationToken))
        {
            throw new ConflictoDeUnicidadException($"Ya existe un proveedor con el nombre '{request.Nombre}'.");
        }

        proveedor.ActualizarNombre(request.Nombre, clock);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return AProveedorDto(proveedor);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proveedor = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Proveedor), id);

        proveedor.EliminarLogicamente(clock);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
    }

    private static ProveedorDto AProveedorDto(Proveedor proveedor) =>
        new(proveedor.Id, proveedor.Nombre, proveedor.CreatedAt, proveedor.UpdatedAt);
}
