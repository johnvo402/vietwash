import { expect, test, type Page } from "@playwright/test";
import { OrderStatus } from "../../src/api/generated";

type Surface = "table" | "card" | "manager" | "cashier";
const surfaces: Surface[] = ["table", "card", "manager", "cashier"];
const profile = {
  id: 7,
  role: "ADMIN",
  displayName: "Lifecycle Staff",
  phoneNumber: "0900000001",
  email: "staff@example.test",
  birthDay: "2000-01-01",
  branchAccounts: [
    { branchId: 1, branchName: "Active Branch One" },
    { branchId: 2, branchName: "Order Branch Two" },
  ],
};
const baseOrder = {
  id: 1001,
  publicId: "order-public",
  code: "OD-1001",
  branchId: 2,
  amount: 100,
  total: 108,
  discountFixed: false,
  discountValue: 0,
  vat: 8,
  vatAmount: 8,
  createdAt: "2026-09-01T10:00:00Z",
  orderDate: "2026-09-03T10:00:00Z",
  deliveryTime: "2030-01-01T12:00:00Z",
  orderItems: [],
  orderEquipments: [],
  customer: {
    id: 501,
    displayName: "Test Customer",
    phoneNumber: "0901234567",
  },
  tariff: { id: 5, name: "Order tariff" },
  cancellationReason: "Customer requested cancellation",
};

async function setup(
  page: Page,
  surface: Surface,
  status: OrderStatus,
  options: { hold?: boolean; reject?: boolean; secondOrder?: boolean } = {},
) {
  const order = { ...baseOrder, status };
  const second = {
    ...baseOrder,
    id: 1002,
    publicId: "order-two",
    code: "OD-1002",
    status: OrderStatus.Pending,
  };
  const equipment = [
    {
      id: 21,
      name: "Washer A",
      code: "W21",
      branchId: 2,
      status: "Active",
      using: status === OrderStatus.InProgress,
    },
    {
      id: 22,
      name: "Washer B",
      code: "W22",
      branchId: 2,
      status: "Active",
      using: false,
    },
    {
      id: 31,
      name: "Wrong branch washer",
      code: "W31",
      branchId: 1,
      status: "Active",
      using: false,
    },
    {
      id: 32,
      name: "Broken washer",
      code: "W32",
      branchId: 2,
      status: "UnderMaintenance",
      using: false,
    },
  ];
  const calls = {
    updates: [] as any[],
    equipment: [] as URL[],
    links: 0,
    orderReads: 0,
  };
  let release = () => {};
  const gate = new Promise<void>((resolve) => {
    release = resolve;
  });
  await page
    .context()
    .addCookies([
      { name: "NEXT_LOCALE", value: "en", url: "http://localhost" },
    ]);
  page.on("dialog", (dialog) => {
    void dialog.accept();
  });
  await page.addInitScript((profile) => {
    sessionStorage.setItem("order-public", "1001");
    sessionStorage.setItem(
      "vietwash-auth-session",
      JSON.stringify({
        state: {
          credentials: {
            token: "fixture-token",
            refresh: "fixture-refresh",
            accessTokenExpiredIn: 9999999999,
          },
          isAuthenticated: true,
          user: profile,
          branchActive: profile.branchAccounts[0],
        },
        version: 0,
      }),
    );
  }, profile);
  await page.route("**/test-payos-checkout", (route) =>
    route.fulfill({
      contentType: "text/html",
      body: "<h1>Mock PayOS checkout</h1>",
    }),
  );
  await page.route("**/api/**", async (route) => {
    const url = new URL(route.request().url());
    const path = url.pathname;
    let results: unknown = {};
    let httpStatus = 200;
    let title: string | undefined;
    if (path.endsWith("/Profile")) results = profile;
    else if (path.includes("UpdateStatus")) {
      const input = route.request().postDataJSON();
      const id = Number(path.match(/(\d+)$/)?.[1]);
      const target = id === 1002 ? second : order;
      calls.updates.push(input);
      if (options.hold) await gate;
      if (options.reject) {
        httpStatus = 400;
        title =
          "Equipment was concurrently claimed. Please choose another machine.";
        if (input.status === "InProgress") equipment[0].using = true;
      } else {
        target.status = input.status;
        if (input.status === "InProgress")
          equipment.forEach((eq) => {
            if (
              input.orderEquipments.some(
                (selected: any) => selected.equipmentId === eq.id,
              )
            )
              eq.using = true;
          });
        if (input.status === "Processed" || input.status === "Cancelled")
          equipment[0].using = false;
      }
    } else if (path.endsWith("/payment-link")) {
      calls.links++;
      results = {
        status: "PENDING",
        checkoutUrl: new URL("/test-payos-checkout", page.url()).href,
      };
    } else if (path.endsWith("/Orders/1001")) {
      results = order;
      calls.orderReads++;
    } else if (path.endsWith("/Orders")) {
      results = {
        data: options.secondOrder ? [order, second] : [order],
        paging: { currentPage: 1, totalPage: 1, totalRecords: 1, pageSize: 10 },
      };
      calls.orderReads++;
    } else if (path.endsWith("/Equipments")) {
      calls.equipment.push(url);
      const ids = [...url.searchParams.entries()]
        .filter(([key]) => key.includes("[id][$in]"))
        .map(([, value]) => Number(value));
      results = {
        data: equipment.filter(
          (eq) => !eq.using && (!ids.length || ids.includes(eq.id)),
        ),
        paging: { currentPage: 1, totalPage: 1 },
      };
    } else if (path.endsWith("/Users")) results = { data: [], paging: {} };
    else if (path.endsWith("/TariffByBranch")) results = [];
    await route.fulfill({
      status: httpStatus,
      contentType: "application/json",
      body: JSON.stringify({ results, status: httpStatus, title }),
    });
  });
  await page.goto("/payment/payos-return?cancel=true&status=CANCELLED");
  await page.goto(
    surface === "manager"
      ? "/manage/orders/order-public"
      : surface === "cashier"
        ? "/manage/cashier/orders/order-public"
        : `/manage/orders?viewMode=${surface === "card" ? "card" : "list"}`,
  );
  await expect(statusLocator(page, surface)).toHaveAttribute(
    "data-order-status",
    status,
  );
  return { calls, release, order, equipment };
}

