#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT_DIR}"

if (( $# == 0 )); then
  echo "Usage: ./scripts/publish.sh <gateway|auth|ecommerce|project|finance|notification> [...]" >&2
  exit 1
fi

publish_service() {
  local service_name="$1"
  local service_root="$2"
  local project_file="$3"
  local output_path="${service_root}/app/publish"

  echo "Publishing ${service_name} to ${output_path}"
  rm -rf "${output_path}"
  dotnet publish "${service_root}/${project_file}" \
    --configuration Release \
    --output "${output_path}"
}

for service in "$@"; do
  case "${service}" in
    gateway)
      publish_service "ApiGateway" "src/ApiGateway" "ApiGateway.csproj"
      ;;
    auth)
      publish_service "AuthService" "src/AuthService" "Presentation/Presentation.csproj"
      ;;
    ecommerce)
      publish_service "EcommerceService" "src/EcommeceService" "Presentation/Presentation.csproj"
      ;;
    project)
      publish_service "ProjectService" "src/ProjectService" "Presentation/Presentation.csproj"
      ;;
    finance)
      publish_service "FinanceService" "src/FinanceService" "Presentation/Presentation.csproj"
      ;;
    notification)
      publish_service "NotificationService" "src/NotificationService" "Presentation/Presentation.csproj"
      ;;
    *)
      echo "Unknown service: ${service}" >&2
      exit 1
      ;;
  esac
done

echo "All requested services published successfully."
