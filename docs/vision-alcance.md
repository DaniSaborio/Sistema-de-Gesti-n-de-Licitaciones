# Visión y alcance

## Propósito

Diseñar, construir, probar, contenerizar y desplegar una aplicación web para
administrar licitaciones, proveedores, ofertas económicas, niveles de aprobación y
conversión referencial de moneda, aplicando Extreme Programming (XP) como única
metodología ágil del proyecto.

## Problema que resuelve

Una entidad que gestiona licitaciones necesita: (1) un catálogo confiable de
proveedores y licitaciones sin duplicados, (2) recepción controlada de ofertas
económicas que respete presupuesto, plazos y unicidad por proveedor, (3) selección
objetiva y trazable de la mejor oferta, (4) resolución automática de quién debe
aprobar la adjudicación según el monto, y (5) visibilidad de los montos tanto en la
moneda oficial (colones) como en dólares, sin depender de una API externa ni perder la
fuente de verdad.

## Alcance funcional

Incluido:
- CRUD completo de Licitaciones, Proveedores, Ofertas, Niveles de aprobación y Tipos
  de cambio, tanto desde la interfaz web (MVC) como desde una API REST versionada.
- Ciclo de estados de licitación (Borrador → Publicada → Cerrada) con cierre implícito
  por fecha y reapertura explícita y auditada.
- Reglas de negocio completas de aceptación de ofertas, cálculo de mejor oferta y su
  clasificación de ahorro, y resolución de aprobador por rango parametrizable.
- Conversión visual CRC↔USD sin alterar los valores oficiales persistidos.
- Autenticación **no** está en alcance (el enunciado no la exige); el sistema es de
  uso interno sin control de acceso por roles.
- Integración con pasarelas de pago, notificaciones o firma digital: **fuera de
  alcance** — no forman parte de ninguna historia de usuario del enunciado.

## Moneda y fuente de verdad

El colón costarricense (CRC) es la moneda oficial: todo monto se almacena únicamente
en CRC, con precisión decimal explícita (`numeric(18,2)`, nunca `float`/`double`). El
dólar (USD) es siempre un valor **calculado** en el momento de mostrarse, usando el
tipo de cambio activo; nunca se persiste ni se usa como base de ninguna regla de
negocio (presupuesto, comparación de ofertas, niveles de aprobación operan siempre
sobre CRC).

## Usuarios y flujo

Un único perfil de usuario (sin roles diferenciados) que actúa como encargado/a de
compras: registra proveedores y licitaciones, publica, recibe ofertas, consulta la
mejor oferta y su aprobador, y administra la configuración (niveles de aprobación,
tipo de cambio). El flujo funcional mínimo completo está descrito paso a paso en
`historias-usuario.md` y se ejecuta de principio a fin en las pruebas funcionales
(`tests/Licitaciones.FunctionalTests`).

## Restricciones técnicas del proyecto

.NET 9, ASP.NET Core MVC + Web API, Entity Framework Core 9, PostgreSQL 16+, Docker,
Kubernetes, GitHub Actions, xUnit + Testcontainers + Playwright. Ver
`arquitectura-general.md` para el detalle de cómo se organizan estas piezas.
