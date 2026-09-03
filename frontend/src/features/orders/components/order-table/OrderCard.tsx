"use client";

import { format } from "date-fns";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { forwardRef } from "react";
import { CustomerGroup, ListOrderResponse } from "@/api/generated";
import { usePushRouter } from "@/utils/router-utli";
import {
  ROUTE_CASHIER_ORDERS_DETAIL,
  ROUTE_ORDERS_DETAIL,
} from "@/types/router-type";
import { formatPriceVN } from "@/utils/format";
import { GetCustomerGroup, GetStatusBadge } from "../../order-utils/order-util";
import { useTranslations } from "next-intl";
import { usePageType } from "@/hooks/use-page-type";
import { OrderActionMenu } from "../order-action-menu";

interface OrderCardProps {
  order: ListOrderResponse;
  onEdit: (orderId: number) => void; // Thêm prop onEdit để xử lý hành động cập nhật
}

// Wrap OrderCard with forwardRef to support ref prop
export const OrderCard = forwardRef<HTMLDivElement, OrderCardProps>(
  ({ order, onEdit }, ref) => {
    const t = useTranslations();
    const pushRouter = usePushRouter();
    const { isCashierPage } = usePageType();

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

    return (
      <Card
        className="overflow-hidden"
        ref={ref}
        data-testid={`order-card-${order.id}`}
      >
        <CardHeader className="pb-2 flex flex-row justify-between items-center">
          <CardTitle className="text-lg">{order.code}</CardTitle>
          <OrderActionMenu
            order={order}
            onEdit={onEdit}
            onView={() =>
              handleViewDetails(order.publicId!.toString(), order.id!)
            }
          />
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
                      "dd/MM/yyyy",
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
                  order.customer?.customerGroup || CustomerGroup.Normal,
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
      </Card>
    );
  },
);

OrderCard.displayName = "OrderCard";
