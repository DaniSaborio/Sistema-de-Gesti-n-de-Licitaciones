# syntax=docker/dockerfile:1

# --- Etapa de compilación --------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copia primero los .csproj para aprovechar el caché de capas de Docker en `dotnet restore`.
COPY Licitaciones.slnx ./
COPY src/Licitaciones.Domain/Licitaciones.Domain.csproj src/Licitaciones.Domain/
COPY src/Licitaciones.Application/Licitaciones.Application.csproj src/Licitaciones.Application/
COPY src/Licitaciones.Infrastructure/Licitaciones.Infrastructure.csproj src/Licitaciones.Infrastructure/
COPY src/Licitaciones.Api/Licitaciones.Api.csproj src/Licitaciones.Api/
COPY src/Licitaciones.Web/Licitaciones.Web.csproj src/Licitaciones.Web/
COPY tests/Licitaciones.UnitTests/Licitaciones.UnitTests.csproj tests/Licitaciones.UnitTests/
COPY tests/Licitaciones.IntegrationTests/Licitaciones.IntegrationTests.csproj tests/Licitaciones.IntegrationTests/
COPY tests/Licitaciones.FunctionalTests/Licitaciones.FunctionalTests.csproj tests/Licitaciones.FunctionalTests/

RUN dotnet restore src/Licitaciones.Web/Licitaciones.Web.csproj

COPY src/ src/

RUN dotnet publish src/Licitaciones.Web/Licitaciones.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# --- Etapa de ejecución ------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# icu-libs: la imagen alpine no trae datos de globalización por defecto, así que
# CultureInfo.GetCultureInfo("es-CR") (formato de montos en colones) falla con
# CultureNotFoundException en modo invariant. Ver
# https://github.com/dotnet/dotnet-docker/blob/main/documentation/scenarios/globalization.md
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Usuario no privilegiado (13.1): la imagen base "aspnet" ya incluye el
# usuario/grupo "app" (uid/gid 64198) pensado para este propósito.
RUN chown -R app:app /app
USER app

COPY --from=build --chown=app:app /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 \
    CMD wget -qO- http://127.0.0.1:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Licitaciones.Web.dll"]
