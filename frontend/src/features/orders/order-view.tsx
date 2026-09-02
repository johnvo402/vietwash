"use client";

import { useMemo, useState } from "react";
import { Grid3X3, List, ScanLine, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { DataTable } from "@/components/ui/table/data-table";
import { useOrder } from "@/features/orders/components/order-table/columns";
import { useTranslations } from "next-intl";
import { useOrderFilters } from "./compositions/useOrderFilters";
import { useOrdersQuery } from "./compositions/use-order-query";
import { OrderFilters } from "./components/order-table/OrderFilters";
import { OrderCard } from "./components/order-table/OrderCard";
import { PaymentModal } from "./components/payment-model";
import { OrderStatus, PaymentMethod } from "@/api/generated";
import { useAuth } from "@/hooks/use-auth";
import { UpdateOrder, useCashier } from "../cashier/hooks/use-cashier";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_CASHIER } from "@/types/router-type";
import { apiClient } from "@/api/client";
import { toast } from "react-toastify";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import CashierEquipmentPicker from "./components/equipment-picker";
import { Input } from "@/components/ui/input";
import { OrderEquipment } from "../cashier/types";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export default function OrderView() {
  const t = useTranslations();
  const queryClient = useQueryClient();

  const { pushRouter } = usePushRouter();
  const { branchActive } = useAuth();
  const [isEquipmentDialogOpen, setIsEquipmentDialogOpen] = useState(false);
  const [orderToInProgress, setOrderToInProgress] = useState<{
    id: string;
    code: string;
  } | null>(null);
  const [selectedEquipments, setSelectedEquipments] =
    useState<OrderEquipment[]>();
  const [searchTermEquipment, setSearchTermEquipment] = useState("");
  const branchId = useMemo(
    () => branchActive?.branchId?.toString() ?? "",
    [branchActive],
  );
  const {
    search,
    statusFilter,
    setStatusFilter,
    customerGroupFilter,
    setCustomerGroupFilter,
    dateRange,
    setDateRange,
    page,
    pageSize,
    viewMode,
    setViewMode,
  } = useOrderFilters();
  const cashier = useCashier();
  const { columns, cancelOrderDialog } = useOrder({
    onEdit(id) {
      handleEdit(id);
    },
    onInProgress(id, code) {
      setOrderToInProgress({ id, code });
      setIsEquipmentDialogOpen(true);
    },
  });

  const handleEdit = async (id: number) => {
    try {
      // Gọi API để lấy chi tiết đơn giặt
      const response = await apiClient.ecommerceApiOrdersId(id);
      const orderDetail = response.data.results;

      // Ánh xạ dữ liệu chi tiết sang định dạng UpdateOrder
      const updateOrderData: UpdateOrder = {
        orderId: orderDetail?.id!,
        code: orderDetail?.code!,
        tariffId: orderDetail?.tariff?.id!,
        point: orderDetail?.point!,
        note: orderDetail?.note || "",
        deliveryTime: orderDetail?.deliveryTime!,
        orderItems:
          orderDetail?.orderItems
            ?.filter(
              (item) =>
                item.serviceId !== undefined &&
                item.unitRelationId !== undefined &&
                item.price !== undefined &&
                item.quantity !== undefined &&
                item.unitPrice !== undefined,
            )
            .map((item) => ({
              serviceId: item.serviceId!,
              unitRelationId: item.unitRelationId!,
              price: item.unitPrice!,
              quantity: item.quantity!,
              unitRelationName: item.unitRelationName ?? "",
              processingTime: item.processingTime ?? 0,
              serviceName: item.serviceName ?? "",
              unitPrice: item.unitPrice!,
            })) ?? [],
        customer: {
          id: orderDetail?.customer?.id!,
          displayName: orderDetail?.customer?.displayName!,
          phoneNumber: orderDetail?.customer?.phoneNumber!,
        },
      };

      // Gọi handleUpdateOrder với dữ liệu chi tiết
      cashier.handleUpdateOrder(updateOrderData);
      pushRouter({
        router: ROUTE_CASHIER,
        redirect: "blank",
      });
    } catch (error) {
      console.error("Lỗi khi lấy chi tiết đơn giặt:", error);
      toast.error(t("common.error"));
    }
  };

  const {
    ordersToDisplay,
    isFetching,
    isLoading,
    error,
    observerRef,
    refetch,
    paging,
  } = useOrdersQuery({
    search,
    statusFilter: statusFilter.map((option) => option.value as OrderStatus),
    customerGroupFilter,
    dateRange,
    viewMode,
    page,
    pageSize,
    branchId: Number(branchId),
  });

  const [isPaymentOpen, setIsPaymentOpen] = useState(false);

  const handlePaymentSuccess = (method: "cash" | "card") => {
    alert(
      t(method === "cash" ? "order.cashConfirmed" : "order.paymentSuccess"),
    );
    refetch();
  };

  const updateStatus = useMutation({
    mutationFn: ({
      id,
      status,
      paymentMethod,
      equipments,
    }: {
      id: string;
      status: OrderStatus;
      paymentMethod?: PaymentMethod;
      equipments?: OrderEquipment[];
    }) =>
      apiClient.ecommerceApiOrdersUpdateStatusidPut(id, {
        status,
        paymentMethod,
        orderEquipments: equipments?.map(({ equipmentId }) => ({
          equipmentId,
        })),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["orders"] });
    },
  });
  const handleToggleEquipment = (equipment: OrderEquipment) => {
    setSelectedEquipments((prev = []) => {
      const exists = prev.some((e) => e.equipmentId === equipment.equipmentId); // giả sử OrderEquipment có field id
      if (exists) {
        // Nếu đã tồn tại => gỡ ra
        return prev.filter((e) => e.equipmentId !== equipment.equipmentId);
      }
      // Nếu chưa => thêm vào
      return [...prev, equipment];
    });
  };

  const onDialogEquipmentClosed = () => {
    setIsEquipmentDialogOpen(false);
    setSelectedEquipments([]);
    setOrderToInProgress(null);
  };
  return (
    <div className="w-full mx-auto py-6 space-y-6">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <OrderFilters
          customerGroupFilter={customerGroupFilter}
          setCustomerGroupFilter={setCustomerGroupFilter}
          statusFilter={statusFilter}
          setStatusFilter={setStatusFilter}
          dateRange={dateRange}
          setDateRange={(value) => value && setDateRange(value)}
          refetch={refetch}
        />
        <div className="flex items-center gap-2">
          <Button
            variant={viewMode === "list" ? "default" : "outline"}
            size="icon"
            onClick={() => setViewMode("list")}
          >
            <List className="h-4 w-4" />
          </Button>
          <Button
            variant={viewMode === "card" ? "default" : "outline"}
            size="icon"
            onClick={() => setViewMode("card")}
          >
            <Grid3X3 className="h-4 w-4" />
          </Button>
          <Button
            variant="default"
            onClick={() => setIsPaymentOpen(true)}
            className="flex items-center gap-2"
          >
            <ScanLine className="h-4 w-4" />
            {t("order.scanOrder")}
          </Button>
        </div>
      </div>

      {viewMode === "list" ? (
        <DataTable
          columns={columns}
          data={ordersToDisplay}
          paging={paging}
          loading={isLoading || isFetching}
          error={error ? new Error(t("common.error")) : undefined}
        />
      ) : (
        <div className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {ordersToDisplay.map((order, index: number) => (
              <OrderCard
                key={order.id!.toString()}
                order={order}
                ref={
                  index === ordersToDisplay.length - 1 ? observerRef : undefined
                }
                onEdit={(id) => handleEdit(id)}
              />
            ))}
          </div>
          {isFetching && (
            <div className="flex justify-center">
              <p>{t("common.loading")}</p>
            </div>
          )}
          {error && (
            <div className="flex justify-center text-red-500">
              <p>{t("common.error")}</p>
            </div>
          )}
        </div>
      )}

      <PaymentModal
        isOpen={isPaymentOpen}
        onClose={() => setIsPaymentOpen(false)}
        onPaymentSuccess={handlePaymentSuccess}
      />
      {cancelOrderDialog}
      <Dialog
        open={isEquipmentDialogOpen}
        onOpenChange={onDialogEquipmentClosed}
      >
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>
              <div className="space-y-2">
                <div className="mb-3 flex justify-start gap-2 items-center">
                  {t("equipment.title")}
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-border h-4 w-4" />
                    <Input
                      type="text"
                      placeholder={t(
                        "equipment.equipmentList.searchPlaceholder",
                      )}
                      value={searchTermEquipment}
                      onChange={(e) => setSearchTermEquipment(e.target.value)}
                      className="pl-9 h-9 text-sm rounded-md"
                    />
                  </div>
                </div>
              </div>
            </DialogTitle>
          </DialogHeader>
          <div className="max-h-[40vh] overflow-auto p-2">
            <CashierEquipmentPicker
              selected={selectedEquipments || []}
              onToggle={(eq) => handleToggleEquipment(eq)}
              searchTerm={searchTermEquipment}
            />
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setIsEquipmentDialogOpen(false)}
            >
              {t("common.cancel")}
            </Button>
            <Button
              onClick={() => {
                if (orderToInProgress) {
                  updateStatus.mutate({
                    id: orderToInProgress.id,
                    status: OrderStatus.InProgress,
                    equipments: selectedEquipments || [],
                  });
                  setIsEquipmentDialogOpen(false);
                  setOrderToInProgress(null);
                }
              }}
            >
              {t("common.status.confirm")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
