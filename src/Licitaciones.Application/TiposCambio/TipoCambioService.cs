using Licitaciones.Application.Common;
using Licitaciones.Domain.Common;
using Licitaciones.Domain.TiposCambio;

namespace Licitaciones.Application.TiposCambio;

public interface ITipoCambioService
{
    Task<TipoCambioDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TipoCambioDto?> ObtenerActivoAsync(CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<TipoCambioDto>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default);
    Task<TipoCambioDto> CrearAsync(CrearTipoCambioRequest request, CancellationToken cancellationToken = default);
    Task<TipoCambioDto> ActualizarAsync(Guid id, ActualizarTipoCambioRequest request, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TipoCambioDto> ActivarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> ConvertirCrcAUsdAsync(decimal montoCRC, CancellationToken cancellationToken = default);
}

public sealed class TipoCambioService(
    ITipoCambioRepository repositorio,
    IUnitOfWork unitOfWork,
    IClock clock) : ITipoCambioService
{
    public async Task<TipoCambioDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tipoCambio = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(TipoCambio), id);
        return ADto(tipoCambio);
    }

    public async Task<TipoCambioDto?> ObtenerActivoAsync(CancellationToken cancellationToken = default)
    {
        var activo = await repositorio.ObtenerActivoAsync(cancellationToken);
        return activo is null ? null : ADto(activo);
    }

    public async Task<ResultadoPaginado<TipoCambioDto>> ListarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken = default)
    {
        var resultado = await repositorio.ListarAsync(consulta, cancellationToken);
        return new ResultadoPaginado<TipoCambioDto>(
            resultado.Elementos.Select(ADto).ToList(), resultado.TotalElementos, resultado.Pagina, resultado.TamanoPagina);
    }

    public async Task<TipoCambioDto> CrearAsync(CrearTipoCambioRequest request, CancellationToken cancellationToken = default)
    {
        var tipoCambio = TipoCambio.Crear(request.CRCporUSD, request.FechaVigencia, clock);
        var yaExisteActivo = await repositorio.ObtenerActivoAsync(cancellationToken) is not null;
        if (!yaExisteActivo)
        {
            tipoCambio.Activar(clock);
        }

        repositorio.Agregar(tipoCambio);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return ADto(tipoCambio);
    }

    public async Task<TipoCambioDto> ActualizarAsync(Guid id, ActualizarTipoCambioRequest request, CancellationToken cancellationToken = default)
    {
        var tipoCambio = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(TipoCambio), id);
        tipoCambio.Actualizar(request.CRCporUSD, request.FechaVigencia, clock);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return ADto(tipoCambio);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tipoCambio = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(TipoCambio), id);

        if (tipoCambio.Activo)
        {
            throw new ConflictoDeUnicidadException(
                "No se puede eliminar el tipo de cambio activo; active otro registro primero.");
        }

        repositorio.Eliminar(tipoCambio);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
    }

    public async Task<TipoCambioDto> ActivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tipoCambio = await repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(TipoCambio), id);

        var activoActual = await repositorio.ObtenerActivoAsync(cancellationToken);
        activoActual?.Desactivar(clock);

        tipoCambio.Activar(clock);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return ADto(tipoCambio);
    }

    public async Task<decimal> ConvertirCrcAUsdAsync(decimal montoCRC, CancellationToken cancellationToken = default)
    {
        var activo = await repositorio.ObtenerActivoAsync(cancellationToken)
            ?? throw new TipoCambioNoConfiguradoException();
        return ConversorMoneda.ConvertirCrcAUsd(montoCRC, activo);
    }

    private static TipoCambioDto ADto(TipoCambio tipoCambio) =>
        new(tipoCambio.Id, tipoCambio.CRCporUSD, tipoCambio.FechaVigencia, tipoCambio.Activo);
}
