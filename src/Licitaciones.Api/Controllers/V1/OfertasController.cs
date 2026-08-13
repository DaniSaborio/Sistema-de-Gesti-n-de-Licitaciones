using Asp.Versioning;
using Licitaciones.Application.Common;
using Licitaciones.Application.Ofertas;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ofertas")]
[Produces("application/json")]
public sealed class OfertasController(IOfertaService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<OfertaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<OfertaDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10,
        [FromQuery] Guid? licitacionId = null, [FromQuery] Guid? proveedorId = null,
        [FromQuery] string? ordenarPor = null, [FromQuery] bool descendente = false,
        CancellationToken cancellationToken = default)
    {
        var consulta = new ConsultaPaginada(pagina, tamanoPagina, OrdenarPor: ordenarPor, Descendente: descendente);
        var resultado = await service.ListarAsync(consulta, licitacionId, proveedorId, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<OfertaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OfertaDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var oferta = await service.ObtenerAsync(id, cancellationToken);
        return Ok(oferta);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<OfertaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OfertaDto>> Actualizar(Guid id, [FromBody] ActualizarOfertaRequest request, CancellationToken cancellationToken)
    {
        var actualizada = await service.ActualizarAsync(id, request, cancellationToken);
        return Ok(actualizada);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await service.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
