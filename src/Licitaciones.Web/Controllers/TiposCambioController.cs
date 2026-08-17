using Licitaciones.Application.Common;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.Common;
using Licitaciones.Web.Models.TiposCambio;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

public sealed class TiposCambioController(ITipoCambioService tipoCambioService)
    : LicitacionesWebControllerBase(tipoCambioService)
{
    public async Task<IActionResult> Index(int pagina = 1, CancellationToken cancellationToken = default)
    {
        var resultado = await TipoCambioService.ListarAsync(new ConsultaPaginada(pagina, 10), cancellationToken);
        return View(resultado);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var tipoCambio = await TipoCambioService.ObtenerAsync(id, cancellationToken);
        return View(tipoCambio);
    }

    public IActionResult Create() => View(new TipoCambioFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TipoCambioFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            await TipoCambioService.CrearAsync(new CrearTipoCambioRequest(modelo.CRCporUSD, modelo.FechaVigencia), cancellationToken);
            TempData["Exito"] = "Tipo de cambio creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var tipoCambio = await TipoCambioService.ObtenerAsync(id, cancellationToken);
        return View(new TipoCambioFormViewModel { Id = tipoCambio.Id, CRCporUSD = tipoCambio.CRCporUSD, FechaVigencia = tipoCambio.FechaVigencia.LocalDateTime });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TipoCambioFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            await TipoCambioService.ActualizarAsync(id, new ActualizarTipoCambioRequest(modelo.CRCporUSD, modelo.FechaVigencia), cancellationToken);
            TempData["Exito"] = "Tipo de cambio actualizado.";
            return RedirectToAction(nameof(Index));
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
        try
        {
            await TipoCambioService.EliminarAsync(id, cancellationToken);
            TempData["Exito"] = "Tipo de cambio eliminado.";
        }
        catch (ConflictoDeUnicidadException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activar(Guid id, CancellationToken cancellationToken)
    {
        await TipoCambioService.ActivarAsync(id, cancellationToken);
        TempData["Exito"] = "Tipo de cambio activado.";
        return RedirectToAction(nameof(Index));
    }
}
