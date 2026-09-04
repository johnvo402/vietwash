"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { formatNumberVN, formatPriceVN } from "@/utils/format";
import Image from "next/image";
import { useTranslations } from "next-intl";
import { Badge } from "@/components/ui/badge";

import {
  BranchProductCardInventoryResponse,
  ListBranchProductResponse,
} from "@/api/generated/api";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { MoreVertical } from "lucide-react";
import { format } from "date-fns";

export const useBranchProducts = (
  onAction?: (
    action: "detail" | "edit",
    row: ListBranchProductResponse,
  ) => void,
) => {
  const t = useTranslations();

  const columns: ColumnDef<ListBranchProductResponse>[] = [
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
      accessorKey: "image",
      header: t("common.image"),
      cell: ({ row }) => {
        const imageUrl = row.getValue("image") as string;
        return (
          <Image
            src={imageUrl ?? "/logo/favicon.svg"}
            alt="Product Image"
            width={50}
            height={50}
            className="object-cover rounded"
          />
        );
      },
    },
    {
      accessorKey: "name",
      header: t("product.productName"),
    },
    {
      accessorKey: "sku",
      header: t("table.accessorKey.sku"),
    },
    {
      accessorKey: "category.name",
      header: t("product.category"),
      cell: ({ row }) => {
        return row.original.category?.name || "-";
      },
    },
    {
      accessorKey: "unitRelations",
      header: t("product.unit"),
      cell: ({ row }) => {
        const baseUnit = row.original.unitRelations?.find(
          (unit) => unit.baseUnit,
        );
        return <div>{baseUnit ? baseUnit.name : "--"}</div>;
      },
    },
    {
      accessorKey: "capitalPrice",
      header: t("product.capitalPrice"),
      cell: ({ row }) => {
        const price = Number.parseFloat(row.getValue("capitalPrice"));
        return (
          <div className="text-right font-medium">{formatPriceVN(price)}</div>
        );
      },
    },
    {
      accessorKey: "stockQuantity",
      header: t("product.stockQuantity"),
      cell: ({ row }) => {
        const stockQuantity = Number.parseFloat(row.getValue("stockQuantity"));
        return (
          <div className="text-center font-medium">
            {formatNumberVN(stockQuantity)}
          </div>
        );
      },
    },

    {
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ row }) => {
        const status = row.getValue("status") as string;
        const isActive = status === "Active";
        return (
          <Badge variant={isActive ? "default" : "destructive"}>
            {isActive ? t("common.status.active") : t("common.status.inactive")}
          </Badge>
        );
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
  const columnCardInv: ColumnDef<BranchProductCardInventoryResponse>[] = [
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
      header: t("table.accessorKey.code"),
    },
    {
      accessorKey: "transactionAt",
      header: t("inventory.transactionAt"),
      cell: ({ row }) => {
        const transactionAt = row.getValue("transactionAt") as string;
        return format(new Date(transactionAt!), "dd/MM/yyyy HH:mm");
      },
    },
    {
      accessorKey: "quantity",
      header: t("table.accessorKey.quantity"),
      cell: ({ row }) => {
        const quantity = row.getValue("quantity") as number;
        return (
          <div
            className={`${quantity > 0 ? "text-green-500" : "text-destructive"} font-medium`}
          >
            {quantity}
          </div>
        );
      },
    },
  ];
  return { columns, columnCardInv };
};
