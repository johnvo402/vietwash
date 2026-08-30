import { test, expect } from "@playwright/test";

test("test-login", async ({ page }) => {
  await page.goto("http://localhost:3000/");
  await page.getByRole("link").filter({ hasText: /^$/ }).click();
  await page.getByRole("textbox", { name: "email" }).click();
  await page
    .getByRole("textbox", { name: "email" })
    .fill("nguyenhoanglong1997@gmail.com");
  await page.getByRole("textbox", { name: "mật khẩu" }).click();
  await page.getByRole("textbox", { name: "mật khẩu" }).fill("long1997");
  await page.getByRole("button", { name: "Login" }).click();
  await expect(page.getByText("Login success")).toBeVisible();
  await page.goto("http://localhost:3000/manage/dashboard");
  await page.getByRole("heading", { name: "Thống kê" }).click();
});
