# Định nghĩa các biến môi trường từ tệp .env (nếu có)
SHELL := /bin/bash

# Mục chính để chạy migration
migration:
	@echo "Running migration for database: $(NAME)"
	./script/run_migration.sh $(NAME)

# Mục để cập nhật migration (tùy chọn)
update:
	@echo "Updating migration..."
	./script/update-db.sh

# Mục để kiểm tra migration status
status:
	@echo "Checking migration status..."
	dotnet ef migrations list

# Mục để chạy Docker container cho môi trường phát triển
dev:
	@echo "Starting Docker containers..."
	docker-compose up -d

# Mục để tắt Docker container và xóa volume
clean:
	@echo "Stopping Docker containers and removing volumes..."
	docker-compose down -v

# Mục để chỉ tắt Docker container mà không xóa volume
down:
	@echo "Stopping Docker containers..."
	docker-compose down

# Mục mục tiêu mặc định
all: help

# Lệnh để hiển thị hướng dẫn sử dụng Makefile
help:
	@echo "Usage:"
	@echo "  make migration NAME=AuthDb      # Run migration for AuthDb"
	@echo "  make migration NAME=ProductDb   # Run migration for ProductDb"
	@echo "  make update                    # Update database migration"
	@echo "  make status                    # Check migration status"
	@echo "  make dev                       # Start Docker containers for development"
	@echo "  make clean                     # Stop Docker containers and remove volumes"
	@echo "  make down                      # Stop Docker containers"
