"use client";
import { ListUnitResponse } from "@/api/generated";
import { useTranslations } from "next-intl";

import { SquarePen, Trash2 } from "lucide-react";

interface DropdownMenuContentProps {
  rowData: ListUnitResponse;
  // deleteUnit: (params: { id: number }) => void;
  openEditDialog: (unit: ListUnitResponse) => void;
}

export const DropdownMenuContentComponent = ({
  rowData,
  // deleteUnit,
  openEditDialog,
}: DropdownMenuContentProps) => {
  const t = useTranslations();

  return (
    <div className="flex space-x-2">
      <SquarePen
        onClick={() => {
          openEditDialog(rowData);
        }}
        className="size-4 text-primary cursor-pointer"
      />
      {/* <Trash2
        onClick={() => {
          deleteUnit({ id: rowData.id! });
        }}
        className="size-4 text-destructive cursor-pointer"
      /> */}
    </div>
  );
};
