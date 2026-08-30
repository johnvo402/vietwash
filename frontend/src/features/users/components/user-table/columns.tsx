"use client";

import { ColumnDef } from "@tanstack/react-table";
import { ActivationStatus, ListAccountResponse } from "@/api/generated";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
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
import Image from "next/image";

export const useUserTable = () => {
  const t = useTranslations();
  const { processText } = useStringUtil();

  const columns: ColumnDef<ListAccountResponse>[] = [
    {
      accessorKey: "index",
      header: "#",
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
                className="h-8 w-8 rounded-full object-contain"
                fill
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
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ row }) => {
        const status = row.getValue("status");
        const getStatusColor = (): string => {
          return status === ActivationStatus.Active
            ? "text-green-800"
            : "text-red-800";
        };

        const getStatusText = (): string => {
          return t(`common.status.${String(status).toLowerCase()}`);
        };

        return (
          <div
            className={`min-w-0 flex-1 min-h-full text-xs ${getStatusColor()}`}
          >
            {getStatusText()}
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
