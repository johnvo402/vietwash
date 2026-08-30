import { defineConfig } from "@playwright/test";

export default defineConfig({
  testDir: "./tests",
  timeout: 60 * 1000,
  expect: {
    timeout: 5000,
  },
  fullyParallel: true,
  retries: 0,
  use: {
    baseURL: "http://localhost:3000", // URL gốc, dùng page.goto('/') là vào trang chủ
    headless: true, // Bật false nếu muốn debug với trình duyệt hiển thị
    screenshot: "only-on-failure", // Tự chụp ảnh nếu test fail
    video: "retain-on-failure", // Lưu video nếu test fail
    trace: "retain-on-failure", // Ghi trace giúp debug nếu test fail
  },
  projects: [
    {
      name: "auth", // login
      testMatch: /auth\/.*\.spec\.ts/, // test login
    },
    {
      name: "setup", // login
      testMatch: /setup\/setup\.spec\.ts/, // test login
    },
    {
      name: "feature-tests", // test cần login
      testMatch: /features\/.*\.spec\.ts/,
      use: {
        storageState: "auth-state.json",
      },
      dependencies: ["setup"],
    },
  ],

  // Playwright sẽ tự chạy `npm run dev` nếu chưa có server
  webServer: {
    command: "npm run dev", // Lệnh để khởi chạy Next.js
    port: 3000,
    reuseExistingServer: !process.env.CI, // Tái sử dụng server nếu đang test local
    timeout: 60 * 1000, // Cho server tối đa 60 giây để khởi động
  },
  reporter: [["html", { outputFolder: "playwright-report", open: "never" }]],
});
