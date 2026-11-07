#!/usr/bin/env bash
# Deploys the API to Cloud Run from your machine, without spending GitHub Actions minutes.
# Equivalent to the "Deploy API" workflow.
#
# 1. Checks you are on main, clean and in sync with GitHub.
# 2. Stops if there are pending migrations (run scripts/migrate-db.sh first).
# 3. Builds the image with Docker and pushes it to Artifact Registry tagged with the commit id.
# 4. Creates a new Cloud Run revision WITHOUT traffic and smoke tests it.
# 5. If it responds, sends it 100% of the traffic. Otherwise the old revision keeps serving.
#
# Requirements: Docker Desktop running, gcloud logged in (gcloud auth login) and dotnet-ef installed.
# See scripts/LEEME-deploy.md.
#
# Usage:
#   scripts/deploy-api.sh                 # deploy the current commit of main
#   scripts/deploy-api.sh <full commit>   # rollback: reuse an image already in Artifact Registry
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/common.sh"

SERVICE="azul-api"
RUNTIME_SA="azul-api-runtime@azul-encuentra.iam.gserviceaccount.com"
IMAGE_REPO="southamerica-east1-docker.pkg.dev/azul-encuentra/azul/azul-api"
SECRETS="Admin__ApiKey=azul-admin-api-key:latest,ProviderConsent__PhoneHashKey=azul-phone-hash-key:latest,ConnectionStrings__Default=azul-db-connection:latest,Proxy__Secret=azul-proxy-secret:latest"

version="${1:-}"
rollback=false
[ -n "$version" ] && rollback=true

step "Checking the repo"
if ! $rollback; then
  require_clean_main
  version=$(git rev-parse HEAD)
fi
image="$IMAGE_REPO:$version"
echo "Version to deploy: $version"

if ! $rollback; then
  docker info >/dev/null 2>&1 || die "Docker is not responding. Open Docker Desktop and wait until it is running."
fi

step "Checking the gcloud session"
account=$(gcloud auth list --filter=status:ACTIVE --format="value(account)" | tr -d '\r')
[ -n "$account" ] || die "No gcloud session. Run: gcloud auth login"
echo "Account: $account"

if ! $rollback; then
  step "Checking there are no pending migrations"
  conn=$(read_db_connection)
  [ -n "$conn" ] || die "Could not read azul-db-connection."
  list=$(list_migrations "$conn") || die "Could not read the migrations from Neon."
  if grep -q "(Pending)" <<<"$list"; then
    die "There are unapplied migrations in Neon. Run scripts/migrate-db.sh first, then run this again."
  fi
  echo "No pending migrations."

  step "Building and pushing the image"
  gcloud auth configure-docker "$REGION-docker.pkg.dev" --quiet
  docker build -t "$image" .
  docker push "$image"
else
  step "Checking the rollback image exists"
  gcloud artifacts docker images describe "$image" --project="$GCP_PROJECT" >/dev/null
fi

step "Creating the new revision (no traffic)"
gcloud run deploy "$SERVICE" \
  --project="$GCP_PROJECT" --region="$REGION" \
  --image="$image" \
  --service-account="$RUNTIME_SA" \
  --max-instances=1 --memory=512Mi --cpu-boost \
  --env-vars-file=deploy/cloudrun-env.yaml \
  --set-secrets="$SECRETS" \
  --labels="commit=${version:0:12}" \
  --no-traffic --tag=candidata --quiet

step "Smoke testing the new revision"
# The tag URL follows the "tag" key in the JSON (keys are alphabetical).
candidate=$(gcloud run services describe "$SERVICE" --project="$GCP_PROJECT" --region="$REGION" --format=json \
  | tr -d '\r' | grep -A1 '"tag": "candidata"' | grep '"url"' | sed -E 's/.*"url": "([^"]+)".*/\1/')
[ -n "$candidate" ] || die "Could not find the candidate revision URL."
echo "Testing $candidate/api/categories"

healthy=false
for attempt in 1 2 3 4 5 6 7; do
  # The first response can be slow (cold start + sleeping Neon).
  if curl -fsS --max-time 30 "$candidate/api/categories" >/dev/null; then
    healthy=true
    break
  fi
  warn "Not responding yet (attempt $attempt). Retrying in 10 s..."
  sleep 10
done
$healthy || die "The new revision did not respond. It got NO traffic: the previous version keeps serving."
echo "The new revision responds correctly."

step "Sending 100% of the traffic"
gcloud run services update-traffic "$SERVICE" --project="$GCP_PROJECT" --region="$REGION" --to-latest --quiet

revision=$(gcloud run services describe "$SERVICE" --project="$GCP_PROJECT" --region="$REGION" --format="value(status.latestReadyRevisionName)" | tr -d '\r')
ok "API deployed. Commit $version, revision $revision"
echo "Rollback: scripts/deploy-api.sh <previous commit>"
