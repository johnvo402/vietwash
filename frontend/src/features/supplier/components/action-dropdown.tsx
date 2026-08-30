"use client";

import { useTranslations } from "next-intl";
import {
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
} from "@/components/ui/dropdown-menu";
import { ListSupplierResponse } from "@/api/generated";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_SUPPLIER_DETAIL, ROUTE_SUPPLIER_EDIT } from "@/types/router-type";

interface DropdownMenuContentProps {
  rowData: ListSupplierResponse;
}

export const DropdownMenuContentComponent = ({
  rowData,
}: DropdownMenuContentProps) => {
  const t = useTranslations();
  const pushRouter = usePushRouter();
  return (
    <DropdownMenuContent>
      <DropdownMenuLabel>{t("table.accessorKey.actions")}</DropdownMenuLabel>
      <DropdownMenuSeparator />
      <DropdownMenuItem
        onClick={() => {
          const publicId = rowData.publicId?.toString()!;
          return pushRouter.pushRouter({
            router: ROUTE_SUPPLIER_DETAIL,
            params: {
              publicId: publicId,
            },
            state: {
              [publicId]: rowData.id!,
            },
          });
        }}
      >
        {t("common.details")}
      </DropdownMenuItem>
      <DropdownMenuItem
        onClick={() =>
          pushRouter.pushRouter({
            router: ROUTE_SUPPLIER_EDIT,
            params: {
              publicId: rowData.publicId?.toString()!,
            },
            state: {
              id: rowData.id!,
            },
          })
        }
      >
        {t("common.edit")}
      </DropdownMenuItem>
    </DropdownMenuContent>
  );
};