function orderScope(page: Page, surface: Surface, id = 1001) {
  return surface === "table"
    ? page
        .getByRole("row", { includeHidden: true })
        .filter({ hasText: `OD-${id}` })
    : page.getByTestId(`order-card-${id}`);
}
function statusLocator(page: Page, surface: Surface, id = 1001) {
  return surface === "table" || surface === "card"
    ? orderScope(page, surface, id).getByRole("button", {
        name: "Open Menu",
        exact: true,
        includeHidden: true,
      })
    : page.getByTestId("persisted-order-status");
}
async function action(page: Page, surface: Surface, name: string, id = 1001) {
  if (surface === "table" || surface === "card") {
    await statusLocator(page, surface, id).click();
    await page.getByRole("menuitem", { name, exact: true }).click();
  } else await page.getByRole("button", { name, exact: true }).click();
}

const actionNames = [
  "Start processing",
  "Mark Processed",
  "Add Payment",
  "Cancel",
];
const expectedActions: Record<OrderStatus, string[]> = {
  Pending: ["Start processing", "Cancel"],
  InProgress: ["Mark Processed", "Cancel"],
  Processed: ["Add Payment", "Cancel"],
  Completed: [],
  Cancelled: [],
};

for (const surface of surfaces) {
  for (const status of Object.values(OrderStatus)) {
    test(`${surface}: ${status} has no invalid jump or terminal mutation`, async ({
      page,
    }) => {
      await setup(page, surface, status);
      const menu = surface === "table" || surface === "card";
      if (menu) await statusLocator(page, surface).click();
      for (const name of actionNames) {
        const locator = page.getByRole(menu ? "menuitem" : "button", {
          name,
          exact: true,
        });
        await expect(locator).toHaveCount(
          expectedActions[status].includes(name) ? 1 : 0,
        );
      }
      if (menu) {
        await expect(
          page.getByRole("menuitem", { name: "Update", exact: true }),
        ).toHaveCount(status === "Pending" ? 1 : 0);
        await expect(
          page.getByRole("menuitem", { name: "View Details", exact: true }),
        ).toHaveCount(1);
      }
      if (!menu && status === "Cancelled")
        await expect(
          page.getByText("Customer requested cancellation", { exact: true }),
        ).toBeVisible();
      if (!menu && status === "Completed") {
        await expect(
          page.getByRole("button", { name: "Print Bill", exact: true }),
        ).toBeVisible();
        await expect(
          page.getByRole("button", { name: "Print E-Invoice", exact: true }),
        ).toBeVisible();
      }
    });
  }

  test(`${surface}: start requires equipment, uses Order.BranchId and awaits success`, async ({
    page,
  }) => {
    const { calls, release } = await setup(page, surface, OrderStatus.Pending, {
      hold: true,
    });
    await action(page, surface, "Start processing");
    const dialog = page.getByRole("dialog", { name: /Start processing/ });
    await expect(
      dialog.getByRole("button", { name: "Confirm", exact: true }),
    ).toBeDisabled();
    expect(calls.updates).toEqual([]);
    await expect(
      dialog.getByRole("button", { name: /Washer A/ }),
    ).toBeVisible();
    await expect(
      dialog.getByRole("button", { name: /Wrong branch|Broken washer/ }),
    ).toHaveCount(0);
    expect(calls.equipment[0].searchParams.get("filter[branchId][$eq]")).toBe(
      "2",
    );
    expect(calls.equipment[0].searchParams.get("filter[status][$eq]")).toBe(
      "Active",
    );
    expect(calls.equipment[0].searchParams.get("filter[using][$eq]")).toBe(
      "false",
    );
    await dialog.getByRole("button", { name: /Washer A/ }).click();
    await dialog.getByRole("button", { name: "Confirm", exact: true }).click();
    await expect.poll(() => calls.updates.length).toBe(1);
    expect(calls.updates[0]).toEqual({
      status: "InProgress",
      orderEquipments: [{ equipmentId: 21 }],
    });
    await expect(dialog).toBeVisible();
    await expect(
      dialog.getByRole("button", { name: "Handling", exact: true }),
    ).toBeDisabled();
    await page.keyboard.press("Escape");
    await expect(dialog).toBeVisible();
    await expect(statusLocator(page, surface)).toHaveAttribute(
      "data-order-status",
      "Pending",
    );
    release();
    await expect(dialog).not.toBeVisible();
    await expect(statusLocator(page, surface)).toHaveAttribute(
      "data-order-status",
      "InProgress",
    );
  });

  test(`${surface}: failed start stays Pending, keeps dialog and refreshes equipment`, async ({
    page,
  }) => {
    const { calls } = await setup(page, surface, OrderStatus.Pending, {
      reject: true,
    });
    await action(page, surface, "Start processing");
    const dialog = page.getByRole("dialog", { name: /Start processing/ });
    await dialog.getByRole("button", { name: /Washer A/ }).click();
    await dialog.getByRole("button", { name: /Washer B/ }).click();
    await dialog.getByRole("button", { name: "Confirm", exact: true }).click();
    await expect(dialog.getByRole("alert")).toContainText(
      "concurrently claimed",
    );
    await expect.poll(() => calls.equipment.length).toBeGreaterThan(2);
    await expect(dialog.getByRole("button", { name: /Washer A/ })).toHaveCount(
      0,
    );
    await expect(
      dialog.getByRole("button", { name: /Washer B/ }),
    ).toHaveAttribute("aria-pressed", "true");
    await expect(
      dialog.getByRole("button", { name: "Confirm", exact: true }),
    ).toBeEnabled();
    await expect(statusLocator(page, surface)).toHaveAttribute(
      "data-order-status",
      "Pending",
    );
  });

  test(`${surface}: rejected Processed transition never displays a fake status`, async ({
    page,
  }) => {
    const { calls } = await setup(page, surface, OrderStatus.InProgress, {
      reject: true,
    });
    await action(page, surface, "Mark Processed");
    await expect.poll(() => calls.updates.length).toBe(1);
    expect(calls.updates[0]).toEqual({ status: "Processed" });
    await expect(page.getByText(/concurrently claimed/)).toBeVisible();
    await expect(statusLocator(page, surface)).toHaveAttribute(
      "data-order-status",
      "InProgress",
    );
  });

  for (const status of [
    OrderStatus.Pending,
    OrderStatus.InProgress,
    OrderStatus.Processed,
  ]) {
    test(`${surface}: ${status} cancellation requires and sends reason`, async ({
      page,
    }) => {
      const { calls } = await setup(page, surface, status);
      await action(page, surface, "Cancel");
      const dialog = page.getByRole("alertdialog");
      await expect(
        dialog.getByRole("button", { name: "Cancel order", exact: true }),
      ).toBeDisabled();
      await dialog
        .getByLabel("Cancellation reason", { exact: false })
        .fill("  Customer requested cancellation  ");
      await dialog
        .getByRole("button", { name: "Cancel order", exact: true })
        .click();
      await expect.poll(() => calls.updates.length).toBe(1);
      expect(calls.updates[0]).toEqual({
        status: "Cancelled",
        cancellationReason: "Customer requested cancellation",
      });
      await expect(statusLocator(page, surface)).toHaveAttribute(
        "data-order-status",
        "Cancelled",
      );
      await expect(dialog).not.toBeVisible();
    });
  }

  for (const method of ["cash", "card"] as const) {
    test(`${surface}: Processed completion uses ${method === "cash" ? "Completed + Cash" : "PayOS only"}`, async ({
      page,
    }) => {
      const { calls, order } = await setup(
        page,
        surface,
        OrderStatus.Processed,
      );
      await action(page, surface, "Add Payment");
      expect(calls.updates).toEqual([]);
      const dialog = page.getByRole("dialog");
      if (surface === "table" || surface === "card") {
        await dialog
          .getByRole("button", {
            name: method === "cash" ? "Tiền mặt" : "Thẻ tín dụng",
            exact: true,
          })
          .click();
        await dialog
          .getByRole("button", { name: "Xác nhận", exact: true })
          .click();
      } else
        await dialog
          .getByRole("button", {
            name: method === "cash" ? "Cash" : "Card",
            exact: true,
          })
          .click();
      if (method === "cash") {
        await expect.poll(() => calls.updates.length).toBe(1);
        expect(calls.updates[0]).toEqual({
          status: "Completed",
          paymentMethod: "Cash",
        });
        await expect(statusLocator(page, surface)).toHaveAttribute(
          "data-order-status",
          "Completed",
        );
      } else {
        await expect(page).toHaveURL(/test-payos-checkout$/);
        expect(calls.links).toBe(1);
        expect(calls.updates).toEqual([]);
        expect(order.status).toBe("Processed");
      }
    });
  }
}

