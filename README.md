# VietWash Backend - Laundry Shop Management System 🚀

The **VietWash Backend** is a **microservices-based** system built with **.NET 8** to streamline laundry shop operations, including authentication, order processing, inventory, financial reporting, and branch management. It integrates seamlessly with the **VietWash frontend** (Next.js 14) and supports features like user authentication, service management, and reporting. The architecture is modular, leveraging **CQRS**, **MediatR**, and **Entity Framework Core** for robust data handling and scalability. 🛠️

## Table of Contents

- [Features](#features-✨)
- [Prerequisites](#prerequisites-📋)
- [Setup for Windows](#setup-for-windows-🪟)
- [Installation](#installation-🛠️)
- [Available Scripts (via Makefile)](#available-scripts-via-makefile-📜)
- [Project Structure](#project-structure-📂)
- [Technologies](#technologies-🛠️)
- [Deployment](#deployment-🚀)
- [Database Migrations](#database-migrations-🗄️)
- [Backup and Restore](#backup-and-restore-💾)
- [Testing](#testing-🧪)
- [Basic Usage](#basic-usage-🚀)
  - [Step-by-Step Run](#step-by-step-run)
  - [Filtering](#filtering-📊)
- [Contributing](#contributing-🤝)
- [License](#license-📜)

## Features ✨

- **ApiGateway** 🌐: Centralized entry point for routing requests with API key validation and device detection.
- **AuthService** 🔒: Manages user authentication (login, logout, OTP verification, password reset, account management).
- **EcommerceService** 🛍️: Handles core business logic for orders, products, services, categories, suppliers, tariffs, and financial reporting.
- **FinanceService** 💰: Manages financial transactions, funds, and fund behaviors.
- **ProjectService** 🏬: Manages branches and warehouses for the laundry chain.
- **Internationalization** 🌍: Supports English and Vietnamese translations (`Message.en.resx`, `Message.vi.resx`).
- **Event-Driven Architecture** 📤: Utilizes domain events (e.g., `UserCreateEvent`, `CreateFundEvent`) for asynchronous communication.
- **Database Management** 🗄️: Uses **PostgreSQL** with migrations for schema management.
- **File Storage** 📁: Integrates with **Minio S3** for media uploads (`nginx.conf` in `nginxs/MinioS3`).
- **Background Jobs** ⏰: Scheduled tasks for user status updates and queue processing (e.g., `UpdateUserStatusJob`).
- **Testing** 🧪: Includes unit and integration tests for robust validation of services.

## Prerequisites 📋

Ensure the following are installed:

- **.NET 8 SDK** 🖥️: Required for building and running the application.
- **Docker** 🐳: For containerized deployment and services (PostgreSQL, Minio S3).
- **PostgreSQL** 🐘: Database for all microservices.
- **make** ⚙️: To run commands defined in the `Makefile` (see Windows setup below).
- **Git** 📚: For version control.

## Setup for Windows 🪟

To run `Makefile` commands on Windows, install **MSYS2** for a Unix-like environment.

1. **Download and Install MSYS2**:
   - Get the installer from [https://www.msys2.org/](https://www.msys2.org/).
   - Follow the installation instructions.

2. **Install Required Packages**:
   - Open the MSYS2 terminal (`MSYS2 MSYS` from the Start menu).
   - Install `make`:
     ```bash
     pacman -S --needed make
     ```

3. **Add MSYS2 to PATH**:
   - Add `C:\msys64\usr\bin` to the system `PATH` environment variable:
     - Go to **Control Panel** > **System and Security** > **System** > **Advanced system settings** > **Environment Variables**.
     - Edit the `PATH` variable under **System Variables**.
   - Verify installation:
     ```bash
     make --version
     ```

## Installation 🛠️

1. **Clone the Repository**:
   ```bash
   git clone <repository-url>
   cd micro
   ```

2. **Set Up Environment Variables**:
   - Copy `.env.example` to `.env` and configure variables (e.g., database connections, S3 credentials).
     ```bash
     cp .env.example .env
     ```

3. **Install Dependencies**:
   ```bash
   dotnet restore Micro.sln
   ```

4. **Run Database Migrations**:
   ```bash
   make migration NAME=AuthDb
   make migration NAME=ProductDb
   ```

## Available Scripts (via Makefile) 📜

The `Makefile` provides the following commands:

- **🔄 `make migration NAME=<db>`**: Runs migrations for the specified database (e.g., `AuthDb`, `ProductDb`).
  ```bash
  make migration NAME=AuthDb
  ```

- **🔄 `make update`**: Updates database migrations.
  ```bash
  make update
  ```

- **📊 `make status`**: Checks migration status.
  ```bash
  make status
  ```

- **🚀 `make dev`**: Starts Docker containers for development (PostgreSQL, microservices).
  ```bash
  make dev
  ```

- **🛠️ `make dev-build`**: Builds and starts Docker containers for development.
  ```bash
  make dev-build
  ```

- **🌐 `make staging`**: Builds and starts Docker containers for staging.
  ```bash
  make staging
  ```

- **🧹 `make clean`**: Stops Docker containers and removes volumes.
  ```bash
  make clean
  ```

- **🛑 `make down`**: Stops Docker containers without removing volumes.
  ```bash
  make down
  ```

- **⏹️ `make stop`**: Stops Docker containers.
  ```bash
  make stop
  ```

- **📦 `make external`**: Starts external services (e.g., Minio S3).
  ```bash
  make external
  ```

- **💾 `make backup`**: Backs up the database.
  ```bash
  make backup
  ```

- **🔄 `make restore`**: Restores the database from a backup.
  ```bash
  make restore
  ```

- **📄 `make sql`**: Generates SQL dump for the database.
  ```bash
  make sql
  ```

- **🚀 `make deploy SERVICE=<service>`**: Deploys the specified service.
  ```bash
  make deploy SERVICE=AuthService
  ```

- **📦 `make publish`**: Publishes the application.
  ```bash
  make publish
  ```

- **📖 `make help`**: Displays available Makefile commands.
  ```bash
  make help
  ```

- **🧪 `make test [TYPE=<type>] [SERVICE=<service>] [NAME=<test-name>]`**: Runs unit and integration tests. Optionally specify `TYPE` (e.g., `UnitTest`, `IntegrationTest`), `SERVICE` (e.g., `AuthService`), and `NAME` (e.g., `CreateAccountTest`) to filter tests.
  ```bash
  make test
  make test TYPE="IntegrationTest" SERVICE="AuthService"
  make test TYPE="IntegrationTest" SERVICE="AuthService" NAME="CreateAccountTest"
  ```

## Project Structure 📂

The backend is organized into microservices with a shared kernel, contracts, and a dedicated test directory:

```
micro/
├── 📁 backup/                        # Database backups
├── 📁 nginxs/                        # Nginx configuration for Minio S3
├── 📁 scripts/                       # Utility scripts for deployment, migrations, and backups
├── 📁 src/                           # Source code for microservices
│   ├── 📁 ApiGateway/                # API Gateway for routing and request validation
│   │   ├── 📁 AppCheck/             # API key validation and device detection
│   │   ├── 📁 Properties/           # Configuration (e.g., launchSettings.json)
│   │   ├── 📜 appsettings*.json     # Environment-specific configurations
│   │   ├── 📜 Dockerfile
│   │   └── 📜 Program.cs
│   ├── 📁 AuthService/              # Authentication and user management
│   │   ├── 📁 Application/          # CQRS commands and queries (e.g., Login, OTP)
│   │   ├── 📁 Domain/               # Domain models and events (e.g., Account, UserCreateEvent)
│   │   ├── 📁 Infrastructure/        # Database context, migrations, and services
│   │   ├── 📁 Presentation/         # API endpoints and translations
│   │   ├── 📜 Dockerfile
│   │   └── 📜 .dockerignore
│   ├── 📁 Contracts/                # Shared interfaces, DTOs, and service contracts
│   │   ├── 📁 Dtos/                 # Data Transfer Objects
│   │   ├── 📁 Interfaces/           # Service interfaces (e.g., gRPC, API contracts)
│   │   └── 📁 Events/               # Shared event definitions
│   ├── 📁 EcommerceService/         # Core business logic (orders, products, services)
│   │   ├── 📁 Application/          # CQRS for orders, products, reports
│   │   ├── 📁 Domain/               # Models for orders, products, tariffs
│   │   ├── 📁 Infrastructure/        # Database context and migrations
│   │   ├── 📁 Presentation/         # API endpoints for ecommerce
│   │   ├── 📜 Dockerfile
│   │   └── 📜 .dockerignore
│   ├── 📁 FinanceService/           # Financial transactions and funds
│   │   ├── 📁 Application/          # CQRS for funds and behaviors
│   │   ├── 📁 Domain/               # Fund and transaction models
│   │   ├── 📁 Infrastructure/        # Database context and migrations
│   │   ├── 📁 Presentation/         # API endpoints for financial operations
│   │   ├── 📜 Dockerfile
│   │   └── 📜 .dockerignore
│   ├── 📁 ProjectService/           # Branch and warehouse management
│   │   ├── 📁 Application/          # CQRS for branches and warehouses
│   │   ├── 📁 Domain/               # Branch and warehouse models
│   │   ├── 📁 Infrastructure/        # Database context and migrations
│   │   ├── 📁 Presentation/         # API endpoints for branch/warehouse operations
│   │   ├── 📜 Dockerfile
│   │   └── 📜 .dockerignore
│   ├── 📁 Shared.Kernel/            # Shared utilities (specifications, entities, extensions)
│   │   ├── 📁 Common/               # Base entities, specifications, and converters
│   │   ├── 📁 Exceptions/           # Custom exception handling
│   │   └── 📁 Extentions/           # Utility extensions
├── 📁 tests/                        # Unit and integration tests
│   ├── 📁 IntegrationTest/          # Integration tests for services
│   ├── 📁 UnitTest/                 # Unit tests for services
├── 📁 .config/                      # Tool configurations
├── 📁 .github/workflows/            # CI/CD workflows (deploy.yml, dotnet.yml)
├── 📁 .husky/                       # Git hooks for pre-commit checks
├── 📜 docker-compose*.yaml          # Docker Compose configurations
├── 📜 Makefile                      # Automation scripts
├── 📜 .env.example                  # Example environment variables
├── 📜 .dockerignore                 # Docker ignore file
├── 📜 .gitignore                    # Git ignore file
├── 📜 LICENSE                       # MIT License
└── 📜 README.md                     # Project documentation
```

## Technologies 🛠️

- **.NET 8** ⚙️: Core framework for microservices.
- **Entity Framework Core** 🗃️: ORM for database operations with PostgreSQL.
- **MediatR** 📨: Implements CQRS pattern.
- **Minio S3** 📂: File storage for media uploads.
- **Hangfire** ⏲️: Background job processing.
- **Redis** 🗄️: Distributed caching (`RedisCacheService`).
- **gRPC** 📡: Inter-service communication (`QueueLogService predisposition ServiceHandler`).
- **OpenTelemetry** 📊: Monitoring and tracing (`OpenTelemetrySettings`).
- **Serilog** 📜: Structured logging (`SerilogSettings`).
- **Docker** 🐳: Containerization for services.
- **Nginx** 🌐: Reverse proxy for Minio S3.
- **PostgreSQL** 🐘: Primary database with migrations.
- **AWS S3** ☁️: Optional cloud storage (`AmazonS3Extension`).

## Deployment 🚀

Deploy using **Docker Compose** for container orchestration:

1. Configure `.env` variables.
2. Build and start services:
   ```bash
   make dev-build
   ```
3. For staging:
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

## Database Migrations 🗄️

Each microservice has its own database context and migrations:

- Run migrations:
  ```bash
  make migration NAME=AuthDb
  ```
- Check status:
  ```bash
  make status
  ```
- Update migrations:
  ```bash
  make update
  ```

## Backup and Restore 💾

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

## Testing 🧪

Run unit and integration tests using the `make test` command, which executes the `run_tests.sh` script. You can filter tests by `TYPE` (e.g., `UnitTest`, `IntegrationTest`), `SERVICE` (e.g., `AuthService`), and `NAME` (e.g., `CreateAccountTest`).

```bash
make test
make test TYPE="IntegrationTest" SERVICE="AuthService"
make test TYPE="IntegrationTest" SERVICE="AuthService" NAME="CreateAccountTest"
```

The test suite includes:
- **Unit Tests**: Validation of individual components (e.g., `CreateAccountCommandValidatorTest.cs`).
- **Integration Tests**: End-to-end testing of services (e.g., `CreateAccountHandlerTest.cs`).

## Basic Usage 🚀

### Step-by-Step Run

1. Open a terminal in the project root.
2. Start Redis and database:
   ```bash
   make dev SERVICE="redis database"
   ```
3. Start external services:
   ```bash
   make external
   ```
4. Run the project.

### Filtering 📊

The system uses **LHS Brackets** for filtering queries. Example:

```
GET /api/accounts?accounts[birthDay][$gt]=1990-10-01
```

Supported operators:

| Operator      | Description                         |
|--------------|-------------------------------------|
| `$eq`        | Equal                              |
| `$eqi`       | Equal (case-insensitive)           |
| `$ne`        | Not equal                          |
| `$nei`       | Not equal (case-insensitive)       |
| `$in`        | Included in an array               |
| `$notin`     | Not included in an array           |
| `$lt`        | Less than                         |
| `$lte`       | Less than or equal to             |
| `$gt`        | Greater than                      |
| `$gte`       | Greater than or equal to          |
| `$between`   | Is between                        |
| `$notcontains` | Does not contain                |
| `$notcontainsi` | Does not contain (case-insensitive) |
| `$contains`  | Contains                          |
| `$containsi` | Contains (case-insensitive)       |
| `$startswith` | Starts with                      |
| `$endswith`  | Ends with                        |

Examples:

```
GET /api/accounts?filter[gender][$in][0]=Male&filter[gender][$in][1]=Female
```

```
GET /api/accounts?filter[displayName][$contains]=abc
```

**$and** and **$or** operators:

```
GET /api/accounts/filter[$and][0][displayName][$containsi]=sa&filter[$and][1][email][$eq]=thu@gmail.com
```

```json
{
  "filter": {
    "$and": {
      "displayName": { "$eq": "sa" },
      "email": { "$eq": "thu@gmail.com" }
    }
  }
}
```

## Contributing 🤝

1. Fork the repository.
2. Create a feature branch:
   ```bash
   git checkout -b feature/YourFeature
   ```
3. Commit changes:
   ```bash
   git commit -m "Add YourFeature"
   ```
4. Push to the branch:
   ```bash
   git push origin feature/YourFeature
   ```
5. Open a Pull Request.

## License 📜

This project is licensed under the **MIT License** (see `LICENSE` file).