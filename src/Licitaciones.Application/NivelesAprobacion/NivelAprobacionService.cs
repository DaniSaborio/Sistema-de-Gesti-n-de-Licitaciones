using Licitaciones.Application.Common;
using Licitaciones.Domain.Common;
using Licitaciones.Domain.NivelesAprobacion;

namespace Licitaciones.Application.NivelesAprobacion;

public interface INivelAprobacionService
{
    Task<NivelAprobacionDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<NivelAprobacionDto>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default);
    Task<NivelAprobacionDto> CrearAsync(CrearNivelAprobacionRequest request, CancellationToken cancellationToken = default);
    Task<NivelAprobacionDto> ActualizarAsync(Guid id, ActualizarNivelAprobacionRequest request, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NivelAprobacionDto> ResolverParaMontoAsync(decimal montoCRC, CancellationToken cancellationToken = default);
}

public sealed class NivelAprobacionService(
    INivelAprobacionRepository repositorio,
    IUnitOfWork unitOfWork,
    IClock clock) : INivelAprobacionService
{
    public async Task<NivelAprobacionDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var nivel = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(NivelAprobacion), id);
        return ANivelDto(nivel);
    }

    public async Task<ResultadoPaginado<NivelAprobacionDto>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default)
    {
        var resultado = await repositorio.ListarAsync(consulta, cancellationToken);
        return new ResultadoPaginado<NivelAprobacionDto>(
            resultado.Elementos.Select(ANivelDto).ToList(), resultado.TotalElementos, resultado.Pagina, resultado.TamanoPagina);
    }

    public async Task<NivelAprobacionDto> CrearAsync(CrearNivelAprobacionRequest request, CancellationToken cancellationToken = default)
    {
        var candidato = NivelAprobacion.Crear(request.MontoMinimoCRC, request.MontoMaximoCRC, request.Aprobador, clock);
        var existentes = await repositorio.ListarTodosAsync(cancellationToken);
        ResolutorNivelAprobacion.ValidarNuevoRango(candidato, existentes);

        repositorio.Agregar(candidato);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return ANivelDto(candidato);
    }

    public async Task<NivelAprobacionDto> ActualizarAsync(Guid id, ActualizarNivelAprobacionRequest request, CancellationToken cancellationToken = default)
    {
        var nivel = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(NivelAprobacion), id);

        var candidato = NivelAprobacion.Crear(request.MontoMinimoCRC, request.MontoMaximoCRC, request.Aprobador, clock);
        var existentes = (await repositorio.ListarTodosAsync(cancellationToken)).Where(n => n.Id != id).ToList();
        ResolutorNivelAprobacion.ValidarNuevoRango(candidato, existentes);

        nivel.Actualizar(request.MontoMinimoCRC, request.MontoMaximoCRC, request.Aprobador, clock);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return ANivelDto(nivel);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var nivel = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(NivelAprobacion), id);
        repositorio.Eliminar(nivel);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
    }

    public async Task<NivelAprobacionDto> ResolverParaMontoAsync(decimal montoCRC, CancellationToken cancellationToken = default)
    {
        var niveles = await repositorio.ListarTodosAsync(cancellationToken);
        var nivel = ResolutorNivelAprobacion.Resolver(montoCRC, niveles);
        return ANivelDto(nivel);
    }

    private static NivelAprobacionDto ANivelDto(NivelAprobacion nivel) =>
        new(nivel.Id, nivel.MontoMinimoCRC, nivel.MontoMaximoCRC, nivel.Aprobador);
}
