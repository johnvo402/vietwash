#!/bin/bash

# Gán giá trị chuỗi kết nối cho các biến môi trường
AuthDb="Server=localhost,1433;Database=AuthDb;User Id=sa;Password=Admin@1234;TrustServerCertificate=True;"
ProductDb="Server=localhost,1433;Database=ProductDb;User Id=sa;Password=Admin@1234;TrustServerCertificate=True;"


# Chạy migration cho AuthDb
echo "Running migration for AuthDb"
dotnet ef database update --project src/AuthService/AuthService.Infrastructure --startup-project src/AuthService/AuthService.API --connection "$AuthDb"
# Kiểm tra kết quả
if [ $? -eq 0 ]; then
    echo "Migration for AuthDb completed successfully."
else
    echo "Migration for AuthDb failed."
    exit 1
fi

# Chạy migration cho ProductDb
echo "Running migration for ProductDb"
dotnet ef database update --project src/ProductService/ProductService.Infrastructure --startup-project src/ProductService/ProductService.API --connection "$ProductDb"
# Kiểm tra kết quả
if [ $? -eq 0 ]; then
    echo "Migration for ProductDb completed successfully."
else
    echo "Migration for ProductDb failed."
    exit 1
fi
