"use client";

import { ListTariffResponse, TariffModel } from "@/api/generated";
import { Eye, SquarePen } from "lucide-react";

interface DropdownMenuContentProps {
  rowData: ListTariffResponse;
  openEditDialog: (tariff: TariffModel) => void;
  openDetailDialog: (tariff: TariffModel) => void;
}

export const DropdownMenuContentComponent = ({
  rowData,
  openEditDialog,
  openDetailDialog,
}: DropdownMenuContentProps) => {
  return (
    <div className="flex justify-around gap-2">
      <Eye
        onClick={() => {
          openDetailDialog(rowData);
        }}
        className="size-4 cursor-pointer"
      />
      <SquarePen
        onClick={() => {
          openEditDialog(rowData);
        }}
        className="size-4 text-primary cursor-pointer"
      />
    </div>
  );
};
