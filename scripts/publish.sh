#!/bin/bash

echo "Starting service publication..."

publish_service() {
    local service_name=$1
    local service_path=$2
    local output_path="../app/publish"

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
for db in "$@"; do
    db=$(echo "$db" | xargs)  # Trim any leading/trailing spaces

    if [ "$db" == "gateway" ]; then
        publish_service "ApiGateway" "src/ApiGateway"
    elif [ "$db" == "auth" ]; then
        publish_service "AuthService" "src/AuthService/Presentation"
    elif [ "$db" == "ecommerce" ]; then
       publish_service "EcommerceService" "src/EcommeceService/Presentation"
    elif [ "$db" == "project" ]; then
        publish_service "ProjectService" "src/ProjectService/Presentation"
    elif [ "$db" == "finance" ]; then
        publish_service "FinanceService" "src/FinanceService/Presentation"
    else
        echo "Unknown database: $db"
        exit 1
    fi
    echo "--------------------------------------"
    echo "🎉 All services published successfully!"
done


