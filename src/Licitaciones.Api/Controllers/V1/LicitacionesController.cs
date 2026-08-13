using Asp.Versioning;
using Licitaciones.Application.Common;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Ofertas;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/licitaciones")]
[Produces("application/json")]
public sealed class LicitacionesController(ILicitacionService service, IOfertaService ofertaService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<LicitacionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<LicitacionDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10,
        [FromQuery] string? busqueda = null, [FromQuery] string? ordenarPor = null, [FromQuery] bool descendente = false,
        CancellationToken cancellationToken = default)
    {
        var resultado = await service.ListarAsync(new ConsultaPaginada(pagina, tamanoPagina, busqueda, ordenarPor, descendente), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<LicitacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LicitacionDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var licitacion = await service.ObtenerAsync(id, cancellationToken);
        return Ok(licitacion);
    }

    [HttpPost]
    [ProducesResponseType<LicitacionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LicitacionDto>> Crear([FromBody] CrearLicitacionRequest request, CancellationToken cancellationToken)
    {
        var creada = await service.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id, version = "1.0" }, creada);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<LicitacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LicitacionDto>> Actualizar(Guid id, [FromBody] ActualizarLicitacionRequest request, CancellationToken cancellationToken)
    {
        var actualizada = await service.ActualizarAsync(id, request, cancellationToken);
        return Ok(actualizada);
    }

    [HttpPatch("{id:guid}/estado")]
    [ProducesResponseType<LicitacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LicitacionDto>> CambiarEstado(Guid id, [FromBody] CambiarEstadoLicitacionRequest request, CancellationToken cancellationToken)
    {
        var actualizada = await service.CambiarEstadoAsync(id, request, cancellationToken);
        return Ok(actualizada);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await service.EliminarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/ofertas")]
    [ProducesResponseType<ResultadoPaginado<OfertaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<OfertaDto>>> ListarOfertas(
        Guid id, [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10, CancellationToken cancellationToken = default)
    {
        var resultado = await ofertaService.ListarAsync(new ConsultaPaginada(pagina, tamanoPagina), id, null, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("{id:guid}/ofertas")]
    [ProducesResponseType<OfertaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OfertaDto>> RegistrarOferta(Guid id, [FromBody] RegistrarOfertaRequest request, CancellationToken cancellationToken)
    {
        var oferta = await ofertaService.RegistrarAsync(id, request, cancellationToken);
        return CreatedAtAction("ObtenerPorId", "Ofertas", new { id = oferta.Id, version = "1.0" }, oferta);
    }

    [HttpGet("{id:guid}/mejor-oferta")]
    [ProducesResponseType<MejorOfertaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MejorOfertaDto>> ObtenerMejorOferta(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await service.ObtenerMejorOfertaAsync(id, cancellationToken);
        return Ok(resultado);
    }
}
