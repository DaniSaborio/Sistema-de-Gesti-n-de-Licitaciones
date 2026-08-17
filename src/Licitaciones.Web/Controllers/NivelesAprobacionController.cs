using Licitaciones.Application.Common;
using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.Common;
using Licitaciones.Web.Models.NivelesAprobacion;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

public sealed class NivelesAprobacionController(INivelAprobacionService service, ITipoCambioService tipoCambioService)
    : LicitacionesWebControllerBase(tipoCambioService)
{
    public async Task<IActionResult> Index(int pagina = 1, CancellationToken cancellationToken = default)
    {
        var resultado = await service.ListarAsync(new ConsultaPaginada(pagina, 10), cancellationToken);
        return View(resultado);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
{
    var nivel = await service.ObtenerAsync(id, cancellationToken);
    return View(nivel);
}

    public IActionResult Create() => View(new NivelAprobacionFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NivelAprobacionFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            await service.CrearAsync(new CrearNivelAprobacionRequest(modelo.MontoMinimoCRC, modelo.MontoMaximoCRC, modelo.Aprobador), cancellationToken);
            TempData["Exito"] = "Nivel de aprobación creado correctamente.";
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
        var nivel = await service.ObtenerAsync(id, cancellationToken);
        return View(new NivelAprobacionFormViewModel
        {
            Id = nivel.Id,
            MontoMinimoCRC = nivel.MontoMinimoCRC,
            MontoMaximoCRC = nivel.MontoMaximoCRC,
            Aprobador = nivel.Aprobador,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, NivelAprobacionFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            await service.ActualizarAsync(id, new ActualizarNivelAprobacionRequest(modelo.MontoMinimoCRC, modelo.MontoMaximoCRC, modelo.Aprobador), cancellationToken);
            TempData["Exito"] = "Nivel de aprobación actualizado.";
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
        await service.EliminarAsync(id, cancellationToken);
        TempData["Exito"] = "Nivel de aprobación eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
