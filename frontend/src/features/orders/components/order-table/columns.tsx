"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { format } from "date-fns";
import {
  MoreHorizontal,
  Clock,
  Truck,
  Package,
  XCircle,
  Search,
} from "lucide-react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { formatPriceVN } from "@/utils/format";
import { apiClient } from "@/api/client";
import { usePushRouter } from "@/utils/router-utli";
import { useTranslations } from "next-intl";
import {
  ROUTE_CASHIER_ORDERS_DETAIL,
  ROUTE_ORDERS_DETAIL,
} from "@/types/router-type";
import { GetCustomerGroup, GetStatusBadge } from "../../order-utils/order-util";
import {
  ListOrderResponse,
  OrderStatus,
  PaymentMethod,
} from "@/api/generated/api";
import { PaymentMethodSelect } from "../PaymentMethodSelect";
import { usePageType } from "@/hooks/use-page-type";
import { OrderEquipment } from "@/features/cashier/types";
import { CancelOrderDialog } from "../cancel-order-dialog";

export const useOrder = ({
  onEdit,
  onInProgress,
}: {
  onEdit?: (id: number) => void;
  onInProgress?: (id: string, code: string) => void;
} = {}) => {
  const pushRouter = usePushRouter();
  const queryClient = useQueryClient();
  const t = useTranslations();
  const { isCashierPage } = usePageType();

  // ----- STATE -----
  const [isCancelDialogOpen, setIsCancelDialogOpen] = useState(false);
  const [isPaymentDialogOpen, setIsPaymentDialogOpen] = useState(false);
  const [orderToComplete, setOrderToComplete] = useState<{
    id: string;
    code: string;
  } | null>(null);
  const [orderToCancel, setOrderToCancel] = useState<{
    id: string;
    code: string;
  } | null>(null);

  // Thêm state cho chọn thiết bị

  // ----- STATUS -----
  const Status = {
    Pending: "Pending",
    InProgress: "InProgress",
    Processed: "Processed",
    Cancelled: "Cancelled",
  } as const;
  type Status = (typeof Status)[keyof typeof Status];
  const statusOrder = Object.values(OrderStatus) as OrderStatus[];

  const statusDisplayMap: Record<
    Status,
    {
      label: string;
      icon: React.ComponentType<{ className?: string }>;
      textColor: string;
      bgColor: string;
    }
  > = {
    Pending: {
      label: t("common.status.pending"),
      icon: Clock,
      textColor: "text-yellow-800",
      bgColor: "bg-yellow-100",
    },
    InProgress: {
      label: t("common.status.handling"),
      icon: Truck,
      textColor: "text-blue-800",
      bgColor: "bg-blue-100",
    },
    Processed: {
      label: t("common.status.handled"),
      icon: Package,
      textColor: "text-orange-800",
      bgColor: "bg-orange-100",
    },
    Cancelled: {
      label: t("common.status.canceled"),
      icon: XCircle,
      textColor: "text-red-800",
      bgColor: "bg-red-100",
    },
  };

  const validTransitions: Partial<Record<OrderStatus, OrderStatus[]>> = {
    Pending: ["InProgress", "Cancelled"],
    InProgress: ["Processed", "Cancelled"],
    Processed: ["Completed", "Cancelled"],
    Completed: [],
    Cancelled: [],
  };

  // ----- MUTATION -----
  const updateStatus = useMutation({
    mutationFn: ({
      id,
      status,
      paymentMethod,
      equipments,
      cancellationReason,
    }: {
      id: string;
      status: OrderStatus;
      paymentMethod?: PaymentMethod;
      equipments?: OrderEquipment[];
      cancellationReason?: string;
    }) =>
      apiClient.ecommerceApiOrdersUpdateStatusidPut(id, {
        status,
        paymentMethod,
        cancellationReason,
        orderEquipments: equipments?.map(({ equipmentId }) => ({
          equipmentId,
        })),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["orders"] });
    },
    onError: (_error, variables) => {
      alert(
        t(
          variables.status === OrderStatus.Cancelled
            ? "order.errorCancellingOrder"
            : "order.updateOrderStatusFailed",
        ),
      );
    },
  });

  // ----- HANDLERS -----
  const handleCompleteOrder = (orderId: string, code: string) => {
    setOrderToComplete({ id: orderId, code });
    setIsPaymentDialogOpen(true);
  };

  const confirmCompleteOrder = (paymentMethod: PaymentMethod) => {
    if (orderToComplete) {
      updateStatus.mutate({
        id: orderToComplete.id,
        status: OrderStatus.Completed,
        paymentMethod,
      });
    }
    setIsPaymentDialogOpen(false);
    setOrderToComplete(null);
  };

  const handleCancelOrder = (orderId: string, code: string) => {
    setOrderToCancel({ id: orderId, code });
    setIsCancelDialogOpen(true);
  };

  const confirmCancelOrder = async (cancellationReason: string) => {
    if (orderToCancel) {
      await updateStatus.mutateAsync({
        id: orderToCancel.id,
        status: OrderStatus.Cancelled,
        cancellationReason,
      });
    }
  };

  const handleChangeStatus = (
    orderId: string,
    newStatus: OrderStatus,
    code: string,
    currentStatus: OrderStatus,
  ) => {
    if (newStatus === OrderStatus.Completed) {
      handleCompleteOrder(orderId, code);
      return;
    }

    if (
      currentStatus === OrderStatus.Pending &&
      newStatus === OrderStatus.InProgress
    ) {
      onInProgress?.(orderId, code);
      return;
    }

    updateStatus.mutate({ id: orderId, status: newStatus });
  };

  // ----- TABLE COLUMNS -----
  const columns: ColumnDef<ListOrderResponse>[] = [
    {
      accessorKey: "index",
      header: t("table.accessorKey.index"),
      cell: ({ row, table }) => {
        const pageIndex = table.getState().pagination.pageIndex;
        const pageSize = table.getState().pagination.pageSize;
        return pageIndex * pageSize + row.index + 1;
      },
    },
    { accessorKey: "code", header: t("table.accessorKey.code") },
    {
      accessorKey: "createdAt",
      header: t("order.orderDate"),
      cell: ({ row }) => {
        const rawDate = row.getValue("createdAt") as string | null;
        if (!rawDate) return <div>--</div>;
        return <div>{format(new Date(rawDate), "dd/MM/yy HH:mm:ss")}</div>;
      },
    },
    {
      accessorKey: "orderDate",
      header: t("order.deliveryTime"),
      cell: ({ row }) => {
        const rawDate = row.getValue("orderDate") as string | null;
        if (!rawDate) return <div>--</div>;
        return <div>{format(new Date(rawDate), "dd/MM/yy HH:mm:ss")}</div>;
      },
    },
    {
      accessorKey: "total",
      header: t("table.accessorKey.total"),
      cell: ({ row }) => {
        const amount = Number.parseFloat(row.getValue("total"));
        return (
          <div className="font-medium text-right">{formatPriceVN(amount)}</div>
        );
      },
    },
    { accessorKey: "customer.displayName", header: t("order.customerName") },
    { accessorKey: "customer.phoneNumber", header: t("order.customerPhone") },
    {
      accessorKey: "customer.customerGroup",
      header: t("user.customerGroup.title"),
      cell: ({ row }) =>
        GetCustomerGroup(t, row.original?.customer?.customerGroup),
    },
    {
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ row }) => GetStatusBadge(row.getValue("status") as OrderStatus),
    },
    {
      id: "actions",
      cell: ({ row }) => {
        const order = row.original;
        return (
          <div className="text-right">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="h-8 w-8 p-0">
                  <span className="sr-only">{t("common.openMenu")}</span>
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem
                  onClick={() =>
                    isCashierPage
                      ? pushRouter.pushRouter({
                          router: ROUTE_CASHIER_ORDERS_DETAIL,
                          params: { publicId: order.publicId?.toString()! },
                          state: { [order.publicId?.toString()!]: order.id! },
                        })
                      : pushRouter.pushRouter({
                          router: ROUTE_ORDERS_DETAIL,
                          params: { publicId: order.publicId?.toString()! },
                          state: { [order.publicId?.toString()!]: order.id! },
                        })
                  }
                >
                  {t("common.viewDetails")}
                </DropdownMenuItem>

                {order.status === OrderStatus.Pending && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={() => onEdit?.(order.id!)}>
                      {t("common.update")}
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() =>
                        handleCancelOrder(order.id!.toString(), order.code!)
                      }
                      className="text-red-600"
                    >
                      <XCircle className="mr-2 h-4 w-4" />
                      {t("common.cancel")}
                    </DropdownMenuItem>
                  </>
                )}

                {statusOrder.map((status) => {
                  if (
                    validTransitions[order.status!]?.includes(status) &&
                    status !== OrderStatus.Cancelled &&
                    status !== OrderStatus.Completed
                  ) {
                    const {
                      icon: Icon,
                      textColor,
                      bgColor,
                      label,
                    } = statusDisplayMap[status as Status];
                    return (
                      <DropdownMenuItem
                        key={status}
                        onClick={() =>
                          handleChangeStatus(
                            order.id!.toString(),
                            status,
                            order.code!,
                            order.status!,
                          )
                        }
                        className={textColor + " " + bgColor}
                      >
                        <Icon className="mr-2 h-4 w-4" />
                        {label}
                      </DropdownMenuItem>
                    );
                  }
                  return null;
                })}
              </DropdownMenuContent>
            </DropdownMenu>

            {/* Payment Dialog */}
            {isPaymentDialogOpen && (
              <PaymentMethodSelect
                isOpen={isPaymentDialogOpen}
                onSubmit={confirmCompleteOrder}
                onClose={() => setIsPaymentDialogOpen(false)}
              />
            )}

            {/* Equipment Dialog */}
          </div>
        );
      },
    },
  ];
  const columnOrderServices: ColumnDef<ListOrderResponse>[] = [
    {
      accessorKey: "index",
      header: t("table.accessorKey.index"),
      cell: ({ row, table }) => {
        const pageIndex = table.getState().pagination.pageIndex;
        const pageSize = table.getState().pagination.pageSize;
        return pageIndex * pageSize + row.index + 1;
      },
    },
    {
      accessorKey: "code",
      header: t("table.accessorKey.code"),
    },

    {
      accessorKey: "orderDate",
      header: t("order.deliveryTime"),
      cell: ({ row }) => {
        const rawDate = row.getValue("orderDate") as string | null;
        if (!rawDate) return <div>--</div>;
        const date = new Date(rawDate);
        return <div>{format(date, "dd/MM/yy HH:mm:ss")}</div>;
      },
    },
    {
      accessorKey: "total",
      header: t("table.accessorKey.total"),
      cell: ({ row }) => {
        const amount = Number.parseFloat(row.getValue("total"));
        return (
          <div className="font-medium text-right">{formatPriceVN(amount)}</div>
        );
      },
    },
    {
      accessorKey: "customer.displayName",
      header: t("order.customerName"),
    },

    {
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ row }) => {
        const status = row.getValue("status") as OrderStatus;
        return GetStatusBadge(status);
      },
    },
  ];

  const columnOrderCustomer: ColumnDef<ListOrderResponse>[] = [
    {
      accessorKey: "index",
      header: t("table.accessorKey.index"),
      cell: ({ row, table }) => {
        const pageIndex = table.getState().pagination.pageIndex;
        const pageSize = table.getState().pagination.pageSize;
        return pageIndex * pageSize + row.index + 1;
      },
    },
    {
      accessorKey: "code",
      header: t("table.accessorKey.code"),
      cell: ({ row }) => {
        const code = row.original.code;
        const publicId = row.original.publicId;
        const referenceId = row.original.id;

        if (!code) return "--";

        return (
          <Button
            variant={"link"}
            onClick={() =>
              pushRouter.pushRouter({
                router: ROUTE_ORDERS_DETAIL,
                params: {
                  publicId: publicId?.toString()!,
                },
                state: {
                  [publicId?.toString()!]: referenceId!,
                },
                redirect: "blank",
              })
            }
            className="p-0"
          >
            {code}
          </Button>
        );
      },
    },

    {
      accessorKey: "orderDate",
      header: t("order.deliveryTime"),
      cell: ({ row }) => {
        const rawDate = row.getValue("orderDate") as string | null;
        if (!rawDate) return <div>--</div>;
        const date = new Date(rawDate);
        return <div>{format(date, "dd/MM/yy HH:mm:ss")}</div>;
      },
    },
    {
      accessorKey: "total",
      header: t("table.accessorKey.total"),
      cell: ({ row }) => {
        const amount = Number.parseFloat(row.getValue("total"));
        return (
          <div className="font-medium text-right">{formatPriceVN(amount)}</div>
        );
      },
    },
    {
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ row }) => {
        const status = row.getValue("status") as OrderStatus;
        return GetStatusBadge(status);
      },
    },
  ];

  return {
    columns,
    columnOrderServices,
    columnOrderCustomer,
    cancelOrderDialog: (
      <CancelOrderDialog
        open={isCancelDialogOpen}
        onOpenChange={(open) => {
          setIsCancelDialogOpen(open);
          if (!open) setOrderToCancel(null);
        }}
        orderCode={orderToCancel?.code}
        onConfirm={confirmCancelOrder}
        isPending={updateStatus.isPending}
      />
    ),
  };
};
