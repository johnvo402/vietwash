"use client";

import { ContentLayout } from "@/components/admin-panel/content-layout";
import { OrderDetail } from "@/features/orders/components/order-detail/order-detail";
import { ROUTE_CASHIER_ORDERS, ROUTE_ORDERS } from "@/types/router-type";
import { useQuery, useMutation } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import {
  GetOrderDetailResponse,
  // GetReceiptResponse,
  OrderStatus,
} from "@/api/generated/api";
import { useEffect, useState } from "react";
import { usePushRouter } from "@/utils/router-utli";
import LoadingSpinner from "@/components/main/LoadingSpinner";
import { useOrderTransition } from "@/features/orders/compositions/use-order-transition";

interface OrderDetailProps {
  params: { publicId: string };
}

export default function OrderDetailPage({ params }: OrderDetailProps) {
  const { pushRouter } = usePushRouter();

  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(params.publicId);
    if (storedId) setId(Number(storedId));
  }, [params.publicId]);
  const {
    data: order,
    isLoading,
    error,
    refetch,
  } = useQuery<GetOrderDetailResponse | undefined>({
    queryKey: ["order", id],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiOrdersId(id!);
      return response.data.results;
    },
    enabled: !!id,
  });

  const { mutateAsync: getReceipt } = useMutation({
    mutationFn: async () => {
      const response = await apiClient.financeApiEInvoiceGetByOrderIdOrderIdGet(
        id!,
      );
      return response.data.results?.url ?? "";
    },
  });

  const handleGetReceipt = async () => {
    const result = await getReceipt();
    if (result != null && result !== "") {
      window.open(result, "_blank");
    }
  };

  const updateStatusMutation = useOrderTransition();
  const cancelOrderMutation = useOrderTransition();

  const handleChangeStatus = async (
    orderId: string,
    newStatus: typeof OrderStatus.Processed,
  ): Promise<void> => {
    await updateStatusMutation.mutateAsync({ id: orderId, status: newStatus });
  };

  const handleCancelOrder = async (
    orderId: string,
    cancellationReason: string,
  ) => {
    await cancelOrderMutation.mutateAsync({
      id: orderId,
      status: OrderStatus.Cancelled,
      cancellationReason,
    });
  };

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <OrderDetail
      order={order}
      onBack={() =>
        pushRouter({
          router: ROUTE_CASHIER_ORDERS,
        })
      }
      onStatusChange={handleChangeStatus}
      onCancel={handleCancelOrder}
      getReceipt={handleGetReceipt}
      refetch={refetch}
    />
  );
}
