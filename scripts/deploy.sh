#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT_DIR}"

compose=(
  docker compose
  -f docker-compose.yaml
  -f docker-compose.database.yaml
  -f docker-compose.staging.yaml
)
services=("$@")

if (( ${#services[@]} > 0 )); then
  echo "Pulling staging images for: ${services[*]}"
else
  echo "Pulling the complete staging image set"
fi

"${compose[@]}" pull "${services[@]}"
"${compose[@]}" up -d --no-build "${services[@]}"

echo "Staging containers are running from prebuilt images."
