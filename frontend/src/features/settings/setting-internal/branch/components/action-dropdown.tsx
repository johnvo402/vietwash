"use client";

import { useTranslations } from "next-intl";
import {
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
} from "@/components/ui/dropdown-menu";
import { BranchModel, ListBranchResponse } from "@/api/generated";
import { SquarePen } from "lucide-react";

interface DropdownMenuContentProps {
  rowData: ListBranchResponse;
  // deleteBranch: (params: { id: number }) => void;
  openEditDialog: (branch: BranchModel) => void;
}

export const DropdownMenuContentComponent = ({
  rowData,
  // deleteBranch,
  openEditDialog,
}: DropdownMenuContentProps) => {
  const t = useTranslations();

  return (
    <div className="flex space-x-2 justify-center">
      <SquarePen
        onClick={() => {
          openEditDialog(rowData);
        }}
        className="size-4 text-primary cursor-pointer"
      />
    </div>
    // <DropdownMenuContent>
    //   <DropdownMenuLabel>{t("table.accessorKey.actions")}</DropdownMenuLabel>
    //   <DropdownMenuSeparator />
    //   <DropdownMenuItem
    //     onClick={() => {
    //       openEditDialog(rowData);
    //     }}
    //   >
    //     {t("common.edit")}
    //   </DropdownMenuItem>
    //   {/* <DropdownMenuItem
    //     onClick={() => {
    //       deleteBranch({ id: rowData.id! });
    //     }}
    //   >
    //     {t("common.delete")}
    //   </DropdownMenuItem> */}
    // </DropdownMenuContent>
  );
};
