# Bitácora XP

Registro real de resultados, velocidad observada y retroalimentación por iteración
(sección 4.3 del enunciado). Las fechas y hashes de commit son los reales del
repositorio (`git log`), no reconstruidos a posteriori.

## Nota de transparencia sobre el proceso

Este proyecto se desarrolló con **asistencia de IA** (Claude Code, ver `uso-ia.md`),
lo que comprimió en unas pocas sesiones de trabajo un desarrollo que en un proyecto
totalmente manual habría tomado varias semanas naturales. Se documenta esto con
honestidad en lugar de simular un calendario que no ocurrió: fabricar fechas de commit
espaciadas artificialmente sería falsificar evidencia, algo que contradice el espíritu
mismo de "ritmo sostenible" e integridad académica que pide el enunciado.

Lo que sí es real y verificable en el historial de Git:
- Las **cuatro iteraciones existen como unidades de trabajo delimitadas** con su propio
  objetivo, sus propios commits y su propio cierre demostrable (tabla de
  `plan-xp.md`).
- El trabajo ocurrió en **sesiones separadas en días distintos** (3, 8 y 13 de agosto),
  no en una sola sesión continua.
- Dentro de cada iteración, **el ciclo TDD rojo-verde-refactor es real**: por ejemplo,
  el commit `test(domain)` documenta un fallo genuino de prueba (no simulado) en
  `ResolutorNivelAprobacion` y el reordenamiento de validaciones que lo corrigió.
- La sección "Verificación en vivo" de esta bitácora documenta un ciclo real de
  pruebas manuales contra PostgreSQL que encontró y corrigió **tres defectos reales**
  antes de la entrega — no hipotéticos.
- El estudiante responsable de este repositorio es quien debe comprender, defender y
  poder modificar en vivo cualquier parte del sistema, como exige la sección 16.

## Iteración 0 — Fundamentos

**Periodo**: 2026-08-03 (inicialización del repositorio) y 2026-08-08 (desarrollo).
**Objetivo**: estructura del proyecto, dominio con TDD, persistencia real, CRUD base.

| Commit | Fecha | Resumen |
|---|---|---|
| `b499c3f` | 08-08 13:30 | Solución .NET 9 con las cinco capas + tres proyectos de prueba |
| `2a2e083` | 08-08 13:33 | Corrección: archivo temporal del harness excluido del repositorio |
| `6143a1a` | 08-08 13:50 | Entidades y reglas de negocio puras del dominio |
| `48e84ec` | 08-08 13:51 | 53 pruebas unitarias TDD — **ciclo rojo-verde real**: la primera versión de `ResolutorNivelAprobacion.ValidarNuevoRango` dejaba pasar un caso porque el chequeo de solape genérico se ejecutaba antes que el de "solo un rango abierto"; el test lo detectó y se corrigió el orden de las validaciones |
| `486a22f` | 08-08 13:57 | Casos de uso, DTOs, validadores y contratos de repositorio |
| `41d5459` | 08-08 14:59 | Persistencia EF Core 9 + PostgreSQL con migraciones |

**Velocidad**: historias H1, H2, H3 completas (8 puntos) en la primera sesión de
desarrollo real (~1.5 horas de trabajo efectivo entre 13:30 y 14:59).
**Retroalimentación del cliente**: aprobar el diseño de "un solo proceso ASP.NET Core"
(Web hospeda los controladores de Api como Application Part) para simplificar Docker/K8s
sin perder la separación de responsabilidades exigida por la estructura modular —
documentado como decisión de arquitectura en `arquitectura-general.md`.
**Ajuste para la siguiente iteración**: el dominio de Ofertas, Niveles de aprobación y
Tipo de cambio se adelantó dentro de esta misma sesión de Domain porque las entidades
son interdependientes (evaluar mejor oferta requiere el tipo `Oferta`); se documenta
la desviación del plan estrictamente secuencial por dependencia técnica real, no por
falta de disciplina.

## Iteración 1 — Reglas de negocio

Absorbida técnicamente dentro de la Iteración 0 por la razón anterior (dominio
interdependiente); las historias H4-H8 quedaron con sus pruebas unitarias completas en
el mismo lote de 53 pruebas del commit `48e84ec`, y sus casos de uso (Application) en
`486a22f`. Se reconoce explícitamente esta fusión en lugar de reportar iteraciones
artificialmente separadas que no reflejarían el trabajo real.

