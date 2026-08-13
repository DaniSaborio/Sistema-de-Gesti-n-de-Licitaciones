using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace Licitaciones.FunctionalTests;

/// <summary>
/// Flujo funcional de extremo a extremo desde el navegador (sección 5.3 y 12.3):
/// landing → proveedor → licitación → publicar → oferta → rechazo de duplicada →
/// mejor oferta, y el ciclo de estados hasta el cierre.
/// </summary>
[Collection("AplicacionWeb")]
public class FlujoCompletoTests(AplicacionWebFixture fixture)
{
    [Fact]
    public async Task La_landing_page_explica_el_flujo_y_permite_navegar_a_los_modulos()
    {
        var page = await fixture.Browser.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        await Expect(page).ToHaveTitleAsync(new Regex("Inicio"));
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Sistema de Gestión de Licitaciones" })).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Link, new() { Name = "Gestionar licitaciones" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex("/Licitaciones$"));

        await page.CloseAsync();
    }

    [Fact]
    public async Task Ciclo_completo_proveedor_licitacion_oferta_y_mejor_oferta()
    {
        var page = await fixture.Browser.NewPageAsync();
        var sufijo = Guid.NewGuid().ToString("N")[..8];

        // 1. Crear proveedor.
        await page.GotoAsync($"{fixture.BaseUrl}/Proveedores/Create");
        await page.GetByLabel("Nombre").FillAsync($"Proveedor E2E {sufijo}");
        await page.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();
        await Expect(page.GetByText($"Proveedor 'Proveedor E2E {sufijo}' creado")).ToBeVisibleAsync();

        // 2. Crear licitación.
        await page.GotoAsync($"{fixture.BaseUrl}/Licitaciones/Create");
        await page.GetByLabel("Código").FillAsync($"LIC-E2E-{sufijo}");
        await page.GetByLabel("Título").FillAsync("Compra de equipo E2E");
        var fechaCierre = DateTime.Now.AddDays(10).ToString("yyyy-MM-ddTHH:mm");
        await page.GetByLabel("Fecha y hora de cierre").FillAsync(fechaCierre);
        await page.GetByLabel("Presupuesto estimado (CRC)").FillAsync("1000000");
        await page.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex("/Licitaciones/Details"));

        // 3. Publicar.
        await page.GetByRole(AriaRole.Button, new() { Name = "Publicar" }).ClickAsync();
        // GetByText("Publicada") es ambiguo: coincide tanto con el badge de estado
        // como con el banner de éxito ("...ahora está en Publicada"); se apunta al
        // badge específicamente por su clase CSS.
        await Expect(page.Locator("span.badge.text-bg-success")).ToHaveTextAsync("Publicada");

        // 4. Registrar oferta.
        await page.GetByLabel("Proveedor").SelectOptionAsync(new SelectOptionValue { Label = $"Proveedor E2E {sufijo}" });
        await page.Locator("input[name='montoOfertadoCRC']").FillAsync("900000");
        await page.GetByRole(AriaRole.Button, new() { Name = "Registrar oferta" }).ClickAsync();
        await Expect(page.GetByText("Oferta registrada correctamente")).ToBeVisibleAsync();

        // 5. La mejor oferta se refleja en el panel correspondiente.
        await Expect(page.GetByText("Oferta conveniente")).ToBeVisibleAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task Registrar_una_oferta_duplicada_muestra_mensaje_de_error_y_no_la_acepta()
    {
        var page = await fixture.Browser.NewPageAsync();
        var sufijo = Guid.NewGuid().ToString("N")[..8];

        await page.GotoAsync($"{fixture.BaseUrl}/Proveedores/Create");
        await page.GetByLabel("Nombre").FillAsync($"Proveedor Dup {sufijo}");
        await page.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await page.GotoAsync($"{fixture.BaseUrl}/Licitaciones/Create");
        await page.GetByLabel("Código").FillAsync($"LIC-DUP-{sufijo}");
        await page.GetByLabel("Título").FillAsync("Licitación para oferta duplicada");
        await page.GetByLabel("Fecha y hora de cierre").FillAsync(DateTime.Now.AddDays(10).ToString("yyyy-MM-ddTHH:mm"));
        await page.GetByLabel("Presupuesto estimado (CRC)").FillAsync("500000");
        await page.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Publicar" }).ClickAsync();

        await page.GetByLabel("Proveedor").SelectOptionAsync(new SelectOptionValue { Label = $"Proveedor Dup {sufijo}" });
        await page.Locator("input[name='montoOfertadoCRC']").FillAsync("100000");
        await page.GetByRole(AriaRole.Button, new() { Name = "Registrar oferta" }).ClickAsync();

        // El proveedor ya no aparece disponible; se intenta registrar de nuevo vía API
        // simulando un reintento — el mensaje de error debe mostrarse, no una excepción sin control.
        await page.GetByLabel("Proveedor").SelectOptionAsync(new SelectOptionValue { Label = $"Proveedor Dup {sufijo}" });
        await page.Locator("input[name='montoOfertadoCRC']").FillAsync("90000");
        await page.GetByRole(AriaRole.Button, new() { Name = "Registrar oferta" }).ClickAsync();

        await Expect(page.GetByText("ya registró una oferta")).ToBeVisibleAsync();

        await page.CloseAsync();
    }
}
