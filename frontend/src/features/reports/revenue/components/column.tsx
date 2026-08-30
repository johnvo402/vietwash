"use client";

import { ColumnDef } from "@tanstack/react-table";
import { formatNumberVN, formatPriceVN } from "@/utils/format";
import { useTranslations } from "next-intl";
import { RevenueReportResponse } from "@/api/generated";
import { useAuth } from "@/hooks/use-auth";
import { format } from "date-fns";

export const useRevenueReportTable = () => {
  const t = useTranslations();
  const { user } = useAuth();
  const getBranchName = (branchId: number): string => {
    const branch = user?.branchAccounts?.find(
      (account) => account.branchId === branchId
    );
    return branch?.branchName || String(branchId);
  };

  const columns: ColumnDef<RevenueReportResponse>[] = [
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
      accessorKey: "date",
      header: t("user.review.sortDate"),
      cell: ({ getValue }) => {
        const date = getValue() as string;
        return date ? format(new Date(date), "dd/MM/yyyy") : "--";
      },
    },
    {
      accessorKey: "branchId",
      header: t("branch.title"),
      cell: ({ getValue }) => {
        const branchId = getValue() as number;
        return getBranchName(branchId);
      },
    },
    {
      accessorKey: "customerQuantity",
      header: t("report.revenue.customerQuantity"),
      cell: ({ getValue }) => (
        <div className="text-right">
          {formatNumberVN((getValue() as number) || 0)}
        </div>
      ),
    },
    {
      accessorKey: "orderQuantity",
      header: t("report.revenue.orderQuantity"),
      cell: ({ getValue }) => (
        <div className="text-right">
          {formatNumberVN((getValue() as number) || 0)}
        </div>
      ),
    },
    {
      accessorKey: "totalRevenue",
      header: t("report.revenue.totalRevenue"),
      cell: ({ getValue }) => (
        <div className="text-right">
          {formatPriceVN((getValue() as number) || 0)}
        </div>
      ),
    },
   {
      accessorKey: "totalDiscount",
      header: t("report.revenue.totalDiscount"),
      cell: ({ getValue }) => (
        <div className="text-right">
          {formatPriceVN((getValue() as number) || 0)}
        </div>
      ),
    },
    {
      accessorKey: "totalNetRevenue",
      header: t("report.revenue.totalNetRevenue"),
      cell: ({ getValue }) => (
        <div className="text-right">
          {formatPriceVN((getValue() as number) || 0)}
        </div>
      ),
    },
    {
      accessorKey: "averageRevenuePerOrder",
      header: t("report.revenue.averageRevenuePerOrder"),
      cell: ({ getValue }) => (
        <div className="text-right">
          {formatPriceVN((getValue() as number) || 0)}
        </div>
      ),
    },
    {
      accessorKey: "averageRevenuePerCustomer",
      header: t("report.revenue.averageRevenuePerCustomer"),
      cell: ({ getValue }) => (
        <div className="text-right">
          {formatPriceVN((getValue() as number) || 0)}
        </div>
      ),
    },
  ];

  return { columns };
};