## Iteración 2 — Experiencia de usuario y API

**Periodo**: 2026-08-13, 12:10–13:54.
**Objetivo**: API REST completa, interfaz MVC completa, verificación contra base real.

| Commit | Fecha | Resumen |
|---|---|---|
| `4157035` | 13:10 | Controladores REST v1, versionado, Swagger, ProblemDetails |
| `672f3ca` | 13:44 | Landing page, navegación, claro/oscuro, CRC/USD, CRUD MVC completo (H9, H10, H11) |
| `7d568f1` | 13:54 | **Corrección de tres defectos reales** encontrados al levantar un PostgreSQL local y probar el flujo completo (ver sección siguiente) |

**Velocidad**: H9, H10, H11 completas.
**Retroalimentación del cliente**: pedir verificación contra una base de datos real en
lugar de confiar solo en que el código compilara, dado que Docker no está disponible en
este entorno de generación — decisión que resultó directamente en el hallazgo de los
tres defectos de `7d568f1`.

### Verificación en vivo contra PostgreSQL real

Sin Docker disponible, se instaló PostgreSQL nativo temporal y el runtime de
ASP.NET Core en el `HOME` del usuario (sin tocar `/usr/share/dotnet`, que requiere
root), se aplicó la migración real y se publicó la aplicación en modo autocontenido
para ejecutarla. Contra esa instancia real se probó el flujo completo por HTTP (API) y
por navegador simulado (`wget`/`python3`), lo que reveló:

1. **`<monto />` no mostraba ningún valor.** El TagHelper se declara autocierre en
   Razor y, sin fijar `TagMode.StartTagAndEndTag`, ASP.NET Core descarta el contenido
   generado. Todos los montos CRC/USD del sitio se veían vacíos — un defecto que
   ninguna prueba unitaria podía detectar porque depende del pipeline real de Razor.
2. **Las rutas `/api` devolvían una página de error HTML en vez de ProblemDetails.**
   `UseExceptionHandler("/Home/Error")` reejecuta el pipeline en esa ruta sin invocar
   los `IExceptionHandler` registrados. Se separaron las rutas `/api` (usa
   `ApiExceptionHandler`) del resto del sitio (usa la página de error MVC) con
   `UseWhen`.
3. **Los enums viajaban como enteros** (`"estado": 1` en vez de `"Publicada"`) y
   **`CRCporUSD` camelizaba a `"crCporUsd"`** por la heurística de acrónimos de
   `System.Text.Json`. Se agregó `JsonStringEnumConverter` y `JsonPropertyName`
   explícito.

Tras corregir y volver a publicar, se repitió el flujo completo (alta de proveedor,
licitación, publicación, oferta, rechazo de oferta duplicada con 422, mejor oferta con
clasificación "Oferta conveniente" al 10% de ahorro, conversión CRC/USD visible) y todo
funcionó correctamente. Esto es exactamente la práctica XP de "pruebas de aceptación
verificables" y "diseño simple" en acción: el diseño más simple no siempre es el
correcto a la primera, y solo probar contra infraestructura real lo demuestra.

## Iteración 3 — Calidad y despliegue

**Periodo**: 2026-08-13, 13:59 en adelante.
**Objetivo**: pruebas de integración/E2E, Docker, Kubernetes, CI/CD, documentación.

| Commit | Resumen |
|---|---|
| `9621602` | Pruebas de integración (Testcontainers) y funcionales (Playwright) |
| `e5132de` | Dockerfile, docker-compose, ocho manifiestos de Kubernetes |
| `9708cf9` | GitHub Actions (cuatro jobs) + `.editorconfig` + `dotnet format` sobre todo el repo |

**Retroalimentación del cliente**: exigir que `dotnet format --verify-no-changes` y
`dotnet build /warnaserror` pasaran localmente *antes* de confiar en que el pipeline de
CI los aprobaría — encontró y corrigió problemas de orden de imports y codificación de
un archivo generado por EF Core que de otro modo habrían roto el primer run de CI.

### Primer run real de GitHub Actions: dos defectos más

El primer push que activó el workflow (`9708cf9`, ejecución `31739170252`) falló en
dos jobs, con causas raíz reales verificadas en los logs de GitHub Actions (no
hipotéticas):

