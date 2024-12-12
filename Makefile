# Định nghĩa các biến môi trường từ tệp .env
SHELL := /bin/bash

# Mục chính để chạy migration
run-migration:
	@echo "Loading environment variables from .env..."
	./script/run_migration.sh

# Mục để kiểm tra migration status (tùy chọn)
status:
	@echo "Checking migration status..."
	dotnet ef migrations list
dev:
	docker-compose up -d
clean:
	docker-compose down --v
down:
	docker-compose down
