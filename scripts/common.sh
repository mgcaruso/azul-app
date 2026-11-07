# Shared helpers for the local deploy scripts. Sourced, not executed.

GCP_PROJECT="azul-encuentra"
REGION="southamerica-east1"

step() { printf '\n\033[36m==> %s\033[0m\n' "$1"; }
warn() { printf '\033[33m%s\033[0m\n' "$1"; }
ok() { printf '\033[32m%s\033[0m\n' "$1"; }
die() { printf '\033[31mERROR: %s\033[0m\n' "$1" >&2; exit 1; }

# Always work from the repo root, wherever the script is launched from.
cd "$(dirname "${BASH_SOURCE[0]}")/.." || exit 1

# Requires: main branch, clean tree, in sync with origin/main.
require_clean_main() {
  [ "$(git branch --show-current)" = "main" ] || die "You must be on the main branch."
  [ -z "$(git status --porcelain)" ] || die "There are uncommitted changes. Commit before running: what gets deployed must be in the repo."
  git fetch origin main || die "git fetch failed."
  [ "$(git rev-parse HEAD)" = "$(git rev-parse origin/main)" ] || die "main is not the same as origin/main. Push or pull first."
}

# Prints the EF migrations of the production database (Neon sleeps, so the first connection is retried).
# Usage: list_migrations "<connection string>"
list_migrations() {
  local conn="$1" out attempt
  # EF starts Program.cs up to Build(): these settings are required but unused here.
  export Admin__BootstrapEmails__0="unused@example.com" Photos__BaseUrl="http://localhost/photos/" Google__ClientId="unused"
  dotnet build src/Azul.Api/Azul.Api.csproj -c Release --nologo >&2 || return 1
  for attempt in 1 2 3; do
    if out=$(cd src/Azul.Api && dotnet ef migrations list --configuration Release --no-build --connection "$conn"); then
      printf '%s\n' "$out"
      return 0
    fi
    warn "Neon did not respond (attempt $attempt). Retrying in 10 s..." >&2
    sleep 10
  done
  return 1
}

read_db_connection() {
  gcloud secrets versions access latest --secret=azul-db-connection --project="$GCP_PROJECT" | tr -d '\r'
}
