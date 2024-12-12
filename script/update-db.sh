#!/bin/bash

# Tải các biến môi trường từ tệp .env
if [ -f "$(dirname "$0")/../.env" ]; then
    source "$(dirname "$0")/../.env"
else
    echo ".env file not found"
    exit 1
fi

# Thay thế sqlserver thành localhost trong chuỗi kết nối
AUTHDB_CONNECTION="${AUTHDB_CONNECTION//sqlserver/localhost}"
PRODUCTDB_CONNECTION="${PRODUCTDB_CONNECTION//sqlserver/localhost}"

# Hàm chạy migration cho cả 2 cơ sở dữ liệu
run_update() {
    local db_name=$1
    local connection=$2
    local project_path=$3
    local api_path=$4

    echo "Running update for $db_name"
    dotnet ef database update --project "$project_path" --startup-project "$api_path" --connection "$connection"
    if [ $? -eq 0 ]; then
        echo "Update for $db_name completed successfully."
    else
        echo "Update for $db_name failed."
        exit 1
    fi
}

# Chạy update cho cả AuthDb và ProductDb
echo "Updating AuthDb..."
run_update "AuthDb" "$AUTHDB_CONNECTION" "src/AuthService/AuthService.Infrastructure" "src/AuthService/AuthService.API"

echo "Updating ProductDb..."
run_update "ProductDb" "$PRODUCTDB_CONNECTION" "src/ProductService/ProductService.Infrastructure" "src/ProductService/ProductService.API"

echo "All updates completed successfully!"
