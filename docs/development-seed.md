# Development seed reliability

## Scope and prerequisites

The Ecommerce initializer and its repair step run only when the host environment is `Development`.
No seed data is added by migrations. Production inventory completion, global domain-event dispatch,
Finance, Notification, payments, authorization and material-consumption rules are unchanged.

**Identity is still an explicit prerequisite.** Auth owns login accounts; Project owns branches and
branch assignments. Ecommerce stores their `User` / `BranchUser` projections, not login credentials
or a standalone Branch aggregate. Before Ecommerce initialization, its database needs an admin,
an active customer and active staff assigned to each demo branch (1, 2 and 3). The existing Auth/Project
synchronization supplies those identities. Missing identities produce a descriptive seed error and
the seed transaction rolls back. The existing background initializer is one-shot after its startup
delay; if synchronization arrives later, restart Ecommerce to retry. This patch does not invent
shadow users or claim that an empty standalone Ecommerce database supports login.

## Why the old inventory seed failed

`SaveChanges` ran inside an uncommitted outer transaction. The saved-changes interceptor dequeued
the inventory completion event and dispatched it in another DI scope. That scope's connection could
not see the new document, so required equipment creation could be skipped even though the event
had been removed. Also, the monthly inventory cursor was shared across branches, leaving branches
2 and 3 without their imports after branch 1 finished.

## Seed behavior

- Completed imports are constructed directly as fixture state; `UpdateStatus(Completed)` is not called.
- `InventoryEquipmentFactory` maps a receipt to equipment only. Both seed and the runtime handler
  reuse it, preserving the `WM`, `WM1`, `WM2` rule, metadata and maintenance dates. Runtime retains
  its explicit legacy branch fallback; seed rejects missing branches or branches outside 1/2/3.
- Reconciliation runs on every development startup, including when inventory already exists. It
  recognizes `DEV-IM-` receipts and legacy `IM` receipts with the exact monthly seed note. Operational
  imports and pending documents are ignored. Existing equipment is matched case-insensitively by
  branch and code, consistent with the current non-unique `citext` code index; no new constraint is added.
  Sequential reruns add only missing equipment. This is not a concurrency guarantee for multiple seeders.
- Missing monthly seed imports are filled per branch. Existing receipt lines are not duplicated.
  Only the four named fixture products with their original seed descriptions are stocked: 100 base
  units in January 2025 and 300 per subsequent month through the initialization date. There is no
  unlimited-stock shortcut. First-month imports provide 10 washers, 5 dryers and 2 irons per branch.
- Product units are explicitly loaded before service resources are linked. Resources are checked by
  the unchanged material resolver and stock validator for a five-unit order: active product and unit,
  same branch, valid multiples, positive quantities and sufficient completed-ledger stock.
- Newly seeded tariffs reference services from their own branch. The common tariff remains usable
  for a year from initial seeding. Existing user-edited tariffs are not rewritten on startup.
- Each branch receives seven recent sample orders: two Completed/Cash, two Processed, two InProgress
  and one Pending. All equipment history belongs to the order's branch. Only InProgress claims set
  `Using`; reserved IDs prevent sharing a machine between active orders. No available machine gives
  a descriptive branch-specific error before random selection. Historical links do not release another
  order's current claim.
- Sample orders use explicit final fixture states, avoiding operational order events. Their material
  exports reuse the existing consumption implementation and are Completed/Export with `SourceOrderId`.
  Finance, notifications, email, SignalR and payments are not called. Optional service-image uploads
  are not part of base seeding. Normal runtime external side effects remain in their handlers.
- Seed timestamps persisted to PostgreSQL are UTC. Rollback errors do not hide the original seed error.

Reconciliation is intentionally not a general repair of existing business orders, resource edits or
tariffs. It does not reset existing equipment usage or refill already-present monthly receipts.

## Verification

`DevelopmentSeedTests` covers equipment mapping, case-insensitive idempotency, legacy receipt scope,
invalid branch rejection, runtime fallback compatibility, branch-scoped active selection, exhaustion,
historical usage and order equipment invariants.

`DevelopmentSeedDatabaseTests` uses the real PostgreSQL provider, relational model, UnitOfWork,
audit interceptor and domain-event interceptor. An isolated schema is created in a **local test database
whose name starts with `vietwash_seed_test`**. It tests missing identity projections first, then inserts
synthetic Auth/Project projections before invoking the actual Ecommerce initializer. It proves:

- 51 equipment / 21 orders, positive stock and valid resources in all three demo branches;
- no cross-branch order equipment or tariff service links, no empty InProgress assignments and no
  duplicate active claims;
- repeated initializations preserve inventory, stock-line, equipment and order counts;
- a deleted unreferenced fixture machine is recreated without duplicating inventory or stock;
- zero domain-event publications throughout seed and repair with no external clients registered;
- real Create and UpdateStatus handlers execute Pending → InProgress → Processed → Cash Completed;
- a 5 kg Combo Giặt Sấy Quần Áo order consumes 0.5 Omo base units and 0.75 Downy base units under
  the existing conversion rules, creates one linked export, releases equipment and contributes to
  completed-order totals.

The test captures runtime event delivery after seeding instead of contacting external consumers.
It is a handler/SQL integration test, **not** a browser login, full migration-chain or report-API test.
Schemas are intentionally retained in the disposable database until its container is removed.

Run the regular suite with:

```powershell
dotnet restore Micro.sln --configfile NuGet.Config
dotnet build Micro.sln -c Release --no-restore
dotnet test tests/UnitTest/EcommerceService.Tests/EcommerceService.Tests.csproj -c Release --no-restore
git diff --check
git status
```

For the PostgreSQL test, set `VIETWASH_SEED_TEST_DATABASE` to an isolated local test connection before
running the same test command. Without it, only that database integration test is explicitly skipped.
Never point this variable at a production or user database. The test creates tables from the current
EF model and requires permission to create a schema and the `citext` extension.

Verified on 2026-09-03 using a temporary PostgreSQL 17.6 container with tmpfs storage: restore and
Release solution build passed; the Ecommerce suite passed with the database test enabled. Frontend
was not changed and was not rebuilt.
