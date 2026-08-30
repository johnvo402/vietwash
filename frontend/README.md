# VietWash - Laundry Shop Management System

VietWash is a **Next.js 14** web application designed to streamline laundry shop operations. Leveraging the **App Router** for optimized routing and server-side rendering, it provides robust features for order management, customer tracking, financial reporting, service management, and supplier coordination. The project adopts a **micro-frontend** architecture for modularity and scalability, with **internationalization (i18n)** support for English and Vietnamese.

## Website

The live application is available at: [https://vietwash.vercel.app](https://vietwash.vercel.app)

## Features

- **Order Management**: Create, view, and manage laundry orders with payment processing and receipt generation.
- **Customer Management**: Track customer details and interactions.
- **Service Management**: Manage laundry services, including creation, editing, and categorization.
- **Financial Reporting**: Generate reports for customer revenue, orders, and services.
- **Supplier Management**: Handle supplier details and interactions.
- **User Management**: Manage user accounts with role-based access.
- **Dashboard**: Visualize key metrics with charts (e.g., monthly revenue, top services).
- **Internationalization**: Support for English (`en.ts`) and Vietnamese (`vi.ts`) languages.
- **Micro-Frontend Architecture**: Modular components for scalability and maintainability.

## Prerequisites

Ensure the following are installed:

- **Node.js**: v18 or higher (recommended for Next.js 14)
- **npm** or **yarn**
- **OpenAPI Generator CLI**: Install globally with `npm install -g @openapitools/openapi-generator-cli`

## Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/johnvo402/micro-frontend.git
   cd micro-frontend
   ```

2. Install dependencies:

   ```bash
   npm install
   ```

3. Generate API client and types:
   ```bash
   npm run generate-types
   ```

## Available Scripts

In the project directory, you can run:

- **`npm run dev`**  
  Starts the development server at [http://localhost:3000](http://localhost:3000).

- **`npm run build`**  
  Generates the API client and builds the production version with Next.js 14 optimizations.

- **`npm run build:test`**  
  Builds the app without generating the API client, useful for testing.

- **`npm run start`**  
  Starts the production server with Next.js 14 optimizations.

- **`npm run lint`**  
  Runs ESLint for code quality checks.

- **`npm run merged-api`**  
  Merges API definitions using `generate-clients.ts`.

- **`npm run generate`**  
  Generates TypeScript Axios API client from `openapi.json` to `src/api/generated`.

- **`npm run generate-types`**  
  Runs `merged-api` and `generate` to update API client and types.

- **`npm run test:record`**  
  Runs the development server and Playwright codegen for test recording at [http://localhost:3000](http://localhost:3000).

- **`npm run test:run`**  
  Runs Playwright tests in headless mode.

- **`npm run test:headed`**  
  Runs Playwright tests in headed mode (with browser UI).

- **`npm run test:report`**  
  Displays the Playwright test report.

## Project Structure

The project uses Next.js 14's **App Router** and a micro-frontend architecture for modularity:

```
micro-frontend
├── messages/                    # Internationalization files (en.ts, vi.ts)
├── public/                      # Static assets (images, SVGs, favicon, manifest)
│   ├── demos/                  # Demo images
│   ├── flag/                   # Flag icons (English, Vietnamese)
│   ├── img/                    # General images
│   ├── logo/                   # Favicon and app icons
│   ├── manifest.json           # Web app manifest
│   ├── sw.js                   # Service worker
│   └── workbox-4754cb34.js     # Workbox for service worker
├── src/
│   ├── api/                    # API client and configuration
│   │   ├── generated/          # Auto-generated API client from OpenAPI
│   │   ├── client.ts           # API client setup
│   │   └── config.ts           # API configuration
│   ├── app/                    # Next.js 14 App Router
│   │   ├── (cashier)/         # Cashier module (orders, payments)
│   │   ├── (manage)/          # Management module (dashboard, customers, etc.)
│   │   ├── 403/               # Forbidden page
│   │   ├── auth/login/        # Login page
│   │   ├── fonts/             # Custom fonts (GeistMonoVF, GeistVF)
│   │   ├── globals.scss       # Global styles
│   │   ├── layout.tsx         # Root layout
│   │   ├── loading.tsx        # Loading state
│   │   └── page.tsx           # Home page
│   ├── components/             # Reusable UI components
│   │   ├── admin-panel/       # Admin panel UI (sidebar, navbar, etc.)
│   │   ├── core/              # Core components (breadcrumb, date picker, etc.)
│   │   ├── main/              # Main layout components
│   │   ├── providers/         # Context providers (e.g., theme)
│   │   ├── tree/              # Tree view components (filters, stats)
│   │   └── ui/                # UI primitives (table, button, dialog, etc.)
│   ├── compositions/           # Table filter compositions
│   ├── constants/             # API constants
│   ├── data/                  # Static data (e.g., order items)
│   ├── features/              # Feature modules (auth, cashier, customer, etc.)
│   ├── hooks/                 # Custom React hooks
│   ├── i18n/                  # Internationalization utilities
│   ├── lib/                   # Utility functions (crypto, filters, etc.)
│   ├── openapi/               # OpenAPI specifications (auth, ecommerce, etc.)
│   ├── providers/             # Query provider for data fetching
│   ├── shared/                # Shared utilities (e.g., Excel export)
│   ├── types/                 # TypeScript type definitions
│   └── utils/                 # General utilities (date, query, router, etc.)
├── tests/                      # Playwright test files
├── .eslintrc.json             # ESLint configuration
├── .gitignore                 # Git ignore file
├── components.json            # Component configuration
├── generate-clients.ts        # Script for merging API clients
├── next.config.mjs            # Next.js configuration
├── openapi.json               # OpenAPI specification
├── openapitools.json          # OpenAPI tools configuration
├── package.json               # Project dependencies and scripts
├── playwright.config.ts       # Playwright configuration
├── postcss.config.mjs         # PostCSS configuration
├── tailwind.config.ts         # Tailwind CSS configuration
├── tsconfig.json              # TypeScript configuration
└── README.md                  # Project documentation
```

## Technologies

- **Next.js 14**: React framework with App Router and server components.
- **TypeScript**: Type-safe code for enhanced developer experience.
- **Tailwind CSS**: Utility-first CSS framework for styling.
- **Axios**: HTTP client for API requests.
- **OpenAPI**: API client generation from `openapi.json`.
- **React Query**: Data fetching and state management.
- **i18n**: Internationalization with English and Vietnamese support.
- **ESLint**: Code linting for quality and consistency.
- **Playwright**: End-to-end testing framework.
- **Vercel**: Deployment platform optimized for Next.js 14.

## Next.js 14 Features Utilized

- **App Router**: File-based routing in `src/app/`.
- **Server Components**: Optimized server-side rendering for performance.
- **TypeScript Support**: Enhanced type safety.
- **Optimized Builds**: Faster builds and smaller bundle sizes.
- **Dynamic Routes**: Used in modules like `orders/[publicId]` and `service/[id]`.
- **Middleware**: Configured for authentication and request handling.

## Deployment

Deploy on **Vercel**, optimized for Next.js 14:

1. Push the code to a GitHub repository.
2. Connect the repository to Vercel via the Vercel dashboard.
3. Configure environment variables (if any) in Vercel.
4. Deploy the application with Vercel’s automatic scaling.

## Contributing

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -m "Add YourFeature"`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Open a Pull Request.

## License

This project is licensed under the MIT License (see `LICENSE` file).
