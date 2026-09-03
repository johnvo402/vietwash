#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT_DIR}"

compose=(
  docker compose
  --env-file "${ENV_FILE:-.env}"
  -f docker-compose.yaml
  -f docker-compose.database.yaml
  -f docker-compose.staging.yaml
)
# Offline verification of already-built images; normal staging always pulls first.
pull_images=true
if [[ "${1:-}" == "--no-pull" ]]; then
  pull_images=false
  shift
fi
services=("$@")

if [[ "${pull_images}" == false ]]; then
  echo "Using previously built/pulled local images (offline verification)."
elif (( ${#services[@]} > 0 )); then
  echo "Pulling staging images for: ${services[*]}"
else
  echo "Pulling the complete staging image set"
fi

if [[ "${pull_images}" == true ]]; then
  "${compose[@]}" pull "${services[@]}"
fi
"${compose[@]}" up -d --no-build --pull never "${services[@]}"

echo "Staging containers are running from prebuilt images."
"${compose[@]}" ps
echo "Only edge publishes a host port. Frontend, API, PostgreSQL and Redis use Docker DNS."
