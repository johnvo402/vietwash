"use client";

import { format } from "date-fns";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogFooter,
} from "@/components/ui/alert-dialog";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { forwardRef, useState } from "react";
import { CustomerGroup, ListOrderResponse, OrderStatus } from "@/api/generated";
import { usePushRouter } from "@/utils/router-utli";
import {
  ROUTE_CASHIER_ORDERS_DETAIL,
  ROUTE_ORDERS_DETAIL,
} from "@/types/router-type";
import { formatPriceVN } from "@/utils/format";
import { apiClient } from "@/api/client";
import {
  Clock,
  Truck,
  Package,
  CheckCircle,
  XCircle,
  MoreHorizontal,
} from "lucide-react";
import { GetCustomerGroup, GetStatusBadge } from "../../order-utils/order-util";
import { useTranslations } from "next-intl";
import { usePageType } from "@/hooks/use-page-type";

// Định nghĩa các trạng thái đơn dịch vụ cho giao diện
const statusOrder = ["pending", "handling", "handled", "completed"];

// Ánh xạ trạng thái số sang chuỗi
const numberToStatusMap: Record<OrderStatus, string> = {
  [OrderStatus.Pending]: "pending",
  [OrderStatus.InProgress]: "handling",
  [OrderStatus.Processed]: "handled",
  [OrderStatus.Completed]: "completed",
  [OrderStatus.Cancelled]: "cancelled",
};

// Ánh xạ trạng thái chuỗi sang số
const statusToNumberMap: Record<string, OrderStatus> = {
  pending: OrderStatus.Pending,
  handling: OrderStatus.InProgress,
  handled: OrderStatus.Processed,
  completed: OrderStatus.Completed,
  cancelled: OrderStatus.Cancelled,
};

// Ánh xạ hiển thị trạng thái với icon và màu sắc
const statusDisplayMap: Record<
  string,
  { icon: any; textColor: string; bgColor: string; label: string }
> = {
  pending: {
    icon: Clock,
    textColor: "text-yellow-600",
    bgColor: "",
    label: "Pending",
  },
  handling: {
    icon: Truck,
    textColor: "text-blue-600",
    bgColor: "",
    label: "In Progress",
  },
  handled: {
    icon: Package,
    textColor: "text-purple-600",
    bgColor: "",
    label: "Processed",
  },
  completed: {
    icon: CheckCircle,
    textColor: "text-green-600",
    bgColor: "",
    label: "Completed",
  },
  cancelled: {
    icon: XCircle,
    textColor: "text-red-600",
    bgColor: "",
    label: "Cancelled",
  },
};

interface OrderCardProps {
  order: ListOrderResponse;
  onEdit: (orderId: number) => void; // Thêm prop onEdit để xử lý hành động cập nhật
}

