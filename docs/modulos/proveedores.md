# Módulo: Proveedores

## Propósito
Mantener el catálogo de proveedores que pueden participar en licitaciones, con
nombre único garantizado en tres capas (interfaz, servidor, base de datos).

## Responsabilidades
- CRUD completo (crear, listar con paginación/búsqueda, consultar, editar, eliminar).
- Normalizar el nombre para comparaciones de unicidad (trim, colapso de espacios,
  Unicode NFKC, sin distinguir mayúsculas/minúsculas).
- Validar el conjunto de caracteres permitidos: letras, números, espacios, punto,
  coma y paréntesis.
- Borrado lógico (nunca físico): un proveedor eliminado deja de aparecer en listados
  y formularios, pero sus ofertas históricas permanecen intactas.

## Dependencias
- `Licitaciones.Domain.Proveedores.Proveedor` (entidad) y `NormalizacionTexto`
  (Domain.Common).
- `IProveedorRepository` (Application) implementado por `ProveedorRepository`
  (Infrastructure/EF Core).
- Consumido por `OfertaService` (para validar que el proveedor de una oferta existe)
  y por las vistas de Licitaciones (para poblar el selector de "registrar oferta").

## Entradas
`CrearProveedorRequest { Nombre }`, `ActualizarProveedorRequest { Nombre }`,
`ConsultaPaginada { Pagina, TamanoPagina, Busqueda, OrdenarPor, Descendente }`.

## Salidas
`ProveedorDto { Id, Nombre, CreatedAt, UpdatedAt }`,
`ResultadoPaginado<ProveedorDto>`.

## Reglas de negocio
- Nombre obligatorio, máximo 200 caracteres, solo caracteres permitidos (regex
  `^[\p{L}\p{N}\.,\(\)\s]+$`).
- Dos nombres normalizados iguales son un conflicto de unicidad, validado antes de
  escribir y reforzado por un índice único en PostgreSQL (`ux_proveedores_nombre_normalizado`).

## Errores
| Situación | Excepción | HTTP |
|---|---|---|
| Nombre vacío o con caracteres no permitidos | `NombreProveedorInvalidoException` | 422 |
| Nombre duplicado (normalizado) | `ConflictoDeUnicidadException` | 409 |
| Id inexistente | `RecursoNoEncontradoException` | 404 |

## Pruebas
Unitarias: `ProveedorTests` (5 casos). Integración:
`El_indice_unico_de_proveedor_normalizado_rechaza_duplicados_en_base_de_datos`,
`El_borrado_logico_de_un_proveedor_lo_excluye_de_las_consultas_por_defecto`,
`No_se_puede_eliminar_fisicamente_un_proveedor_con_ofertas_relacionadas`. Funcionales:
creación desde formulario, validación visible, paginación/filtro del listado.
