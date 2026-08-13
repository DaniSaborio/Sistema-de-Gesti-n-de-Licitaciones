using Asp.Versioning;
using Licitaciones.Application.Common;
using Licitaciones.Application.TiposCambio;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tipos-cambio")]
[Produces("application/json")]
public sealed class TiposCambioController(ITipoCambioService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<TipoCambioDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<TipoCambioDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10, CancellationToken cancellationToken = default)
    {
        var resultado = await service.ListarAsync(new ConsultaPaginada(pagina, tamanoPagina), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TipoCambioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TipoCambioDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var tipoCambio = await service.ObtenerAsync(id, cancellationToken);
        return Ok(tipoCambio);
    }

    [HttpPost]
    [ProducesResponseType<TipoCambioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TipoCambioDto>> Crear([FromBody] CrearTipoCambioRequest request, CancellationToken cancellationToken)
    {
        var creado = await service.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id, version = "1.0" }, creado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<TipoCambioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TipoCambioDto>> Actualizar(Guid id, [FromBody] ActualizarTipoCambioRequest request, CancellationToken cancellationToken)
    {
        var actualizado = await service.ActualizarAsync(id, request, cancellationToken);
        return Ok(actualizado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await service.EliminarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activar")]
    [ProducesResponseType<TipoCambioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TipoCambioDto>> Activar(Guid id, CancellationToken cancellationToken)
    {
        var activado = await service.ActivarAsync(id, cancellationToken);
        return Ok(activado);
    }
}
