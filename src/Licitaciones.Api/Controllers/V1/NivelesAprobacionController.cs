using Asp.Versioning;
using Licitaciones.Application.Common;
using Licitaciones.Application.NivelesAprobacion;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/niveles-aprobacion")]
[Produces("application/json")]
public sealed class NivelesAprobacionController(INivelAprobacionService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<NivelAprobacionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<NivelAprobacionDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10, CancellationToken cancellationToken = default)
    {
        var resultado = await service.ListarAsync(new ConsultaPaginada(pagina, tamanoPagina), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<NivelAprobacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NivelAprobacionDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var nivel = await service.ObtenerAsync(id, cancellationToken);
        return Ok(nivel);
    }

    [HttpPost]
    [ProducesResponseType<NivelAprobacionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<NivelAprobacionDto>> Crear([FromBody] CrearNivelAprobacionRequest request, CancellationToken cancellationToken)
    {
        var creado = await service.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id, version = "1.0" }, creado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<NivelAprobacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<NivelAprobacionDto>> Actualizar(Guid id, [FromBody] ActualizarNivelAprobacionRequest request, CancellationToken cancellationToken)
    {
        var actualizado = await service.ActualizarAsync(id, request, cancellationToken);
        return Ok(actualizado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await service.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
