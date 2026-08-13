using Licitaciones.Application.Common;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Proveedores;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.Common;
using Licitaciones.Web.Models.Ofertas;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

public sealed class OfertasController(
    IOfertaService service,
    IProveedorService proveedorService,
    ILicitacionService licitacionService,
    ITipoCambioService tipoCambioService)
    : LicitacionesWebControllerBase(tipoCambioService)
{
    public async Task<IActionResult> Index(Guid? licitacionId, Guid? proveedorId, int pagina = 1, CancellationToken cancellationToken = default)
    {
        var resultado = await service.ListarAsync(new ConsultaPaginada(pagina, 10), licitacionId, proveedorId, cancellationToken);

        ViewData["LicitacionId"] = licitacionId;
        ViewData["ProveedorId"] = proveedorId;
        ViewData["Licitaciones"] = (await licitacionService.ListarAsync(new ConsultaPaginada(1, 200), cancellationToken)).Elementos;
        ViewData["Proveedores"] = await proveedorService.ListarActivosAsync(cancellationToken);

        return View(resultado);
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var oferta = await service.ObtenerAsync(id, cancellationToken);
        var licitacion = await licitacionService.ObtenerAsync(oferta.LicitacionId, cancellationToken);
        return View(new OfertaEditViewModel
        {
            Id = oferta.Id,
            LicitacionCodigo = licitacion.Codigo,
            ProveedorNombre = oferta.ProveedorNombre,
            MontoOfertadoCRC = oferta.MontoOfertadoCRC,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, OfertaEditViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            await service.ActualizarAsync(id, new ActualizarOfertaRequest(modelo.MontoOfertadoCRC), cancellationToken);
            TempData["Exito"] = "Oferta actualizada correctamente.";
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
            await service.EliminarAsync(id, cancellationToken);
            TempData["Exito"] = "Oferta eliminada.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
