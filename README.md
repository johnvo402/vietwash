# VietWash

[![Backend CI](https://github.com/johnvo402/vietwash/actions/workflows/dotnet.yml/badge.svg?branch=dev)](https://github.com/johnvo402/vietwash/actions/workflows/dotnet.yml)
[![Frontend CI](https://github.com/johnvo402/vietwash/actions/workflows/frontend.yml/badge.svg?branch=dev)](https://github.com/johnvo402/vietwash/actions/workflows/frontend.yml)

VietWash is a full-stack laundry operations platform for branches, orders, services, inventory, customers, finance, and staff workflows. The repository combines a Next.js 14 web application with a .NET 8 microservice backend behind a YARP API Gateway.

Live application: [vietwash.vercel.app](https://vietwash.vercel.app)

## Core capabilities

- Role-aware authentication, account administration, and branch selection
- Counter workflows for customers, orders, payments, and service pricing
- Inventory, material, supplier, warehouse, and equipment management
- Branch and organizational configuration
- Finance, fund, revenue, and operational reporting
- Real-time notifications through SignalR
- S3-compatible media storage, background jobs, caching, tracing, and structured logs

## Architecture

Browser traffic enters through the YARP gateway and is routed to independently bounded services. Each service owns its persistence concerns while shared contracts and kernel libraries hold cross-cutting primitives. PostgreSQL/PostGIS, Redis, MinIO, Hangfire, OpenTelemetry, Jaeger, and Seq support the application locally.

See [System Architecture](docs/architecture.md) for the Mermaid topology, service boundaries, request flow, and security decisions. The older PNG is retained only as historical context; the Mermaid document is the maintained source of truth and does not claim a Kubernetes deployment.

## Tech stack

| Area       | Technologies                                                                    |
| ---------- | ------------------------------------------------------------------------------- |
| Web        | Next.js 14, React 18, TypeScript, Tailwind CSS, React Query, Zustand, next-intl |
| Backend    | .NET 8, ASP.NET Core, YARP, Entity Framework Core, CQRS/Mediator, gRPC, SignalR |
| Data       | PostgreSQL/PostGIS, Redis, MinIO/S3                                             |
| Operations | Docker Compose, Hangfire, Serilog, OpenTelemetry, Jaeger, Seq                   |
| Quality    | xUnit, Playwright, ESLint, TypeScript, GitHub Actions                           |

## Repository structure

```text
.
├── frontend/                         Next.js application and Playwright tests
├── src/
│   ├── ApiGateway/                   YARP edge gateway
│   ├── AuthService/                  Identity, accounts, roles, and OTP
│   ├── EcommeceService/              Orders, catalog, inventory, and payments
│   ├── FinanceService/               Funds and financial transactions
│   ├── NotificationService/          Notifications and SignalR hub
│   ├── ProjectService/               Branches, warehouses, and organization data
│   ├── Contracts/                    Shared DTOs and service contracts
│   └── Shared.Kernel/                Shared domain and infrastructure primitives
├── tests/                            Backend unit and integration tests
├── docs/                             Maintained architecture documentation
├── .github/workflows/                Backend/frontend CI and image publishing workflows
├── docker-compose*.yaml              Application and supporting infrastructure
├── Directory.Packages.props          Central NuGet package versions
├── Makefile                          Monorepo developer commands
└── Micro.sln                         .NET solution
```

The legacy `EcommeceService` directory spelling is intentionally retained because solution, deployment, and Docker paths depend on it. The product and documentation use “Ecommerce Service.”

## Engineering decisions and tradeoffs

- The frontend is a feature-based modular application, not a separately deployed micro-frontend system.
- The gateway's `X-Api-Key` value is a public client identifier when enabled through route metadata. It is not a secret or an authorization boundary; JWT claims enforce application access.
- Gateway API-key enforcement is disabled in Development because no route has `ApiKeyRequired=true`. This avoids giving a browser-visible value false security meaning.
- The backend currently returns access and refresh tokens in JSON. The browser stores them in tab-scoped `sessionStorage`; moving refresh tokens to `HttpOnly`, `Secure`, `SameSite` cookies requires a coordinated backend contract change.
- Service boundaries and current CQRS/Mediator conventions are preserved to keep this cleanup low-risk. Mediator is kept on the stable 3.0 line to avoid unnecessary API churn.
- TypeScript typechecking and the Next.js Core Web Vitals rules are enforced. Three broad legacy lint rules remain disabled because the form-heavy codebase currently violates them widely; they should be enabled incrementally per feature instead of producing a noisy CI signal.

## Getting started

### Prerequisites

- .NET 8 SDK
- Node.js 20 and npm
- Java 17 or newer for OpenAPI Generator
- Docker with Docker Compose
- GNU Make and Bash for the convenience targets (optional on Windows)

### Clone and configure

```bash
git clone https://github.com/johnvo402/vietwash.git
cd vietwash
cp .env.example .env
cp frontend/.env.example frontend/.env.local
```

Edit both local environment files before starting infrastructure. Never commit production credentials. ASP.NET Core configuration can be overridden with double-underscore environment variables, for example `SecuritySettings__JwtSettings__SecretKey`, `S3AwsSettings__AccessKey`, `S3AwsSettings__SecretKey`, `OtpOption__ApiKey`, and `PayOsSetting__ApiKey`.

### Install and build

```bash
dotnet restore Micro.sln --configfile NuGet.Config
dotnet build Micro.sln --configuration Release --no-restore
make frontend-install
make frontend-check
```

Without Make:

```bash
cd frontend
npm ci
npm run generate
npm run typecheck
npm run lint
npm run build:test
```

### Run locally

Start the core data services:

```bash
make dev SERVICE="database redis"
make external
```

Run the five domain services in separate terminals or through the helper, then start the gateway:

```bash
make run SERVICE="auth ecommerce project finance notification"
dotnet run --project src/ApiGateway/ApiGateway.csproj
```

Start the web application in another terminal:

```bash
make frontend-dev
```

The default endpoints are the web application at `http://localhost:3000`, gateway at `http://localhost:5000`, MinIO at `http://127.0.0.1:9000`, and pgAdmin at `http://localhost:5050` when the Development Compose overlay is running.

## Common commands

| Command                             | Purpose                                                           |
| ----------------------------------- | ----------------------------------------------------------------- |
| `make frontend-install`             | Install locked frontend dependencies                              |
| `make frontend-dev`                 | Start Next.js in development mode                                 |
| `make frontend-check`               | Generate the API client, typecheck, lint, and build               |
| `make backend-build`                | Build the complete .NET solution in Release mode                  |
| `make backend-test`                 | Run every pure unit-test project under `tests/UnitTest`           |
| `make backend-test-all`             | Run all backend tests; PostgreSQL test infrastructure is required |
| `make check`                        | Run the CI-equivalent backend and frontend checks                 |
| `make dev SERVICE="database redis"` | Start selected local infrastructure                               |
| `make staging`                      | Pull and start the complete prebuilt staging stack                |
| `make staging SERVICE="auth gateway"` | Pull and restart selected prebuilt staging services            |
| `make deploy SERVICE=auth`          | Run the local staging-host pull/restart helper                    |
| `make down`                         | Stop the local Compose stack without deleting volumes             |

## Testing

```bash
dotnet test tests/UnitTest/AuthService.Tests/AuthService.Tests.csproj --configuration Release
dotnet test Micro.sln --configuration Release
```

`tests/UnitTest/AuthService.Tests/AuthService.Tests.csproj` is currently the only pure unit-test project. Backend CI discovers every project under `tests/UnitTest`; the full solution command also includes the database-dependent integration projects and needs PostgreSQL configured in `appsettings.Testing-Development.json`.

Playwright tests need a running backend plus non-committed test credentials:

```bash
cd frontend
E2E_EMAIL=user@example.test E2E_PASSWORD=change-me npm run test:run
```

## CI/CD

- Backend CI restores and builds `Micro.sln`, then discovers and runs every pure unit-test project under `tests/UnitTest`.
- Frontend CI installs from `package-lock.json`, generates the API client, typechecks, lints, and creates a production build.
- `Publish Backend Images` publishes all six backend application images for one commit. Matrix jobs first push immutable `sha-<12-character-commit>` tags; only after every build succeeds are the matching manifests promoted to the mutable `dev` tags.
- The workflow publishes images only. It does not SSH to a server or restart staging containers.

```text
push to dev
     |
     +--> Backend CI
     |
     `--> Publish Backend Images
              |
              +--> GHCR gateway
              +--> GHCR auth
              +--> GHCR ecommerce
              +--> GHCR project
              +--> GHCR finance
              `--> GHCR notification

staging host
     |
     +--> docker compose pull
     `--> docker compose up -d --no-build
```

The backend images are `ghcr.io/johnvo402/vietwash-{gateway,auth,ecommerce,project,finance,notification}`. Both `gateway` and `gateway-2` use the same gateway image. The `dev` tag is the current staging candidate; immutable `sha-*` tags provide traceable rollback points. The frontend remains a separate deployment and the public application is currently hosted on Vercel.

The staging host needs this repository's Compose files, an untracked `.env`, Docker, and Docker Compose. It does not need the .NET SDK and never builds backend images:

```bash
make staging
make staging SERVICE="auth ecommerce gateway"
IMAGE_TAG=sha-8c5ba35f6393 make staging
IMAGE_TAG=sha-8c5ba35f6393 ./scripts/deploy.sh
```

For public GHCR packages, staging pulls without authentication. GitHub may create a package as private initially; either change each package's visibility to public in GitHub Packages, or log in once on the server with a read-only `read:packages` credential stored only on that server. Never add the credential to `.env` or Compose.

GitHub Actions authenticates to GHCR with the repository-scoped `GITHUB_TOKEN`. Application credentials remain environment-specific ASP.NET Core configuration and must never use frontend `NEXT_PUBLIC_*` variables.

## Security notes

- Values prefixed with `NEXT_PUBLIC_` are embedded in the browser bundle and must be treated as public.
- Development keys in tracked configuration are local-only placeholders. Override every signing key, provider credential, database password, and storage credential outside Development.
- Credentials removed from Git history should be considered exposed and rotated at the provider; deleting the current value does not erase earlier commits.
- The reviewed frontend dependency-audit baseline and constrained findings are documented in [Frontend dependency audit](docs/frontend-dependency-audit.md).

## License

VietWash is available under the [MIT License](LICENSE).
