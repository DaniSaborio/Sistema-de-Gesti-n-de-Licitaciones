(function () {
    "use strict";

    const TEMA_KEY = "licitaciones.tema";
    const MONEDA_KEY = "licitaciones.moneda";

    function aplicarTema(tema) {
        document.documentElement.setAttribute("data-bs-theme", tema);
        const boton = document.getElementById("toggle-tema");
        if (boton) {
            boton.textContent = tema === "dark" ? "☀ Claro" : "🌙 Oscuro";
        }
    }

    function aplicarMoneda(moneda) {
        document.body.classList.toggle("moneda-usd", moneda === "usd");
        const boton = document.getElementById("toggle-moneda");
        if (boton) {
            boton.textContent = moneda === "usd" ? "$ USD" : "₡ CRC";
        }
    }

    const temaGuardado = localStorage.getItem(TEMA_KEY) || "light";
    aplicarTema(temaGuardado);

    const monedaGuardada = localStorage.getItem(MONEDA_KEY) || "crc";
    aplicarMoneda(monedaGuardada);

    document.getElementById("toggle-tema")?.addEventListener("click", function () {
        const actual = document.documentElement.getAttribute("data-bs-theme");
        const nuevo = actual === "dark" ? "light" : "dark";
        localStorage.setItem(TEMA_KEY, nuevo);
        aplicarTema(nuevo);
    });

    document.getElementById("toggle-moneda")?.addEventListener("click", function () {
        const actual = document.body.classList.contains("moneda-usd") ? "usd" : "crc";
        const nuevo = actual === "usd" ? "crc" : "usd";
        localStorage.setItem(MONEDA_KEY, nuevo);
        aplicarMoneda(nuevo);
    });
})();
