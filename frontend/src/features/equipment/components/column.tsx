"use client";

import { ColumnDef } from "@tanstack/react-table";
import { ListEquipmentActivityResponse } from "@/api/generated";
import { formatPriceVN } from "@/utils/format";
import { useTranslations } from "next-intl";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { MoreVertical } from "lucide-react";

export const useEquipmentActivityTable = (
  onAction?: (
    action: "detail" | "edit",
    row: ListEquipmentActivityResponse,
  ) => void,
) => {
  const t = useTranslations();

  const columns: ColumnDef<ListEquipmentActivityResponse>[] = [
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
      accessorKey: "type",
      header: t("equipment.activityType.title"),
      cell: ({ row }) =>
        t(
          `equipment.activityType.${(row.getValue("type") as string).toLowerCase()}`,
        ),
    },
    {
      accessorKey: "supervisorName",
      header: t("common.entityName", {
        Entity: t("user.staffInformation").toLowerCase(),
      }),
    },
    {
      accessorKey: "laborCost",
      header: t("equipment.activity.laborCost"),
      cell: ({ row }) => (
        <div className="text-right">
          {formatPriceVN(row.getValue("laborCost"))}
        </div>
      ),
    },
    {
      accessorKey: "totalCost",
      header: t("inventory.totalAmount"),
      cell: ({ row }) => (
        <div className="text-right">
          {formatPriceVN(row.getValue("totalCost"))}
        </div>
      ),
    },
    {
      accessorKey: "createdAt",
      header: t("table.accessorKey.createdAt"),
      cell: ({ row }) => {
        const value = row.getValue("createdAt");
        const date = new Date(String(value));
        return date.toLocaleString();
      },
    },
    {
      id: "actions",
      header: t("table.accessorKey.actions"),
      cell: ({ row }) => (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button
              variant="ghost"
              size="sm"
              className="h-11 w-11"
              aria-label={t("common.openMenu")}
            >
              <MoreVertical className="h-5 w-5" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent>
            <DropdownMenuItem
              onClick={() => onAction?.("detail", row.original)}
            >
              {t("common.details")}
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => onAction?.("edit", row.original)}>
              {t("common.edit")}
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    },
  ];

  return { columns };
};
