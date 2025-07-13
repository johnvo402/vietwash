#!/bin/bash

cd "$(dirname "$0")/.." || exit 1

if [ -n "$1" ]; then
  NAME="$1"
fi

declare -A SERVICES=(
    ["ecommerce"]="src/EcommeceService/Presentation"
    ["auth"]="src/AuthService/Presentation"
    ["finance"]="src/FinanceService/Presentation"
    ["project"]="src/ProjectService/Presentation"
)

if [ -z "$NAME" ]; then
    echo "No NAME provided, running all services..."
    SELECTED_SERVICES=("${!SERVICES[@]}")
else
    echo "Running only services in: $NAME"
    IFS=' ' read -ra SELECTED_SERVICES <<< "$NAME"
fi

PIDS=()

trap 'echo ""; echo "🛑 Stopping all services..."; for pid in "${PIDS[@]}"; do kill $pid 2>/dev/null; done; exit 0' SIGINT

for service in "${SELECTED_SERVICES[@]}"; do
    path="${SERVICES[$service]}"
    if [ -n "$path" ]; then
        echo "▶️  Starting $service service..."
        dotnet run --project "$path" &
        PIDS+=($!)
    else
        echo "❌ Service '$service' not recognized. Skipped."
    fi
done

echo "✅ All selected services are running in background."
echo "Press Ctrl+C to stop."

wait
