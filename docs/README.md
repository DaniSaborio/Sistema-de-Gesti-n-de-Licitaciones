# Sistema de Gestión de Licitaciones

Proyecto final — Metodologías Ágiles de Desarrollo de Software (ITI-822), Universidad
Técnica Nacional. Aplicación web para administrar licitaciones, proveedores, ofertas
económicas, niveles de aprobación y conversión referencial de moneda, construida
**exclusivamente con Extreme Programming (XP)**.

Este archivo es el índice de toda la documentación del proyecto y, a la vez, el
resumen de qué se construyó y cómo se usó XP para construirlo. Toda la documentación
vive en `/docs`, en Markdown — no hay Word, PDF, PowerPoint ni enlaces externos
(sección 15 del enunciado).

## Qué es este sistema

Una entidad que gestiona licitaciones necesita un catálogo confiable de proveedores y
licitaciones, un proceso controlado de recepción de ofertas, selección objetiva de la
mejor oferta, resolución automática de quién debe aprobarla, y visibilidad de los
montos tanto en colones (CRC, la fuente de verdad) como en dólares (USD, un valor
calculado). Eso es exactamente lo que este sistema hace, de punta a punta: desde la
landing page hasta la API REST, pasando por PostgreSQL real con índices únicos,
restricciones y concurrencia optimista.

## Qué se construyó

- **Dominio rico** (`Licitaciones.Domain`): cinco entidades con invariantes propias
  (Licitación, Proveedor, Oferta, NivelAprobación, TipoCambio) y los servicios de
  dominio puros que implementan cada regla de negocio del enunciado —
  `RegistroOfertaService`, `EvaluadorOfertas`, `ResolutorNivelAprobacion`,
  `ConversorMoneda`, `NormalizacionTexto` — todos probables sin base de datos.
- **Casos de uso** (`Licitaciones.Application`): un `*Service` por módulo que
  orquesta el dominio y los repositorios, DTOs de entrada/salida y validadores
  FluentValidation.
- **Persistencia real** (`Licitaciones.Infrastructure`): EF Core 9 + PostgreSQL 16,
  con índices únicos (incluido uno parcial para "un solo tipo de cambio activo"),
  restricciones `CHECK`, claves foráneas con `Restrict`, concurrencia optimista vía la
  columna `xmin` de PostgreSQL, migraciones versionadas y datos semilla.
- **API REST versionada** (`Licitaciones.Api`): `/api/v1/...` completo, Swagger UI en
  `/docs/api`, respuestas `ProblemDetails` controladas.
- **Interfaz web completa** (`Licitaciones.Web`): landing page, navegación, CRUD de
  los cinco módulos con paginación/búsqueda/orden, modo claro/oscuro persistente,
  alternancia visual CRC/USD, confirmación de eliminación por modal.
- **Pruebas en tres niveles**: 53 pruebas unitarias (Domain/Application, corren sin
  base de datos), pruebas de integración contra PostgreSQL real (Testcontainers) y
  pruebas funcionales E2E con Playwright.
- **Contenerización y despliegue**: `Dockerfile` multi-stage, `docker-compose.yml`
  (app + PostgreSQL con volumen persistente), ocho manifiestos de Kubernetes
  (Deployment con probes, StatefulSet de PostgreSQL, ConfigMap, Secret de ejemplo).
- **Integración continua**: GitHub Actions con cuatro jobs (build+formato+pruebas
  unitarias, integración+funcionales, imagen Docker, validación de manifiestos K8s).

## Cómo se usó XP para construirlo

El proyecto se organizó en **cuatro iteraciones** con Planning Game, historias de
usuario con criterios de aceptación verificables, pequeñas liberaciones demostrables al
cierre de cada una, TDD real (no solo declarado), integración continua desde la
primera iteración con pruebas, y refactorización motivada por pruebas en rojo — nunca
Scrum, Kanban ni ninguna combinación de metodologías.

| Documento | Qué contiene |
|---|---|
| [`plan-xp.md`](plan-xp.md) | Planning Game, plan de liberación, las cuatro iteraciones y cómo se evidencia cada práctica XP en este repositorio concreto |
| [`historias-usuario.md`](historias-usuario.md) | Las 13 historias de usuario, con prioridad, estimación, criterios de aceptación y enlace a sus pruebas y commits |
| [`bitacora-xp.md`](bitacora-xp.md) | Resultado real de cada iteración: commits, velocidad, retroalimentación, y el relato completo (con evidencia) de la verificación en vivo contra PostgreSQL real que encontró y corrigió tres defectos reales |

**La evidencia de TDD más concreta y verificable**: el commit `test(domain)` documenta
un fallo genuino de prueba en `ResolutorNivelAprobacion` (dos rangos de aprobación
abiertos deberían rechazarse con un mensaje específico, pero un chequeo genérico de
solape lo interceptaba primero) y la refactorización que lo corrigió — ciclo
rojo-verde-refactor real, no narrado.

**La evidencia de "diseño simple no es garantía de correcto a la primera"**: sin
Docker disponible en este entorno, se instaló PostgreSQL nativo temporal y se probó el
sistema completo en vivo (API por HTTP, interfaz por HTML renderizado). Eso encontró
tres defectos reales — un TagHelper que no mostraba ningún monto, rutas de API que
devolvían HTML de error en vez de JSON, y una serialización de enums/nombres de campo
poco profesional — documentados y corregidos con su propio commit (`fix: corregir
bugs encontrados al probar contra PostgreSQL real`). El detalle completo, con el
razonamiento de cada corrección, está en `bitacora-xp.md`.

## Uso de inteligencia artificial

