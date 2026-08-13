# Plan XP (Planning Game, plan de liberación y de iteraciones)

Este proyecto se desarrolla **exclusivamente con Extreme Programming (XP)**, sin mezclar
roles, ceremonias ni artefactos de Scrum o Kanban (sección 4 del enunciado). No hay
Product Backlog ni Sprint Board: las historias viven en `historias-usuario.md`, la
planificación en este documento y los resultados en `bitacora-xp.md`.

## Modalidad de trabajo

Proyecto individual con asistencia de IA (declarada en `uso-ia.md`), tal como permite
la sección 3 del enunciado para esa modalidad: no aplica programación en parejas ni
rotación de roles, pero sí TDD, integración continua, refactorización y propiedad
colectiva del código completo (una sola persona es dueña de todo el código y debe
poder explicarlo y modificarlo íntegramente).

## Planning Game

Las 13 historias de `historias-usuario.md` se estimaron en conjunto (puntos relativos,
no horas) y se priorizaron con el cliente (criterio: qué necesita existir primero para
que el flujo mínimo de la sección 5.3 funcione de punta a punta). El orden de
implementación siguió estrictamente la dependencia técnica real:

1. Dominio y persistencia no pueden posponerse: sin entidades ni base de datos no hay
   nada que planificar arriba.
2. Las reglas de negocio (ofertas, mejor oferta, niveles, tipo de cambio) son el
   corazón del sistema y el criterio de evaluación con más peso (18 puntos de la
   rúbrica).
3. La interfaz y la API son la forma en que el cliente y otros sistemas *usan* esas
   reglas — no tienen valor sin ellas, pero tampoco se puede demostrar valor sin
   interfaz, de ahí que se planificaran como iteración propia inmediatamente después.
4. Calidad (pruebas de integración/E2E), despliegue (Docker/K8s) y documentación
   cierran cada entrega para que sea *demostrable y desplegable*, no solo código que
   compila.

## Plan de liberación (release plan)

| Iteración | Objetivo entregable | Historias | Versión demostrable |
|---|---|---|---|
| 0 — Fundamentos | Estructura del repo, dominio con TDD, persistencia real, CRUD de Proveedores y Licitaciones | H1, H2, H3 | Se pueden crear/editar/eliminar proveedores y licitaciones desde la API y persisten en PostgreSQL |
| 1 — Reglas de negocio | Ciclo de estados, ofertas con todas sus validaciones, mejor oferta, niveles de aprobación, tipo de cambio | H4, H5, H6, H7, H8 | El flujo de negocio completo funciona vía API: publicar, ofertar, ver mejor oferta y aprobador |
| 2 — Experiencia de usuario y API | Landing page, navegación, claro/oscuro, CRC/USD, tablas paginadas, Swagger completo | H9, H10, H11 | El sistema es usable desde el navegador de principio a fin |
| 3 — Calidad y despliegue | Pruebas de integración/E2E, Docker, Kubernetes, CI/CD, documentación completa | H12, H13 | `docker compose up --build` levanta el sistema completo; CI verde en GitHub Actions |

Se cumplen las cuatro iteraciones de duración uniforme por objetivo (más de las tres
mínimas exigidas), cada una cerrando con una versión ejecutable y demostrable
(pequeñas liberaciones, sección 4.1).

## Prácticas XP aplicadas y cómo se evidencian

| Práctica | Cómo se aplicó en este proyecto |
|---|---|
| Planning Game | Esta sección y `historias-usuario.md`: historias con prioridad/estimación acordadas antes de codificar cada iteración. |
| Historias de usuario | `historias-usuario.md`, con criterios de aceptación verificables y enlace a pruebas/commits. |
| Iteraciones cortas | Cuatro iteraciones de alcance uniforme (tabla anterior), cada una con su propio cierre en `bitacora-xp.md`. |
| Pequeñas liberaciones | Cada iteración termina con un sistema ejecutable (no solo compilable): API funcional (it. 0-1), UI funcional (it. 2), sistema desplegable (it. 3). |
| TDD | Ver `pruebas.md`: las reglas de negocio más significativas (Domain) tienen ciclo rojo-verde-refactor documentado en los mensajes de commit (`test(domain)`, con el hallazgo real en `ResolutorNivelAprobacion`). |
| Integración continua | GitHub Actions (`.github/workflows/ci.yml`) en cada push/PR a `main`: build, formato, pruebas, imagen Docker, manifiestos K8s. |
| Diseño simple | Arquitectura en capas mínima suficiente (Domain sin dependencias, Application orquesta, Infrastructure implementa); sin patrones especulativos no exigidos por una historia real (ver `arquitectura-general.md`). |
| Refactorización | Ejemplo real: reordenar las validaciones de `ResolutorNivelAprobacion.ValidarNuevoRango` tras un test en rojo (commit `test(domain)`); extracción de `PaginacionExtensions` para no repetir lógica de paginación en cinco repositorios. |
| Propiedad colectiva | Aplicable a la modalidad individual: una sola persona es responsable y puede explicar/modificar cualquier módulo (sección 3). |
| Estándares de código | `.editorconfig` + `dotnet format` verificado en CI; convenciones documentadas en `arquitectura-general.md`. |
| Ritmo sostenible | Ver nota de transparencia en `bitacora-xp.md` sobre el ritmo real de esta entrega. |
| Cliente disponible | El "cliente" (autor/a del proyecto) definió y priorizó las historias antes de cada iteración; retroalimentación registrada en `bitacora-xp.md`. |

## Reglas de trabajo XP para este proyecto

- Ninguna historia se da por terminada sin sus criterios de aceptación verificados por
  al menos una prueba automatizada.
- Los commits son pequeños, frecuentes y con propósito técnico único (Conventional
  Commits: `feat`, `fix`, `test`, `refactor`, `docs`, `chore`).
- No se usa terminología ni artefactos de Scrum/Kanban en ningún documento del
  repositorio (sección 4.2).
- La documentación vive exclusivamente en `/docs` en Markdown (sección 15).
