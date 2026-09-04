# Maintenance verification — 2026-09-04

Baseline closure commit: `84582067` on `dev`. This follow-up supersedes the DI, STAFF landing, modal-refresh and npm-audit limitations recorded in `closure-verification.md`. It does not certify a live PayOS merchant integration.

## Changes

- All five services use scoped audit interceptors and one scoped DbContext shared by `IDbContext` and its concrete type. Npgsql connection pooling remains enabled; EF context pooling is removed because its singleton options captured request identity.
- Mediator pipelines are scoped too. `LoggingBehavior` consumes `ICurrentAccount`; the original open-generic singleton was missed by startup-only DI validation. Validators now share request identity/context and execute sequentially instead of opening an unrelated scope or issuing concurrent EF queries.
- Tests build the real registrations with `ValidateOnBuild` and `ValidateScopes`, resolve closed generic pipelines, and verify context/interceptor isolation in both Development and Staging for every service.
- STAFF login waits for the authoritative profile and opens the cashier. ADMIN/MANAGER still open Dashboard; unauthorized roles still reach 403. Logout clears query cache. The home/workspace link respects role.
- Order menu navigation uses Radix `onSelect` and a non-modal dropdown inside the existing orders dialog. The live regression uses keyboard selection followed by pointer interaction, without a detail-page reload workaround.
- Next 15.5.25 (Maintenance LTS), next-intl 4.14.2 and Serwist replace the vulnerable Next 14/next-pwa tree. React 18 and the existing UI remain. Server route params/cookies are migrated to async access. Node 22 LTS is used in Docker and CI.
- Unused secp256k1 helpers and `elliptic` were removed after verifying there were no callers. Reviewed dependency overrides and verification are described in `frontend-dependency-audit.md`.
- Service-worker caching is limited to public versioned `/_next/static` assets. Authenticated documents, API responses and signed media remain network-only. Legacy next-pwa caches are removed on activation.
- Rich text is sanitized on write and again before browser rendering; authorization now defaults to authenticated access, with anonymous endpoints explicitly documented in code. Icon-only controls have accessible names and 44 px targets, while heavy editors and QR scanning load on demand.

## Durable processed-order notifications

`NotificationOutbox` captures the immutable processed-order intent before EF saves. It is part of the same database transaction as the order/equipment transition, so a rollback leaves no deliverable notification. Seeded historical statuses do not produce new notifications.

The worker claims a row with PostgreSQL `FOR UPDATE SKIP LOCKED`, persists a one-minute lease, and commits before making a 15-second-bounded gRPC call. Failed attempts back off from two seconds to one hour. There is no attempt cutoff that silently discards a message. Expired leases recover after crashes. Updates are fenced by lease ID. Cancellation after receiver acknowledgement but before marking delivery is safe to retry.

The receiver inserts a persistent receipt keyed by message ID in the same transaction as inbox items. `ON CONFLICT` serializes concurrent duplicates. Replays for the same template/recipients acknowledge the original accepted payload; reuse for different recipients/templates is rejected. Receipts must outlive notification deletion and any producer replay window. Older producers without message IDs retain legacy non-idempotent semantics.

SignalR is sent **after** commit. A disconnected browser cannot roll back inbox persistence. The frontend refreshes unread data after connection/reconnection. This provides at-least-once transport with deduplicated durable inbox creation, not a guarantee that a person sees a toast. This outbox covers `laundry_processed`; it does not convert unrelated Redis invoice/fund integrations into a transactional message bus.

### Rollout / operations

1. Back up the databases and apply/review the additive `DurableNotificationReceipts` and `DurableNotificationOutbox` EF migrations (the existing startup migration mechanism also applies them).
2. Deploy Notification receiver support first, then Ecommerce producers/workers. An old receiver cannot honor the new idempotency contract. Mixed-version producer rollout before receiver upgrade is unsafe.
3. Verify the notification template exists and internal gRPC port 8444 is reachable. Docker gRPC addresses are explicitly configured with `GrpcEndpoints__Project` / `GrpcEndpoints__Notification`; native Development retains localhost defaults. HTTPS endpoints use normal certificate validation.
4. Alert on pending age, attempt count and worker scan errors. Keep retrying while investigating configuration/transport/template failures; do not delete pending rows to silence an alert.

