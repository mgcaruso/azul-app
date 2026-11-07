# Deploy local de la API (plan B de GitHub Actions)

Dos scripts de bash que hacen lo mismo que los workflows "Migrar base" y "Deploy API", pero desde tu PC o tu Mac. Sirven si GitHub Actions no anda o si querés ahorrar minutos (el plan gratis da 2.000 por mes). En Windows se corren con **Git Bash** (viene con git).

| Script | Equivale a | Qué hace |
|---|---|---|
| `scripts/migrate-db.sh` | Migrar base | Aplica las migraciones de EF en Neon (te pide confirmar) |
| `scripts/deploy-api.sh` | Deploy API | Arma la imagen, la sube y deploya en Cloud Run sin cortes |

`scripts/common.sh` tiene las funciones compartidas; no se ejecuta solo.

## Una sola vez

1. **Docker Desktop** instalado (tiene que estar abierto cuando deployás).
2. **gcloud** con tu sesión: `gcloud auth login`. Tu cuenta necesita permisos sobre el proyecto `azul-encuentra` (Cloud Run, Artifact Registry, Secret Manager).
3. **dotnet-ef**: `dotnet tool install --global dotnet-ef --version "10.*"`.
4. **Git Bash** (Windows): abrí "Git Bash" desde el menú de inicio, o desde Rider: Terminal → Git Bash.

## Cada deploy

```bash
cd /c/asp-learning/azul-app
git checkout main
git pull

scripts/migrate-db.sh     # solo si hay migraciones nuevas
scripts/deploy-api.sh
```

Los scripts se niegan a correr si no estás en `main`, si hay cambios sin commitear o si `main` no está igual que en GitHub. Así lo que se deploya siempre está en el repo.

`deploy-api.sh` también frena si quedan migraciones sin aplicar (corré antes `migrate-db.sh`). La revisión nueva se prueba en una URL aparte (`candidata`) y recién cuando responde bien recibe el tráfico: si falla, la versión anterior sigue atendiendo.

## Volver a una versión anterior

```bash
scripts/deploy-api.sh <id completo del commit>
```

Usa la imagen que ya está guardada en Artifact Registry (no arma nada nuevo). Ojo: la política de limpieza guarda solo las últimas 10 imágenes.

## Diferencias con GitHub Actions

- Usa **tu** cuenta de Google, no la de `github-deployer`. Si algo da "permission denied", es de tu cuenta.
- No hay historial en la pestaña Actions de GitHub; la revisión de Cloud Run queda con la etiqueta `commit` igual que siempre.
- Los dos caminos conviven: podés usar uno u otro cuando quieras.
- El front tiene su propio comando: en `azul-web`, `yarn deploy`.
