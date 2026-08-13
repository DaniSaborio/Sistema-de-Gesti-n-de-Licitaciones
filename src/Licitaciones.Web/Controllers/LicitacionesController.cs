using Licitaciones.Application.Common;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Proveedores;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.Common;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Web.Models.Licitaciones;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

public sealed class LicitacionesController(
    ILicitacionService service,
    IOfertaService ofertaService,
    IProveedorService proveedorService,
    ITipoCambioService tipoCambioService)
    : LicitacionesWebControllerBase(tipoCambioService)
{
    public async Task<IActionResult> Index(string? busqueda, string? ordenarPor, bool descendente, int pagina = 1, CancellationToken cancellationToken = default)
    {
        var resultado = await service.ListarAsync(new ConsultaPaginada(pagina, 10, busqueda, ordenarPor, descendente), cancellationToken);
        ViewData["Busqueda"] = busqueda;
        ViewData["OrdenarPor"] = ordenarPor;
        ViewData["Descendente"] = descendente;
        return View(resultado);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var licitacion = await service.ObtenerAsync(id, cancellationToken);
        var mejorOferta = await service.ObtenerMejorOfertaAsync(id, cancellationToken);
        var ofertas = await ofertaService.ListarAsync(new ConsultaPaginada(1, 100), id, null, cancellationToken);
        var proveedores = await proveedorService.ListarActivosAsync(cancellationToken);

        return View(new LicitacionDetalleViewModel
        {
            Licitacion = licitacion,
            MejorOferta = mejorOferta,
            Ofertas = ofertas.Elementos,
            ProveedoresDisponibles = proveedores,
        });
    }

    public IActionResult Create() => View(new LicitacionFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LicitacionFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            var creada = await service.CrearAsync(
                new CrearLicitacionRequest(modelo.Codigo, modelo.Titulo, modelo.FechaCierre, modelo.PresupuestoEstimadoCRC),
                cancellationToken);
            TempData["Exito"] = $"Licitación '{creada.Codigo}' creada en estado Borrador.";
            return RedirectToAction(nameof(Details), new { id = creada.Id });
        }
        catch (Exception ex) when (ex is ConflictoDeUnicidadException or DomainException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var licitacion = await service.ObtenerAsync(id, cancellationToken);
        return View(new LicitacionFormViewModel
        {
            Id = licitacion.Id,
            Codigo = licitacion.Codigo,
            Titulo = licitacion.Titulo,
            FechaCierre = licitacion.FechaCierre.LocalDateTime,
            PresupuestoEstimadoCRC = licitacion.PresupuestoEstimadoCRC,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LicitacionFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            await service.ActualizarAsync(
                id, new ActualizarLicitacionRequest(modelo.Titulo, modelo.FechaCierre, modelo.PresupuestoEstimadoCRC), cancellationToken);
            TempData["Exito"] = "Licitación actualizada correctamente.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.EliminarAsync(id, cancellationToken);
        TempData["Exito"] = "Licitación eliminada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(Guid id, EstadoLicitacion estadoDestino, CancellationToken cancellationToken)
    {
        try
        {
            await service.CambiarEstadoAsync(id, new CambiarEstadoLicitacionRequest(estadoDestino), cancellationToken);
            TempData["Exito"] = $"La licitación ahora está en estado {estadoDestino}.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarOferta(Guid id, Guid proveedorId, decimal montoOfertadoCRC, CancellationToken cancellationToken)
    {
        try
        {
            await ofertaService.RegistrarAsync(id, new RegistrarOfertaRequest(proveedorId, montoOfertadoCRC), cancellationToken);
            TempData["Exito"] = "Oferta registrada correctamente.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (RecursoNoEncontradoException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
