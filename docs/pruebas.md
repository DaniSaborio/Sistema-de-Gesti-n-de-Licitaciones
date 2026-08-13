# Pruebas

## Estrategia

Tres niveles, cada uno con una responsabilidad distinta y sin solaparse (sección 12):

| Nivel | Proyecto | Qué verifica | Infraestructura | Se ejecuta aquí |
|---|---|---|---|---|
| Unitarias | `Licitaciones.UnitTests` | Reglas de negocio puras (Domain), en aislamiento | Ninguna (repos simulados/no usados; `IClock` fijo) | ✅ Sí, localmente |
| Integración | `Licitaciones.IntegrationTests` | EF Core + PostgreSQL real: migraciones, índices, FK, CHECK, concurrencia, endpoints HTTP completos | PostgreSQL real vía Testcontainers | ⚠️ Requiere Docker |
| Funcionales/E2E | `Licitaciones.FunctionalTests` | El sistema completo desde un navegador real | Kestrel real + PostgreSQL (Testcontainers) + Chromium (Playwright) | ⚠️ Requiere Docker |

Este entorno de generación no tiene un daemon de Docker disponible (confirmado con
`docker info`), así que las pruebas de integración y funcionales se escribieron,
compilan y están listas, pero no se ejecutaron aquí — se ejecutan en GitHub Actions
(que sí tiene Docker) y localmente por cualquier persona con Docker instalado. Como
compensación, se verificó el sistema **manualmente contra una instancia real de
PostgreSQL** (documentado en detalle en `bitacora-xp.md`), lo que de hecho encontró y
corrigió tres defectos reales que ninguna prueba unitaria podía detectar.

## TDD (Test-Driven Development)

TDD se aplicó de forma real, no solo declarada, en las reglas de negocio del dominio.
El ejemplo más claro y verificable en el historial: el primer diseño de
`ResolutorNivelAprobacion.ValidarNuevoRango` comprobaba el solape genérico de rangos
antes que la regla "solo un rango abierto a la vez"; al escribir
`ValidarNuevoRango_rechaza_un_segundo_rango_abierto` la prueba falló en rojo porque el
mensaje de error específico quedaba oculto por el genérico. Se refactorizó el orden de
las validaciones y la prueba pasó en verde — commit `test(domain)`, con la explicación
completa en el propio mensaje de commit y en `bitacora-xp.md`.

## Cómo ejecutar cada nivel

```bash
# Unitarias (no requieren nada más que el SDK de .NET 9)
dotnet test tests/Licitaciones.UnitTests/Licitaciones.UnitTests.csproj

# Integración (requiere Docker corriendo)
dotnet test tests/Licitaciones.IntegrationTests/Licitaciones.IntegrationTests.csproj

# Funcionales (requiere Docker corriendo + navegadores de Playwright instalados una vez)
dotnet build tests/Licitaciones.FunctionalTests/Licitaciones.FunctionalTests.csproj
pwsh tests/Licitaciones.FunctionalTests/bin/Debug/net9.0/playwright.ps1 install --with-deps chromium
dotnet test tests/Licitaciones.FunctionalTests/Licitaciones.FunctionalTests.csproj
```

## Casos cubiertos

### Unitarias (53 casos, `Licitaciones.UnitTests`)

- Normalización de texto (código de licitación, nombre de proveedor) y caracteres permitidos.
- Ciclo de vida de Licitación: creación, validaciones, publicar/cerrar/reabrir, transiciones inválidas, cierre implícito por fecha, reducción de presupuesto por debajo de una oferta existente.
- Proveedor: creación, normalización, caracteres inválidos.
- `RegistroOfertaService`: oferta válida, oferta igual al presupuesto, licitación no publicada, licitación vencida, monto no positivo, oferta sobre presupuesto, oferta duplicada.
- `EvaluadorOfertas`: sin ofertas, menor monto gana, empate gana la primera registrada, los tres umbrales de clasificación de ahorro (10%, entre 0-10%, 0%).
- `ResolutorNivelAprobacion`: resolución por rango, sin nivel configurado, rango solapado, dos rangos abiertos, rangos contiguos válidos, monto mínimo/máximo inválido.
- `ConversorMoneda`: conversión correcta, no modifica el monto original, tipo de cambio no positivo rechazado, activación.

### Integración (`Licitaciones.IntegrationTests`)

- Las migraciones dejan las cinco tablas esperadas.
- Índice único de proveedor normalizado e índice único compuesto de ofertas, forzados
  a nivel de PostgreSQL simulando una condición de carrera entre dos `DbContext`.
- `DeleteBehavior.Restrict`: no se puede eliminar físicamente un proveedor con ofertas.
- Restricción `CHECK` de presupuesto positivo, probada con un `INSERT` crudo que evita
  la validación de dominio a propósito.
- Concurrencia optimista: dos lecturas del mismo proveedor, la segunda escritura lanza
  `DbUpdateConcurrencyException`.
- Persistencia y recuperación de una licitación completa; borrado lógico excluido de
  las consultas por defecto (`HasQueryFilter`) pero visible con `IgnoreQueryFilters`.
- Datos semilla presentes tras migrar (tres niveles de aprobación, un tipo de cambio activo).
- Endpoints HTTP completos vía `WebApplicationFactory<Program>`: alta y consulta de
  proveedor, conflicto 409 con `ProblemDetails`, 404 en recurso inexistente.

### Funcionales (`Licitaciones.FunctionalTests`)

- Landing page y navegación a los cinco módulos.
- Ciclo completo desde el navegador: crear proveedor → crear licitación → publicar →
  registrar oferta → ver la mejor oferta reflejada en pantalla.
- Rechazo visible (mensaje de error, no una excepción sin control) de una oferta
  duplicada.
- Modo claro/oscuro: alterna y persiste tras recargar la página.
- CRC/USD: alterna la visualización de montos sin recargar la página.
- Mensaje de validación junto al campo cuando el nombre del proveedor está vacío.
- Paginación/filtro del listado de proveedores desde el navegador.

## Cobertura

Objetivo del enunciado: al menos 80% de líneas en Domain y Application, al menos 70%
en el proyecto completo. El workflow de CI recolecta cobertura con
`--collect:"XPlat Code Coverage"` sobre las pruebas unitarias y la publica como
artefacto (`coverage/`); las pruebas de integración/funcionales, al no ejecutarse en
este entorno, no aportan su cobertura aquí — se generaría igual en CI al correr los
tres niveles juntos. La cobertura numérica es un indicador, no un sustituto de la
calidad de los escenarios: por eso cada regla de negocio del enunciado tiene un caso
de prueba explícito con nombre descriptivo de la situación que cubre, no solo
pruebas genéricas para "tocar líneas".
