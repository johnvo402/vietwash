#!/bin/bash

echo "Starting service publication..."

publish_service() {
    local service_name=$1
    local service_path=$2
    local output_path="app/publish"

    echo "--------------------------------------"
    echo "Publishing $service_name..."

    cd "$service_path" || { echo "❌ Failed to cd into $service_path"; exit 1; }

    # Publish service
    dotnet publish -c Release -o "$output_path"
    if [ $? -ne 0 ]; then
        echo "❌ Failed to publish $service_name."
        exit 1
    fi

    echo "✅ $service_name published successfully."

    cd - > /dev/null
}

# Danh sách các service cần publish
publish_service "ApiGateway" "src/ApiGateway"
publish_service "AuthService" "src/AuthService/Presentation"
publish_service "EcommerceService" "src/EcommeceService/Presentation"
publish_service "ProjectService" "src/ProjectService/Presentation"

echo "--------------------------------------"
echo "🎉 All services published successfully!"
