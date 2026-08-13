using System.Globalization;
using Licitaciones.Web.Controllers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Licitaciones.Web.TagHelpers;

/// <summary>
/// Renderiza un monto en CRC (fuente de verdad) y, si hay tipo de cambio activo,
/// su equivalente en USD; el botón de alternancia CRC/USD del layout controla
/// cuál se muestra mediante CSS (sección 9 del enunciado).
/// Uso: &lt;monto crc-amount="Model.PresupuestoEstimadoCRC" /&gt;
/// </summary>
[HtmlTargetElement("monto")]
public sealed class MontoTagHelper : TagHelper
{
    private static readonly CultureInfo CulturaCostaRica = CultureInfo.GetCultureInfo("es-CR");

    [HtmlAttributeName("crc-amount")]
    public decimal CrcAmount { get; set; }

    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.Attributes.SetAttribute("class", "monto-valor");

        var textoCrc = CrcAmount.ToString("C2", CulturaCostaRica);
        output.Content.AppendHtml($"<span class=\"monto-crc\">{textoCrc}</span>");

        var tipoCambio = ViewContext.ViewData[LicitacionesWebControllerBase.TipoCambioActivoViewDataKey] as decimal?;
        if (tipoCambio is > 0)
        {
            var montoUsd = Math.Round(CrcAmount / tipoCambio.Value, 2, MidpointRounding.AwayFromZero);
            var textoUsd = montoUsd.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
            output.Content.AppendHtml($"<span class=\"monto-usd\">{textoUsd}</span>");
        }
    }
}
