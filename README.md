# VietWash Backend - Laundry Shop Management System

The VietWash backend is a **microservices-based** system built with **.NET 8** to manage laundry shop operations, including authentication, order processing, inventory, financial reporting, and branch management. It integrates with the VietWash frontend (built with Next.js 14) and supports features like user authentication, service management, and reporting. The architecture is modular, leveraging **CQRS**, **MediatR**, and **Entity Framework Core** for robust data handling and scalability.

## Features

- **ApiGateway**: Centralized entry point for routing requests to microservices with API key validation and device detection.
- **AuthService**: Manages user authentication, including login, logout, OTP verification, password reset, and account management.
- **EcommerceService**: Handles core business logic for orders, products, services, categories, suppliers, tariffs, and financial reporting.
- **FinanceService**: Manages financial transactions, funds, and fund behaviors.
- **ProjectService**: Manages branches and warehouses for the laundry chain.
- **Internationalization**: Supports English and Vietnamese translations (`Message.en.resx`, `Message.vi.resx`).
- **Event-Driven Architecture**: Utilizes domain events (e.g., `UserCreateEvent`, `CreateFundEvent`) for asynchronous communication.
- **Database Management**: Uses PostgreSQL with migrations for schema management.
- **File Storage**: Integrates with Minio S3 for media uploads (`nginx.conf` in `nginxs/MinioS3`).
- **Background Jobs**: Scheduled tasks for user status updates and queue processing (e.g., `UpdateUserStatusJob`).

## Prerequisites

Ensure you have the following installed:

- **.NET 8 SDK**: Required for building and running the application.
- **Docker**: For containerized deployment and services (e.g., PostgreSQL, Minio S3).
- **PostgreSQL**: Database for all microservices.
- **make**: To run commands defined in the `Makefile` (see Windows setup below for installation).
- **Git**: For version control.

## Setup for Windows

To run the `Makefile` commands on Windows, you need to install **MSYS2** to provide a Unix-like environment for `make`.

