import { EquipmentStatus, OrderStatus } from "@/api/generated";
import type { ListEquipmentResponse } from "@/api/generated";

// Actions, not arbitrary target statuses: start requires equipment and complete requires payment.
const actions = {
  [OrderStatus.Pending]: {
    edit: true,
    start: true,
    process: false,
    complete: false,
    cancel: true,
  },
  [OrderStatus.InProgress]: {
    edit: false,
    start: false,
    process: true,
    complete: false,
    cancel: true,
  },
  [OrderStatus.Processed]: {
    edit: false,
    start: false,
    process: false,
    complete: true,
    cancel: true,
  },
  [OrderStatus.Completed]: {
    edit: false,
    start: false,
    process: false,
    complete: false,
    cancel: false,
  },
  [OrderStatus.Cancelled]: {
    edit: false,
    start: false,
    process: false,
    complete: false,
    cancel: false,
  },
} as const;

export const getOrderActions = (status?: OrderStatus) =>
  status
    ? (actions[status] ?? actions[OrderStatus.Cancelled])
    : actions[OrderStatus.Cancelled];
export const canEditOrder = (status?: OrderStatus) =>
  getOrderActions(status).edit;
export const canStartOrder = (status?: OrderStatus) =>
  getOrderActions(status).start;
export const canProcessOrder = (status?: OrderStatus) =>
  getOrderActions(status).process;
export const canCompleteOrder = (status?: OrderStatus) =>
  getOrderActions(status).complete;
export const canCancelOrder = (status?: OrderStatus) =>
  getOrderActions(status).cancel;

export const availableOrderEquipmentFilter = (branchId: number) => ({
  branchId: { $eq: branchId },
  status: { $eq: EquipmentStatus.Active },
  using: { $eq: false },
});

export const isAvailableOrderEquipment = (
  equipment: ListEquipmentResponse,
  branchId: number,
) =>
  branchId > 0 &&
  !!equipment.id &&
  equipment.branchId === branchId &&
  equipment.status === EquipmentStatus.Active &&
  equipment.using === false;
