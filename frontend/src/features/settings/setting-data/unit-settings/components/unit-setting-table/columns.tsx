"use client";

import { ColumnDef } from "@tanstack/react-table";
import { ActivationStatus, ListUnitResponse } from "@/api/generated";
import { useTranslations } from "next-intl";
import { Badge } from "@/components/ui/badge";
import { DropdownMenuContentComponent } from "./action-dropdown";

interface FormValues {
  name: string;
  status: 0 | 1;
}

interface UnitSettingTableProps {
  // deleteUnit: (params: { id: number }) => void;
  openEditDialog: (unit: ListUnitResponse) => void;
}

export const useUnitSettingTable = ({
  // deleteUnit,
  openEditDialog,
}: UnitSettingTableProps) => {
  const t = useTranslations();

  const columns: ColumnDef<ListUnitResponse>[] = [
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
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ getValue }) => {
        const value = getValue();
        const getStatusColor = (): string => {
          return value === ActivationStatus.Inactive
            ? "bg-red-100 text-red-800 border-red-200 hover:bg-red-100"
            : "bg-green-100 text-green-800 border-green-200 hover:bg-green-100";
        };

        const getStatusText = (): string => {
          return value === ActivationStatus.Inactive
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
          <DropdownMenuContentComponent
            rowData={rowData}
            // deleteUnit={deleteUnit}
            openEditDialog={openEditDialog}
          />
        );
      },
    },
  ];

  return { columns };
};
