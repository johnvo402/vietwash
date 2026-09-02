"use client";

import { ContentLayout } from "@/components/admin-panel/content-layout";
import { OrderDetail } from "@/features/orders/components/order-detail/order-detail";
import { ROUTE_ORDERS } from "@/types/router-type";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import {
  GetOrderDetailResponse,
  // GetReceiptResponse,
  OrderStatus,
} from "@/api/generated/api";
import { useRouter } from "nextjs-toploader/app";
import { useEffect, useState } from "react";
import LoadingSpinner from "@/components/main/LoadingSpinner";

interface OrderDetailProps {
  params: { publicId: string };
}

export default function OrderDetailPage({ params }: OrderDetailProps) {
  const router = useRouter();
  const queryClient = useQueryClient();

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

  // Mutation để cập nhật trạng thái đơn dịch vụ
  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: OrderStatus }) =>
      apiClient.ecommerceApiOrdersUpdateStatusidPut(id, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["order", id] });
      console.log("Order status updated successfully");
    },
    onError: (error) => {
      console.error("Failed to update order status:", error);
      alert("Failed to update order status. Please try again.");
    },
  });

  // Mutation để hủy đơn dịch vụ
  const cancelOrderMutation = useMutation({
    mutationFn: ({
      id,
      cancellationReason,
    }: {
      id: string;
      cancellationReason: string;
    }) =>
      apiClient.ecommerceApiOrdersUpdateStatusidPut(id, {
        status: OrderStatus.Cancelled,
        cancellationReason,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["order", id] });
      console.log("Order canceled successfully");
    },
    onError: (error) => {
      console.error("Failed to cancel order:", error);
    },
  });

  const handleChangeStatus = async (
    orderId: string,
    newStatus: OrderStatus,
  ) => {
    await updateStatusMutation.mutate({ id: orderId, status: newStatus });
  };

  const handleCancelOrder = async (
    orderId: string,
    cancellationReason: string,
  ) => {
    await cancelOrderMutation.mutateAsync({ id: orderId, cancellationReason });
  };

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <ContentLayout scrollable={false}>
      <OrderDetail
        order={order}
        onBack={() => router.push(ROUTE_ORDERS)}
        onStatusChange={handleChangeStatus}
        onCancel={handleCancelOrder}
        getReceipt={handleGetReceipt}
        refetch={refetch}
      />
    </ContentLayout>
  );
}
