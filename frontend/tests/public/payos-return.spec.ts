import { expect, test } from "@playwright/test";

const orderResponse = (status: string) => ({
  status: 200,
  message: "Success",
  results: { id: 10, status },
});

test("cancelled PayOS return never mutates or cancels the business order", async ({
  page,
}) => {
  let apiCalls = 0;
  await page.route("**/Ecommerce/api/Orders/**", async (route) => {
    apiCalls += 1;
    await route.abort();
  });

  await page.goto(
    "/payment/payos-return?code=00&id=link-10&cancel=true&status=CANCELLED&orderCode=10",
  );

  await expect(
    page.getByRole("heading", {
      name: /thanh toán trực tuyến đã bị hủy|online payment cancelled/i,
    }),
  ).toBeVisible();
  await expect(
    page.getByText(/vẫn ở trạng thái đã xử lý|remains processed/i),
  ).toBeVisible();
  expect(apiCalls).toBe(0);
});

test("PAID return waits for the local order before showing success", async ({
  page,
}) => {
  let reads = 0;
  let localStatus = "Processed";
  await page.route("**/Ecommerce/api/Orders/10", async (route) => {
    reads += 1;
    await route.fulfill({
      contentType: "application/json",
      body: JSON.stringify(orderResponse(localStatus)),
    });
  });

  await page.goto(
    "/payment/payos-return?code=00&id=link-10&cancel=false&status=PAID&orderCode=10",
  );

  await expect(
    page.getByRole("heading", { name: /đã nhận thanh toán|payment received/i }),
  ).toBeVisible();
  await expect.poll(() => reads).toBeGreaterThan(0);
  await expect(
    page.getByRole("heading", {
      name: /thanh toán đã được xác nhận|payment confirmed/i,
    }),
  ).toHaveCount(0);
  // Keep the backend pending until asserted; repeated mount reads must not advance the fixture.
  localStatus = "Completed";
  await expect(
    page.getByRole("heading", {
      name: /thanh toán đã được xác nhận|payment confirmed/i,
    }),
  ).toBeVisible({ timeout: 5_000 });
  expect(reads).toBeGreaterThanOrEqual(2);
});

test("PENDING return trusts a completed local order", async ({ page }) => {
  await page.route("**/Ecommerce/api/Orders/10", async (route) => {
    await route.fulfill({
      contentType: "application/json",
      body: JSON.stringify(orderResponse("Completed")),
    });
  });

  await page.goto(
    "/payment/payos-return?code=00&cancel=false&status=PENDING&orderCode=10",
  );

  await expect(
    page.getByRole("heading", {
      name: /thanh toán đã được xác nhận|payment confirmed/i,
    }),
  ).toBeVisible();
});

test("invalid return data does not call the order API", async ({ page }) => {
  let apiCalls = 0;
  await page.route("**/Ecommerce/api/Orders/**", async (route) => {
    apiCalls += 1;
    await route.abort();
  });

  await page.goto(
    "/payment/payos-return?code=01&status=PAID&orderCode=not-a-number",
  );

  await expect(
    page.getByRole("heading", {
      name: /thông tin trả về không hợp lệ|invalid return information/i,
    }),
  ).toBeVisible();
  expect(apiCalls).toBe(0);
});
