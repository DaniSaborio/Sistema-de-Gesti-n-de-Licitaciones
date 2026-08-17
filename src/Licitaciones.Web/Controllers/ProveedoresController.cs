using Licitaciones.Application.Common;
using Licitaciones.Application.Proveedores;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Web.Models.Proveedores;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

public sealed class ProveedoresController(IProveedorService service, ITipoCambioService tipoCambioService)
    : LicitacionesWebControllerBase(tipoCambioService)
{
    public async Task<IActionResult> Index(string? busqueda, int pagina = 1, CancellationToken cancellationToken = default)
    {
        var resultado = await service.ListarAsync(new ConsultaPaginada(pagina, 10, busqueda, "nombre"), cancellationToken);
        ViewData["Busqueda"] = busqueda;
        return View(resultado);
    }

public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
{
    var proveedor = await service.ObtenerAsync(id, cancellationToken);
    return View(proveedor);
}
    public IActionResult Create() => View(new ProveedorFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProveedorFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            await service.CrearAsync(new CrearProveedorRequest(modelo.Nombre), cancellationToken);
            TempData["Exito"] = $"Proveedor '{modelo.Nombre}' creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (ConflictoDeUnicidadException ex)
        {
            ModelState.AddModelError(nameof(modelo.Nombre), ex.Message);
            return View(modelo);
        }
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var proveedor = await service.ObtenerAsync(id, cancellationToken);
        return View(new ProveedorFormViewModel { Id = proveedor.Id, Nombre = proveedor.Nombre });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProveedorFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            await service.ActualizarAsync(id, new ActualizarProveedorRequest(modelo.Nombre), cancellationToken);
            TempData["Exito"] = "Proveedor actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (ConflictoDeUnicidadException ex)
        {
            ModelState.AddModelError(nameof(modelo.Nombre), ex.Message);
            return View(modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.EliminarAsync(id, cancellationToken);
        TempData["Exito"] = "Proveedor eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