Este proyecto se desarrolló con asistencia de IA, declarada con transparencia según
exige la sección 16 del enunciado en [`uso-ia.md`](uso-ia.md): herramienta, finalidad,
módulos asistidos, decisiones de diseño relevantes y todas las validaciones
realizadas para confirmar que el sistema funciona de verdad, no solo que compila.

## Índice completo de la documentación

| Documento | Contenido |
|---|---|
| [`vision-alcance.md`](vision-alcance.md) | Propósito, problema que resuelve, alcance funcional incluido/excluido |
| [`historias-usuario.md`](historias-usuario.md) | Las 13 historias de usuario con criterios de aceptación |
| [`plan-xp.md`](plan-xp.md) | Planning Game, plan de liberación, prácticas XP aplicadas |
| [`bitacora-xp.md`](bitacora-xp.md) | Resultados reales por iteración, velocidad, retroalimentación |
| [`arquitectura-general.md`](arquitectura-general.md) | Arquitectura en capas, diagrama, decisiones de diseño |
| [`modelo-datos.md`](modelo-datos.md) | Diagrama entidad-relación, decisiones de modelado, migraciones |
| [`api.md`](api.md) | Todos los endpoints REST, ejemplos, códigos de error |
| [`pruebas.md`](pruebas.md) | Estrategia de pruebas, TDD, cómo ejecutar cada nivel, cobertura |
| [`docker.md`](docker.md) | Dockerfile, docker-compose, variables de entorno |
| [`kubernetes.md`](kubernetes.md) | Los ocho manifiestos, cómo desplegar, probes, decisiones |
| [`uso-ia.md`](uso-ia.md) | Declaración de uso de IA (sección 16) |
| [`integracion-modulos.md`](integracion-modulos.md) | Cómo cooperan los módulos, diagrama de secuencia |
| [`modulos/licitaciones.md`](modulos/licitaciones.md) | Módulo Licitaciones en detalle |
| [`modulos/proveedores.md`](modulos/proveedores.md) | Módulo Proveedores en detalle |
| [`modulos/ofertas.md`](modulos/ofertas.md) | Módulo Ofertas en detalle |
| [`modulos/niveles-aprobacion.md`](modulos/niveles-aprobacion.md) | Módulo Niveles de aprobación en detalle |
| [`modulos/tipo-cambio.md`](modulos/tipo-cambio.md) | Módulo Tipo de cambio en detalle |
| [`modulos/interfaz-web.md`](modulos/interfaz-web.md) | Módulo Interfaz web (MVC) en detalle |
| [`modulos/api-rest.md`](modulos/api-rest.md) | Módulo API REST en detalle |
| [`modulos/persistencia.md`](modulos/persistencia.md) | Módulo Persistencia (EF Core) en detalle |
| [`assets/licitaciones.http`](assets/licitaciones.http) | Colección de solicitudes HTTP reproducible |

## Cómo ejecutar el proyecto

```bash
# Con Docker (recomendado, reproducible sin pasos manuales)
docker compose up --build
# La aplicación queda en http://localhost:8080

# Sin Docker (requiere PostgreSQL propio y .NET 9 SDK)
export ConnectionStrings__LicitacionesDb="Host=localhost;Port=5432;Database=licitaciones;Username=licitaciones;Password=..."
dotnet run --project src/Licitaciones.Web

# Pruebas
dotnet test tests/Licitaciones.UnitTests/Licitaciones.UnitTests.csproj   # sin Docker
dotnet test tests/Licitaciones.IntegrationTests/...                     # requiere Docker
dotnet test tests/Licitaciones.FunctionalTests/...                      # requiere Docker
```

Ver `docker.md`, `kubernetes.md` y `pruebas.md` para el detalle completo de cada
comando y de las variables de entorno necesarias.

## Trazabilidad con la rúbrica de evaluación

| Criterio de la rúbrica | Dónde está la evidencia |
|---|---|
| 1. Aplicación de XP (15 pts) | `plan-xp.md`, `historias-usuario.md`, `bitacora-xp.md` |
| 2. Lógica de negocio y validaciones (18 pts) | `modulos/*.md`, `Licitaciones.Domain`, `Licitaciones.UnitTests` (53 casos) |
| 3. CRUD completo y API REST (12 pts) | `api.md`, `Licitaciones.Api`, `Licitaciones.Web.Controllers` |
| 4. Arquitectura, modularidad y calidad del código (10 pts) | `arquitectura-general.md`, `.editorconfig`, CI (`dotnet format` + `/warnaserror`) |
| 5. PostgreSQL, modelo de datos, auditoría y concurrencia (10 pts) | `modelo-datos.md`, `Licitaciones.Infrastructure`, `Licitaciones.IntegrationTests` |
| 6. Interfaz y experiencia de usuario (8 pts) | `modulos/interfaz-web.md`, `Licitaciones.Web/Views`, pruebas funcionales |
| 7. TDD y pruebas automatizadas (12 pts) | `pruebas.md`, los tres proyectos de prueba |
| 8. Docker, Kubernetes e integración continua (10 pts) | `docker.md`, `kubernetes.md`, `.github/workflows/ci.yml` |
| 9. Git, GitHub y documentación interna (5 pts) | Este índice, historial de commits (Conventional Commits), `/docs` completo |

Cada módulo (`modulos/*.md`) enlaza explícitamente sus propias pruebas y reglas de
negocio; cada historia de usuario (`historias-usuario.md`) enlaza sus pruebas; cada
commit describe una unidad de trabajo verificable — la trazabilidad histoira↔prueba↔
commit↔documentación que exige la sección 18 del enunciado se cumple de extremo a
extremo, no solo en este resumen.
