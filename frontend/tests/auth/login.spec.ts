import { test, expect } from "@playwright/test";

const email = process.env.E2E_EMAIL ?? "";
const password = process.env.E2E_PASSWORD ?? "";

test.skip(!email || !password, "E2E_EMAIL and E2E_PASSWORD are required");

test("test-login", async ({ page }) => {
  await page.goto("/");
  await page.getByRole("link").filter({ hasText: /^$/ }).click();
  await page.getByRole("textbox", { name: "email" }).click();
  await page.getByRole("textbox", { name: "email" }).fill(email);
  await page.getByRole("textbox", { name: "mật khẩu" }).click();
  await page.getByRole("textbox", { name: "mật khẩu" }).fill(password);
  await page.getByRole("button", { name: "Login" }).click();
  await expect(page.getByText("Login success")).toBeVisible();
  await page.goto("/manage/dashboard");
  await page.getByRole("heading", { name: "Thống kê" }).click();
});
