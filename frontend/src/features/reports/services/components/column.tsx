"use client";

import { ColumnDef } from "@tanstack/react-table";
import { formatNumberVN, formatPriceVN } from "@/utils/format";
import { useTranslations } from "next-intl";
import { ServiceRevenueReportResponse } from "@/api/generated";

export const useServiceReportTable = () => {
  const t = useTranslations();

  const columns: ColumnDef<ServiceRevenueReportResponse>[] = [
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
      accessorKey: "serviceName",
      header: t("common.entityName", {
        Entity: t("common.service").toLowerCase(),
      }),
    },
    {
      accessorKey: "unitName",
      header: t("product.unit"),
    },
    {
      accessorKey: "totalOrderCount",
      header: t("common.numberOfOrders"),
      cell: ({ row }) => (
        <div className="text-right">
          {formatNumberVN((row.getValue("totalOrderCount") as number) || 0)}
        </div>
      ),
    },
    {
      accessorKey: "totalRevenue",
      header: t("report.totalRevenue"),
      cell: ({ row }) => (
        <div className="text-right">
          {formatPriceVN((row.getValue("totalRevenue") as number) || 0)}
        </div>
      ),
    },
    {
      accessorKey: "totalDiscount",
      header: t("report.totalDiscount"),
      cell: ({ row }) => (
        <div className="text-right">
          {formatPriceVN((row.getValue("totalDiscount") as number) || 0)}
        </div>
      ),
    },
    {
      accessorKey: "totalNetRevenue",
      header: t("report.totalNetRevenue"),
      cell: ({ row }) => (
        <div className="text-right">
          {formatPriceVN((row.getValue("totalNetRevenue") as number) || 0)}
        </div>
      ),
    },
  ];

  return { columns };
};
