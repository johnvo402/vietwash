"use client";

import { ColumnDef } from "@tanstack/react-table";
import { ImportExportHistoryResponse, InventoryType } from "@/api/generated";
import { useTranslations } from "next-intl";
import { formatPriceVN } from "@/utils/format";
import { format } from "date-fns";
import { usePushRouter } from "@/utils/router-utli";
import { Button } from "@/components/ui/button";
import { ROUTE_INVENTORY_DOC_DETAIL } from "@/types/router-type";
const getType = (t: any, type: InventoryType) => {
  switch (type) {
    case InventoryType.Import:
      return <div className="text-primary">{t(`inventory.detail.import`)}</div>;
    case InventoryType.Export:
      return (
        <div className="text-destructive">{t(`inventory.detail.export`)}</div>
      );
    default:
      return <div>--</div>;
  }
};
export const useSupplierHistoryImExTable = () => {
  const t = useTranslations();
  const { pushRouter } = usePushRouter();
  const columns: ColumnDef<ImportExportHistoryResponse>[] = [
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
      accessorKey: "documentCode",
      header: t("inventory.code"),
      cell: ({ row, getValue }) => {
        const original = row.original;
        const value = getValue() as string;
        const invDocId = original.invDocId;
        const invDocPublicId = original.invDocPublicId;
        const type = original.type?.toString();
        return (
          <Button
            variant={"link"}
            onClick={() =>
              pushRouter({
                router: ROUTE_INVENTORY_DOC_DETAIL,
                params: {
                  publicId: invDocPublicId?.toString()!,
                  type: type?.toLowerCase()!,
                },
                state: {
                  [invDocPublicId?.toString()!]: invDocId,
                },
              })
            }
          >
            {value || "--"}
          </Button>
        );
      },
    },
    {
      accessorKey: "transactionAt",
      header: t("inventory.transactionAt"),
      cell: ({ row }) => {
        const rawDate = row.getValue("transactionAt") as string | null;
        if (!rawDate) return <div>--</div>;
        const date = new Date(rawDate);
        return <div>{format(date, "dd/MM/yy HH:mm:ss")}</div>;
      },
    },
    {
      accessorKey: "type",
      header: t("inventory.type"),
      cell: ({ row }) => {
        const type = row.getValue("type") as InventoryType;
        if (!type) return <div>--</div>;
        return getType(t, type);
      },
    },
    {
      accessorKey: "total",
      header: t("table.accessorKey.total"),
      cell: ({ getValue }) => {
        const totalInventory = getValue() as number;
        return (
          <div className="text-right">{formatPriceVN(totalInventory)}</div>
        );
      },
    },
  ];

  return { columns };
};
