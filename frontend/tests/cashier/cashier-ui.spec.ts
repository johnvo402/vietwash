import { expect, test, Page } from "@playwright/test";
import { emptyDraft } from "../../src/features/cashier/hooks/cashier-draft";

const customer = {
  id: 501,
  displayName: "Synced Customer",
  phoneNumber: "0901234567",
  role: "CUSTOMER",
  status: "Active",
};
const draft = {
  ...emptyDraft(1),
  tariffId: 5,
  customer,
  items: [
    {
      id: 10,
      unitRelationId: 2,
      unitRelationName: "kg",
      name: "Wash",
      quantity: 3,
      price: 999,
    },
  ],
  note: "Keep this note",
  deliveryTime: "2030-01-01T12:00:00Z",
};
const profile = {
  id: 7,
  displayName: "Test Staff",
  email: "staff@example.test",
  phoneNumber: "0900000001",
  birthDay: "2000-01-01",
  role: "STAFF",
  branchAccounts: [
    { branchId: 1, branchName: "Branch One" },
    { branchId: 2, branchName: "Branch Two" },
  ],
};

async function setup(
  page: Page,
  initial = draft,
  options: { timeout?: boolean; failCreate?: boolean; holdSync?: boolean } = {},
) {
  const calls = {
    authCreates: 0,
    lookups: 0,
    creates: [] as any[],
    previews: [] as any[],
    tariffs: [] as number[],
    services: [] as number[],
  };
  let releaseLookup = () => {};
  const lookupGate = new Promise<void>((resolve) => {
    releaseLookup = resolve;
  });
  await page
    .context()
    .addCookies([
      { name: "NEXT_LOCALE", value: "en", url: "http://localhost:3012" },
    ]);
  await page.addInitScript(
    ({ initial, profile }) => {
      if (sessionStorage.getItem("cashier-test-seeded")) return;
      sessionStorage.setItem(
        "vietwash-auth-session",
        JSON.stringify({
          state: {
            credentials: {
              token: "test-fixture-token",
              refresh: "test-fixture-refresh",
              accessTokenExpiredIn: 9999999999,
            },
            isAuthenticated: true,
            user: profile,
            branchActive: profile.branchAccounts[0],
          },
          version: 0,
        }),
      );
      const request = indexedDB.open("cashier-orders", 1);
      request.onupgradeneeded = () => {
        request.result.createObjectStore("orderTabs", { keyPath: "id" });
        request.result.createObjectStore("orderState", { keyPath: "tabId" });
      };
      request.onsuccess = () => {
        const tx = request.result.transaction(
          ["orderTabs", "orderState"],
          "readwrite",
        );
        tx.objectStore("orderTabs").put({ id: "#1", isActive: true });
        tx.objectStore("orderState").put({ tabId: "#1", ...initial });
        tx.oncomplete = () => {
          sessionStorage.setItem("cashier-test-seeded", "true");
          request.result.close();
        };
      };
      window.print = () => {};
    },
    { initial, profile },
  );
  await page.route("**/api/**", async (route) => {
    const url = new URL(route.request().url());
    const path = url.pathname;
    let results: any = {};
    let status = 200;
    if (path.endsWith("/Profile")) results = profile;
    else if (path.endsWith("/Auth/api/Customers")) {
      calls.authCreates++;
      results = { ...customer, displayName: "Auth-only name" };
    } else if (path.endsWith("/Users/501")) {
      calls.lookups++;
      if (options.holdSync && calls.lookups === 1) await lookupGate;
      status = options.timeout || calls.lookups < 3 ? 404 : 200;
      results = status === 200 ? customer : null;
    } else if (path.endsWith("/Users"))
      results = { data: initial.customer ? [customer] : [], paging: {} };
    else if (path.endsWith("/TariffByBranch")) {
      const branch = Number(
        url.searchParams.get("BranchId") ?? url.searchParams.get("branchId"),
      );
      calls.tariffs.push(branch);
      results =
        branch === 2
          ? [{ id: 7, name: "Tariff Branch Two" }]
          : [
              { id: 5, name: "Tariff A" },
              { id: 6, name: "Tariff B" },
            ];
    } else if (path.endsWith("/ServicesByTariff")) {
      calls.services.push(
        Number(
          url.searchParams.get("TariffId") ?? url.searchParams.get("tariffId"),
        ),
      );
      results = {
        data: [
          {
            id: 1,
            name: "Laundry",
            services: [
              {
                id: 10,
                name: "Wash",
                unitRelations: [
                  { id: 2, name: "kg", price: 999, processingTime: 10 },
                ],
              },
            ],
          },
        ],
        paging: { totalPage: 1, currentPage: 1 },
      };
    } else if (path.endsWith("/Orders/preview")) {
      const input = route.request().postDataJSON();
      calls.previews.push(input);
      results = {
        amount: 375,
        discountAmount: 75,
        discountValue: 20,
        discountFixed: false,
        netBeforeVat: 300,
        vatPercent: 8,
        vatAmount: 24,
        total: 324,
        orderItems: input.orderItems.map((item: any) => ({
          ...item,
          serviceName: "Wash",
          unitRelationName: "kg",
          unitPrice: 125,
          lineAmount: 125 * item.quantity,
        })),
      };
    } else if (
      path.endsWith("/Orders") &&
      route.request().method() === "POST"
    ) {
      const input = route.request().postDataJSON();
      calls.creates.push(input);
      status = options.failCreate ? 400 : 201;
      results = {
        id: 1001,
        code: "OD-1001",
        customer,
        amount: 600,
        discountFixed: true,
        discountValue: 50,
        discountAmount: 50,
        vat: 8,
        vatAmount: 44,
        total: 594,
        branchId: 1,
        orderItems: [
          {
            serviceId: 10,
            serviceName: "Wash",
            unitRelationId: 2,
            unitRelationName: "kg",
            price: 200,
            quantity: 3,
          },
        ],
        note: input.note,
        deliveryTime: input.deliveryTime,
      };
    }
    await route.fulfill({
      status,
      contentType: "application/json",
      body: JSON.stringify({
        status,
        results,
        title:
          status === 400
            ? "Voucher was already used by another order."
            : undefined,
      }),
    });
  });
  await page.goto("/payment/payos-return?cancel=true&status=CANCELLED");
  await page.waitForFunction(
    () => sessionStorage.getItem("cashier-test-seeded") === "true",
  );
  await page.goto("/manage/cashier");
  await expect(
    page.getByRole("combobox", { name: "Tariff", exact: true }),
  ).toContainText("Tariff A");
  return Object.assign(calls, { releaseLookup });
}

