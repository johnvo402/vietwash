# Project closure verification

Date: 2026-09-03. Baseline: `fdfb17cb1f794cb9c26b65cdd8a27fdabdc97a3b`, branch `dev`, plus this closure patch. This is local verification, not a claim that the new workflow has already published or deployed remotely.

## Build and regression checks

| Check | Result |
| --- | --- |
| `dotnet restore Micro.sln --configfile NuGet.Config` | Pass |
| `dotnet build Micro.sln -c Release --no-restore` | Pass; existing compiler warnings remain |
| Ecommerce tests, PostgreSQL cases enabled | **409 passed, 0 failed, 0 skipped** |
| Focused cashier/customer/voucher tests | 49 passed |
| `npm ci` / `generate` / `typecheck` / `lint` / `build:test` | Pass |
| `docker build -f frontend/Dockerfile -t vietwash-frontend:test .` | Pass |
| Standalone runtime inspection | UID 1001; no Java, TypeScript package or OpenAPI tooling; no public/static source maps found |
| All six backend images built from local closure code | Pass |
| `nginx -t`, actual edge HTTP and live UI flow | Pass |

Dependency audit output is not a clean bill of health: `npm ci` reported 40 findings (5 low, 9 moderate, 26 high). Existing generator/compiler/Browserslist warnings were not turned into a framework-wide upgrade project.

## Staging network proof

Used the canonical `scripts/deploy.sh --no-pull` with an ignored, test-only environment file and locally prebuilt images. The `--no-pull` switch avoided publishing temporary images merely to test the deployment command. Normal `make staging` pulls first.

Project namespace: `vietwash-closure-20260903`. The merged base + database + staging configuration contained exactly 11 standard services. With the normal port value, the only publication was:

```text
edge: 80 -> 80
```

The isolated test selected `STAGING_HTTP_PORT=18080`. Running containers showed:

```text
edge           0.0.0.0:18080->80/tcp
frontend       3000/tcp
gateway        8080/tcp
gateway-2      8080/tcp
auth           8080/tcp
ecommerce      8080/tcp
project        8080/tcp, 8443/tcp
finance        8080/tcp
notification   8080/tcp, 8444/tcp
database       5432/tcp
redis          6379/tcp
```

`docker inspect`/Compose and Windows `Get-NetTCPConnection` were used as the host-side equivalent of `ss -lntp`:

- Edge `/`: HTTP 200, Next.js page.
- Edge `/health`: HTTP 200, `{"status":"ok","service":"gateway"}`.
- Edge `/Ecommerce/api/Orders`: HTTP 401 without credentials; authenticated operations returned 200 during the live flow.
- Direct host PostgreSQL `5432` and Redis `6379`: connection refused.
- No stack-owned direct frontend, gateway, microservice or admin port mapping.
- Host `8080` belonged to the unrelated `factory-mind-prod-frontend-1` container. Host `3000` belonged to a pre-existing Next.js process started on September 2. Neither was created, modified or stopped for this test; neither was a direct binding of the staging stack.

An additional temporary MinIO fixture was reachable by Docker DNS only, with **no host ports**. A separate disposable PostgreSQL instance on localhost `55439` was used solely for unit/integration tests, not the staging stack. Optional support stacks were not merged into the standard deployment.

## Live core flow

Run with `frontend/playwright.closure.config.ts`. The test logs in through the UI using real credentials from environment variables. No API mocks, synthetic auth state, forced clicks or CSP bypass are used. The Codex in-app browser runtime was unavailable on this host, so the same real UI was exercised with the repository's Playwright CLI.

The final recorded run completed in about seven seconds:

```text
STAFF login → cashier → valid CUSTOMER → tariff/service → server preview
→ Create/Pending → same-branch equipment → InProgress/material export
→ Processed → Cash/Completed → persisted reload → MANAGER revenue report
```

Evidence:

