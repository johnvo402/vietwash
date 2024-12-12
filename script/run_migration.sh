#!/bin/bash


# Kiểm tra tham số truyền vào
if [ -z "$1" ]; then
    echo "Usage: $0 <AuthDb|ProductDb>"
    exit 1
fi

db=$1


# Lấy ngày tháng năm giờ phút giây hiện tại
current_datetime=$(date +"%Y%m%d%H%M%S")

# Hàm chạy migration
run_migration() {
    local db_name=$1
    local project_path=$2
    local api_path=$3

    echo "Running migration for $db_name"
    dotnet ef migrations add "${current_datetime}_${db_name}_Migration" --project "$project_path" --startup-project "$api_path"
    if [ $? -eq 0 ]; then
        echo "Migration for $db_name completed successfully."
    else
        echo "Migration for $db_name failed."
        exit 1
    fi
}

# Kiểm tra và chạy migration theo tham số
if [ "$db" == "AuthDb" ]; then
    run_migration "AuthDb" "src/AuthService/AuthService.Infrastructure" "src/AuthService/AuthService.API"
elif [ "$db" == "ProductDb" ]; then
    run_migration "ProductDb" "src/ProductService/ProductService.Infrastructure" "src/ProductService/ProductService.API"
else
    echo "Unknown database: $db"
    exit 1
fi