test("full table lifecycle claims/releases equipment before Cash completion", async ({
  page,
}) => {
  const { calls } = await setup(page, "table", OrderStatus.Pending, {
    secondOrder: true,
  });
  await action(page, "table", "Start processing");
  await page
    .getByRole("dialog")
    .getByRole("button", { name: /Washer A/ })
    .click();
  await page
    .getByRole("dialog")
    .getByRole("button", { name: "Confirm", exact: true })
    .click();
  await expect(statusLocator(page, "table")).toHaveAttribute(
    "data-order-status",
    "InProgress",
  );
  await action(page, "table", "Start processing", 1002);
  await expect(
    page.getByRole("dialog").getByRole("button", { name: /Washer B/ }),
  ).toBeVisible();
  await expect(
    page.getByRole("dialog").getByRole("button", { name: /Washer A/ }),
  ).toHaveCount(0);
  await page
    .getByRole("dialog")
    .getByRole("button", { name: "Cancel", exact: true })
    .click();
  await action(page, "table", "Mark Processed");
  await expect(statusLocator(page, "table")).toHaveAttribute(
    "data-order-status",
    "Processed",
  );
  await action(page, "table", "Start processing", 1002);
  await expect(
    page.getByRole("dialog").getByRole("button", { name: /Washer A/ }),
  ).toBeVisible();
  await page.screenshot({
    path: test.info().outputPath("available-after-process.png"),
    fullPage: true,
    animations: "disabled",
  });
  await page
    .getByRole("dialog")
    .getByRole("button", { name: "Cancel", exact: true })
    .click();
  await action(page, "table", "Add Payment");
  await page
    .getByRole("dialog")
    .getByRole("button", { name: "Tiền mặt", exact: true })
    .click();
  await page
    .getByRole("dialog")
    .getByRole("button", { name: "Xác nhận", exact: true })
    .click();
  await expect(statusLocator(page, "table")).toHaveAttribute(
    "data-order-status",
    "Completed",
  );
  expect(calls.updates.map((input) => input.status)).toEqual([
    "InProgress",
    "Processed",
    "Completed",
  ]);
});

