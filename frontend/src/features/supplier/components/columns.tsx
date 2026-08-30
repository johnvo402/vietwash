"use client";

import { ColumnDef } from "@tanstack/react-table";
import { ActivationStatus, ListSupplierResponse } from "@/api/generated";
import { useTranslations } from "next-intl";
import { useStringUtil } from "@/lib/stringUtil";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { MoreVertical } from "lucide-react";
import { DropdownMenuContentComponent } from "./action-dropdown";
import { formatPriceVN } from "@/utils/format";

export const useSupplierTable = () => {
  const t = useTranslations();
  const { processText } = useStringUtil();

  const columns: ColumnDef<ListSupplierResponse>[] = [
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
      header: t("table.accessorKey.name"),
      cell: ({ row }) => {
        const value = row.getValue("name");
        return processText(String(value));
      },
    },
    {
      accessorKey: "code",
      header: t("table.accessorKey.code"),
    },
    {
      accessorKey: "phone",
      header: t("user.phoneNumber.title"),
    },
    {
      accessorKey: "totalInventory",
      header: t("supplier.totalInventory"),
      cell: ({ getValue }) => {
        const totalInventory = getValue() as number;

        return (
          <div className="text-right">{formatPriceVN(totalInventory)}</div>
        );
      },
    },
    {
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ row }) => {
        const status = row.getValue("status");
        const getStatusColor = (): string => {
          return status === ActivationStatus.Active
            ? "bg-green-100 text-green-800 border-green-200 hover:bg-green-100"
            : "bg-red-100 text-red-800 border-red-200 hover:bg-red-100";
        };

        const getStatusText = (): string => {
          return t(`common.status.${(status + "").toLowerCase()}`);
        };

        return (
          <div className="min-w-0 flex-1 min-h-full">
            <Badge className={`text-xs ${getStatusColor()}`}>
              {getStatusText()}
            </Badge>
          </div>
        );
      },
    },
    {
      accessorKey: "actions",
      header: t("table.accessorKey.actions"),
      cell: ({ row }) => {
        const rowData = row.original;

        return (
          <div className="flex space-x-2">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 p-0"
                  aria-label={t("table.menu")}
                >
                  <MoreVertical className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContentComponent rowData={rowData} />
            </DropdownMenu>
          </div>
        );
      },
    },
  ];

  return { columns };
};