// Wrap OrderCard with forwardRef to support ref prop
export const OrderCard = forwardRef<HTMLDivElement, OrderCardProps>(
  ({ order, onEdit }, ref) => {
    const t = useTranslations();
    const pushRouter = usePushRouter();
    const queryClient = useQueryClient();
    const [isCancelDialogOpen, setIsCancelDialogOpen] = useState(false);
    const [orderToCancel, setOrderToCancel] = useState<{
      id: string;
      code: string;
    } | null>(null);
    const { isCashierPage } = usePageType();

    // Mutation để cập nhật trạng thái đơn dịch vụ
    const updateStatus = useMutation({
      mutationFn: ({ id, status }: { id: string; status: OrderStatus }) =>
        apiClient.ecommerceApiOrdersUpdateStatusidPut(id, { status }),
      onSuccess: (data) => {
        queryClient.invalidateQueries({
          queryKey: ["orders"],
        });
        alert(
          t("toast.update.success", {
            entity:
              t("common.status.title").charAt(0).toLowerCase() +
              t("common.status.title").slice(1),
          }) +
            t("order.title") +
            "!"
        );
      },
      onError: (error) => {
        console.error("Lỗi khi cập nhật trạng thái:", error);
        alert(t("order.updateOrderStatusFailed"));
      },
    });

    // Hàm xử lý hủy đơn dịch vụ
    const handleCancelOrder = (orderId: string, code: string) => {
      setOrderToCancel({ id: orderId, code });
      setIsCancelDialogOpen(true);
    };

    // Hàm xác nhận hủy đơn dịch vụ
    const confirmCancelOrder = () => {
      if (orderToCancel) {
        updateStatus.mutate({
          id: orderToCancel.id,
          status: OrderStatus.Cancelled,
        });
      }
      setIsCancelDialogOpen(false);
      setOrderToCancel(null);
    };

    // Hàm thay đổi trạng thái đơn dịch vụ
    const handleChangeStatus = (orderId: string, newStatus: string) => {
      updateStatus.mutate({
        id: orderId,
        status: statusToNumberMap[newStatus],
      });
    };

    // Hàm xử lý xem chi tiết
    const handleViewDetails = (publicId: string, orderId: number) => {
      pushRouter.pushRouter({
        router: isCashierPage
          ? ROUTE_CASHIER_ORDERS_DETAIL
          : ROUTE_ORDERS_DETAIL,
        params: { publicId },
        state: { [publicId]: orderId },
      });
    };

    // Chuyển đổi status từ number sang chuỗi để hiển thị
    const currentStatus =
      order.status !== undefined
        ? numberToStatusMap[order.status]
        : t("common.nA");

    return (
      <Card className="overflow-hidden" ref={ref}>
        <CardHeader className="pb-2 flex flex-row justify-between items-center">
          <CardTitle className="text-lg">{order.code}</CardTitle>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="h-8 w-8 p-0">
                <span className="sr-only">{t("common.openMenu")}</span>
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuLabel>
                {t("table.accessorKey.actions")}
              </DropdownMenuLabel>
              <DropdownMenuItem
                onClick={() =>
                  handleViewDetails(order.publicId!.toString(), order.id!)
                }
              >
                {t("common.viewDetails")}
              </DropdownMenuItem>
              {order.status === OrderStatus.Pending && (
                <>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={() => onEdit(order.id!)}>
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
                    {t("common.cancel")} {t("order.title")}
                  </DropdownMenuItem>
                </>
              )}
              {statusOrder.map((status) => {
                const currentStatusIndex = statusOrder.indexOf(currentStatus);
                const statusIndex = statusOrder.indexOf(status);
                if (
                  statusIndex > currentStatusIndex &&
                  status !== "cancelled" &&
                  status !== "completed"
                ) {
                  const {
                    icon: Icon,
                    textColor,
                    label,
                  } = statusDisplayMap[status];
                  return (
                    <DropdownMenuItem
                      key={status}
                      onClick={() =>
                        handleChangeStatus(order.id!.toString(), status)
                      }
                      className={textColor}
                    >
                      <Icon className="mr-2 h-4 w-4" />
                      {t("order.setTo")} {label}
                    </DropdownMenuItem>
                  );
                }
                return null;
              })}
            </DropdownMenuContent>
          </DropdownMenu>
        </CardHeader>
        <CardContent className="pb-2">
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {t("table.accessorKey.createdAt")}:
              </span>
              <span>
                {order.createdAt
                  ? format(
                      new Date(order.createdAt ?? new Date()),
                      "dd/MM/yyyy"
                    )
                  : "--"}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {t("common.customer").charAt(0).toUpperCase() +
                  t("common.customer").slice(1)}
                :
              </span>
              <span>{order.customer?.displayName || t("common.nA")}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {t("user.phoneNumber.title")}:
              </span>
              <span>{order.customer?.phoneNumber || t("common.nA")}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {t("user.customerGroup.title")}:
              </span>
              <span>
                {GetCustomerGroup(
                  t,
                  order.customer?.customerGroup || CustomerGroup.Normal
                )}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {t("table.accessorKey.amount")}:
              </span>
              <span>{order.amount}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {t("table.accessorKey.total")}:
              </span>
              <span className="font-medium text-right">
                {order.total ? formatPriceVN(order.total) : t("common.nA")}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {t("common.status.title")}:
              </span>
              <span>
                {order.status !== undefined ? (
                  GetStatusBadge(order.status)
                ) : (
                  <Badge>{t("common.nA")}</Badge>
                )}
              </span>
            </div>
          </div>
        </CardContent>
        <CardFooter className="pt-2"></CardFooter>
        {/* AlertDialog để xác nhận hủy đơn dịch vụ */}
        <AlertDialog
          open={isCancelDialogOpen}
          onOpenChange={setIsCancelDialogOpen}
        >
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>
                {t("common.deleteConfirm.title", {
                  entity: t("order.title"),
                })}
              </AlertDialogTitle>
              <AlertDialogDescription>
                {t("common.deleteConfirm.description", {
                  entity: t("order.title"),
                  entityName: orderToCancel?.code,
                })}
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel onClick={() => setOrderToCancel(null)}>
                {t("common.deleteConfirm.cancel")}
              </AlertDialogCancel>
              <AlertDialogAction onClick={confirmCancelOrder}>
                {t("common.deleteConfirm.confirm")}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </Card>
    );
  }
);

OrderCard.displayName = "OrderCard";
