"use client";

import { useTranslations } from "next-intl";
import {
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
} from "@/components/ui/dropdown-menu";
import { ListUserResponse } from "@/api/generated";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_USER_DETAIL, ROUTE_USER_EDIT } from "@/types/router-type";
import { EyeIcon, PenIcon } from "lucide-react";

interface DropdownMenuContentProps {
  rowData: ListUserResponse;
}

export const DropdownMenuContentComponent = ({
  rowData,
}: DropdownMenuContentProps) => {
  const t = useTranslations();
  const pushRouter = usePushRouter();
  return (
    <DropdownMenuContent>
      {/* <DropdownMenuLabel>{t("table.accessorKey.actions")}</DropdownMenuLabel>
      <DropdownMenuSeparator /> */}

      <DropdownMenuItem
        onClick={() =>
          pushRouter.pushRouter({
            router: ROUTE_USER_DETAIL,
            params: {
              publicId: rowData.publicId?.toString()!,
            },
            state: {
              [rowData.publicId?.toString()!]: rowData.id!,
            },
          })
        }
      >
        <EyeIcon /> {t("common.details")}
      </DropdownMenuItem>
      <DropdownMenuItem
        onClick={() =>
          pushRouter.pushRouter({
            router: ROUTE_USER_EDIT,
            params: {
              publicId: rowData.publicId?.toString()!,
            },
            state: {
              [rowData.publicId?.toString()!]: rowData.id!,
            },
          })
        }
      >
        <PenIcon /> {t("common.edit")}
      </DropdownMenuItem>
    </DropdownMenuContent>
  );
};
