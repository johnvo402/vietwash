# Order runtime reliability and Cash-only operations

## Optional notification boundary

Previously, `UpdateStatusHandler` updated the order and released its equipment inside a transaction,
then called `SaveAsync`. The unchanged saved-changes interceptor dispatched `UpdateStatusOrderEvent`
before commit. The Processed event handler could throw from either its branch-name lookup or its
Notification gRPC call, which caused the primary transaction to roll back.

`UpdateStatusOrderEventHandler` now treats the entire preparation/delivery path as best-effort:

- Only Processed orders with a customer attempt notification delivery.
- Branch lookup receives the handler's cancellation token. Missing branch text falls back to the
  branch ID, rather than assigning null to the protobuf map.
- A `false` delivery result logs `Processed-order notification was not delivered` at Warning level.
- A preparation or delivery exception logs `Failed to send processed-order notification` at Error
  level, with the exception, and returns normally.
- Both failure logs include `OrderId`, `OrderCode`, `BranchId`, `CustomerId` and `Status`. They describe
  a notification failure, not an order-transition failure.

The domain lifecycle, equipment claim/release policy, global interceptor, material rules and
completion events remain unchanged. Caller cancellation and actual persistence errors are still
handled by the primary command. This is not an outbox: optional messages can be lost, and this patch
does not add delivery retries or accounting reconciliation.

## Cash-only Order UI

The normal operational payment entry points are:

| Surface | Cash UI |
| --- | --- |
| Order table action menu | `PaymentMethodSelect`: direct cash confirmation |
| Order card action menu | Same shared `OrderActionMenu` and confirmation |
| Manager order detail | `PaymentModal`: Cash only |
| Cashier order detail | Same shared detail and payment modal |
| Scan Order / QR modal | Existing QR scanner followed by Cash only for Processed orders |

The confirmation tells staff to receive the cash first. Cash is the single action, not a method
selector with a fake choice. Submission is guarded against repeated clicks, buttons are disabled
while pending, and dialog dismissal is blocked while the request is in progress. Backend failures
remain visible and do not optimistically change the order to Completed.
The detail/scan modal is bounded to the viewport with scrollable content. Regression checks cover
visible Cash/close controls on desktop and at 375 px, preventing the old minimum-content width
from pushing the close button offscreen.

Every entry point sends exactly:

```json
{"status":"Completed","paymentMethod":"Cash"}
```

Card buttons and operational PayOS-link handlers are removed. Existing PayOS helpers, return routes,
backend endpoints and historical payment-method display are retained. No pricing, authorization,
OTP, voucher handling, Finance ledger, seed, report or migration code is changed.

## Regression coverage

`ProcessedOrderNotificationTests` checks true/false delivery, unavailable gRPC, branch lookup failure,
missing branch text, cancellation, non-Processed orders and orders without a customer. It also checks
the forwarded token and structured log fields.

`OrderRuntimeDatabaseTests` runs against an isolated local PostgreSQL schema, with the real
`UpdateStatusHandler`, UnitOfWork, audit interceptor, domain-event interceptor and event handlers.
A strict publisher adapter routes the known events to those real handlers in the new event scope.
Notification throws `RpcException(Unavailable)`. A separate verification context confirms the
committed Processed status and `Equipment.Using == false`.

The test then completes with Cash. The real `PubSubService` is exercised twice: its Redis subscriber
returns zero, then its subscriber throws a connection error. In both cases the existing service
returns false, the real Finance/e-invoice event handlers return normally, and a separate context
confirms Completed/Cash with the equipment still released. Retrying completion emits no new events.
No external Notification, Finance, Redis or PayOS server is contacted by these tests.

Enable the database test using `VIETWASH_SEED_TEST_DATABASE` pointing to an isolated local database
whose name starts with `vietwash_seed_test`. Like the existing database tests, it creates a unique
schema from the current EF model and leaves it for disposal with the test database. It is skipped
explicitly when that environment variable is absent. Never use a production or user database.

The existing lifecycle Playwright suite exercises actual frontend pages/components with stateful API
fixtures, covering table, card, manager detail, cashier detail and QR payment. It asserts exact Cash
payloads, no Card button or payment-link request, status after reload, pending/error handling and
status guards. QR tests feed a generated QR frame through a synthetic camera stream to the real
scanner/decoder; they do not replace the scanner component or invoke its callback directly.
These UI tests and the PostgreSQL regression test cover the respective boundaries separately, not
a full deployed multi-service end-to-end environment.

## Verification commands

```powershell
dotnet restore Micro.sln --configfile NuGet.Config
dotnet build Micro.sln -c Release --no-restore
dotnet test tests/UnitTest/EcommerceService.Tests/EcommerceService.Tests.csproj -c Release --no-restore

cd frontend
npm run generate
npm run typecheck
npm run lint
npm run build:test
npm run test:lifecycle
```

The focused backend command is `dotnet test tests/UnitTest/EcommerceService.Tests/EcommerceService.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~ProcessedOrderNotification|FullyQualifiedName~OrderRuntimeDatabase"`.

## Verified results (2026-09-03)

- Solution restore and Release build passed.
- Ecommerce tests: 387 passed, 0 failed, 0 skipped, with the isolated database tests enabled.
- Focused notification/runtime tests: 12 passed, including both Redis failure scenarios in the
  persisted runtime regression.
- Frontend API generation, typecheck, lint and final `build:test` passed. API generation produced
  no generated-code diff.
- Lifecycle Playwright: 73 passed, including all five payment surfaces, real QR decoding from a
  synthetic camera stream, failure/pending guards and the 375 px cashier viewport.
- `git diff --check` passed. The disposable PostgreSQL container and its temporary data were removed
  after verification; no user database was used.
