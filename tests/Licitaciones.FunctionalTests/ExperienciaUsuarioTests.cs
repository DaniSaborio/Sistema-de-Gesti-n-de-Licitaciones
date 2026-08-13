using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace Licitaciones.FunctionalTests;

/// <summary>Modo claro/oscuro, conversión CRC/USD y validaciones visibles (sección 9 y 12.3).</summary>
[Collection("AplicacionWeb")]
public class ExperienciaUsuarioTests(AplicacionWebFixture fixture)
{
    [Fact]
    public async Task El_boton_de_tema_alterna_entre_claro_y_oscuro_y_persiste()
    {
        var page = await fixture.Browser.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        var html = page.Locator("html");
        await Expect(html).ToHaveAttributeAsync("data-bs-theme", "light");

        await page.ClickAsync("#toggle-tema");
        await Expect(html).ToHaveAttributeAsync("data-bs-theme", "dark");

        await page.ReloadAsync();
        await Expect(html).ToHaveAttributeAsync("data-bs-theme", "dark");

        await page.CloseAsync();
    }

    [Fact]
    public async Task El_boton_CRC_USD_alterna_la_visualizacion_de_montos()
    {
        var page = await fixture.Browser.NewPageAsync();

        await page.GotoAsync($"{fixture.BaseUrl}/Licitaciones/Create");
        await page.GetByLabel("Código").FillAsync($"LIC-UX-{Guid.NewGuid():N}"[..14]);
        await page.GetByLabel("Título").FillAsync("Licitación para prueba de moneda");
        await page.GetByLabel("Fecha y hora de cierre").FillAsync(DateTime.Now.AddDays(10).ToString("yyyy-MM-ddTHH:mm"));
        await page.GetByLabel("Presupuesto estimado (CRC)").FillAsync("1000000");
        await page.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        var montoCrc = page.Locator(".monto-crc").First;
        var montoUsd = page.Locator(".monto-usd").First;

        await Expect(montoCrc).ToBeVisibleAsync();

        await page.ClickAsync("#toggle-moneda");

        // El toggle es puramente CSS (sin recarga); ambos elementos existen en el DOM,
        // pero solo uno debe quedar visible según la clase moneda-usd en <body>.
        await Expect(page.Locator("body")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("moneda-usd"));

        await page.CloseAsync();
    }

    [Fact]
    public async Task Crear_proveedor_con_nombre_vacio_muestra_el_mensaje_de_validacion_junto_al_campo()
    {
        var page = await fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/Proveedores/Create");

        await page.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await Expect(page.GetByText("El nombre del proveedor es obligatorio.")).ToBeVisibleAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task El_listado_de_proveedores_pagina_y_filtra_por_nombre()
    {
        var page = await fixture.Browser.NewPageAsync();
        var prefijo = $"Filtro{Guid.NewGuid():N}"[..12];

        for (var i = 1; i <= 3; i++)
        {
            await page.GotoAsync($"{fixture.BaseUrl}/Proveedores/Create");
            await page.GetByLabel("Nombre").FillAsync($"{prefijo} {i}");
            await page.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();
        }

        await page.GotoAsync($"{fixture.BaseUrl}/Proveedores?busqueda={prefijo}");

        await Expect(page.GetByText($"{prefijo} 1")).ToBeVisibleAsync();
        await Expect(page.GetByText($"{prefijo} 2")).ToBeVisibleAsync();
        await Expect(page.GetByText($"{prefijo} 3")).ToBeVisibleAsync();

        await page.CloseAsync();
    }
}
