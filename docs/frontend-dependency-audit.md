# Frontend dependency audit

## Current result — 2026-09-04

The previously reported 40 findings have been remediated. Both `npm audit` and `npm audit --omit=dev` return **0 vulnerabilities** for the checked lockfile. CI now fails on any audit severity. No advisory ignores, `--force`, or downgrade-to-vulnerable workarounds were used.

- Next 15.5.25 replaces unsupported 14.x; matched eslint-config-next/bundle analyzer, next-intl 4.14.2, async route params and cookie access are verified by typecheck/build. React 18 is retained. Node 22 is used in CI/production images. Next 15 remains Maintenance LTS, so this is not a promise of indefinite support.
- Serwist 9.5.12 replaces next-pwa/its old Workbox/Rollup chain. Its exact Browserslist pin is overridden with patched 4.28.8 (same API). The worker caches only versioned static assets, never account/order APIs.
- Next's internal PostCSS pin is overridden with the directly declared patched 8.5.26 line, retaining PostCSS 8 compatibility; CSS compilation and production build pass.
- ExcelJS only imports `uuid.v4` without buffer arguments. A targeted override to CommonJS-compatible uuid 11.1.1 preserves that API; an XLSX round trip including UUID-dependent data-bar conditional formatting passes.
- Unused `src/lib/crypto.ts` exports had no callers. Removing them eliminates the unpatched elliptic dependency rather than inventing a replacement signing protocol.
- `@swc/helpers` is explicitly declared for next-intl's newer optional SWC peer; Next retains its own exact helper version. This was verified through Linux `npm ci` and the actual standalone runtime, not only a Windows build.

The full graph is smaller after removing the legacy PWA toolchain. Remaining package deprecation/compiler notices are distinct from security advisories. See [maintenance verification and limits](maintenance-completion.md).

## Historical baseline — 2026-08-31

This baseline was reviewed on 2026-08-31 with the locked dependency graph and both `npm audit` and `npm audit --omit=dev`.

## Result

| Scope | Before | After reviewed updates |
| --- | ---: | ---: |
| Full install | 64 (6 low, 14 moderate, 41 high, 3 critical) | 37 (4 low, 8 moderate, 25 high, 0 critical) |
| Production omission view | 60 total | 33 (4 low, 8 moderate, 21 high, 0 critical) |
| Development-only difference | 4 total | 4 high, all in tooling paths |

The production omission view includes build-time transitive packages pulled by Next.js and `next-pwa`; its total is not a count of independently runtime-reachable browser vulnerabilities.

The review removed unused direct dependencies (`jspdf`, `node-rsa`, `uuid`, `uploadthing`, and `@uploadthing/react` plus redundant type packages), moved the OpenAPI generator to development dependencies, and updated the generator, Playwright, PostCSS, qs, and Sharp to compatible patched releases. The completed maintenance migration then moved the runtime to Next.js 15.5.25. `dompurify`, which application notification code imports, is now declared directly instead of arriving accidentally through the removed `jspdf` dependency. `npm audit fix --force` was not used. A non-breaking dry run proposed no additional lockfile changes.

## Historical constrained findings

The following table records the pre-migration constraints. The current resolved state is documented in the maintenance summary above.

| Dependency path | Reachability | Decision |
| --- | --- | --- |
| `next` 14.x and its bundled PostCSS | Server and App Router runtime | Keep the requested Next 14 major. npm reports that the remaining advisories require a framework-major upgrade. |
| `next-intl` 3.x | Locale loading and rendering | A fix requires next-intl 4 and migration testing; defer rather than introduce an unreviewed breaking API change. |
| `next-pwa` → Workbox → `rollup-plugin-terser` / `serialize-javascript` | Trusted build pipeline and deployed service-worker output | The package is unmaintained and npm suggests an invalid downgrade. Replacing the PWA integration is a separate migration. |
| `exceljs` → `uuid` | Browser workbook export | The advisory affects UUID buffer-writing APIs that this application does not call directly. The upstream package has no compatible patched release; npm suggests a downgrade. |
| `elliptic` | Browser signing helpers in `src/lib/crypto.ts` | The advisory has no patched release. Replacing secp256k1 primitives needs protocol-level compatibility testing. |
| `eslint-config-next` and transitive Babel/glob/minimatch packages | Lint/build tooling | Safe lockfile updates are exhausted; the remaining direct lint finding is tied to the retained Next 14 toolchain. |

Re-run both audit commands when planning the eventual Next/PWA dependency migration, and reassess actual application use before accepting any major-version remediation.
