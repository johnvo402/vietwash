"use client";

import { ColumnDef } from "@tanstack/react-table";
import { ListTariffResponse, ActivationStatus } from "@/api/generated";
import { useTranslations } from "next-intl";
import { Badge } from "@/components/ui/badge";
import { DropdownMenuContentComponent } from "./action-dropdown";
import { format } from "date-fns";

interface TariffSettingTableProps {
  openEditDialog: (tariff: ListTariffResponse) => void;
  openDetailDialog: (tariff: ListTariffResponse) => void;
}

export const useTariffTable = ({
  openEditDialog,
  openDetailDialog,
}: TariffSettingTableProps) => {
  const t = useTranslations();

  const columns: ColumnDef<ListTariffResponse>[] = [
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
      accessorKey: "startAt",
      header: t("table.accessorKey.startAt"),
      cell: ({ row }) => {
        return (
          <div>
            {row.getValue("startAt")
              ? format(new Date(row.getValue("startAt")), "dd/MM/yy HH:mm:ss")
              : "-"}
          </div>
        );
      },
    },
    {
      accessorKey: "endAt",
      header: t("table.accessorKey.endAt"),
      cell: ({ row }) => {
        return (
          <div>
            {row.getValue("endAt")
              ? format(new Date(row.getValue("endAt")), "dd/MM/yy HH:mm:ss")
              : "-"}
          </div>
        );
      },
    },

    {
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ row }) => {
        const { status } = row.original;
        const getStatusColor = (): string => {
          return status === ActivationStatus.Inactive
            ? "bg-red-100 text-red-800 border-red-200 hover:bg-red-100"
            : "bg-green-100 text-green-800 border-green-200 hover:bg-green-100";
        };

        const getStatusText = (): string => {
          return status === ActivationStatus.Inactive
            ? t("common.status.inactive")
            : t("common.status.active");
        };

        return (
          <div className="min-w-0 flex-1 min-h-full">
            <div className="flex items-center gap-2 flex-wrap">
              <Badge className={`text-xs ${getStatusColor()}`}>
                {getStatusText()}
              </Badge>
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
          <div className="flex space-x-2">
            <DropdownMenuContentComponent
              rowData={rowData}
              openEditDialog={openEditDialog}
              openDetailDialog={openDetailDialog}
            />
          </div>
        );
      },
    },
  ];

  return { columns };
};
