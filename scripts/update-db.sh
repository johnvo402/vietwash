#!/bin/bash

echo "Starting database migrations..."

run_update() {
    local service_name=$1
    local infra_path=$2

    echo "--------------------------------------"
    echo "Running migration for $service_name..."

    cd "$infra_path" || { echo "❌ Failed to cd into $infra_path"; exit 1; }

    dotnet ef database update
    if [ $? -eq 0 ]; then
        echo "✅ Migration for $service_name completed successfully."
    else
        echo "❌ Migration for $service_name failed."
        exit 1
    fi

    cd - > /dev/null
}

# Danh sách các service cần migrate
run_update "AuthService" "src/AuthService/Infrastructure"
run_update "EcommeceService" "src/EcommeceService/Infrastructure"
run_update "ProjectService" "src/ProjectService/Infrastructure"

echo "--------------------------------------"
echo "🎉 All migrations completed successfully!"
