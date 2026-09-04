import { expect, test } from "@playwright/test";
import { format } from "date-fns";

test("live staging: staff creates, processes and completes one Cash order", async ({
  page,
  baseURL,
}) => {
  page.setDefaultTimeout(20000);
  const email = process.env.E2E_EMAIL;
  const password = process.env.E2E_PASSWORD;
  expect(
    email,
    "Set credentials for an assigned STAFF in the disposable demo database",
  ).toBeTruthy();
  expect(password).toBeTruthy();
  expect(
    process.env.E2E_REPORT_EMAIL,
    "Set a MANAGER/ADMIN for report UI authorization",
  ).toBeTruthy();
  expect(process.env.E2E_REPORT_PASSWORD).toBeTruthy();
  await page
    .context()
    .addCookies([{ name: "NEXT_LOCALE", value: "en", url: baseURL! }]);
  page.on("dialog", (dialog) => void dialog.accept());
  const apiOrigins = new Set<string>();
  const onlinePayments: string[] = [];
  const forbiddenLandings: string[] = [];
  page.on("framenavigated", (frame) => {
    if (frame === page.mainFrame() && new URL(frame.url()).pathname === "/403")
      forbiddenLandings.push(frame.url());
  });
  page.context().on("request", (request) => {
    const url = new URL(request.url());
    if (url.pathname.includes("/api/")) apiOrigins.add(url.origin);
    if (url.pathname.includes("payment-link"))
      onlinePayments.push(url.pathname);
  });
  page.on("response", (response) => {
    if (response.url().includes("/api/") && response.status() >= 400)
      console.log(
        "API failure",
        response.status(),
        new URL(response.url()).pathname,
      );
  });
  await page.goto("/auth/login");
  await page.locator('input[name="email"]').fill(email!);
  await page.locator('input[name="password"]').fill(password!);
  const loginResponse = page.waitForResponse(
    (r) =>
      r.url().includes("/Accounts/Login") && r.request().method() === "POST",
  );
  await page.getByRole("button", { name: "Login", exact: true }).click();
  expect((await loginResponse).status()).toBe(200);
  await page.waitForURL("**/manage/cashier");
  await expect(page.locator("#customer-select")).toBeVisible();
  await expect(
    page.getByRole("combobox", { name: "Tariff", exact: true }),
  ).toContainText("Bảng giá");
  await page.locator("#customer-select").click();
  await page.getByRole("option").filter({ hasText: / - / }).first().click();
  await page
    .getByRole("button", { name: /Combo Giặt Sấy Quần Áo/ })
    .first()
    .click();
  const previewResponse = page.waitForResponse(
    (r) =>
      new URL(r.url()).pathname === "/Ecommerce/api/Orders/preview" &&
      r.request().method() === "POST",
  );
  await page.getByRole("button", { name: "More", exact: true }).click();
  const preview = await previewResponse;
  expect(preview.status()).toBe(200);
  const pricing = (await preview.json()).results;
  expect(pricing.total).toBeGreaterThan(0);
  await page
    .locator("#booking-datetime")
    .fill(format(new Date(Date.now() + 86400000), "dd/MM/yyyy HH:mm:ss"));
  await page
    .getByRole("button", { name: "Create Order", exact: true })
    .waitFor({ state: "visible" });
  await page.screenshot({
    path: test.info().outputPath("cashier-live.png"),
    fullPage: true,
  });
  const createResponse = page.waitForResponse(
    (r) =>
      new URL(r.url()).pathname === "/Ecommerce/api/Orders" &&
      r.request().method() === "POST",
  );
  await page.getByRole("button", { name: "Create Order", exact: true }).click();
  const creation = await createResponse;
  expect(creation.status()).toBe(200);
  const created = (await creation.json()).results;
  expect(created.total).toBe(pricing.total);
  expect(creation.request().postDataJSON()).not.toHaveProperty("total");
  await expect(page.getByTestId("persisted-order-total")).toContainText(
    created.code,
  );

  await page.goto("/manage/cashier/orders");
  const row = page.getByRole("row").filter({ hasText: created.code });
  await row.getByRole("button", { name: "Open Menu", exact: true }).click();
  const detailsItem = page.getByRole("menuitem", {
    name: "View Details",
    exact: true,
  });
  await detailsItem.focus();
  await detailsItem.press("Enter");
  const status = page.getByTestId("persisted-order-status");
  await expect(status).toHaveAttribute("data-order-status", "Pending");
  await page
    .getByRole("button", { name: "Start processing", exact: true })
    .click();
  const start = page.getByRole("dialog", { name: /Start processing/ });
  const machine = start.locator("button[aria-pressed]").first();
  await machine.click();
  const startResponse = page.waitForResponse(
    (r) =>
      r.url().includes("/Orders/UpdateStatus") &&
      r.request().method() === "PUT",
  );
  await start.getByRole("button", { name: "Confirm", exact: true }).click();
  const started = await startResponse;
  expect(started.status()).toBe(200);
  const startPayload = started.request().postDataJSON();
  expect(startPayload.status).toBe("InProgress");
  await expect(status).toHaveAttribute("data-order-status", "InProgress");
  await page
    .getByRole("button", { name: "Mark Processed", exact: true })
    .click();
  await expect(status).toHaveAttribute("data-order-status", "Processed");
  await page.getByRole("button", { name: "Add Payment", exact: true }).click();
  const cash = page.getByRole("dialog", { name: "Confirm cash payment" });
  await expect(cash.getByRole("button", { name: /Card|PayOS/ })).toHaveCount(0);
  const cashResponse = page.waitForResponse(
    (r) =>
      r.url().includes("/Orders/UpdateStatus") &&
      r.request().method() === "PUT",
  );
  await cash.getByRole("button", { name: "Cash", exact: true }).click();
  const paid = await cashResponse;
  expect(paid.status()).toBe(200);
  expect(paid.request().postDataJSON()).toEqual({
    status: "Completed",
    paymentMethod: "Cash",
  });
  await expect(status).toHaveAttribute("data-order-status", "Completed");
  await page.reload();
  await expect(status).toHaveAttribute("data-order-status", "Completed");
  await page.screenshot({
    path: test.info().outputPath("completed-live.png"),
    fullPage: true,
  });
  const reportPage = await page.context().newPage();
  const signalRConnections = new Set<string>();
  reportPage.on("websocket", (socket) => {
    const url = new URL(socket.url());
    if (url.pathname === "/notification/hub")
      socket.on("framereceived", () =>
        signalRConnections.add(`${url.protocol}//${url.host}${url.pathname}`),
      );
  });
  await reportPage.goto("/auth/login");
  await reportPage
    .locator('input[name="email"]')
    .fill(process.env.E2E_REPORT_EMAIL!);
  await reportPage
    .locator('input[name="password"]')
    .fill(process.env.E2E_REPORT_PASSWORD!);
  const managerLogin = reportPage.waitForResponse(
    (r) =>
      r.url().includes("/Accounts/Login") && r.request().method() === "POST",
  );
  await reportPage.getByRole("button", { name: "Login", exact: true }).click();
  expect((await managerLogin).status()).toBe(200);
  await reportPage.waitForURL("**/manage/dashboard");
  const reportFrom = Math.floor(Date.now() / 86400000) * 86400;
  const reportQuery = new URLSearchParams({
    from: String(reportFrom),
    to: String(reportFrom + 86399),
    branchIds: JSON.stringify([created.branchId]),
  });
  const revenueResponse = reportPage.waitForResponse(
    (r) =>
      new URL(r.url()).pathname ===
        "/Ecommerce/api/ReportRoute/RevenueReport" &&
      new URL(r.url()).searchParams.get("From") === String(reportFrom),
  );
  await reportPage.goto(`/manage/report/revenue?${reportQuery}`);
  const revenue = await revenueResponse;
  expect(revenue.status()).toBe(200);
  const revenueRows = (await revenue.json()).results.data;
  const todayRevenue = revenueRows.find(
    (row: { date: string; branchId: number }) =>
      row.date === new Date().toISOString().slice(0, 10) &&
      row.branchId === created.branchId,
  );
  expect(todayRevenue).toBeDefined();
  expect(todayRevenue.totalNetRevenue).toBeGreaterThanOrEqual(created.total);
  await expect(reportPage.getByRole("table")).toContainText(
    new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(todayRevenue.totalNetRevenue),
  );
  await reportPage.screenshot({
    path: test.info().outputPath("revenue-live.png"),
    fullPage: true,
  });
  await expect.poll(() => signalRConnections.size).toBeGreaterThan(0);
  await expect(
    reportPage.getByRole("img", { name: "Avatar", exact: true }),
  ).toBeVisible();
  await expect
    .poll(() =>
      reportPage
        .getByRole("img", { name: "Avatar", exact: true })
        .evaluate(
          (img: HTMLImageElement) => img.complete && img.naturalWidth > 0,
        ),
    )
    .toBe(true);
  await expect
    .poll(() =>
      reportPage.evaluate(
        async () =>
          (await navigator.serviceWorker.getRegistration())?.active?.state,
      ),
    )
    .toBe("activated");
  const cachedPaths = await reportPage.evaluate(async () => {
    const paths: string[] = [];
    for (const name of await caches.keys()) {
      for (const request of await (await caches.open(name)).keys())
        paths.push(new URL(request.url).pathname);
    }
    return paths;
  });
  expect(cachedPaths.length).toBeGreaterThan(0);
  expect(cachedPaths.every((path) => path.startsWith("/_next/static/"))).toBe(true);
  expect([...apiOrigins]).toEqual([new URL(baseURL!).origin]);
  expect(onlinePayments).toEqual([]);
  expect(forbiddenLandings).toEqual([]);
  const proof = {
    orderId: created.id,
    orderCode: created.code,
    branchId: created.branchId,
    total: created.total,
    equipmentIds: startPayload.orderEquipments.map(
      (x: { equipmentId: number }) => x.equipmentId,
    ),
    apiOrigins: [...apiOrigins],
    signalRConnections: [...signalRConnections],
    paymentMethod: "Cash",
    finalStatus: "Completed",
    revenueRows,
  };
  console.log(JSON.stringify(proof));
  await test.info().attach("closure-proof", {
    body: JSON.stringify(proof, null, 2),
    contentType: "application/json",
  });
});