test("first tariff is actual state; tariff and branch changes reset stale drafts", async ({
  page,
}) => {
  const calls = await setup(page, { ...draft, tariffId: 0, items: [] });
  await expect.poll(() => calls.services).toContain(5);
  await page.getByRole("button", { name: /Wash/ }).click();
  await page.getByRole("button", { name: "More", exact: true }).click();
  await expect.poll(() => calls.previews.length).toBeGreaterThan(0);
  await page.getByRole("combobox", { name: "Tariff", exact: true }).click();
  await page.getByRole("option", { name: "Tariff B" }).click();
  await expect(
    page.getByRole("button", { name: "Create Order", exact: true }),
  ).toBeDisabled();
  await expect.poll(() => calls.services).toContain(6);
  await page.getByRole("combobox").filter({ hasText: "Branch One" }).click();
  await page.getByRole("option", { name: "Branch Two" }).click();
  await expect(
    page.getByRole("combobox", { name: "Tariff", exact: true }),
  ).toContainText("Tariff Branch Two");
  await expect.poll(() => calls.tariffs).toContain(2);
  await expect.poll(() => calls.services).toContain(7);
  expect(calls.creates).toEqual([]);
});

test("new customer waits for Ecommerce; persisted response wins over preview and prints", async ({
  page,
}) => {
  const calls = await setup(
    page,
    { ...draft, customer: null! },
    { holdSync: true },
  );
  await page.locator("#customer-select").click();
  await page.getByRole("option", { name: "Create", exact: true }).click();
  const dialog = page.getByRole("dialog");
  await dialog
    .getByLabel("Customer Name", { exact: true })
    .fill("New Customer");
  await dialog.getByLabel("Phone Number", { exact: true }).fill("0901234567");
  await dialog.getByRole("button", { name: "Create", exact: true }).click();
  await expect(dialog.locator('button[type="submit"]')).toBeDisabled();
  await expect(dialog.getByRole("status")).toHaveText(
    "Synchronizing customer...",
  );
  expect(calls.creates).toHaveLength(0);
  calls.releaseLookup();
  await expect(dialog).not.toBeVisible();
  await expect(page.locator("#customer-select")).toContainText(
    "Synced Customer",
  );
  await expect(page.getByText("VAT 8%", { exact: true })).toBeVisible();
  await expect(
    page.getByRole("button", { name: "Create Order", exact: true }),
  ).toBeEnabled();
  await page.screenshot({
    path: test.info().outputPath("cashier-preview.png"),
    fullPage: true,
  });
  await page.getByRole("button", { name: "Create Order", exact: true }).click();
  await expect(page.getByTestId("persisted-order-total")).toContainText("594");
  expect(calls.authCreates).toBe(1);
  expect(calls.lookups).toBe(3);
  expect(calls.creates).toHaveLength(1);
  expect(calls.creates[0]).toMatchObject({
    branchId: 1,
    tariffId: 5,
    customerId: 501,
    note: "Keep this note",
  });
  expect(calls.creates[0]).not.toHaveProperty("total");
  await expect
    .poll(() => page.frames().some((frame) => frame !== page.mainFrame()))
    .toBe(true);
  const printed = page.frames().find((frame) => frame !== page.mainFrame())!;
  for (const persistedValue of ["OD-1001", "600", "50", "44", "594"])
    await expect(printed.locator("body")).toContainText(persistedValue);
  await expect(printed.locator("body")).not.toContainText("324");
  const cache = await page.evaluate(
    () =>
      new Promise((resolve) => {
        const request = indexedDB.open("CustomerDB", 1);
        request.onsuccess = () => {
          const read = request.result
            .transaction("customers")
            .objectStore("customers")
            .get(501);
          read.onsuccess = () => resolve(read.result);
        };
      }),
  );
  expect(cache).toMatchObject({ id: 501, displayName: "Synced Customer" });
});

