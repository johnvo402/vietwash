#!/bin/bash

echo "Starting service publication..."

publish_service() {
    local service_name=$1
    local service_root=$2
    local csproj_path=$3

    echo "--------------------------------------"
    echo "Publishing $service_name..."

    local output_path="./app/publish"

    echo "🧹 Cleaning old publish directory..."
    rm -rf "$output_path"
    mkdir -p "$output_path"

    cd "$service_root" || { echo "❌ Failed to cd into $service_root"; exit 1; }

    # Publish service (chỉ rõ file csproj nếu có)
    if [ -n "$csproj_path" ]; then
        dotnet publish "$csproj_path" -c Release -o "$output_path"
    else
        dotnet publish -c Release -o "$output_path"
    fi

    if [ $? -ne 0 ]; then
        echo "❌ Failed to publish $service_name."
        exit 1
    fi

    echo "✅ $service_name published successfully."
    cd - > /dev/null
}

for db in "$@"; do
    db=$(echo "$db" | xargs)  # Trim spaces

    case "$db" in
        gateway)
            publish_service "ApiGateway" "src/ApiGateway" ""
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
            echo "Unknown service: $db"
            exit 1
            ;;
    esac
done

echo "🎉 All services published successfully!"
