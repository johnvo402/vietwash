# Frontend dependency audit

This baseline was reviewed on 2026-08-31 with the locked dependency graph and both `npm audit` and `npm audit --omit=dev`.

## Result

| Scope | Before | After reviewed updates |
| --- | ---: | ---: |
| Full install | 64 (6 low, 14 moderate, 41 high, 3 critical) | 37 (4 low, 8 moderate, 25 high, 0 critical) |
| Production omission view | 60 total | 33 (4 low, 8 moderate, 21 high, 0 critical) |
| Development-only difference | 4 total | 4 high, all in tooling paths |

The production omission view includes build-time transitive packages pulled by Next.js and `next-pwa`; its total is not a count of independently runtime-reachable browser vulnerabilities.

The review removed unused direct dependencies (`jspdf`, `node-rsa`, `uuid`, `uploadthing`, and `@uploadthing/react` plus redundant type packages), moved the OpenAPI generator to development dependencies, and updated the generator, Playwright, Next 14 patch line, PostCSS, qs, and Sharp to compatible patched releases. `dompurify`, which application notification code imports, is now declared directly instead of arriving accidentally through the removed `jspdf` dependency. `npm audit fix --force` was not used. A non-breaking dry run proposed no additional lockfile changes.

## Constrained findings

| Dependency path | Reachability | Decision |
| --- | --- | --- |
| `next` 14.x and its bundled PostCSS | Server and App Router runtime | Keep the requested Next 14 major. npm reports that the remaining advisories require a framework-major upgrade. |
| `next-intl` 3.x | Locale loading and rendering | A fix requires next-intl 4 and migration testing; defer rather than introduce an unreviewed breaking API change. |
| `next-pwa` → Workbox → `rollup-plugin-terser` / `serialize-javascript` | Trusted build pipeline and deployed service-worker output | The package is unmaintained and npm suggests an invalid downgrade. Replacing the PWA integration is a separate migration. |
| `exceljs` → `uuid` | Browser workbook export | The advisory affects UUID buffer-writing APIs that this application does not call directly. The upstream package has no compatible patched release; npm suggests a downgrade. |
| `elliptic` | Browser signing helpers in `src/lib/crypto.ts` | The advisory has no patched release. Replacing secp256k1 primitives needs protocol-level compatibility testing. |
| `eslint-config-next` and transitive Babel/glob/minimatch packages | Lint/build tooling | Safe lockfile updates are exhausted; the remaining direct lint finding is tied to the retained Next 14 toolchain. |

Re-run both audit commands when planning the eventual Next/PWA dependency migration, and reassess actual application use before accepting any major-version remediation.
