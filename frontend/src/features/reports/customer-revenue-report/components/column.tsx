"use client";

import { ColumnDef } from "@tanstack/react-table";
import { useStringUtil } from "@/lib/stringUtil";
import { formatPriceVN } from "@/utils/format";
import { useRouter } from "next/navigation";
import { usePushRouter } from "@/utils/router-utli";
import { useTranslations } from "next-intl";

// Giả định type mới
type CustomerRevenueResult = {
  customerId: number;
  customerCode: string;
  phoneNumber: string;
  displayName: string;
  avtUrl: string;
  revenue: number;
  cancelValue: number;
  netRevenue: number;
  orderSaleQuantity: number;
  orderCancelQuantity: number;
};

export const useCustomerRevenueTable = () => {
  const { processText } = useStringUtil();
  const t = useTranslations();
  const columns: ColumnDef<CustomerRevenueResult>[] = [
    {
      header: "STT",
      cell: ({ row, table }) => {
        const pageIndex = table.getState().pagination.pageIndex;
        const pageSize = table.getState().pagination.pageSize;
        return pageIndex * pageSize + row.index + 1;
      },
    },
    {
      accessorKey: "displayName",
      header: t("common.entityName", {
        Entity: t("common.customer").replace(/^./, (c) => c.toUpperCase()),
      }),
      cell: ({ row }) => {
        const name = row.getValue("displayName") as string;
        const code = row.original.customerCode;
        return (
          <div className="flex flex-col">
            <span className="font-medium text-gray-900">{name}</span>
            <span className="text-sm text-gray-500">{code}</span>
          </div>
        );
      },
    },
    {
      accessorKey: "phoneNumber",
      header: t("user.phoneNumber.title"),
      cell: ({ row }) => row.getValue("phoneNumber") as string,
    },
    {
      accessorKey: "orderSaleQuantity",
      header: t("common.numberOfOrders"),
      cell: ({ row }) => row.getValue("orderSaleQuantity"),
      meta: {
        body: { className: "text-right" },
      },
    },
    {
      accessorKey: "orderCancelQuantity",
      header: t("common.numberOfCancelOrders"),
      meta: {
        body: { className: "text-right" },
      },
    },
    {
      accessorKey: "revenue",
      header: t("revenue.title"),
      cell: ({ row }) => (
        <div className="text-right">
          {formatPriceVN(row.getValue("revenue"))}
        </div>
      ),
    },
    {
      accessorKey: "cancelValue",
      header: t("common.cancelValue"),
      cell: ({ row }) => (
        <div className="text-right">
          {formatPriceVN(row.getValue("cancelValue"))}
        </div>
      ),
    },
    {
      accessorKey: "netRevenue",
      header: t("revenue.netRevenue"),
      cell: ({ row }) => (
        <div className="text-right font-medium text-green-700">
          {formatPriceVN(
            Number(row.getValue("revenue")) -
              Number(row.getValue("cancelValue"))
          )}
        </div>
      ),
    },
  ];

  return { columns };
};
