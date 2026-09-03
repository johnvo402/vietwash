# System Architecture

This document is the maintained architecture source for VietWash. It describes the current repository and Docker Compose topology; Kubernetes is not part of the checked-in deployment model.

## Runtime topology

```mermaid
flowchart LR
    User[Staff browser] --> Edge[Nginx edge / only host port 80]
    Edge --> Web[Next.js 14 standalone web app]
    Edge -->|same-origin API / SignalR| Gateway[YARP API Gateway x2]

    Gateway --> Auth[Auth Service]
    Gateway --> Ecommerce[Ecommerce Service]
    Gateway --> Project[Project Service]
    Gateway --> Finance[Finance Service]
    Gateway --> Notification[Notification Service]
    Gateway -->|/image proxy| MinIO[(MinIO / S3)]

    Auth --> AuthDb[(Auth PostgreSQL)]
    Ecommerce --> EcommerceDb[(Ecommerce PostgreSQL)]
    Project --> ProjectDb[(Project PostgreSQL)]
    Finance --> FinanceDb[(Finance PostgreSQL)]
    Notification --> NotificationDb[(Notification PostgreSQL)]

    Auth --> Redis[(Redis)]
    Ecommerce --> Redis
    Project --> Redis
    Finance --> Redis
    Notification --> Redis

    Auth -. shared contracts / selected gRPC .-> Project
    Project -. selected gRPC .-> Notification

    Auth --> Jobs[Hangfire workers]
    Ecommerce --> Jobs
    Project --> Jobs
    Finance --> Jobs
    Notification --> Jobs

    Auth --> OTel[OpenTelemetry Collector]
    Ecommerce --> OTel
    Project --> OTel
    Finance --> OTel
    Notification --> OTel
    OTel --> Jaeger[Jaeger traces]

    Auth --> Seq[Seq structured logs]
    Ecommerce --> Seq
    Project --> Seq
    Finance --> Seq
    Notification --> Seq
```

PostgreSQL is represented per service to show data ownership. Compose hosts those logical databases in one PostGIS container. Staging publishes only Nginx; frontend, gateways, services, PostgreSQL and Redis share a project-scoped Docker network without host ports. MinIO, Seq, Jaeger and OTEL shown here are optional supporting integrations, not part of the standard staging command. The frontend's relative API and media URLs return to the same Nginx origin.

## Service boundaries

| Component                 | Responsibility                                                                                                  |
| ------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Next.js web               | Role-aware staff UI, localization, client-side query/cache state, and generated API integration                 |
| API Gateway               | YARP routing, CORS, rate limiting, device/client metadata, and optional route-level client identification       |
| Auth Service              | Accounts, profiles, roles, authentication, refresh tokens, OTP, and account jobs                                |
| Ecommerce Service         | Orders, customers, products, services, pricing, suppliers, materials, and payments                              |
| Project Service           | Branches, warehouses, organization structure, and branch-scoped configuration                                   |
| Finance Service           | Funds, fund behavior, and financial transactions                                                                |
| Notification Service      | Notification persistence, unread state, server pushes, and the SignalR hub                                      |
| Contracts / Shared Kernel | Cross-service DTOs, integration contracts, common persistence helpers, telemetry, and infrastructure primitives |

## Request and authentication flow

```mermaid
sequenceDiagram
    actor Staff
    participant Web as Next.js web
    participant Gateway as YARP gateway
    participant Auth as Auth Service
    participant Domain as Domain service

    Staff->>Web: Submit credentials
    Web->>Gateway: POST /Auth/api/Accounts/Login (through Nginx)
    Gateway->>Auth: Forward request
    Auth-->>Web: Access token + refresh token
    Note over Web: Credentials are kept in tab-scoped sessionStorage
    Web->>Gateway: API request with Bearer token
    Gateway->>Domain: Route request
    Domain-->>Web: Domain response
    Web->>Gateway: One refresh request after a 401
    Gateway->>Auth: Refresh token
    Auth-->>Web: Rotated credentials
    Note over Web: Concurrent 401 responses share one refresh operation
```

The current JSON token contract prevents a frontend-only `HttpOnly` refresh-token implementation. A future migration should set and rotate the refresh token in an `HttpOnly`, `Secure`, `SameSite` cookie at Auth Service, add CSRF protections where needed, and return only short-lived access-token material to JavaScript.

## API gateway client identification

The gateway can validate `X-Api-Key` and `Platform` only for routes whose YARP metadata contains `ApiKeyRequired=true`. Development routes do not enable that metadata. The browser variable is consequently named `NEXT_PUBLIC_CLIENT_ID`: it documents that the value is public identification, not a secret and not authorization. JWT validation and domain permissions remain the security boundary.

## Configuration and operations

- Development uses explicit local placeholders in tracked settings. Real secrets belong in environment variables, .NET user secrets, or deployment secret stores.
- Domain-service health checks remain internal. Nginx proxies `/health` to Gateway liveness; this does not certify dependency readiness.
- Hangfire workers and dashboards are configured in the domain services.
- Optional OpenTelemetry/Seq support stacks use localhost/private administration. Standard staging uses console logs and does not expose their receivers/UIs.
- Docker Compose is the repository's declared local/staging orchestration. Infrastructure images use explicit stable tags to avoid silent major upgrades.

## Deliberate constraints

- Service boundaries and the existing CQRS/Mediator organization remain unchanged in this cleanup.
- The legacy `src/EcommeceService` path is preserved for solution and deployment compatibility.
- Backend Dockerfiles retain their existing runtime users because mounted log-directory ownership varies by deployment host. The new standalone frontend image runs as non-root.
- Integration tests depend on PostgreSQL and are kept separate from the fast pull-request unit-test gate.
