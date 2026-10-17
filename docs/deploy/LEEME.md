# Deploy de prueba de Azul

Versión de prueba, sin fotos, pensada para gastar lo menos posible. Piezas:

| Pieza | Dónde | Costo |
|---|---|---|
| Base de datos Postgres | Neon, plan gratis (sin tarjeta) | gratis |
| API .NET en contenedor | Google Cloud Run, región São Paulo | cupo gratis mensual; pide cuenta de facturación con tarjeta |
| Front (SPA) | Cloudflare Pages | gratis |

Datos verificados el 17/10 en las páginas oficiales (pueden cambiar): Neon gratis permanente, 1 GB, se duerme a los 5 minutos sin uso. Cloud Run: 2 millones de pedidos, 180.000 segundos de CPU y 360.000 GiB-segundos de memoria gratis por mes, y solo cobra mientras procesa pedidos. Sin verificar: si el cupo gratis rige igual en São Paulo (la página lo basa en `us-central1`). Por eso se configura una alerta de presupuesto.

Los nombres de ejemplo son `azul-api` (Cloud Run) y `azul-web` (Pages).

Orden: claves → base → API → front → CORS → pruebas. Las fotos quedan afuera: `main` ya tiene el selector oculto (`VITE_PHOTOS=false`) y todo lo de fotos está en la rama `feature/fotos`.

---

## 0. Antes de empezar (en tu máquina)

1. Compilá y probá los últimos cambios en `azul-app` (`main`): CORS por configuración, `Seed:Catalog` y el `Dockerfile`. En `src\Azul.Api`: `dotnet build`.
2. Probá que la imagen arma, desde la raíz de `azul-app` y con Docker Desktop abierto: `docker build -t azul-api .`
3. Commiteá y pusheá `main` de los dos repos (`azul-app` y `azul-web`).
4. Cuentas: Neon, Google Cloud (con tarjeta) y Cloudflare.
5. Instalá la CLI de Google Cloud (`gcloud`, desde su documentación) y comprobá con `gcloud --version`.

## 1. Claves secretas

Generá dos valores largos y guardalos en tu gestor de contraseñas (no en git, no en el chat):

```powershell
[guid]::NewGuid().ToString("N") + [guid]::NewGuid().ToString("N")
```

- **`Admin__ApiKey`**: la clave del panel admin (valor 1).
- **`ProviderConsent__PhoneHashKey`**: se usa para guardar el hash de los celulares (valor 2). **No la cambies después**: los hashes guardados dejarían de coincidir.

## 2. Base de datos (Neon)

1. Creá un proyecto en Neon: Postgres 17, región AWS São Paulo (`sa-east-1`).
2. Con los datos de conexión (host, base, usuario, contraseña), armá la cadena para .NET:

```
Host=<host de Neon>;Port=5432;Database=<base>;Username=<usuario>;Password=<contraseña>;SSL Mode=Require
```

3. Aplicá las migraciones contra Neon, desde `src\Azul.Api` en PowerShell, **sin variables de entorno** (así no te queda nada puesto en la terminal):

```powershell
dotnet ef database update --connection "<la cadena de arriba>"
```

Tiene que terminar en `Done.` y aplicar todas las migraciones. Las migraciones crean las extensiones `unaccent` y `pg_trgm`; Neon las permite. Si falla en ese punto, pasame el error.

## 3. API en Google Cloud Run

### 3.1 Proyecto y facturación

```powershell
gcloud auth login
gcloud projects create azul-prueba --name "Azul prueba"
gcloud config set project azul-prueba
```

(Si el id `azul-prueba` ya está tomado, usá otro y reemplazalo en todos los comandos.)

En la consola web: **Facturación** → vinculá una cuenta de facturación al proyecto, y creá un **presupuesto con alerta** de US$ 1 (Facturación → Presupuestos y alertas). La alerta avisa, no corta el gasto.

Activá los servicios:

```powershell
gcloud services enable run.googleapis.com cloudbuild.googleapis.com artifactregistry.googleapis.com
```

### 3.2 Archivo de configuración (no va a git)

En la raíz de `azul-app` creá `env.yaml`. **Está ignorado por git y por Docker**, y tiene secretos: no lo compartas ni lo commitees.

```yaml
ASPNETCORE_ENVIRONMENT: "Production"
Admin__ApiKey: "<valor 1>"
ProviderConsent__PhoneHashKey: "<valor 2>"
ConnectionStrings__Default: "<cadena de Neon>"
Photos__BaseUrl: "https://azul-api.example/photos/"
Seed__Catalog: "true"
```

