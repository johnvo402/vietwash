"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { formatPriceVN } from "@/utils/format";
import { useTranslations } from "next-intl";
import { Badge } from "@/components/ui/badge";
import {
  InventoryStatus,
  InventoryType,
  ListInventoryDocumentResponse,
} from "@/api/generated/api";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSub,
  DropdownMenuSubTrigger,
  DropdownMenuSubContent,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { MoreHorizontal } from "lucide-react";
import { usePushRouter } from "@/utils/router-utli";
import {
  ROUTE_INVENTORY_DOC_DETAIL,
  ROUTE_INVENTORY_DOC_UPDATE,
} from "@/types/router-type";
import { getBranch } from "@/utils/branch-util";
import { useAuth } from "@/hooks/use-auth";
import { useState, useCallback } from "react";
import { format } from "date-fns";

interface UseInventoryDocumentsProps {
  type: InventoryType;
  onUpdateStatus?: (
    id: number,
    newStatus: InventoryStatus,
    reason?: string
  ) => void;
}

export const useInventoryDocuments = ({
  type,
  onUpdateStatus,
}: UseInventoryDocumentsProps) => {
  const t = useTranslations();
  const router = usePushRouter();
  const { user } = useAuth();
  const [isCancelDialogOpen, setIsCancelDialogOpen] = useState(false);
  const [selectedDocumentId, setSelectedDocumentId] = useState<number | null>(
    null
  );
  const [cancelReason, setCancelReason] = useState("");

  const branchName = (id: number) => {
    const branch = getBranch(id, user);
    return branch?.branchName;
  };

  const handleCancelConfirm = useCallback(() => {
    if (selectedDocumentId && onUpdateStatus) {
      onUpdateStatus(
        selectedDocumentId,
        InventoryStatus.Canceled,
        cancelReason
      );
      setIsCancelDialogOpen(false);
      setCancelReason("");
      setSelectedDocumentId(null);
    }
  }, [selectedDocumentId, cancelReason, onUpdateStatus]);

  const columns: ColumnDef<ListInventoryDocumentResponse>[] = [
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
        return (
          <div className="text-primary font-semibold">
            {row.getValue("code") || "-"}
          </div>
        );
      },
    },
    {
      accessorKey: "branchId",
      header: t("common.branch"),
      maxSize: 50,
      cell: ({ row }) => {
        const branchId = row.getValue("branchId") as number;
        return branchName(branchId) ?? "--";
      },
    },
    {
      accessorKey: "amount",
      header: t("table.accessorKey.amount"),
      size: 120,
      cell: ({ row }) => {
        const amount = Number.parseFloat(row.getValue("amount"));
        return <div className="text-right">{formatPriceVN(amount)}</div>;
      },
    },
    {
      accessorKey: "transactionAt",
      header: t("inventory.transactionAt"),
      cell: ({ row }) => {
        return (
          format(
            new Date(row.getValue("transactionAt")),
            "dd/MM/yy HH:mm:ss"
          ) || "-"
        );
      },
    },
    {
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ row }) => {
        const status = row.getValue("status") as InventoryStatus;

        let variant: "default" | "destructive" | "secondary" | "outline" =
          "secondary";
        let label = "";
        let customClass = "";

        switch (status) {
          case InventoryStatus.Pending:
            variant = "secondary";
            label = t("common.status.pending");
            customClass =
              "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200";
            break;
          case InventoryStatus.Completed:
            variant = "default";
            label = t("common.status.completed");
            customClass =
              "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200";
            break;
          case InventoryStatus.Canceled:
            variant = "destructive";
            label = t("common.status.canceled");
            customClass =
              "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200";
            break;
          default:
            label = t("common.status.unknown");
            customClass =
              "bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200";
            break;
        }

        return (
          <Badge className={customClass} variant={variant}>
            {label}
          </Badge>
        );
      },
    },
    {
      id: "actions",
      header: "",
      enableSorting: false,
      enableHiding: false,
      cell: ({ row }) => {
        const data = row.original;
        const currentStatus = data.status as InventoryStatus;

        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                className="h-8 w-8 p-0 hover:bg-blue-100 dark:hover:bg-blue-900"
              >
                <span className="sr-only">Open menu</span>
                <MoreHorizontal className="h-4 w-4 text-gray-600 dark:text-gray-400" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem
                className=" hover:bg-primary"
                onClick={() =>
                  router.pushRouter({
                    router: ROUTE_INVENTORY_DOC_DETAIL,
                    params: {
                      type: type.toLowerCase(),
                      publicId: data.publicId!.toString(),
                    },
                    state: {
                      [data.publicId!.toString()]: data.id,
                    },
                  })
                }
              >
                {t("common.viewDetails")}
              </DropdownMenuItem>

              {currentStatus === InventoryStatus.Pending && (
                <>
                  <DropdownMenuItem
                    className="text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900"
                    onClick={() =>
                      router.pushRouter({
                        router: ROUTE_INVENTORY_DOC_UPDATE,
                        params: {
                          type: type.toLowerCase(),
                          publicId: data.publicId!.toString(),
                        },
                        state: {
                          [data.publicId!.toString()]: data.id,
                        },
                      })
                    }
                  >
                    {t("common.update")}
                  </DropdownMenuItem>
                </>
              )}
              {onUpdateStatus && currentStatus !== InventoryStatus.Canceled && (
                <DropdownMenuSub>
                  <DropdownMenuSubTrigger className="text-purple-600 dark:text-purple-400 hover:bg-purple-50 dark:hover:bg-purple-900">
                    {t("common.updateStatus")}
                  </DropdownMenuSubTrigger>
                  <DropdownMenuSubContent className="bg-white dark:bg-gray-800">
                    {currentStatus === InventoryStatus.Pending && (
                      <DropdownMenuItem
                        className="text-yellow-800 dark:text-yellow-200 hover:bg-yellow-50 dark:hover:bg-yellow-900"
                        onClick={() =>
                          onUpdateStatus(data.id!, InventoryStatus.Pending)
                        }
                      >
                        {t("common.status.pending")}
                      </DropdownMenuItem>
                    )}
                    {currentStatus === InventoryStatus.Pending && (
                      <DropdownMenuItem
                        className="text-green-800 dark:text-green-200 hover:bg-green-50 dark:hover:dark:bg-green-900"
                        onClick={() =>
                          onUpdateStatus(data.id!, InventoryStatus.Completed)
                        }
                      >
                        {t("common.status.completed")}
                      </DropdownMenuItem>
                    )}
                    {(currentStatus === InventoryStatus.Pending ||
                      currentStatus === InventoryStatus.Completed) && (
                      <DropdownMenuItem
                        className="text-red-800 dark:text-red-200 hover:bg-red-50 dark:hover:bg-red-900"
                        onClick={() => {
                          if (data.id) {
                            setSelectedDocumentId(data.id);
                            setIsCancelDialogOpen(true);
                          }
                        }}
                      >
                        {t("common.status.canceled")}
                      </DropdownMenuItem>
                    )}
                  </DropdownMenuSubContent>
                </DropdownMenuSub>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];

  return {
    columns,
    isCancelDialogOpen,
    setIsCancelDialogOpen,
    selectedDocumentId,
    setSelectedDocumentId,
    cancelReason,
    setCancelReason,
    handleCancelConfirm,
  };
};