test("InProgress cancellation refreshes released equipment in the next start dialog", async ({
  page,
}) => {
  const { calls } = await setup(page, "table", OrderStatus.InProgress, {
    secondOrder: true,
  });
  await action(page, "table", "Start processing", 1002);
  const startDialog = page.getByRole("dialog", { name: /Start processing/ });
  await expect(
    startDialog.getByRole("button", { name: /Washer B/ }),
  ).toBeVisible();
  await expect(
    startDialog.getByRole("button", { name: /Washer A/ }),
  ).toHaveCount(0);
  await startDialog
    .getByRole("button", { name: "Cancel", exact: true })
    .click();
  const readsBeforeCancel = calls.equipment.length;
  await action(page, "table", "Cancel");
  const cancelDialog = page.getByRole("alertdialog");
  await cancelDialog
    .getByLabel("Cancellation reason", { exact: false })
    .fill("Customer requested cancellation");
  await cancelDialog
    .getByRole("button", { name: "Cancel order", exact: true })
    .click();
  await expect(statusLocator(page, "table")).toHaveAttribute(
    "data-order-status",
    "Cancelled",
  );
  await expect(cancelDialog).not.toBeVisible();
  await action(page, "table", "Start processing", 1002);
  await expect(
    startDialog.getByRole("button", { name: /Washer A/ }),
  ).toBeVisible();
  expect(calls.equipment.length).toBeGreaterThan(readsBeforeCancel);
});