| Field | Observed value |
| --- | --- |
| Order | `OD426658` / `2046411781873792` |
| Branch | `1` |
| Server preview and persisted total | `8,800 VND` |
| Final persisted status/payment | Completed / Cash |
| Equipment | `2046407839308367`, branch 1, released after completion |
| Customer | Role CUSTOMER, status Active, disabled false |
| Source-order material export | `2046411787694208`, Completed, branch 1, exactly one document |
| Material lines | Omo `-0.10`; Downy `-0.05` in their configured units |
| Report, UTC financial date 2026-09-03 / branch 1 | 5 completed smoke orders, gross 40,000 VND, collected 44,000 VND |
| Browser API origin | `http://localhost:18080` only |
| Actual SignalR socket, receiving frames | `ws://localhost:18080/notification/hub` |
| Media/PWA | Avatar loaded through edge `/image`; service worker activated |
| PayOS calls | None |

Earlier test-script iterations also created orders in this disposable database, which is why the report contains five completed orders. The final test itself creates and completes one order; the report total is not represented as revenue from that order alone.

The post-run SQL checks were read-only and confirmed persisted order/customer/equipment state and its material export. To repeat for another test order, use its ID in:

```sql
SELECT o.id, o.code, o.status, o.total, o.payment_method, o.order_date,
       u.role, u.status AS customer_status, u.disabled,
       e.id AS equipment_id, e.branch_id, e.using
FROM "order" o
JOIN "user" u ON u.id = o.customer_id
JOIN order_equipment oe ON oe.order_id = o.id
JOIN equipment e ON e.id = oe.equipment_id
WHERE o.id = 2046411781873792;

SELECT d.id, d.source_order_id, d.branch_id, d.status, d.type,
       p.product_id, p.quantity
FROM inventory_document d
JOIN product_supplying p ON p.inventory_document_id = d.id
WHERE d.source_order_id = 2046411781873792;
```

The test produces cashier/completed/report screenshots and a redacted JSON proof attachment under ignored `frontend/test-results`. Traces from failures can contain temporary login credentials/tokens; do not publish them or reuse their credentials.

## Demo data and existing setup caveats

This run used a fresh isolated stack with **valid prepared seed data**, not an unmodified one-command fresh staging bootstrap:

- Auth/Project initialization populated identities/branches; their normal integration path synchronized Ecommerce data.
- The existing Ecommerce Development initializer populated tariffs, service prices, equipment and material stock through an isolated one-shot harness. Seed orders did not fire operational lifecycle events.
- Explicit fixture branch assignments were needed for STAFF/MANAGER accounts in both Auth and Ecommerce. The stock account seed does not assign these roles automatically. In a maintained demo environment, provision these assignments through its normal administration workflow before running the smoke.
- Starting the entire Ecommerce host under Development exposed an existing DI lifetime validation problem: a singleton audit interceptor depends on scoped `ICurrentAccount`. The one-shot seed harness ran the unchanged initializer without that host validation. This patch does **not** claim that Development-host startup issue is fixed. Restore a prepared demo database or address that separate maintenance issue before relying on a fresh local bootstrap.
- The default STAFF landing page requests manager-only dashboard endpoints and reaches 403. The smoke then opens the authorized cashier. Revenue is viewed through a real MANAGER login, not by relaxing roles.
- Opening detail from the cashier's nested modal can leave pointer interaction locked. The smoke reloads the persisted detail page before starting; this is a documented manual workaround, not a forced click or a hidden UI fix.

Keep fixtures, local secrets and evidence artifacts out of Git. Only the closure-owned temporary containers/data are cleaned up after verification; unrelated host applications remain untouched.

## Closure status

**FEATURE FROZEN. Core demo flow complete, with the setup/navigation caveats above. Maintenance/security fixes only.**

No distributed outbox, online-payment certification, guaranteed notification delivery, HA or zero-downtime deployment is claimed. `/health` is Gateway liveness. GHCR publication and real-server deployment must run after this patch is committed/pushed and the workflows succeed.