Read-only Ecommerce monitoring query:

```sql
SELECT count(*) AS pending, min(created_at) AS oldest_pending,
       max(attempts) AS highest_attempt_count
FROM notification_outbox WHERE delivered_at IS NULL;

SELECT id, attempts, next_attempt_at, locked_until, last_error
FROM notification_outbox WHERE delivered_at IS NULL
ORDER BY created_at LIMIT 50;
```

Delivered outbox cleanup requires a documented retention/replay policy. Do not truncate Notification receipts during routine notification cleanup. Roll back application binaries without dropping the new tables; dropping them destroys delivery/replay evidence.

## Verification

- 503 backend tests passed, zero failures/skips: Ecommerce 421, Auth 34, Finance 39, Notification 7, Project 2. PostgreSQL tests use an isolated localhost database and unique schemas. CI now provisions PostgreSQL and enables these tests rather than silently skipping outbox/inbox regressions.
- Transaction rollback, failed transport/negative acknowledgement, retry identity, backoff, concurrent claims, expired leases, shutdown, concurrent duplicate receiver delivery, receiver rollback and offline-client persistence are covered.
- Full and production-only npm audits report **0 findings** at verification time. Lint/typecheck, production build and Linux standalone image build pass. ExcelJS workbook export/import with UUID-dependent conditional formatting passes.
- Real PayOS SDK HMAC verification accepts a locally signed fixture and rejects amount tampering/wrong keys. Existing payment amount, state, role and replay tests remain. No provider API or real transfer was used.
- The edge forwards only `POST /Webhook/api/CompletedOrder` to Ecommerce, with a 64 KiB limit. An unsigned fixture reaches the backend and returns 400; unauthenticated order API remains 401. Nginx configuration validates.

Build/deprecation warnings are not hidden: the clean backend build has zero warnings and CI treats future warnings as errors. Zero npm advisories is not certification of OS packages, .NET packages, the whole application or future advisories. Next 15 is on Maintenance LTS: plan its next supported-major migration before its support window ends.

## PayOS production sign-off: blocked on the merchant environment

PayOS documents a single production API, not a sandbox. A verified merchant/payment channel, runtime-only secrets and public HTTPS webhook are required. No such environment/merchant access was supplied in this task, so no production certification is claimed and `PayOsSetting__IsEnabled` stays false by default. Cash remains the visible POS payment action.

Outside Development, enabling PayOS now rejects non-HTTPS, localhost/IP, credential-bearing/fragment URLs and an incorrect webhook route. DNS, TLS reachability and ownership still require deployment verification; URL validation alone cannot establish them.

Merchant/operator checklist before enabling production:

1. Provision approved merchant keys through secret management; set HTTPS return/cancel URLs and the exact public webhook route. Keep secrets and captured signed payloads out of source control/log artifacts.
2. Register/confirm the public webhook in the merchant channel; verify the provider's signed confirmation sample is acknowledged without mutating an order. Check edge/TLS/request-size behavior from outside the private network.
3. With the account owner's explicit authorization, the owner performs a small real payment against a designated test order. Verify authoritative amount, signed webhook, Card/Completed persistence and reconciliation against the provider. Repeat webhook delivery must not duplicate completion effects.
4. Check cancellation, incorrect/duplicate signatures and timeout/recovery paths without treating a return-page query string as proof of payment. Record deployment revision and redacted provider references with an operator sign-off.

References: [PayOS test environment](https://payos.vn/docs/moi-truong-test/), [signature verification](https://payos.vn/docs/tich-hop-webhook/kiem-tra-du-lieu-voi-signature/), [PayOS API](https://payos.vn/docs/api/), [Next.js support policy](https://nextjs.org/support-policy).