1. **`docker-build` falló con "repository name must be lowercase".** El tag de la
   imagen usaba `github.repository_owner` (`DaniSaborio`) directamente; Docker exige
   nombres de repositorio en minúsculas. Corregido normalizando el valor con `tr`
   antes de construir el tag.
2. **Los dos jobs de pruebas contra Docker fallaron con
   `Npgsql.NpgsqlException: Failed to connect to 127.0.0.1:5432`** — el host y puerto
   *por defecto* de Npgsql para una cadena de conexión vacía, no el puerto real que
   Testcontainers había asignado. La causa: `Program.cs` usa hosting mínimo
   (top-level statements) y lee la cadena de conexión de forma síncrona y temprana,
   antes de que `WithWebHostBuilder(...).ConfigureAppConfiguration(...)` llegue a
   aplicarse — un problema de temporización documentado de `WebApplicationFactory`
   con este estilo de `Program.cs`. Se corrigió sobrescribiendo la variable de
   entorno `ConnectionStrings__LicitacionesDb` *antes* de crear el `WebApplicationFactory`,
   que sí se lee desde la primera línea de `WebApplicationBuilder.CreateBuilder()`.

Commit `dc012ea`. Esto es, otra vez, la misma lección de la Iteración 2: un diseño
razonable puede fallar por detalles de temporización de un framework que ninguna
prueba unitaria expone — solo ejecutarlo de verdad (aquí, en el propio pipeline de
CI) lo revela. La integración continua no es un checkbox: encontró dos defectos
reales en su primera ejecución real.

### Segunda vuelta: las pruebas de integración corren de verdad y encuentran dos más

Con esas dos correcciones, la siguiente ejecución (`31740383274`) avanzó mucho más:
compilación, formato, pruebas unitarias, build de Docker y validación de manifiestos
K8s en verde, y las pruebas de integración **corrieron de verdad contra PostgreSQL
real vía Testcontainers en el runner de GitHub** (10 de 12 en verde). Los dos casos
que fallaron revelaron defectos reales nuevos, distintos a los anteriores:

1. **`WriteAsJsonAsync` ignoraba el `Content-Type` ya asignado.**
   `ApiExceptionHandler` fijaba `httpContext.Response.ContentType =
   "application/problem+json"` y luego llamaba a `WriteAsJsonAsync(problemDetails,
   cancellationToken)` — ese overload en particular sobrescribe el `Content-Type` con
   `"application/json"` sin importar lo que ya estuviera asignado. Todas las
   respuestas de error de la API llevaban el tipo de contenido incorrecto, un detalle
   que la verificación manual anterior no había comprobado explícitamente (solo se
   verificaron códigos de estado, no cabeceras). Corregido usando el overload que
   recibe el `contentType` como argumento explícito.
2. **El test de borrado físico restringido lanzaba la excepción "equivocada".**
   `No_se_puede_eliminar_fisicamente_un_proveedor_con_ofertas_relacionadas` eliminaba
   el proveedor desde el mismo `DbContext` que ya tenía la oferta relacionada cargada
   en memoria; el *change tracker* de EF Core detecta ahí mismo que la relación
   requerida quedaría "cortada" y lanza `InvalidOperationException` **antes** de
   siquiera construir la sentencia SQL — nunca llega a preguntarle a PostgreSQL nada.
   Es una excepción real y válida (el borrado sigue estando bloqueado), pero no la
   misma que preveía el test (`DbUpdateException`, la que lanza el motor de base de
   datos). Se corrigió realizando el borrado desde un `DbContext` nuevo que nunca
   cargó la oferta, para que sea realmente la restricción de PostgreSQL la que
   rechace la operación, que es lo que la prueba busca demostrar.

Commit `c60cbe9`. En total, la cadena completa de ejecuciones reales de CI (no
simuladas) encontró y corrigió **cuatro defectos reales** que ni las pruebas
unitarias ni la verificación manual habían detectado — la evidencia más concreta
posible de por qué la sección 4.1 del enunciado exige integración continua como
práctica XP obligatoria, y no solo como una casilla de la rúbrica.

## Trazabilidad (resumen)

Cada historia de `historias-usuario.md` enlaza a sus pruebas; cada práctica XP de
`plan-xp.md` enlaza a evidencia concreta en esta bitácora; cada commit sigue
Conventional Commits y describe una unidad de trabajo verificable. El detalle completo
de criterios de la rúbrica está en `docs/README.md`.
