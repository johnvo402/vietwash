import { useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { OrderStatus, PaymentMethod } from "@/api/generated";
import { invalidateOrderLifecycle } from "../order-lifecycle-cache";

type LifecycleUpdate =
  | {
      status: typeof OrderStatus.InProgress;
      orderEquipments: { equipmentId: number }[];
    }
  | { status: typeof OrderStatus.Processed }
  | {
      status: typeof OrderStatus.Completed;
      paymentMethod: typeof PaymentMethod.Cash;
    }
  | { status: typeof OrderStatus.Cancelled; cancellationReason: string };

export function useOrderTransition() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...command }: LifecycleUpdate & { id: string }) =>
      apiClient.ecommerceApiOrdersUpdateStatusidPut(id, command),
    // No optimistic status: list/detail only change after a successful persisted response/refetch.
    onSuccess: async () => {
      await invalidateOrderLifecycle(client);
    },
  });
}
