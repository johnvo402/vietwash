"use client";

import { ColumnDef } from "@tanstack/react-table";
import { formatNumberVN, formatPriceVN } from "@/utils/format";
import { useTranslations } from "next-intl";
import { ProductSupplierReportResponse } from "@/api/generated";
import { useAuth } from "@/hooks/use-auth";

export const useSupplierReportTable = () => {
  const t = useTranslations();
  const { user } = useAuth();
  const getBranchName = (branchId: number): string => {
    const branch = user?.branchAccounts?.find(
      (account) => account.branchId === branchId
    );
    return branch?.branchName || String(branchId);
  };

  const columns: ColumnDef<ProductSupplierReportResponse>[] = [
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
      accessorKey: "name",
      header: t("common.entityName", {
        Entity: t("common.supplier").toLowerCase(),
      }),
    },
    {
      accessorKey: "code",
      header: t("table.accessorKey.code"),
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
      accessorKey: "supplierProductTypeCount",
      header: t("report.supplier.supplierProductTypeCount"),
      cell: ({ getValue }) => (
        <div className="text-right">
          {formatNumberVN((getValue() as number) || 0)}
        </div>
      ),
    },
    {
      accessorKey: "importedValueTotal",
      header: t("report.supplier.importedValueTotal"),
      cell: ({ getValue }) => (
        <div className="text-right">
          {formatPriceVN((getValue() as number) || 0)}
        </div>
      ),
    },
    {
      accessorKey: "exportedValueTotal",
      header: t("report.supplier.exportedValueTotal"),
      cell: ({ getValue }) => (
        <div className="text-right">
          {formatPriceVN((getValue() as number) || 0)}
        </div>
      ),
    },
  ];

  return { columns };
};
