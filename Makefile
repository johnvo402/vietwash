# Định nghĩa các biến môi trường từ tệp .env (nếu có)
SHELL := /bin/bash
# .EXPORT_ALL_VARIABLES:

# # REGISTRY ?= johnvo402
# # PROJECT ?= $(shell basename $(PWD))
# VERSION ?= $(shell date +"%Y%m%d")
# TAG ?= $(shell ./scripts/get-version.sh)
# GIT_COMMIT ?= $(shell git rev-parse HEAD)
# GIT_BRANCH ?= $(shell git rev-parse --abbrev-ref HEAD)
ENV_FILE ?= .env
# args=$(filter-out $@,$(MAKECMDGOALS))

export .env file
-include $(ENV_FILE)
export

rwildcard = $(foreach entry,$(wildcard $1*),$(if $(filter bin obj,$(notdir $(entry))),,$(call rwildcard,$(entry)/,$2) $(filter $(subst *,%,$2),$(entry))))
UNIT_TEST_PROJECTS := $(sort $(call rwildcard,tests/UnitTest/,*.csproj))

.PHONY: all help migration update deploy publish test run status dev dev-build staging clean external down stop ssh backup restore sql sql_to_server frontend-install frontend-dev frontend-generate frontend-build frontend-check backend-restore backend-build backend-test backend-test-all check

# Monorepo developer workflow
frontend-install:
	cd frontend && npm ci

frontend-dev:
	cd frontend && npm run dev

frontend-generate:
	cd frontend && npm run generate

frontend-build: frontend-generate
	cd frontend && npm run build:test

frontend-check: frontend-install frontend-generate
	cd frontend && npm run typecheck
	cd frontend && npm run lint
	cd frontend && npm run build:test

backend-restore:
	dotnet restore Micro.sln --configfile NuGet.Config

backend-build: backend-restore
	dotnet build Micro.sln --configuration Release --no-restore

backend-test: backend-build
	@if [ -z "$(UNIT_TEST_PROJECTS)" ]; then echo "No pure unit test projects found under tests/UnitTest."; exit 1; fi
	@set -e; for project in $(UNIT_TEST_PROJECTS); do \
		echo "Running unit tests: $$project"; \
		dotnet test "$$project" --configuration Release --no-build --no-restore; \
	done

# Integration tests require the local PostgreSQL test infrastructure.
backend-test-all: backend-build
	dotnet test Micro.sln --configuration Release --no-build --no-restore

check: backend-test frontend-check
# Mục chính để chạy migration
migration:
	@echo "Running migration for database(s): $(NAME)"
	./scripts/run_migration.sh $(foreach db,$(NAME),$(db))

# Mục để cập nhật migration (tùy chọn)
update:
	./scripts/update-db.sh
deploy:
	./scripts/deploy.sh $(SERVICE)
publish:
	./scripts/publish.sh $(foreach db,$(NAME),$(db))
test:
	./scripts/run_tests.sh ${TYPE} ${NAME} ${SERVICE}

run:
	./scripts/run-services.sh ${SERVICE}


# Mục để kiểm tra migration status
status:
	@echo "Checking migration status..."
	dotnet ef migrations list

# Mục để chạy Docker container cho môi trường phát triển
dev:
	docker compose -f docker-compose.yaml -f docker-compose.database.yaml -f docker-compose.dev.yaml up -d ${SERVICE}

dev-build:
	docker compose -f docker-compose.yaml -f docker-compose.database.yaml -f docker-compose.dev.yaml up -d --build ${SERVICE}

staging:
	./scripts/deploy.sh $(SERVICE)
# Mục để tắt Docker container và xóa volume
clean:
	@echo "Stopping Docker containers and removing volumes..."
	docker compose -f docker-compose.yaml -f docker-compose.database.yaml -f docker-compose.dev.yaml down --remove-orphans -v
external:
	docker compose -f docker-compose.s3.yaml -f docker-compose.extension.yaml up -d
# Mục để chỉ tắt Docker container mà không xóa volume
down:
	@echo "Stopping Docker containers..."
	docker compose -f docker-compose.yaml -f docker-compose.database.yaml -f docker-compose.dev.yaml -f docker-compose.s3.yaml down
stop:
	@echo "Stopping Docker containers..."
	docker compose -f docker-compose.yaml -f docker-compose.database.yaml -f docker-compose.dev.yaml -f docker-compose.s3.yaml stop
ssh:
	ssh -p $(REMOTE_SERVER_PORT) $(REMOTE_SERVER_USER)@$(REMOTE_SERVER_IP)
backup: 
	@echo "Backup database..."
	./scripts/pgdump.sh backup
restore: 
	./scripts/pgdump.sh restore

sql:
	./scripts/pgdump.sh sql
sql_to_server:
	./scripts/pgdump.sh sql_to_server
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
	@echo "  make dev-build                 # Build and start local development containers"
	@echo "  make staging                   # Pull and start all prebuilt staging images"
	@echo "  make staging SERVICE=auth      # Pull and restart selected staging services"
	@echo "  make deploy SERVICE=auth       # Run the local staging-host deployment helper"
	@echo "  make frontend-install          # Install frontend dependencies"
	@echo "  make frontend-dev              # Start the Next.js development server"
	@echo "  make frontend-check            # Generate, typecheck, lint, and build frontend"
	@echo "  make backend-build             # Build the .NET solution in Release mode"
	@echo "  make backend-test              # Run backend unit tests"
	@echo "  make backend-test-all          # Run all backend tests (requires PostgreSQL)"
	@echo "  make check                     # Run the repository CI-equivalent checks"
	@echo "  make clean                     # Stop Docker containers and remove volumes"
	@echo "  make down                      # Stop Docker containers"