1. **Download and Install MSYS2**:

   - Download the MSYS2 installer from [https://www.msys2.org/](https://www.msys2.org/).
   - Follow the installation instructions to set up MSYS2 on your system.

2. **Install Required Packages**:

   - Open the MSYS2 terminal (e.g., `MSYS2 MSYS` from the Start menu).
   - Install the `make` package by running:
     ```bash
     pacman -S --needed make
     ```

3. **Add MSYS2 to PATH**:
   - Add the MSYS2 binary directory to your system's `PATH` environment variable:
     - Open **Control Panel** > **System and Security** > **System** > **Advanced system settings** > **Environment Variables**.
     - Under **System Variables**, find and edit the `PATH` variable.
     - Add the path `C:\msys64\usr\bin` (adjust if MSYS2 is installed in a different directory).
   - Verify the installation by opening a new Command Prompt or PowerShell and running:
     ```bash
     make --version
     ```

## Installation

1. Clone the repository:

   ```bash
   git clone <repository-url>
   cd micro
   ```

2. Set up environment variables:

   - Copy `.env.example` to `.env` and configure necessary variables (e.g., database connections, S3 credentials).

   ```bash
   cp .env.example .env
   ```

3. Install dependencies:

   ```bash
   dotnet restore Micro.sln
   ```

4. Run database migrations:
   ```bash
   make migration NAME=AuthDb
   make migration NAME=ProductDb
   ```

## Available Scripts (via Makefile)

The `Makefile` provides the following commands:

- **`make migration NAME=<db>`**: Runs migrations for the specified database (e.g., `AuthDb`, `ProductDb`).

  ```bash
  make migration NAME=AuthDb
  ```

- **`make update`**: Updates database migrations.

  ```bash
  make update
  ```

- **`make status`**: Checks the migration status.

  ```bash
  make status
  ```

- **`make dev`**: Starts Docker containers for development (PostgreSQL, microservices).

  ```bash
  make dev
  ```

- **`make dev-build`**: Builds and starts Docker containers for development.

  ```bash
  make dev-build
  ```

- **`make staging`**: Builds and starts Docker containers for staging.

  ```bash
  make staging
  ```

- **`make clean`**: Stops Docker containers and removes volumes.

  ```bash
  make clean
  ```

- **`make down`**: Stops Docker containers without removing volumes.

  ```bash
  make down
  ```

- **`make stop`**: Stops Docker containers.

  ```bash
  make stop
  ```

- **`make external`**: Starts external services (e.g., Minio S3).

  ```bash
  make external
  ```

- **`make backup`**: Backs up the database.

  ```bash
  make backup
  ```

- **`make restore`**: Restores the database from a backup.

  ```bash
  make restore
  ```

- **`make sql`**: Generates SQL dump for the database.

  ```bash
  make sql
  ```

- **`make deploy SERVICE=<service>`**: Deploys the specified service.

  ```bash
  make deploy SERVICE=AuthService
  ```

- **`make publish`**: Publishes the application.

  ```bash
  make publish
  ```

- **`make help`**: Displays available Makefile commands.
  ```bash
  make help
  ```

## Project Structure

The backend is organized into microservices with a shared kernel and contracts for inter-service communication:

```
micro/
├── backup/                        # Database backups
├── nginxs/                        # Nginx configuration for Minio S3
│   └── MinioS3/nginx.conf
├── scripts/                       # Utility scripts for deployment, migrations, and backups
│   ├── docker-entrypoint-initdb.d/ # Database initialization scripts
│   ├── deploy.sh
│   ├── get-version.sh
│   ├── pgdump.sh
│   ├── publish.sh
│   ├── run_migration.sh
│   └── update-db.sh
├── src/
│   ├── ApiGateway/                # API Gateway for routing and request validation
│   │   ├── AppCheck/              # API key validation and device detection
│   │   ├── Properties/            # Configuration (e.g., launchSettings.json)
│   │   ├── appsettings*.json      # Environment-specific configurations
│   │   ├── Dockerfile             # Docker configuration
│   │   └── Program.cs             # Entry point
│   ├── AuthService/               # Authentication and user management
│   │   ├── Application/           # CQRS commands and queries (e.g., Login, OTP)
│   │   ├── Domain/                # Domain models and events (e.g., Account, UserCreateEvent)
│   │   ├── Infrastructure/         # Database context, migrations, and services (e.g., SmsOtpClient)
│   │   ├── Presentation/          # API endpoints and translations (en/vi)
│   │   ├── Dockerfile
│   │   └── .dockerignore
│   ├── Contracts/                 # Shared interfaces, DTOs, and service contracts
│   │   ├── Dtos/                  # Data Transfer Objects for inter-service communication
│   │   ├── Interfaces/            # Service interfaces (e.g., gRPC or API contracts)
│   │   └── Events/                # Shared event definitions (e.g., domain event contracts)
│   ├── EcommerceService/          # Core business logic (orders, products, services)
│   │   ├── Application/           # CQRS for orders, products, reports, etc.
│   │   ├── Domain/                # Models for orders, products, tariffs, suppliers
│   │   ├── Infrastructure/         # Database context, migrations, and services
│   │   ├── Presentation/          # API endpoints for ecommerce operations
│   │   ├── Dockerfile
│   │   └── .dockerignore
│   ├── FinanceService/            # Financial transactions and funds
│   │   ├── Application/           # CQRS for funds and fund behaviors
│   │   ├── Domain/                # Fund and transaction models
│   │   ├── Infrastructure/         # Database context and migrations
│   │   ├── Presentation/          # API endpoints for financial operations
│   │   ├── Dockerfile
│   │   └── .dockerignore
│   ├── ProjectService/            # Branch and warehouse management
│   │   ├── Application/           # CQRS for branches and warehouses
│   │   ├── Domain/                # Branch and warehouse models
│   │   ├── Infrastructure/         # Database context and migrations
│   │   ├── Presentation/          # API endpoints for branch/warehouse operations
│   │   ├── Dockerfile
│   │   └── .dockerignore
│   ├── Shared.Kernel/             # Shared utilities (specifications, entities, extensions)
│   │   ├── Common/                # Base entities, specifications, and converters
│   │   ├── Exceptions/            # Custom exception handling
│   │   └── Extentions/            # Utility extensions (e.g., ExpressionExtensions)
├── .config/                       # Tool configurations
├── .github/workflows/             # CI/CD workflows (deploy.yml, dotnet.yml)
├── .husky/                        # Git hooks for pre-commit checks
├── docker-compose*.yaml           # Docker Compose configurations (dev, staging, database, S3)
├── Makefile                       # Automation scripts for development and deployment
├── .env.example                   # Example environment variables
├── .dockerignore                  # Docker ignore file
├── .gitignore                     # Git ignore file
└── README.md                      # Project documentation
```

## Technologies

- **.NET 8**: Core framework for building microservices.
- **Entity Framework Core**: ORM for database operations with PostgreSQL.
- **MediatR**: Implements CQRS pattern for command and query handling.
- **Minio S3**: File storage for media uploads.
- **Hangfire**: Background job processing (e.g., `UpdateUserStatusJob`).
- **Redis**: Distributed caching (`RedisCacheService`).
- **gRPC**: Inter-service communication (`QueueLogServiceHandler`).
- **OpenTelemetry**: Monitoring and tracing (`OpenTelemetrySettings`).
- **Serilog**: Structured logging (`SerilogSettings`).
- **Docker**: Containerization for services and dependencies.
- **Nginx**: Reverse proxy for Minio S3.
- **PostgreSQL**: Primary database with migrations for schema management.
- **AWS S3**: Optional cloud storage integration (`AmazonS3Extension`).

## Deployment

The backend is deployed using **Docker Compose** for container orchestration. To deploy:

1. Configure environment variables in `.env`.
2. Build and start services:
   ```bash
   make dev-build
   ```
3. For staging, use:
   ```bash
   make staging
   ```
4. Deploy specific services:
   ```bash
   make deploy SERVICE=AuthService
   ```
5. External services (e.g., Minio S3):
   ```bash
   make external
   ```

CI/CD pipelines are defined in `.github/workflows/` for automated deployment.

## Database Migrations

Each microservice (AuthService, EcommerceService, FinanceService, ProjectService) has its own database context and migrations:

- Run migrations for a specific database:
  ```bash
  make migration NAME=AuthDb
  ```
- Check migration status:
  ```bash
  make status
  ```
- Update migrations:
  ```bash
  make update
  ```

## Backup and Restore

- Backup the database:
  ```bash
  make backup
  ```
- Restore from a backup:
  ```bash
  make restore
  ```
- Generate SQL dump:
  ```bash
  make sql
  ```

## Contributing

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -m "Add YourFeature"`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Open a Pull Request.

## License

This project is licensed under the MIT License (see `LICENSE` file).
