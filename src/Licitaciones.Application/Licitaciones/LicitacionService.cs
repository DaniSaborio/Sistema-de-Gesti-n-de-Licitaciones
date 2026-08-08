using Licitaciones.Application.Common;
using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Application.Ofertas;
using Licitaciones.Domain.Common;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.NivelesAprobacion;
using Licitaciones.Domain.Ofertas;

namespace Licitaciones.Application.Licitaciones;

public interface ILicitacionService
{
    Task<LicitacionDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<LicitacionDto>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default);
    Task<LicitacionDto> CrearAsync(CrearLicitacionRequest request, CancellationToken cancellationToken = default);
    Task<LicitacionDto> ActualizarAsync(Guid id, ActualizarLicitacionRequest request, CancellationToken cancellationToken = default);
    Task<LicitacionDto> CambiarEstadoAsync(Guid id, CambiarEstadoLicitacionRequest request, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MejorOfertaDto> ObtenerMejorOfertaAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class LicitacionService(
    ILicitacionRepository repositorio,
    IOfertaRepository ofertaRepositorio,
    INivelAprobacionRepository nivelAprobacionRepositorio,
    IUnitOfWork unitOfWork,
    IClock clock) : ILicitacionService
{
    public async Task<LicitacionDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var licitacion = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Licitacion), id);
        return ADto(licitacion);
    }

    public async Task<ResultadoPaginado<LicitacionDto>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default)
    {
        var resultado = await repositorio.ListarAsync(consulta, cancellationToken);
        return new ResultadoPaginado<LicitacionDto>(
            resultado.Elementos.Select(ADto).ToList(), resultado.TotalElementos, resultado.Pagina, resultado.TamanoPagina);
    }

    public async Task<LicitacionDto> CrearAsync(CrearLicitacionRequest request, CancellationToken cancellationToken = default)
    {
        var normalizado = NormalizacionTexto.Normalizar(request.Codigo);
        if (await repositorio.ExisteCodigoNormalizadoAsync(normalizado, excluirId: null, cancellationToken))
        {
            throw new ConflictoDeUnicidadException($"Ya existe una licitación con el código '{request.Codigo}'.");
        }

        var licitacion = Licitacion.Crear(request.Codigo, request.Titulo, request.FechaCierre, request.PresupuestoEstimadoCRC, clock);
        repositorio.Agregar(licitacion);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return ADto(licitacion);
    }

    public async Task<LicitacionDto> ActualizarAsync(Guid id, ActualizarLicitacionRequest request, CancellationToken cancellationToken = default)
    {
        var licitacion = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Licitacion), id);
        var montoMinimoExistente = await ofertaRepositorio.ObtenerMontoMinimoAsync(id, cancellationToken);

        licitacion.ActualizarDatos(request.Titulo, request.FechaCierre, request.PresupuestoEstimadoCRC, montoMinimoExistente, clock);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return ADto(licitacion);
    }

    public async Task<LicitacionDto> CambiarEstadoAsync(Guid id, CambiarEstadoLicitacionRequest request, CancellationToken cancellationToken = default)
    {
        var licitacion = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Licitacion), id);

        switch (request.EstadoDestino)
        {
            case EstadoLicitacion.Publicada when licitacion.Estado == EstadoLicitacion.Borrador:
                licitacion.Publicar(clock);
                break;
            case EstadoLicitacion.Publicada:
                licitacion.Reabrir(EstadoLicitacion.Publicada, clock);
                break;
            case EstadoLicitacion.Cerrada:
                licitacion.Cerrar(clock);
                break;
            case EstadoLicitacion.Borrador:
                licitacion.Reabrir(EstadoLicitacion.Borrador, clock);
                break;
            default:
                throw new TransicionEstadoInvalidaException(licitacion.Estado, request.EstadoDestino);
        }

        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return ADto(licitacion);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var licitacion = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Licitacion), id);
        licitacion.EliminarLogicamente(clock);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
    }

    public async Task<MejorOfertaDto> ObtenerMejorOfertaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var licitacion = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Licitacion), id);
        var ofertas = await ofertaRepositorio.ListarPorLicitacionAsync(id, cancellationToken);

        var mejorOferta = EvaluadorOfertas.MejorOferta(ofertas);
        var clasificacion = EvaluadorOfertas.Clasificar(licitacion, mejorOferta);

        string? aprobador = null;
        if (mejorOferta is not null)
        {
            var niveles = await nivelAprobacionRepositorio.ListarTodosAsync(cancellationToken);
            aprobador = TryResolverAprobador(mejorOferta.MontoOfertadoCRC, niveles);
        }

        decimal? porcentajeAhorro = mejorOferta is null
            ? null
            : Math.Round((licitacion.PresupuestoEstimadoCRC - mejorOferta.MontoOfertadoCRC) / licitacion.PresupuestoEstimadoCRC * 100m, 2);

        return new MejorOfertaDto(
            clasificacion,
            mejorOferta?.Id,
            mejorOferta?.ProveedorId,
            mejorOferta?.MontoOfertadoCRC,
            porcentajeAhorro,
            aprobador);
    }

    private static string? TryResolverAprobador(decimal monto, IEnumerable<NivelAprobacion> niveles)
    {
        try
        {
            return ResolutorNivelAprobacion.Resolver(monto, niveles).Aprobador;
        }
        catch (NivelAprobacionNoConfiguradoException)
        {
            return null;
        }
    }

    private LicitacionDto ADto(Licitacion licitacion) => new(
        licitacion.Id,
        licitacion.Codigo,
        licitacion.Titulo,
        licitacion.Estado,
        licitacion.EstaCerradaFuncionalmente(clock),
        licitacion.FechaCierre,
        licitacion.PresupuestoEstimadoCRC,
        licitacion.CreatedAt,
        licitacion.UpdatedAt);
}
