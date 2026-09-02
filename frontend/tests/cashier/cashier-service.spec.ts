import { expect, test } from "@playwright/test";
import { QueryClient } from "@tanstack/react-query";
import { execFileSync } from "node:child_process";
import { readFileSync } from "node:fs";
import {
  changeDraftTariff,
  emptyDraft,
  itemKey,
  previewInput,
  pricesQueryKey,
  restoreDraft,
  selectTariffId,
} from "../../src/features/cashier/hooks/cashier-draft";
import {
  CustomerSyncPendingError,
  synchronizeCustomer,
} from "../../src/features/cashier/hooks/customer-sync";
import { formatPriceVN, formatOrderMoney } from "../../src/utils/format";

const customer = { id: 501, displayName: "Synced customer" };
const item = {
  id: 10,
  unitRelationId: 2,
  quantity: 3,
  price: 999,
  name: "Wash",
};
const draft = {
  ...emptyDraft(1),
  tariffId: 5,
  customer,
  items: [item],
  voucherCode: "SAVE",
  note: "Keep me",
  deliveryTime: "2030-01-01T12:00:00Z",
};

test("customer 501 becomes order-ready only after 404, 404, Ecommerce response", async () => {
  let attempts = 0;
  let selected: typeof customer | null = null;
  const cache: (typeof customer)[] = [];
  const client = new QueryClient();
  client.setQueryData(["customer"], { users: [] });
  await synchronizeCustomer(
    501,
    async (id) => {
      expect(id).toBe(501);
      expect(selected).toBeNull();
      if (++attempts < 3) throw { response: { status: 404 } };
      return customer;
    },
    async (synced) => {
      cache.push(synced);
      await client.invalidateQueries({ queryKey: ["customer"] });
      selected = synced;
    },
    { delayMs: 0 },
  );
  expect(attempts).toBe(3);
  expect(selected).toEqual(customer);
  expect(cache).toEqual([customer]);
  expect(client.getQueryState(["customer"])?.isInvalidated).toBe(true);
  expect(previewInput({ ...draft, customer: selected })).not.toBeNull();
});

test("sync timeout is bounded and does not select customer or submit an order", async () => {
  let attempts = 0;
  let readyCalls = 0;
  await expect(
    synchronizeCustomer(
      501,
      async () => {
        attempts++;
        throw { response: { status: 404 } };
      },
      async () => {
        readyCalls++;
      },
      { attempts: 4, delayMs: 0 },
    ),
  ).rejects.toThrow(CustomerSyncPendingError);
  expect(attempts).toBe(4);
  expect(readyCalls).toBe(0);
  expect(
    previewInput({ ...draft, customer: null, pendingCustomerId: 501 }),
  ).toBeNull();
});

test("non-404 lookup errors are not misclassified as propagation delay", async () => {
  let attempts = 0;
  await expect(
    synchronizeCustomer(
      501,
      async () => {
        attempts++;
        throw { response: { status: 500 } };
      },
      async () => {},
    ),
  ).rejects.toEqual({ response: { status: 500 } });
  expect(attempts).toBe(1);
});

test("tariff caches are branch-specific; first option becomes actual state", () => {
  expect(pricesQueryKey(1)).toEqual(["prices", 1]);
  expect(pricesQueryKey(2)).toEqual(["prices", 2]);
  expect(selectTariffId(0, [{ id: 5 }, { id: 6 }])).toBe(5);
  expect(selectTariffId(5, [{ id: 7 }])).toBe(7);
  expect(selectTariffId(6, [{ id: 5 }, { id: 6 }])).toBe(6);
  expect(selectTariffId(5, [])).toBe(0);
});

test("branch mismatch and legacy branchless drafts discard all incompatible selections", () => {
  expect(restoreDraft(draft, 2)).toEqual(emptyDraft(2));
  expect(restoreDraft({ tariffId: 5, items: [item] }, 2)).toEqual(
    emptyDraft(2),
  );
  expect(previewInput(restoreDraft(draft, 2))).toBeNull();
  expect(restoreDraft(draft, 1)).toEqual(draft);
});

test("tariff changes clear items, voucher, preview but preserve note/customer", () => {
  const changed = changeDraftTariff(draft, 6);
  expect(changed.items).toEqual([]);
  expect(changed.voucherCode).toBe("");
  expect(changed.customer).toEqual(customer);
  expect(changed.note).toBe("Keep me");
  expect(previewInput(changed)).toBeNull();
  expect(changeDraftTariff(draft, 5)).toBe(draft);
});

test("preview submits selection-only inputs and distinguishes unit relations", () => {
  expect(previewInput(draft)).toEqual({
    branchId: 1,
    customerId: 501,
    tariffId: 5,
    voucherCode: "SAVE",
    orderItems: [{ serviceId: 10, unitRelationId: 2, quantity: 3 }],
  });
  expect(itemKey(item)).not.toBe(itemKey({ ...item, unitRelationId: 3 }));
  for (const invalid of [
    { branchId: 0 },
    { tariffId: 0 },
    { customer: null },
    { items: [] },
  ]) {
    expect(previewInput({ ...draft, ...invalid })).toBeNull();
  }
});

const labels = {
  amount: "Amount",
  discount: "Discount",
  total: "Total",
  calculating: "Calculating...",
  error: "Unable to preview",
};
const renderSummary = (props: object) =>
  execFileSync(
    process.execPath,
    ["tests/cashier/render-pricing.cjs", JSON.stringify(props)],
    { encoding: "utf8" },
  );
test("summary renders backend VAT, discount amount and total, not formulas", () => {
  const preview = {
    amount: 10000,
    discountAmount: 1500,
    discountValue: 15,
    vatPercent: 8,
    vatAmount: 680,
    total: 9180,
  };
  const html = renderSummary({
    preview,
    calculating: false,
    error: false,
    labels,
  });
  expect(html).toContain("VAT 8%");
  for (const value of [1500, 680, 9180])
    expect(html).toContain(formatPriceVN(value));
  expect(html).not.toContain("VAT 10%");
  expect(html).not.toContain("points");
  const fractional = renderSummary({
    preview: { ...preview, total: 9180.125 },
    calculating: false,
    error: false,
    labels,
  });
  expect(fractional).toContain(formatOrderMoney(9180.125));
});

test("loading and error states do not present stale totals", () => {
  for (const flags of [
    { calculating: true, error: false },
    { calculating: false, error: true },
  ]) {
    const html = renderSummary({ preview: { total: 12345 }, ...flags, labels });
    expect(html).toContain('role="status"');
    expect(html).not.toContain(formatPriceVN(12345));
  }
});

test("active cashier has no point redemption or price editing authority", () => {
  for (const name of [
    "hooks/use-cashier.ts",
    "components/order-payment.tsx",
    "components/order-summary.tsx",
  ]) {
    const source = readFileSync(`src/features/cashier/${name}`, "utf8");
    expect(source).not.toMatch(
      /pointsDeduction|handleUpdatePoints|onUpdatePrice|handleUpdatePrice|\*\s*0\.1\b|cashier\.usePoints/,
    );
  }
});
