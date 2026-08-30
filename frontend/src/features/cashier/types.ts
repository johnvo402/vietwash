import { Customer } from "@/utils/customer-indexedDb";

export type UnitRelationOrder = {
  id: number;
  name: string;
};

export type ServiceItem = {
  id: number;
  name: string;
  price: number;
  quantity: number;
  serviceName?: string;
  unitRelationName?: string;
  unitRelationId?: number;
  unitPrice?: number;
  processingTime?: number;
};
export type OrderEquipment = { equipmentId: number; equipmentName: string };
export type Order = {
  id?: number | undefined;
  customer?: Customer | null;
  staff?: Customer | null;
  orderItems: ServiceItem[];
  voucherCode?: string;
  discountFixed: boolean;
  discountValue: number;
  total: number;
  vat?: number;
  vatAmount?: number;
  amount: number;
  note: string;
  deliveryTime?: Date;
  branchId: number;
  code?: string;
  qrCode?: string;
  orderDate?: Date;
  createdAt?: Date;
  point?: number;
  tariffId?: number;
};

export type DiscountInfo = {
  amount: number;
  isPercentage: boolean;
  error: string | null;
};
