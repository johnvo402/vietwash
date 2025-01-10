# Định nghĩa các biến môi trường từ tệp .env (nếu có)
SHELL := /bin/bash
# .EXPORT_ALL_VARIABLES:

# # REGISTRY ?= johnvo402
# # PROJECT ?= $(shell basename $(PWD))
# VERSION ?= $(shell date +"%Y%m%d")
# TAG ?= $(shell ./scripts/get-version.sh)
# GIT_COMMIT ?= $(shell git rev-parse HEAD)
# GIT_BRANCH ?= $(shell git rev-parse --abbrev-ref HEAD)
# ENV_FILE ?= .env
# args=$(filter-out $@,$(MAKECMDGOALS))

# # export .env file
# -include $(ENV_FILE)
# export
# Mục chính để chạy migration
migration:
	@echo "Running migration for database(s): $(NAME)"
	./scripts/run_migration.sh $(foreach db,$(NAME),$(db))

# Mục để cập nhật migration (tùy chọn)
update:
	@echo "Updating migration... $(NAME)"
	./scripts/update-db.sh $(foreach db,$(NAME),$(db))

# Mục để kiểm tra migration status
status:
	@echo "Checking migration status..."
	dotnet ef migrations list

# Mục để chạy Docker container cho môi trường phát triển
dev:
	docker-compose -f docker-compose.yaml -f docker-compose.sql.yaml -f docker-compose.dev.yaml up -d ${SERVICE}

dev-build:
	docker-compose -f docker-compose.yaml -f docker-compose.sql.yaml -f docker-compose.dev.yaml up -d --build ${SERVICE}

staging:
	docker-compose -f docker-compose.yaml -f docker-compose.sql.yaml -f docker-compose.staging.yaml up -d --build ${SERVICE}
# Mục để tắt Docker container và xóa volume
clean:
	@echo "Stopping Docker containers and removing volumes..."
	docker-compose -f docker-compose.yaml -f docker-compose.sql.yaml -f docker-compose.dev.yaml down --remove-orphans -v

# Mục để chỉ tắt Docker container mà không xóa volume
down:
	@echo "Stopping Docker containers..."
	docker-compose -f docker-compose.yaml -f docker-compose.sql.yaml -f docker-compose.dev.yaml down
stop:
	@echo "Stopping Docker containers..."
	docker-compose -f docker-compose.yaml -f docker-compose.sql.yaml -f docker-compose.dev.yaml stop
.PHONY: backup

backup: 
	@echo "Backup database..."
	./scripts/backup.sh backup
.PHONY: restore
restore: 
	./scripts/backup.sh restore

.PHONY: sql
sql:
	./scripts/backup.sh sql
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
