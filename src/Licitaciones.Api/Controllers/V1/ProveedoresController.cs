using Asp.Versioning;
using Licitaciones.Application.Common;
using Licitaciones.Application.Proveedores;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/proveedores")]
[Produces("application/json")]
public sealed class ProveedoresController(IProveedorService service) : ControllerBase
{
    /// <summary>Lista proveedores con paginación, búsqueda y orden.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<ProveedorDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<ProveedorDto>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10,
        [FromQuery] string? busqueda = null, [FromQuery] string? ordenarPor = null, [FromQuery] bool descendente = false,
        CancellationToken cancellationToken = default)
    {
        var resultado = await service.ListarAsync(new ConsultaPaginada(pagina, tamanoPagina, busqueda, ordenarPor, descendente), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProveedorDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProveedorDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var proveedor = await service.ObtenerAsync(id, cancellationToken);
        return Ok(proveedor);
    }

    [HttpPost]
    [ProducesResponseType<ProveedorDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProveedorDto>> Crear([FromBody] CrearProveedorRequest request, CancellationToken cancellationToken)
    {
        var creado = await service.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id, version = "1.0" }, creado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ProveedorDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProveedorDto>> Actualizar(Guid id, [FromBody] ActualizarProveedorRequest request, CancellationToken cancellationToken)
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
