"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { format } from "date-fns";
import { Button } from "@/components/ui/button";
import { formatPriceVN } from "@/utils/format";
import { usePushRouter } from "@/utils/router-utli";
import { useTranslations } from "next-intl";
import {
  ROUTE_CASHIER_ORDERS_DETAIL,
  ROUTE_ORDERS_DETAIL,
} from "@/types/router-type";
import { GetCustomerGroup, GetStatusBadge } from "../../order-utils/order-util";
import { ListOrderResponse, OrderStatus } from "@/api/generated/api";
import { usePageType } from "@/hooks/use-page-type";
import { OrderActionMenu } from "../order-action-menu";

export const useOrder = ({
  onEdit,
}: { onEdit?: (id: number) => void } = {}) => {
  const pushRouter = usePushRouter();
  const t = useTranslations();
  const { isCashierPage } = usePageType();

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
      cell: ({ row }) => (
        <div className="text-right">
          <OrderActionMenu
            order={row.original}
            onEdit={onEdit}
            onView={() => {
              const order = row.original;
              pushRouter.pushRouter({
                router: isCashierPage
                  ? ROUTE_CASHIER_ORDERS_DETAIL
                  : ROUTE_ORDERS_DETAIL,
                params: { publicId: order.publicId?.toString()! },
                state: { [order.publicId?.toString()!]: order.id! },
              });
            }}
          />
        </div>
      ),
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

  return { columns, columnOrderServices, columnOrderCustomer };
};
