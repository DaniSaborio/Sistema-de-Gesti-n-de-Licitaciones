# Docker

## Dockerfile (multi-stage)

`Dockerfile` en la raíz del repositorio, dos etapas:

1. **`build`** (`mcr.microsoft.com/dotnet/sdk:9.0-alpine`): copia primero solo los
   `.csproj` para que `dotnet restore` quede en su propia capa de caché (no se vuelve a
   ejecutar si solo cambia código fuente), luego copia `src/` y publica
   `Licitaciones.Web` en modo `Release`.
2. **`final`** (`mcr.microsoft.com/dotnet/aspnet:9.0-alpine`, solo el runtime, sin SDK):
   copia el resultado publicado, corre como el usuario no privilegiado `app` (incluido
   en la imagen base de Microsoft para este propósito), expone el puerto `8080`
   (convención de los contenedores oficiales de ASP.NET Core desde .NET 8) y define un
   `HEALTHCHECK` contra `/health/live`.

Como `Licitaciones.Web` es el único proceso ejecutable (ver `arquitectura-general.md`),
una sola imagen sirve tanto la interfaz MVC como la API REST — no se necesitan
contenedores separados para "Web" y "Api".

## Ejecución local

```bash
docker compose up --build
```

Esto levanta:

- `postgres`: PostgreSQL 16, con un volumen nombrado (`licitaciones-postgres-data`)
  para persistencia real entre reinicios, y `healthcheck` con `pg_isready`.
- `app`: la imagen construida desde el `Dockerfile`, que espera a que `postgres` esté
  saludable (`depends_on: condition: service_healthy`) antes de iniciar. Al arrancar,
  `Licitaciones.Web` aplica las migraciones de EF Core automáticamente (`Program.cs`),
  así que no se requiere ningún paso manual adicional.

La aplicación queda disponible en `http://localhost:8080`.

## Variables de entorno

| Variable | Dónde se usa | Valor por defecto en `docker-compose.yml` |
|---|---|---|
| `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD` | Contenedor `postgres` | `licitaciones` / `licitaciones` / `licitaciones` (solo desarrollo local) |
| `ConnectionStrings__LicitacionesDb` | Contenedor `app` (formato estándar de ASP.NET Core: `__` mapea a la jerarquía `ConnectionStrings:LicitacionesDb`) | Construida a partir de las anteriores |
| `ASPNETCORE_ENVIRONMENT` | Contenedor `app` | `Production` |

Ninguna credencial real está en el repositorio: los valores por defecto son
explícitamente de desarrollo local (mismo usuario/contraseña que Postgres usa consigo
mismo dentro del `docker-compose`, sin exposición externa más allá de `localhost`).
Para producción, sobrescribir estas variables (archivo `.env` no versionado, secretos
del orquestador, etc.) — nunca commitear un `.env` con valores reales (ver
`.gitignore`).

## Persistencia verificada

El volumen `licitaciones-postgres-data` sobrevive a `docker compose down` (sin `-v`) y
a reinicios del contenedor `postgres`; los datos solo se pierden con
`docker compose down -v` explícito. Esto se probó de forma equivalente en este
entorno (sin Docker disponible) usando una instancia nativa de PostgreSQL: se aplicó
la migración, se insertaron datos, se detuvo y volvió a iniciar el servidor, y los
datos permanecieron intactos — el mismo comportamiento que un volumen de Docker
provee para el contenedor.

## Build manual de la imagen

```bash
docker build -t sistema-gestion-licitaciones:local .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__LicitacionesDb="Host=host.docker.internal;Port=5432;Database=licitaciones;Username=licitaciones;Password=licitaciones" \
  sistema-gestion-licitaciones:local
```

## Integración con CI

El job `docker-build` de `.github/workflows/ci.yml` construye la imagen en cada
push/PR (sin publicarla) para detectar errores del `Dockerfile` temprano, y la publica
en GitHub Container Registry (`ghcr.io/<usuario>/sistema-gestion-licitaciones:latest`)
solo en push a `main`.
