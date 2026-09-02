import type { Customer } from "@/utils/customer-indexedDb";
import type { OrderEquipment, ServiceItem } from "../types";
import type { PreviewOrderQuery } from "@/api/generated";

export const pricesQueryKey = (branchId: number) =>
  ["prices", branchId] as const;
export const selectTariffId = (current: number, tariffs: { id: number }[]) =>
  tariffs.some((tariff) => tariff.id === current)
    ? current
    : (tariffs[0]?.id ?? 0);

export const emptyDraft = (branchId: number) => ({
  branchId,
  customer: null as Customer | null,
  pendingCustomerId: null as number | null,
  items: [] as ServiceItem[],
  voucherCode: "",
  note: "",
  tariffId: 0,
  deliveryTime: "",
  orderId: null as number | null,
  orderEquipments: [] as OrderEquipment[],
});
export type CashierDraft = ReturnType<typeof emptyDraft>;

export function restoreDraft(
  saved: Partial<CashierDraft> | undefined,
  branchId: number,
): CashierDraft {
  if (!saved || saved.branchId !== branchId) return emptyDraft(branchId);
  return {
    ...emptyDraft(branchId),
    ...saved,
    items: Array.isArray(saved.items) ? saved.items : [],
  };
}

export function changeDraftTariff(
  draft: CashierDraft,
  tariffId: number,
): CashierDraft {
  if (draft.tariffId === tariffId) return draft;
  return {
    ...draft,
    tariffId,
    items: [],
    voucherCode: "",
    orderEquipments: [],
  };
}

export function previewInput(draft: CashierDraft): PreviewOrderQuery | null {
  if (
    draft.branchId <= 0 ||
    draft.tariffId <= 0 ||
    !draft.customer?.id ||
    draft.pendingCustomerId ||
    !draft.items.length ||
    draft.orderId
  )
    return null;
  return {
    branchId: draft.branchId,
    tariffId: draft.tariffId,
    customerId: draft.customer.id,
    voucherCode: draft.voucherCode || undefined,
    orderItems: draft.items.map((item) => ({
      serviceId: item.id,
      unitRelationId: item.unitRelationId,
      quantity: item.quantity,
    })),
  };
}

export const itemKey = (item: ServiceItem) =>
  `${item.id}:${item.unitRelationId}`;
