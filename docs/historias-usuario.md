# Historias de usuario

Redactadas desde la perspectiva del cliente (persona encargada de compras públicas),
con prioridad, estimación relativa (puntos, escala Fibonacci corta: 1-2-3-5-8) y
criterios de aceptación verificables. Cada historia enlaza con las pruebas y los
commits donde se implementó (trazabilidad exigida en la sección 18 del enunciado).

| # | Prioridad | Estimación |
|---|-----------|------------|
| Todas | Alta = imprescindible para el flujo mínimo · Media = necesaria para la operación normal · Baja = calidad de vida | Puntos relativos, no horas |

## Iteración 0 — Fundamentos

### H1 — Registrar proveedores (Alta, 3 pts)
**Como** encargada de compras, **quiero** registrar proveedores con un nombre único
**para** poder asociarles ofertas sin ambigüedad.

**Criterios de aceptación**
- El nombre es obligatorio y solo admite letras, números, espacios, punto, coma y paréntesis.
- Dos nombres que difieran solo en mayúsculas/minúsculas o espacios repetidos se consideran el mismo proveedor y el segundo registro se rechaza.
- El proveedor creado aparece de inmediato en el listado.

**Pruebas**: `ProveedorTests` (unit), `El_indice_unico_de_proveedor_normalizado_rechaza_duplicados_en_base_de_datos` (integration).
**Commits**: `feat(domain): entidades y reglas de negocio puras del dominio`, `feat(application): casos de uso...`, `feat(web): ... CRUD MVC completo`.

### H2 — Crear una licitación en borrador (Alta, 3 pts)
**Como** encargada de compras, **quiero** crear una licitación con código único, título,
presupuesto y fecha de cierre **para** empezar a prepararla antes de publicarla.

**Criterios de aceptación**
- El código es único (comparación normalizada); el presupuesto debe ser mayor que cero; la fecha de cierre debe ser futura.
- La licitación nace en estado Borrador y no admite ofertas todavía.

**Pruebas**: `LicitacionTests`. **Commits**: igual que H1 + `feat(infrastructure)...`.

### H3 — Editar y eliminar proveedores y licitaciones (Media, 2 pts)
**Como** encargada de compras, **quiero** corregir datos o retirar registros erróneos
**para** mantener el catálogo confiable, sin perder el historial de ofertas.

**Criterios de aceptación**
- Editar un proveedor revalida la unicidad del nombre.
- Eliminar aplica borrado lógico (nunca borra físicamente un registro con ofertas relacionadas).
- El presupuesto de una licitación no puede reducirse por debajo de una oferta ya registrada.

**Pruebas**: `LicitacionTests.ActualizarDatos_rechaza_reducir_el_presupuesto...`, `No_se_puede_eliminar_fisicamente_un_proveedor_con_ofertas_relacionadas`.

## Iteración 1 — Reglas de negocio

### H4 — Publicar y cerrar una licitación (Alta, 3 pts)
**Como** encargada de compras, **quiero** publicar una licitación para recibir ofertas
y cerrarla cuando corresponda **para** controlar el ciclo de vida del proceso.

**Criterios de aceptación**
- Solo se publica desde Borrador; solo se cierra desde Borrador o Publicada.
- Una licitación cuya fecha de cierre ya pasó se considera cerrada funcionalmente aunque el campo Estado no se haya actualizado todavía.
- Reabrir una licitación cerrada es una acción explícita y separada, no una transición libre.

**Pruebas**: `LicitacionTests.Publicar_*`, `EstaCerradaFuncionalmente_*`, `Reabrir_*`.

### H5 — Registrar una oferta económica (Alta, 5 pts)
**Como** proveedor, **quiero** registrar una única oferta en colones por licitación
**para** participar en el proceso de forma justa.

**Criterios de aceptación**
- Solo sobre licitaciones publicadas y no vencidas; el monto debe ser mayor que cero y no superar el presupuesto.
- Un mismo proveedor no puede tener dos ofertas para la misma licitación.
- Rechazar: oferta duplicada, oferta sobre el presupuesto, oferta sobre licitación no publicada, oferta sobre licitación vencida.

**Pruebas**: `RegistroOfertaServiceTests` (7 casos). **Commits**: `feat(domain)`, `feat(application)`.

