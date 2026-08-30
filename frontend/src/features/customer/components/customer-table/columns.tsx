"use client";

import { ColumnDef } from "@tanstack/react-table";
import {
  ActivationStatus,
  ListCustomerResponse,
  ListTransactionResponse,
} from "@/api/generated";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
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
import { formatNumberVN } from "@/utils/format";
import { format } from "date-fns";
import { ROUTE_ORDERS_DETAIL } from "@/types/router-type";
import { usePushRouter } from "@/utils/router-utli";
import Image from "next/image";

export interface CustomerTableCallbacks {
  onDetail: (customer: ListCustomerResponse) => void;
  onEdit: (customer: ListCustomerResponse) => void;
}

export const useCustomerTable = ({
  onDetail,
  onEdit,
}: CustomerTableCallbacks) => {
  const t = useTranslations();
  const { processText } = useStringUtil();

  const columns: ColumnDef<ListCustomerResponse>[] = [
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
      accessorKey: "avtUrl",
      header: t("user.avatar.title"),
      cell: ({ row }) => {
        const avatarUrl = (row.getValue("avtUrl") as string) || "";
        const displayName = (row.getValue("displayName") as string) || "?";

        return (
          <Avatar className="h-8 w-8">
            {avatarUrl ? (
              <Image
                src={avatarUrl}
                alt="Avatar"
                className="h-8 w-8 rounded-full object-cover"
                fill
                style={{ objectFit: "contain" }}
              />
            ) : (
              <AvatarFallback className="bg-secondary text-primary">
                {displayName[0].toUpperCase()}
              </AvatarFallback>
            )}
          </Avatar>
        );
      },
    },
    {
      accessorKey: "displayName",
      header: t("user.displayName.title"),
      cell: ({ row }) => {
        const value = row.getValue("displayName");
        return processText(String(value));
      },
    },
    {
      accessorKey: "phoneNumber",
      header: t("user.phoneNumber.title"),
    },
    {
      accessorKey: "customerGroup",
      header: t("user.customerGroup.title"),
      cell: ({ row }) => {
        const value = row.getValue("customerGroup") as string;
        return value ? t(`customer.${value?.toLowerCase()}`) : "--";
      },
    },
    {
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ row }) => {
        const status = row.getValue("status") as string;
        const getStatusColor = (): string => {
          return status === ActivationStatus.Active
            ? "bg-green-100 text-green-800 border-green-200 hover:bg-green-100"
            : "bg-red-100 text-red-800 border-red-200 hover:bg-red-100";
        };

        const getStatusText = (): string => {
          return t(`common.status.${status.toLowerCase()}`);
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
              <DropdownMenuContentComponent
                customer={rowData}
                onDetail={onDetail}
                onEdit={onEdit}
              />
            </DropdownMenu>
          </div>
        );
      },
    },
  ];

  return { columns };
};

export const useCustomerTransactionTable = () => {
  const t = useTranslations();
  const pushRouter = usePushRouter();
  const columns: ColumnDef<ListTransactionResponse>[] = [
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
      accessorKey: "metadata",
      header: t("user.avatar.title"),
      cell: ({ getValue }) => {
        let code = "";
        let publicId = "";
        let referenceId = "";
        const metadataValue = getValue() as any;
        try {
          const metadata = JSON.parse(metadataValue || "{}");
          code = metadata.code;
          publicId = metadata.publicId;
          referenceId = metadata.id;
        } catch {
          code = "";
          referenceId = "";
        }
        if (!code) return <div className="text-gray-400">{"--"}</div>;

        return (
          <Button
            variant={"link"}
            onClick={() =>
              pushRouter.pushRouter({
                router: ROUTE_ORDERS_DETAIL,
                params: {
                  publicId: publicId,
                },
                state: {
                  [publicId]: referenceId!,
                },
                redirect: "blank",
              })
            }
            className="p-0"
          >
            {code}
          </Button>
        );
      },
    },
    {
      accessorKey: "transactionAt",
      header: t("inventory.transactionAt"),
      cell: ({ getValue }) => {
        const value = getValue() as string;
        return value ? format(new Date(value), "dd/MM/yyyy HH:mm") : "--";
      },
    },

    {
      accessorKey: "amount",
      header: t("customer.point"),
      cell: ({ getValue }) => {
        const value = getValue() as number;
        return (
          <div
            className={`text-right ${value < 0 ? "text-destructive" : "text-green-600"}`}
          >
            {formatNumberVN(value)}
          </div>
        );
      },
    },
  ];

  return { columns };
};