test("sync timeout keeps the created ID; retry does not create another customer", async ({
  page,
}) => {
  const calls = await setup(
    page,
    { ...draft, customer: null! },
    { timeout: true },
  );
  await page.locator("#customer-select").click();
  await page.getByRole("option", { name: "Create", exact: true }).click();
  const dialog = page.getByRole("dialog");
  await dialog
    .getByLabel("Customer Name", { exact: true })
    .fill("New Customer");
  await dialog.getByLabel("Phone Number", { exact: true }).fill("0901234567");
  await dialog.getByRole("button", { name: "Create", exact: true }).click();
  await expect(dialog.getByRole("alert")).toContainText("created successfully");
  expect(calls.lookups).toBe(6);
  await dialog
    .getByRole("button", { name: "Retry synchronization", exact: true })
    .click();
  await expect.poll(() => calls.lookups).toBe(12);
  expect(calls.authCreates).toBe(1);
  expect(calls.creates).toEqual([]);
});

test("failed creation retains customer, items, note, pickup time and tariff", async ({
  page,
}) => {
  const calls = await setup(page, draft, { failCreate: true });
  await expect(
    page.getByRole("button", { name: "Create Order", exact: true }),
  ).toBeEnabled();
  await page.getByRole("button", { name: "Create Order", exact: true }).click();
  await expect.poll(() => calls.creates.length).toBe(1);
  await expect(
    page.getByText("Voucher was already used by another order."),
  ).toBeVisible();
  await expect(page.locator("#customer-note")).toHaveValue("Keep this note");
  await expect(page.locator("#customer-select")).toContainText(
    "Synced Customer",
  );
  await expect(
    page.getByRole("combobox", { name: "Tariff", exact: true }),
  ).toContainText("Tariff A");
  await page.getByRole("button", { name: "Create Order", exact: true }).click();
  await expect.poll(() => calls.creates.length).toBe(2);
  expect(calls.creates[1]).toEqual(calls.creates[0]);
});

test("applying a voucher hides stale pricing and blocks create until fresh preview", async ({
  page,
}) => {
  await setup(page);
  const create = page.getByRole("button", {
    name: "Create Order",
    exact: true,
  });
  await expect(create).toBeEnabled();
  await expect(page.locator("dd").filter({ hasText: "324" })).toBeVisible();
  let releasePreview = () => {};
  const previewGate = new Promise<void>((resolve) => {
    releasePreview = resolve;
  });
  let submittedVoucher: string | undefined;
  await page.route("**/Orders/preview", async (route) => {
    submittedVoucher = route.request().postDataJSON().voucherCode;
    await previewGate;
    await route.fulfill({
      contentType: "application/json",
      body: JSON.stringify({
        results: {
          amount: 375,
          discountAmount: 0,
          discountFixed: true,
          discountValue: 0,
          netBeforeVat: 375,
          vatPercent: 8,
          vatAmount: 30,
          total: 405,
          orderItems: [
            {
              serviceId: 10,
              serviceName: "Wash",
              unitRelationId: 2,
              unitRelationName: "kg",
              unitPrice: 125,
              quantity: 3,
              lineAmount: 375,
            },
          ],
        },
      }),
    });
  });
  await page
    .getByRole("textbox", { name: "Enter voucher code", exact: true })
    .fill("ABC");
  await page.getByRole("button", { name: "Apply", exact: true }).click();
  await expect(create).toBeDisabled();
  await expect.poll(() => submittedVoucher).toBe("ABC");
  await expect(page.getByText("Calculating...", { exact: true })).toBeVisible();
  await expect(page.locator("dd").filter({ hasText: "324" })).toHaveCount(0);
  releasePreview();
  await expect(page.locator("dd").filter({ hasText: "405" })).toBeVisible();
  await expect(create).toBeEnabled();
});
