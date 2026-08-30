"use client";

import { ColumnDef } from "@tanstack/react-table";
import { OrderSummaryResult } from "@/api/generated";
import { useAuth } from "@/hooks/use-auth";
import { useRouter } from "next/navigation";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_ORDERS_DETAIL } from "@/types/router-type";
import { formatNumberVN, formatPriceVN } from "@/utils/format";
import { useTranslations } from "next-intl";
import { formatDate } from "date-fns";

export const useOrderReportTable = () => {
  const { user } = useAuth();
  const pushRouter = usePushRouter();
  const t = useTranslations();

  const getBranchName = (branchId: number): string => {
    const branch = user?.branchAccounts?.find(
      (account) => account.branchId === branchId
    );
    return branch?.branchName || String(branchId);
  };

  const columns: ColumnDef<OrderSummaryResult>[] = [
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
      header: t("order.orderCode").replace(/^./, (c) => c.toUpperCase()),
      cell: ({ row }) => {
        const order = row.original;
        const code = row.getValue("code") as string;

        if (!order.publicId) {
          return code;
        }

        return (
          <span
            style={{ cursor: "pointer", color: "#1E90FF" }}
            role="button"
            tabIndex={0}
            onClick={() => {
              pushRouter.pushRouter({
                router: ROUTE_ORDERS_DETAIL,
                params: {
                  publicId: order.publicId!.toString(),
                },
                state: {
                  [order.publicId!.toString()]: order.orderId,
                },
                redirect: "blank",
              });
            }}
          >
            {code}
          </span>
        );
      },
    },
    {
      accessorKey: "branchId",
      header: t("common.branch").replace(/^./, (c) => c.toUpperCase()),
      cell: ({ row }) => {
        const branchId = row.getValue("branchId") as number;
        return getBranchName(branchId);
      },
    },
    {
      accessorKey: "customerName",
      header: t("order.customerName"),
    },
    {
      accessorKey: "orderItemCount",
      header: t("service.numberOfServices"),
      cell: ({ getValue }) => (
        <div className="text-right">{formatNumberVN(getValue() as number)}</div>
      ),
    },
    {
      accessorKey: "amount",
      header: t("table.accessorKey.amount"),
      cell: ({ row }) => (
        <div className="text-right">
          {formatPriceVN(row.getValue("amount"))}
        </div>
      ),
    },
    {
      accessorKey: "orderDate",
      header: t("order.orderDate"),
      cell: ({ row }) => {
        const value = row.getValue("orderDate") as string;
        const date = new Date(value);
        return formatDate(date, "dd/MM/yyyy HH:mm"); // Format ngày kiểu Việt
      },
    },
  ];

  return { columns };
};
