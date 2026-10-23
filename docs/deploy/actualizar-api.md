# Actualizar la API en Cloud Run

Cómo subir una versión nueva de `main` al servicio `azul-api` que ya existe en Cloud Run. La primera puesta en marcha (Neon, proyecto de Google Cloud, claves y front) está en [LEEME.md](LEEME.md).

Datos del servicio:

| Dato | Valor |
|---|---|
| Proyecto de Google Cloud | `azul-prueba` (si usaste otro id, reemplazalo en todos los comandos) |
| Servicio | `azul-api` |
| Región | `southamerica-east1` (São Paulo) |
| Variables y secretos | `env.yaml` en la raíz del repo (no va a git) |

Todos los comandos son para PowerShell.

---

## 1. Preparar la máquina (una sola vez)

### 1.1 Instalar gcloud

```powershell
winget install Google.CloudSDK
```

Cerrá la terminal, abrí una nueva y comprobá con `gcloud --version`.

### 1.2 Si PowerShell no reconoce `gcloud`

**"The term 'gcloud' is not recognized"**: la carpeta de gcloud no está en el PATH, o la terminal se abrió antes de instalarlo. Este script la agrega al PATH de tu usuario de forma permanente (no hace falta ser administradora):

```powershell
$bin = @("$env:LOCALAPPDATA\Google\Cloud SDK\google-cloud-sdk\bin",
         "${env:ProgramFiles(x86)}\Google\Cloud SDK\google-cloud-sdk\bin",
         "$env:ProgramFiles\Google\Cloud SDK\google-cloud-sdk\bin") |
       Where-Object { Test-Path "$_\gcloud.cmd" } | Select-Object -First 1
if (-not $bin) { "No encontré gcloud instalado" }
else {
  $userPath = [Environment]::GetEnvironmentVariable("Path", "User")
  if ($userPath -notlike "*$bin*") {
    [Environment]::SetEnvironmentVariable("Path", "$userPath;$bin", "User")
    "Agregado: $bin"
  } else { "Ya estaba en el PATH: $bin" }
}
```

Después cerrá **todas** las terminales y también Rider o VS Code si usás su terminal, porque toman el PATH al arrancar.

**"gcloud.ps1 cannot be loaded because running scripts is disabled"**: Windows bloquea los scripts de PowerShell. Habilitalos para tu usuario (una sola vez, respondé `Y`):

```powershell
Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
```

Si no querés cambiar la política, usá `gcloud.cmd` en lugar de `gcloud`.

### 1.3 Iniciar sesión y elegir el proyecto

```powershell
gcloud auth login
gcloud config set project azul-prueba
```

`gcloud auth login` abre el navegador: elegí la cuenta de Google con la que creaste el proyecto. La sesión queda guardada y no hace falta repetirla en cada deploy.

Para ver con qué cuenta y proyecto estás: `gcloud config list`. Para ver tus proyectos: `gcloud projects list` (columna `PROJECT_ID`).

---

## 2. Antes de cada deploy

1. **Código al día**: en la raíz del repo, `git checkout main` y `git pull`.
2. **Sin cambios sueltos**: `gcloud run deploy --source .` sube la carpeta tal como está en tu disco, no lo que hay en GitHub. Revisá `git status`. Si solo aparecen archivos con cambios de fin de línea (CRLF), no afectan; si hay cambios reales sin commitear, se van a subir.
3. **Compila y arma la imagen** (opcional pero recomendado): `dotnet build` en `src\Azul.Api`, y con Docker Desktop abierto, `docker build -t azul-api .` en la raíz.
4. **Migraciones nuevas**: si desde el último deploy se agregó algo en `src\Azul.Api\Migrations`, aplicalas contra Neon antes de desplegar (paso 3).
5. **Variables nuevas**: si el código empezó a leer una configuración nueva, agregala a `env.yaml`. Con `--env-vars-file` se reemplazan **todas** las variables del servicio, así que el archivo tiene que tener siempre la lista completa.

Variables que hoy tiene que tener `env.yaml`:

```yaml
ASPNETCORE_ENVIRONMENT: "Production"
Admin__ApiKey: "<clave del panel admin>"
ProviderConsent__PhoneHashKey: "<clave del hash de celulares; no cambiarla nunca>"
ConnectionStrings__Default: "<cadena de Neon>"
Photos__BaseUrl: "https://azul-api.example/photos/"
Cors__Origins__0: "<URL del front>"
```

Opcionales: `Seed__Catalog: "true"` (carga el catálogo si la base está vacía; solo hace falta en el primer arranque) y `Seq__ServerUrl` (manda los logs a Seq).

---

## 3. Migraciones (solo si hay nuevas)

Desde `src\Azul.Api`, pasando la cadena de Neon por parámetro para no dejarla en variables de la terminal:

```powershell
dotnet ef migrations list --connection "<cadena de Neon>"
dotnet ef database update --connection "<cadena de Neon>"
```

`migrations list` marca con `(Pending)` las que faltan aplicar. `database update` tiene que terminar en `Done.`. Neon se duerme a los 5 minutos sin uso: si la primera conexión falla, reintentá.

---

## 4. Deploy

Desde la raíz del repo (donde está el `Dockerfile`):

```powershell
gcloud run deploy azul-api --source . --region southamerica-east1 --allow-unauthenticated --max-instances 1 --memory 512Mi --cpu-boost --env-vars-file env.yaml
```

Arma la imagen en Google Cloud Build y tarda unos minutos. Si pregunta algo (por ejemplo, crear el repositorio de imágenes), respondé `Y`. Al terminar muestra la `Service URL`.

`env.yaml` está en `.gitignore` y `.dockerignore`: no se sube con el código, pero `--env-vars-file` carga sus valores en el servicio.

---

## 5. Comprobar

1. `<Service URL>/api/categories` devuelve las categorías.
2. Desde el front, una búsqueda trae resultados.
3. Logs:

```powershell
gcloud run services logs read azul-api --region southamerica-east1 --limit 50
```

`Falta la configuración ...` en los logs significa que falta una variable en `env.yaml`.

---

## 6. Volver a la versión anterior

Si la versión nueva falla, Cloud Run guarda las anteriores (revisiones):

```powershell
gcloud run revisions list --service azul-api --region southamerica-east1
gcloud run services update-traffic azul-api --region southamerica-east1 --to-revisions <REVISION_ANTERIOR>=100
```

Ojo: volver de revisión no deshace las migraciones aplicadas en la base.

Mientras el tráfico esté fijado a una revisión, los deploys nuevos no lo reciben. Cuando la arregles y despliegues, devolvé el tráfico a la última revisión:

```powershell
gcloud run services update-traffic azul-api --region southamerica-east1 --to-latest
```