`Photos__BaseUrl` hoy es un valor de relleno: la API lo exige para arrancar, pero con las fotos apagadas no se usa. Más adelante lo ajustás con la URL real.

### 3.3 Deploy

Desde la raíz de `azul-app` (donde está el `Dockerfile`):

```powershell
gcloud run deploy azul-api --source . --region southamerica-east1 --allow-unauthenticated --max-instances 1 --memory 512Mi --cpu-boost --env-vars-file env.yaml
```

- `--source .` arma la imagen con tu `Dockerfile` (la primera vez ofrece crear un repositorio de imágenes: aceptá).
- `--allow-unauthenticated` es lo normal para una API pública: la protección es la clave admin y los límites de la propia API.
- `--max-instances 1` evita que un pico o un ataque dispare gastos.
- `--cpu-boost` acelera el arranque de .NET.

Al terminar imprime la URL, del estilo `https://azul-api-xxxxxxxx-rj.a.run.app`. Anotala.

Comprobá: `<URL>/api/categories` tiene que devolver las categorías (el catálogo se carga solo en el primer arranque por `Seed__Catalog`; después podés sacar esa línea de `env.yaml` y volver a desplegar).

Para ver los logs: `gcloud run services logs read azul-api --region southamerica-east1 --limit 50`.

### 3.4 Lo que ya está resuelto en el contenedor

El puerto 8080 y los encabezados del proxy (`ASPNETCORE_FORWARDEDHEADERS_ENABLED`, en el `Dockerfile`), para que el límite por IP vea la IP real y no la de Google. Se comprueba en las pruebas (paso 6).

## 4. Front en Cloudflare Pages

1. Cloudflare → Workers & Pages → Create → Pages → conectar con GitHub → repo `azul-web`, rama `main`.
2. Configuración de build:
   - Build command: `yarn build`
   - Build output directory: `build/client`
   - Variables de entorno: `VITE_API_URL=<URL de Cloud Run>`, `VITE_PHOTOS=false` y `NODE_VERSION=22`.
3. El archivo `public/_redirects` ya está en el repo: hace que las rutas del front (`/buscar`, `/admin`...) funcionen al recargar.
4. Si la instalación de Yarn falla en el build, pasame el log.
5. Anotá la URL que te da Pages (por ejemplo `https://azul-web.pages.dev`).

## 5. CORS: que la API acepte al front

La API solo acepta `http://localhost:5173` salvo que la configures. Agregá a `env.yaml`:

```yaml
Cors__Origins__0: "https://azul-web.pages.dev"
```

y volvé a ejecutar el comando de deploy del paso 3.3 (con `--env-vars-file` reemplaza todas las variables, por eso van todas juntas en el archivo). Si después sumás un dominio propio, agregá `Cors__Origins__1`.

## 6. Pruebas

1. Abrí la URL de Pages en el celular **con datos móviles**: se ven las categorías y la búsqueda anda (el primer pedido tras un rato sin uso puede tardar unos segundos).
2. Hacé un alta de prueba ("Sumá tu negocio") sin foto. Tiene que quedar en Pending.
3. Entrá al panel admin con tu clave, publicá la ficha y buscala en el buscador.
4. Límite por IP: fallá el login admin 6 veces seguidas; la sexta tiene que dar 429 (límite de 5 por minuto). En los logs, la línea del pedido tiene que mostrar tu IP real (`ClientIp`) y no una de Google. Si muestra la de Google, avisame: hay que ajustar los encabezados.
5. Recargá una página interna (por ejemplo `/buscar`) para confirmar que no da 404.

## 7. Qué mirar si algo falla

- Logs: `Falta la configuración ...` significa que falta una variable en `env.yaml`.
- CORS: si el front carga pero no trae datos y en la consola del navegador aparece un error de CORS, falta o está mal `Cors__Origins__0`.
- Base: si la API no arranca por la conexión, revisá `SSL Mode=Require` y las comillas en `env.yaml`. Neon se duerme a los 5 minutos: el primer pedido puede tardar y, si la conexión falla una vez, reintentá.
- Costos: revisá Facturación en la consola de Google unos días después del primer deploy.

## 8. Después (no hace falta para esta prueba)

- Pasar los secretos de `env.yaml` a Secret Manager de Google.
- Seq o un servicio de logs externo (hoy se usa Cloud Logging).
- Dominio propio y Cloudflare delante de la API (filtra bots, suma límites).
- Fotos: volver a `feature/fotos`, terminar la migración, activar R2 y poner `VITE_PHOTOS=true`. El disco de Cloud Run es efímero: por eso las fotos tienen que ir a R2.
- Copias de seguridad de la base.
