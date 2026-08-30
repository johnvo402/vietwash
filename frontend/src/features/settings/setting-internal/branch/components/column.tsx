"use client";

import { ColumnDef } from "@tanstack/react-table";
import { ActivationStatus, ListBranchResponse } from "@/api/generated";
import { useTranslations } from "next-intl";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { MoreVertical } from "lucide-react";
import { DropdownMenuContentComponent } from "./action-dropdown";

interface BranchSettingTableProps {
  // deleteBranch: (params: { id: number }) => void;
  openEditDialog: (branch: ListBranchResponse) => void;
}

export const useBranchSettingTable = ({
  // deleteBranch,
  openEditDialog,
}: BranchSettingTableProps) => {
  const t = useTranslations();

  const columns: ColumnDef<ListBranchResponse>[] = [
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
    },
    {
      accessorKey: "code",
      header: t("table.accessorKey.code"),
    },
    {
      accessorKey: "contact",
      header: t("user.contact").replace(/^./, (c) => c.toUpperCase()),
      cell: ({ row }) => {
        const { email, phoneNumber } = row.original;
        return (
          <div className="min-w-0 flex-1 min-h-full">
            <div className="flex flex-col gap-1">
              {email || phoneNumber ? (
                <>
                  <span className="text-sm">{email}</span>
                  <span className="text-sm">{phoneNumber}</span>
                </>
              ) : (
                "--"
              )}
            </div>
          </div>
        );
      },
    },
    {
      accessorKey: "address",
      header: t("user.address.title"),
      cell: ({ row }) => {
        const { addressName } = row.original;
        return (
          <div className="min-w-0 flex-1 min-h-full">
            <div className="flex flex-col gap-1">
              {addressName ? (
                <>
                  <span className="text-sm">{addressName}</span>
                </>
              ) : (
                "--"
              )}
            </div>
          </div>
        );
      },
    },
    {
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ row }) => {
        const { main, status } = row.original;
        const getStatusColor = (): string => {
          return status === ActivationStatus.Inactive
            ? "bg-red-100 text-red-800 border-red-200 hover:bg-red-100"
            : "bg-green-100 text-green-800 border-green-200 hover:bg-green-100";
        };

        const getStatusText = (): string => {
          return status === ActivationStatus.Active
            ? t("common.status.active")
            : t("common.status.inactive");
        };

        return (
          <div className="min-w-0 flex-1 min-h-full">
            <div className="flex items-center gap-2 flex-wrap">
              <Badge className={`text-xs ${getStatusColor()}`}>
                {getStatusText()}
              </Badge>
              {main && (
                <Badge
                  className={`text-xs ${
                    main
                      ? "bg-blue-100 text-blue-800 border-blue-200 hover:bg-blue-100"
                      : "bg-gray-100 text-gray-800 border-gray-200 hover:bg-gray-100"
                  }`}
                >
                  {t("branch.main")}
                </Badge>
              )}
            </div>
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
          // <div className="flex space-x-2">
          //   <DropdownMenu>
          //     <DropdownMenuTrigger asChild>
          //       <Button
          //         variant="ghost"
          //         size="icon"
          //         className="h-8 w-8 p-0"
          //         aria-label={t("table.menu")}
          //       >
          //         <MoreVertical className="h-4 w-4" />
          //       </Button>
          //     </DropdownMenuTrigger>
          //     <DropdownMenuContentComponent
          //       rowData={rowData}
          //       openEditDialog={openEditDialog}
          //     />
          //   </DropdownMenu>
          // </div>
          <DropdownMenuContentComponent
            rowData={rowData}
            openEditDialog={openEditDialog}
          />
        );
      },
    },
  ];

  return { columns };
};
