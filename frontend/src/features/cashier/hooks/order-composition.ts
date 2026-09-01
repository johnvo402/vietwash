import { apiClient } from "@/api/client";
import { useMutation } from "@tanstack/react-query";
import { Order, ServiceItem } from "../types";
import { OrderItemSelectionModel } from "@/api/generated/api";

export const useOrder = () => {
  const createOrder = useMutation({
    mutationFn: async (order: Order) => {
      const items: OrderItemSelectionModel[] = order.orderItems.map(
        (x: ServiceItem) => ({
          serviceId: x.id,
          quantity: x.quantity,
          unitRelationId: x.unitRelationId,
        })
      );

      return await apiClient.ecommerceApiOrdersPost({
        customerId: order.customer?.id,
        branchId: order.branchId,
        voucherCode: order.voucherCode,
        note: order.note,
        orderItems: items,
        deliveryTime: order.deliveryTime?.toISOString(),
        tariffId: order.tariffId || undefined,
      });
    },
  });

  const updateOrder = useMutation({
    mutationFn: async (order: Order) => {
      const items: OrderItemSelectionModel[] = order.orderItems.map(
        (x: ServiceItem) => ({
          serviceId: x.id,
          quantity: x.quantity,
          unitRelationId: x.unitRelationId,
        })
      );
      return await apiClient.ecommerceApiOrdersIdPut(order.id!, {
        note: order.note,
        orderItems: items,
        deliveryTime: order.deliveryTime?.toISOString(),
        tariffId: order.tariffId || undefined,
      });
    },
  });

  const checkVoucher = useMutation({
    mutationFn: async ({
      code,
      customerId,
    }: {
      code: string;
      customerId: number;
    }) => {
      return await apiClient.ecommerceApiVoucherUsageIdCheckCodeCustomerIdGet(
        code,
        customerId
      );
    },
  });

  return { createOrder, checkVoucher, updateOrder };
};
