import { defineConfig } from "@playwright/test";

// Live staging smoke: no mock routes, no synthetic authentication, no CSP bypass.
export default defineConfig({
  testDir: "./tests/closure-smoke",
  timeout: 180000,
  expect: { timeout: 20000 },
  workers: 1,
  use: {
    baseURL: process.env.E2E_BASE_URL ?? "http://localhost:18080",
    headless: true,
    actionTimeout: 20000,
    navigationTimeout: 30000,
    screenshot: "only-on-failure",
    trace: "retain-on-failure",
    viewport: { width: 1440, height: 1000 },
  },
  reporter: [["list"]],
});
