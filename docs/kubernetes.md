# Kubernetes

Manifiestos en `/k8s`, pensados para aplicarse en orden (o todos juntos, ya que no hay
dependencias circulares):

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/app-configmap.yaml
cp k8s/app-secret.example.yaml k8s/app-secret.yaml   # editar con credenciales reales
kubectl apply -f k8s/app-secret.yaml
kubectl apply -f k8s/postgres-pvc.yaml
kubectl apply -f k8s/postgres-service.yaml
kubectl apply -f k8s/postgres-statefulset.yaml
kubectl apply -f k8s/app-deployment.yaml
kubectl apply -f k8s/app-service.yaml
```

`k8s/app-secret.yaml` está en `.gitignore` a propósito: `app-secret.example.yaml` es
la única plantilla versionada, sin credenciales reales (sección 14.2: "no subir
secretos, archivos .env, binarios... credenciales").

## Manifiestos

| Archivo | Recurso | Responsabilidad |
|---|---|---|
| `namespace.yaml` | Namespace `licitaciones` | Aísla todos los recursos del proyecto |
| `app-configmap.yaml` | ConfigMap | Configuración no sensible: entorno, URL de escucha, nombre/host/puerto de la base de datos |
| `app-secret.example.yaml` | Secret (plantilla) | Credenciales de PostgreSQL y cadena de conexión completa |
| `postgres-pvc.yaml` | PersistentVolumeClaim (2Gi, `ReadWriteOnce`) | Almacenamiento persistente de PostgreSQL |
| `postgres-service.yaml` | Service headless (`clusterIP: None`) | DNS estable para el StatefulSet (`licitaciones-postgres.licitaciones.svc.cluster.local`) |
| `postgres-statefulset.yaml` | StatefulSet (1 réplica) | PostgreSQL con `pg_isready` como readiness/liveness probe |
| `app-deployment.yaml` | Deployment (1 réplica) | La aplicación, con `startupProbe`/`readinessProbe`/`livenessProbe` contra `/health/live` y `/health/ready` |
| `app-service.yaml` | Service `ClusterIP` (puerto 80 → 8080) | Expone la aplicación dentro del clúster |

## Por qué StatefulSet para PostgreSQL y Deployment para la app

PostgreSQL necesita identidad de red estable y un volumen que siga al mismo pod
(StatefulSet + Service headless es el patrón estándar de Kubernetes para bases de
datos con una sola réplica). La aplicación es sin estado entre solicitudes (el estado
vive en PostgreSQL), así que un Deployment normal es suficiente.

## Por qué una sola réplica de la aplicación

`Program.cs` aplica las migraciones de EF Core automáticamente al iniciar
(`dbContext.Database.Migrate()`). Con una sola réplica esto es seguro y reproducible
sin pasos manuales; escalar horizontalmente exigiría mover ese paso a un `Job` de
Kubernetes ejecutado antes del `Deployment` (o a un *init container* dedicado) para
evitar que dos réplicas intenten migrar al mismo tiempo. Se documenta esta limitación
en vez de ignorarla: el `replicas: 1` en `app-deployment.yaml` tiene un comentario
explicando exactamente esto.

## Probes

- **`startupProbe`** (`/health/live`, hasta 60s de margen): da tiempo a que la
  aplicación aplique migraciones antes de que el `livenessProbe` la mate por "no
  responder".
- **`readinessProbe`** (`/health/ready`): usa `AddDbContextCheck<LicitacionesDbContext>`,
  así que el pod no recibe tráfico hasta que puede conectarse realmente a PostgreSQL.
- **`livenessProbe`** (`/health/live`): verificación liviana de que el proceso sigue
  respondiendo, sin depender de la base de datos (para no reiniciar la app si
  PostgreSQL tiene un problema transitorio que el `readinessProbe` ya está manejando).

## Recursos

Tanto la aplicación como PostgreSQL declaran `requests`/`limits` de CPU y memoria
conservadores (suficientes para una carga de demostración/evaluación, ajustables según
el clúster real).

## Validación en este entorno

Este entorno de generación no tiene `kubectl` ni un clúster disponible. Los ocho
manifiestos se validaron parseando el YAML (sintaxis correcta) y se validan además en
CI con `kubeconform` (esquema de Kubernetes, sin necesitar un clúster real) en el job
`validar-manifiestos-k8s` de `.github/workflows/ci.yml`.
