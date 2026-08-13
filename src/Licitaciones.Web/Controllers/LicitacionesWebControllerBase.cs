using Licitaciones.Application.TiposCambio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Licitaciones.Web.Controllers;

/// <summary>
/// Controlador base para las páginas MVC: precarga el tipo de cambio activo una
/// sola vez por solicitud en ViewData, para que el TagHelper &lt;monto&gt; pueda
/// alternar CRC/USD sin consultas adicionales por cada valor mostrado.
/// </summary>
public abstract class LicitacionesWebControllerBase(ITipoCambioService tipoCambioService) : Controller
{
    public const string TipoCambioActivoViewDataKey = "TipoCambioActivo";

    /// <summary>Expuesto para que controladores derivados (p. ej. TiposCambioController) no necesiten inyectarlo dos veces.</summary>
    protected ITipoCambioService TipoCambioService { get; } = tipoCambioService;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var activo = await TipoCambioService.ObtenerActivoAsync(context.HttpContext.RequestAborted);
        ViewData[TipoCambioActivoViewDataKey] = activo?.CRCporUSD;

        await base.OnActionExecutionAsync(context, next);
    }
}
