#!/usr/bin/env bash
# Applies EF Core migrations to the production database (Neon) from your machine.
# Equivalent to the "Migrar base" GitHub Actions workflow. Run it BEFORE deploy-api.sh.
#
# 1. Checks you are on main, clean and in sync with GitHub.
# 2. Reads the Neon connection string from Secret Manager (kept in memory, never printed).
# 3. Shows the migrations and asks for confirmation before applying them.
#
# Requirements: gcloud logged in (gcloud auth login) and dotnet-ef installed
# (dotnet tool install --global dotnet-ef). See scripts/LEEME-deploy.md.
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/common.sh"

step "Checking the repo"
require_clean_main

step "Reading the connection string from Secret Manager"
conn=$(read_db_connection)
[ -n "$conn" ] || die "Could not read azul-db-connection. Did you run gcloud auth login?"

step "Migrations (Neon sleeps: the first connection is retried)"
list=$(list_migrations "$conn") || die "Could not read the migrations from Neon."
echo "$list"

if ! grep -q "(Pending)" <<<"$list"; then
  ok "No pending migrations. Nothing to apply."
  exit 0
fi

read -r -p "The migrations marked (Pending) will be applied to PRODUCTION. Continue? (y/N) " answer
[[ "$answer" =~ ^[yYsS]$ ]] || { echo "Cancelled."; exit 0; }

step "Applying migrations"
(cd src/Azul.Api && dotnet ef database update --configuration Release --no-build --connection "$conn")
ok "Migrations applied to Neon. You can now run scripts/deploy-api.sh"
