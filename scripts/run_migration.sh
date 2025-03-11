#!/bin/bash

# Lấy ngày tháng năm giờ phút giây hiện tại
current_datetime=$(date +"%Y%m%d%H%M%S")

# Hàm chạy migration
run_migration() {
    local db_name=$1
    local project_path=$2
    local api_path=$3

    echo "Running migration for $db_name"
    dotnet ef migrations add "${current_datetime}_${db_name}_Migration" --project "$project_path" --startup-project "$api_path" -o Data/Migrations
    if [ $? -eq 0 ]; then
        echo "Migration for $db_name completed successfully."
    else
        echo "Migration for $db_name failed."
        exit 1
    fi
}

# Loop through each database and run migration
for db in "$@"; do
    db=$(echo "$db" | xargs)  # Trim any leading/trailing spaces

    if [ "$db" == "Auth" ]; then
        run_migration "Auth" "src/AuthService/Infrastructure" "src/AuthService/Presentation"
    elif [ "$db" == "Project" ]; then
        run_migration "Project" "src/ProjectService/Infrastructure" "src/ProjectService/Presentation"
    elif [ "$db" == "Ecommerce" ]; then
        run_migration "Project" "src/EcommerceService/Infrastructure" "src/EcommerceService/Presentation"
    
    else
        echo "Unknown database: $db"
        exit 1
    fi
done
