# VietWash Web

The VietWash web application is the Next.js 15.5.25 frontend for the laundry management platform in this monorepo. It uses the App Router, TypeScript, React Query, Zustand, Tailwind CSS, next-intl, and an Axios client generated from the backend OpenAPI documents.

The codebase is a feature-based modular frontend. It is not an independently deployed micro-frontend system.

## Local setup

Prerequisites:

- Node.js 22 LTS or newer
- npm
- Java, used by OpenAPI Generator

Clone the monorepo and enter the frontend directory:

```bash
git clone https://github.com/johnvo402/vietwash.git
cd vietwash/frontend
```

Create local configuration and install dependencies:

```bash
cp .env.example .env.local
npm ci
npm run generate
npm run dev
```

The development server runs at [http://localhost:3000](http://localhost:3000). By default, `.env.example` targets the API Gateway at `http://localhost:5000` and MinIO at `http://127.0.0.1:9000`.

## Commands

| Command                  | Purpose                                                         |
| ------------------------ | --------------------------------------------------------------- |
| `npm run dev`            | Start the development server                                    |
| `npm run generate-types` | Merge service OpenAPI documents and regenerate the Axios client |
| `npm run generate`       | Generate the Axios client from the existing `openapi.json`      |
| `npm run typecheck`      | Run the TypeScript compiler without emitting files              |
| `npm run lint`           | Run the Next.js ESLint configuration                            |
| `npm run build`          | Generate the client and create a production build               |
| `npm run build:test`     | Create a production build from an already generated client      |
| `npm run test:run`       | Run the Playwright auth and feature projects                    |

## Structure

```text
frontend/
├── messages/              # English and Vietnamese translations
├── public/                # Static assets and PWA icons
├── src/
│   ├── api/               # Axios setup and generated OpenAPI client
│   ├── app/               # Next.js App Router routes and layouts
│   ├── components/        # Shared UI and layout components
│   ├── features/          # Business features grouped by domain
│   ├── hooks/             # Shared React and Zustand hooks
│   ├── openapi/           # Per-service OpenAPI documents
│   └── types/             # Application types
├── tests/                 # Playwright end-to-end tests
├── generate-clients.ts    # OpenAPI merge script
└── next.config.mjs        # Next.js, i18n, PWA, and image configuration
```

## Configuration and security

All `NEXT_PUBLIC_*` variables are shipped to the browser and must be treated as public. `NEXT_PUBLIC_CLIENT_ID` is only a client identifier sent through the legacy `X-Api-Key` header for gateway compatibility; it is not authentication or authorization. JWT roles and permissions are the application security boundary.

The current backend returns refresh tokens in JSON. The frontend therefore keeps credentials in tab-scoped `sessionStorage` for compatibility. Migrating refresh tokens to `HttpOnly`, `Secure`, `SameSite` cookies requires a coordinated backend change.

Production CSP connection origins are derived from `NEXT_PUBLIC_API_URL`, `NEXT_PUBLIC_MEDIA_URL`, and the optional `CSP_CONNECT_SRC` list. Use explicit `http` or `https` origins; local loopback origins are accepted only in Development. The matching `ws` or `wss` origin is added automatically for SignalR.

## Testing

Playwright tests require a running backend and test credentials supplied as `E2E_EMAIL` and `E2E_PASSWORD`. The feature suite reuses the authenticated storage state created by the setup project.

See the [root README](../README.md) for backend, infrastructure, architecture, and full-monorepo instructions.
