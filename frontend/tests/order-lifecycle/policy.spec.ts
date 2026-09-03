import { expect, test } from "@playwright/test";
import { QueryClient } from "@tanstack/react-query";
import { readFileSync } from "node:fs";
import { OrderStatus, EquipmentStatus } from "../../src/api/generated";
import {
  availableOrderEquipmentFilter,
  canEditOrder,
  canStartOrder,
  canProcessOrder,
  canCompleteOrder,
  canCancelOrder,
  getOrderActions,
  isAvailableOrderEquipment,
} from "../../src/features/orders/order-lifecycle";
import { invalidateOrderLifecycle } from "../../src/features/orders/order-lifecycle-cache";

for (const [status, expected] of [
  [OrderStatus.Pending, [true, true, false, false, true]],
  [OrderStatus.InProgress, [false, false, true, false, true]],
  [OrderStatus.Processed, [false, false, false, true, true]],
  [OrderStatus.Completed, [false, false, false, false, false]],
  [OrderStatus.Cancelled, [false, false, false, false, false]],
] as const) {
  test(`${status} exposes only explicit lifecycle actions`, () => {
    expect([
      canEditOrder(status),
      canStartOrder(status),
      canProcessOrder(status),
      canCompleteOrder(status),
      canCancelOrder(status),
    ]).toEqual(expected);
    expect(Object.values(getOrderActions(status))).toEqual(expected);
  });
}

test("unknown status is fail-closed and invalid jumps are never offered", () => {
  expect(Object.values(getOrderActions(undefined)).some(Boolean)).toBe(false);
  expect(
    Object.values(getOrderActions("unknown" as OrderStatus)).some(Boolean),
  ).toBe(false);
  expect(canProcessOrder(OrderStatus.Pending)).toBe(false);
  expect(canCompleteOrder(OrderStatus.Pending)).toBe(false);
  expect(canCompleteOrder(OrderStatus.InProgress)).toBe(false);
  expect(canCancelOrder(OrderStatus.Completed)).toBe(false);
});

test("equipment availability is scoped to the persisted order branch", () => {
  expect(availableOrderEquipmentFilter(2)).toEqual({
    branchId: { $eq: 2 },
    status: { $eq: EquipmentStatus.Active },
    using: { $eq: false },
  });
  const row = {
    id: 21,
    branchId: 2,
    status: EquipmentStatus.Active,
    using: false,
  };
  expect(isAvailableOrderEquipment(row, 2)).toBe(true);
  expect(isAvailableOrderEquipment(row, 1)).toBe(false);
  expect(isAvailableOrderEquipment({ ...row, using: true }, 2)).toBe(false);
  expect(
    isAvailableOrderEquipment(
      { ...row, status: EquipmentStatus.UnderMaintenance },
      2,
    ),
  ).toBe(false);
});

test("successful transitions invalidate list/detail and all equipment caches", async () => {
  const client = new QueryClient();
  const keys = [
    ["orders", { branchId: 2 }],
    ["order", 1001],
    ["form-equipments", { query: availableOrderEquipmentFilter(2) }],
    ["equipments", 2],
  ];
  keys.forEach((key) => client.setQueryData(key, {}));
  await invalidateOrderLifecycle(client);
  keys.forEach((key) =>
    expect(client.getQueryState(key)?.isInvalidated).toBe(true),
  );
  client.clear();
});

test("both detail adapters await mutateAsync and detail has no optimistic status setter", () => {
  for (const group of [
    "(manage)/manage/orders",
    "(cashier)/manage/cashier/orders",
  ]) {
    const code = readFileSync(`src/app/${group}/[publicId]/page.tsx`, "utf8");
    expect(code).toContain("await updateStatusMutation.mutateAsync(");
    expect(code).not.toMatch(/await\s+updateStatusMutation\.mutate\(/);
  }
  const detail = readFileSync(
    "src/features/orders/components/order-detail/order-detail.tsx",
    "utf8",
  );
  expect(detail).not.toMatch(/setStatus\(|validTransitions/);
  const card = readFileSync(
    "src/features/orders/components/order-table/OrderCard.tsx",
    "utf8",
  );
  expect(card).not.toMatch(/statusIndex|statusOrder|statusToNumberMap/);
});
