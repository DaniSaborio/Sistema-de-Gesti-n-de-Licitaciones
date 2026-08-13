# Módulo: Interfaz web (Licitaciones.Web / MVC)

## Propósito
Ofrecer una interfaz web completa (landing page, navegación, CRUD de los cinco
módulos) sin necesidad de conocimientos técnicos, con modo claro/oscuro y
visualización alternable de montos en CRC o USD.

## Responsabilidades
- Composition root de toda la aplicación (`Program.cs`): registra Application,
  Infrastructure y Api, aplica migraciones al iniciar, expone health checks.
- Controladores MVC delgados para Proveedores, Licitaciones, Ofertas, Niveles de
  aprobación y Tipos de cambio — cada uno delega en el `*Service` de Application
  correspondiente, nunca contiene lógica de negocio.
- Landing page que explica el flujo licitación→ofertas→mejor oferta→nivel de
  aprobación→conversión.
- Tema claro/oscuro (Bootstrap 5.3 `data-bs-theme`, persistente en `localStorage`) y
  alternancia CRC/USD (clase CSS en `<body>`, también persistente).
- Confirmación de eliminación mediante modal reutilizable
  (`_ModalConfirmarEliminacion.cshtml`), nunca borrado sin confirmación.
- Paginación, búsqueda y orden en los listados (`_Paginacion.cshtml`, parámetros de
  consulta compartidos con la API).

## Dependencias
`Licitaciones.Application` (todos los `*Service`), `Licitaciones.Api` (montado como
Application Part para servir también `/api/v1/...` y Swagger UI en `/docs/api`),
`Licitaciones.Infrastructure` (registro de DI, no acceso directo a EF Core desde
controladores MVC).

## Entradas
Formularios HTML con `[ValidateAntiForgeryToken]`, parámetros de consulta
(`busqueda`, `ordenarPor`, `descendente`, `pagina`).

## Salidas
Vistas Razor renderizadas; mensajes de éxito/error vía `TempData` (banners Bootstrap
dismissibles).

## Piezas técnicas propias de este módulo
- **`LicitacionesWebControllerBase`**: controlador base que precarga el tipo de
  cambio activo una sola vez por solicitud (`ViewData`), para que el TagHelper
  `<monto>` no dispare una consulta por cada valor monetario mostrado en una tabla.
- **`MontoTagHelper`** (`<monto crc-amount="..." />`): renderiza el monto en CRC
  (formato es-CR) y, si hay tipo de cambio activo, su equivalente en USD, en dos
  `<span>` que el CSS/JS alternan. *Nota de diseño*: al declararse como elemento
  autocierre en Razor, requiere `TagMode.StartTagAndEndTag` explícito — omitirlo
  descarta el contenido generado (defecto real encontrado y corregido, ver
  `bitacora-xp.md`).
- **`PaginacionViewModel`** + `_Paginacion.cshtml`: control de paginación reutilizado
  por los cinco listados.

## Errores y validación
Los `DomainException`/`ConflictoDeUnicidadException` que lanzan los `*Service` se
capturan en el controlador MVC y se traducen a `ModelState.AddModelError` (errores de
formulario) o `TempData["Error"]` (acciones sin formulario, como cambiar de estado o
registrar una oferta desde el detalle de una licitación) — nunca una página de error
genérica para un error de negocio esperado.

## Pruebas
Funcionales (Playwright): landing page y navegación, ciclo completo desde el
navegador, rechazo visible de oferta duplicada, modo claro/oscuro persistente,
alternancia CRC/USD, mensaje de validación junto al campo, paginación/filtro.