### H6 — Consultar la mejor oferta y su clasificación (Alta, 5 pts)
**Como** encargada de compras, **quiero** ver automáticamente cuál es la mejor oferta
y qué tan conveniente es **para** decidir con criterios objetivos.

**Criterios de aceptación**
- La mejor oferta es la de menor monto válido; en empate gana la registrada primero.
- Clasificación: ≥10% de ahorro → "Oferta conveniente"; entre 0% y 10% → "Oferta aceptable"; monto igual al presupuesto → "Oferta válida sin ahorro"; sin ofertas → "Sin ofertas válidas".

**Pruebas**: `EvaluadorOfertasTests` (5 casos).

### H7 — Configurar niveles de aprobación por rango de monto (Media, 3 pts)
**Como** administradora del sistema, **quiero** configurar rangos de monto con su
aprobador correspondiente **para** que el sistema resuelva automáticamente quién
debe aprobar cada adjudicación.

**Criterios de aceptación**
- Los rangos no pueden solaparse; solo puede existir un rango abierto (sin monto máximo) a la vez.
- Dado el monto de la mejor oferta, el sistema resuelve el aprobador consultando la tabla, sin condicionales fijos en el código.

**Pruebas**: `ResolutorNivelAprobacionTests` (7 casos, incluye el ciclo rojo-verde documentado en el commit `test(domain)`).

### H8 — Configurar el tipo de cambio (Media, 2 pts)
**Como** administradora del sistema, **quiero** mantener un tipo de cambio CRC/USD
activo **para** que el sistema pueda mostrar montos en dólares sin depender de una
API externa.

**Criterios de aceptación**
- Solo un tipo de cambio puede estar activo a la vez (también garantizado por índice único parcial en PostgreSQL).
- Los valores oficiales siempre se almacenan en CRC; USD es un valor calculado, nunca persistido como fuente de verdad.

**Pruebas**: `ConversorMonedaTests`.

## Iteración 2 — Experiencia de usuario y API

### H9 — Landing page y navegación (Media, 2 pts)
**Como** usuaria nueva del sistema, **quiero** una página de inicio que explique el
flujo completo **para** entender el sistema sin capacitación previa.

**Criterios de aceptación**: landing con las tres etapas del proceso, menú a los cinco módulos y a la documentación interactiva de la API.
**Pruebas**: `La_landing_page_explica_el_flujo_y_permite_navegar_a_los_modulos` (funcional).

### H10 — Modo claro/oscuro y conversión visual CRC/USD (Media, 3 pts)
**Como** usuaria del sistema, **quiero** alternar el tema visual y la moneda mostrada
**para** adaptar la interfaz a mi preferencia sin perder los valores oficiales en CRC.

**Criterios de aceptación**: el tema persiste tras recargar la página; el botón CRC/USD alterna todos los montos visibles sin recargar ni modificar los valores originales.
**Pruebas**: `El_boton_de_tema_alterna...`, `El_boton_CRC_USD_alterna...` (funcionales).

### H11 — API REST documentada para integraciones (Alta, 5 pts)
**Como** integradora de otro sistema, **quiero** una API REST versionada con
documentación interactiva **para** automatizar la gestión de licitaciones sin usar
la interfaz web.

**Criterios de aceptación**: todos los endpoints de la sección 10.1 disponibles bajo `/api/v1`, documentados en Swagger UI (`/docs/api`), con respuestas `ProblemDetails` controladas ante error.
**Pruebas**: `ApiEndpointsIntegrationTests`.

## Iteración 3 — Calidad y despliegue

### H12 — Ejecutar el sistema con un solo comando (Alta, 3 pts)
**Como** persona evaluadora, **quiero** levantar la aplicación completa con
`docker compose up --build` **para** verificarla sin configurar nada manualmente.

**Criterios de aceptación**: la aplicación y PostgreSQL inician, las migraciones se aplican automáticamente y los datos persisten tras reiniciar los contenedores.

### H13 — Desplegar en Kubernetes con configuración segura (Media, 3 pts)
**Como** persona operadora, **quiero** manifiestos de Kubernetes con secretos
externos al repositorio **para** desplegar el sistema de forma reproducible y segura.

**Criterios de aceptación**: Deployment con probes de arranque/listo/vivo, StatefulSet de PostgreSQL con almacenamiento persistente, ConfigMap para configuración no sensible y Secret separado (con plantilla de ejemplo) para credenciales.
