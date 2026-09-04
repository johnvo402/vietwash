# VietWash

[![Backend CI](https://github.com/johnvo402/vietwash/actions/workflows/dotnet.yml/badge.svg?branch=dev)](https://github.com/johnvo402/vietwash/actions/workflows/dotnet.yml)
[![Frontend CI](https://github.com/johnvo402/vietwash/actions/workflows/frontend.yml/badge.svg?branch=dev)](https://github.com/johnvo402/vietwash/actions/workflows/frontend.yml)

VietWash is a multi-branch laundry operations platform. It connects the cashier counter to service pricing, machines, material stock and revenue reporting, with a Next.js web application and .NET microservices.

**FEATURE FROZEN — Core demo flow complete. Maintenance/security fixes only.**

## Core features

- Cashier order creation with server-authoritative pricing preview and persisted totals
- Customer eligibility, assigned vouchers and atomic voucher claims
- Same-branch equipment assignment and transactional material inventory consumption
- Order lifecycle: Pending → InProgress → Processed → Completed, with guarded cancellation
- Cash completion and completed-order revenue reports
- Multi-branch authorization for staff, managers and administrators
- Customer OTP registration and Auth-to-Ecommerce synchronization
- Service/tariff catalogs, inventory, suppliers, finance and real-time notifications

Operational payment is **Cash**. The PayOS implementation is retained but is not exposed in the normal operational UI and is not production-certified.

## Architecture

```text
Browser
   |
Nginx edge :80                 only host-published application port
   |             |
Next.js :3000   YARP Gateway ×2 :8080
                 |
       Auth / Ecommerce / Project / Finance / Notification
                 |
          PostgreSQL / Redis
```

Everything below the edge communicates over one project-scoped Docker bridge using service DNS names. The browser uses the same origin for pages, APIs, SignalR and `/image` media. The bridge permits outbound provider calls; it is not an egress sandbox. Each domain service owns a logical database.

See [architecture](docs/architecture.md) for service boundaries and authentication. The legacy spelling `src/EcommeceService` is retained for compatibility.

## Stack

| Layer | Technologies |
| --- | --- |
| Backend | .NET 8, ASP.NET Core, EF Core, YARP, CQRS/Mediator, gRPC, SignalR |
| Data | PostgreSQL/PostGIS, Redis, S3-compatible storage |
| Frontend | Next.js 15.5.25, React 18, TypeScript, Tailwind CSS, React Query, Zustand |
| Delivery | Docker Compose, Nginx, GitHub Actions, GHCR |
| Quality | xUnit, PostgreSQL integration tests, Playwright, ESLint |

## Development

Prerequisites: .NET 8 SDK, Node.js 22/npm, Java 17+ for API generation, Docker Compose, and optionally GNU Make/Bash.

```bash
git clone https://github.com/johnvo402/vietwash.git
cd vietwash
cp .env.example .env
cp frontend/.env.example frontend/.env.local
dotnet restore Micro.sln --configfile NuGet.Config
dotnet build Micro.sln -c Release --no-restore
make frontend-check
```

Start local infrastructure, then run backend services and frontend in separate terminals:

```bash
make dev SERVICE="database redis pgadmin"
make external                              # optional local S3/observability support
make run SERVICE="auth ecommerce project finance notification"
dotnet run --project src/ApiGateway/ApiGateway.csproj
make frontend-dev
```

The development overlay binds Gateway `5000`, PostgreSQL `5432`, Redis `6379` and pgAdmin `5050` to **localhost only**. Frontend development remains `npm run dev` on `3000`, with `NEXT_PUBLIC_API_URL=http://localhost:5000`. Configure Development connection/provider settings for these local dependencies. See the [existing seed/startup caveats](docs/closure-verification.md#demo-data-and-existing-setup-caveats) before preparing a fresh demo database.

Without Make, frontend checks are:

```bash
cd frontend
npm ci
npm run generate
npm run typecheck
npm run lint
npm run build:test
```

## Staging: one public entrypoint

Only Nginx edge is host-exposed. Frontend, both gateways, all microservices, PostgreSQL and Redis stay inside the Docker network. The standard stack contains exactly:

```text
database redis auth ecommerce project finance notification
gateway gateway-2 frontend edge
```

The host needs Docker Compose, the repository's Compose/configuration files, an untracked `.env`, and Bash for the helper (Make optional). **No npm, .NET SDK or Java is needed on the staging host.**

1. Copy `.env.example` to `.env`. Replace database, Redis and storage placeholders; set a unique `STAGING_JWT_SECRET`, a 32-character `STAGING_ENCRYPTION_KEY`, a 16-character `STAGING_ENCRYPTION_IV`, and `STAGING_JOBS_PASSWORD`. Never expose demo credentials to the internet.
2. Configure the existing private S3 provider with `S3_SERVICE_URL`, `MINIO_ACCESS_KEY` and `MINIO_SECRET_KEY`. The default `http://minio:9000` requires that DNS name on the application network. For the separate repository S3 support stack on the same network, use its API proxy `http://nginx:9000`. The bucket is `the-template-project`; non-secret defaults live in `deploy/staging/services.json`. Storage must be available before seed media initialization.
3. Select published images using `IMAGE_TAG` for backend and `FRONTEND_IMAGE_TAG` for frontend. Their immutable tags may differ because publication is independent. Log in to GHCR with a server-only read credential if packages are private.
4. Deploy:

```bash
make staging
# Same canonical helper without Make:
./scripts/deploy.sh
```

The helper pulls images and runs `up -d --no-build --pull never` with only the base, database and staging Compose files. `make staging SERVICE="frontend edge"` selects services; `ENV_FILE=/path/to/staging.env` selects another environment file. `--no-pull` is only for offline verification of existing local images.

Use `sha-<12-character-commit>` tags for traceable deployment/rollback. The default edge binding is `80:80`; `STAGING_HTTP_PORT` can select another host port. HTTPS termination belongs to the existing external reverse proxy/load balancer; restrict direct edge access to that trusted proxy when using it. This repository does not provision certificates.

| Public path | Internal destination |
| --- | --- |
| `/`, including `/auth/login` | Next.js standalone |
| `/{Auth,Ecommerce,Project,Finance,Notification}/api/...` | Gateway → domain service |
| `/notification/hub` | Gateway, with WebSocket upgrades |
| `/image/...` | Existing S3 API through Gateway, not the console |
| `/health` | Gateway liveness, not full dependency readiness |

The frontend image builds with an empty `NEXT_PUBLIC_API_URL`. Never put Docker hostnames or credentials in `NEXT_PUBLIC_*`. It runs `node server.js` as non-root and excludes Java/OpenAPI generator tooling from runtime.

```bash
docker compose -f docker-compose.yaml -f docker-compose.database.yaml \
  -f docker-compose.staging.yaml config
docker compose -f docker-compose.yaml -f docker-compose.database.yaml \
  -f docker-compose.staging.yaml ps
curl http://localhost/
curl http://localhost/health
curl -i http://localhost/Ecommerce/api/Orders   # 401 without a token is expected
ss -lntp
```

Only `edge` may have a host binding. Internal `8080/tcp`, `3000/tcp`, `5432/tcp` and `6379/tcp` listings without a host mapping are expected. Direct host access must fail unless an unrelated process owns the port. Compose `config` expands secrets: do not publish its full output.

Seq, Jaeger, OTEL, pgAdmin and MinIO support stacks are **not** automatically included by `make staging`. Optional host bindings are localhost-only; use SSH tunnels/private networking for administration. Never publish MinIO console `9001` to the internet.

## Demo flow

Use a disposable, valid seeded database with an assigned STAFF, an active/non-disabled CUSTOMER, an active tariff/service, available same-branch equipment and sufficient material stock.

1. Log in as STAFF and open `/manage/cashier`.
2. Select customer, tariff and service; verify the server pricing preview.
3. Create the order: persisted **Pending** status and total.
4. Start with same-branch equipment: **InProgress** and one material export.
5. Mark **Processed**, then **Cash**: **Completed**. Reload to verify persistence.
6. As an authorized MANAGER/ADMIN, view revenue for that branch and completion date.

The live smoke creates real data without API mocks, injected authentication, CSP bypass or PayOS calls:

```bash
cd frontend
# Set E2E_EMAIL / E2E_PASSWORD for STAFF and
# E2E_REPORT_EMAIL / E2E_REPORT_PASSWORD for MANAGER/ADMIN.
E2E_BASE_URL=http://localhost:80 npx playwright test --config playwright.closure.config.ts
```

The default post-login dashboard is manager-only; STAFF should open the cashier directly. A legacy nested-modal navigation issue can require refreshing order detail after opening it from the cashier list. The smoke includes this refresh, without forced clicks.

## Verification and delivery

```bash
dotnet test tests/UnitTest/EcommerceService.Tests/EcommerceService.Tests.csproj \
  -c Release --no-restore
docker build -f frontend/Dockerfile -t vietwash-frontend:test .
git diff --check
```

Set `VIETWASH_SEED_TEST_DATABASE` to a **disposable local PostgreSQL database with citext and hstore** to enable database-dependent Ecommerce/Notification cases. Its name must start with `vietwash_seed_test`. Never use operational data. Maintenance validation passed **489 backend tests, zero skips**; [current verification](docs/maintenance-completion.md) and [historical closure evidence](docs/closure-verification.md) record scope and limitations.

- Backend CI provisions disposable PostgreSQL so transactional outbox/inbox regressions run. The existing six-image publishing workflow remains separate.
- Frontend CI runs install, generation, typecheck, lint and build. On `dev`, after checks, it publishes `ghcr.io/johnvo402/vietwash-frontend:sha-<12-character-commit>`, then promotes that image to `:dev`.
- Frontend publication is independent of backend changes. Pull requests run checks without publication; manual dispatch is available.
- Publishing does not automatically deploy a server. `make staging` remains the host-side command.

## Current status and known limitations

**Core demo flow: complete. Project status: feature-frozen / maintenance only.**

- Cash is operational; online-payment production certification is outside this release.
- Processed-order notifications use a transactional outbox and deduplicated persistent inbox. Live SignalR hints remain best-effort; reconnect refreshes unread data. Other Redis integrations are not covered by this outbox. Follow the [receiver-first rollout and monitoring guide](docs/maintenance-completion.md).
- Docker Compose is a demo/staging deployment, not database HA or a zero-downtime platform.
- Development DI and STAFF/modal navigation are fixed. Demo staff/manager branch assignments still need provisioning; fresh Staging intentionally does not populate the Ecommerce demo catalog. Development can run the existing initializer after identity synchronization and valid assignments.
- The former 40 npm findings are remediated; full and production-only audits currently report 0. See [dependency assessment](docs/frontend-dependency-audit.md) and [maintenance verification](docs/maintenance-completion.md), including the Next 15 Maintenance LTS follow-up window.
- Tokens retain the current tab-scoped `sessionStorage` contract. Browser client identifiers are public; JWT/backend authorization enforce access. Rotate historical/demo credentials before exposure.

## License

[MIT](LICENSE).
