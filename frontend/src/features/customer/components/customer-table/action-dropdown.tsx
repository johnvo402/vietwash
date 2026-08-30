"use client";

import {
  DropdownMenuContent,
  DropdownMenuItem,
} from "@/components/ui/dropdown-menu";
import { useTranslations } from "next-intl";
import { ListCustomerResponse } from "@/api/generated";

interface DropdownMenuContentComponentProps {
  customer: ListCustomerResponse;
  onDetail: (customer: ListCustomerResponse) => void;
  onEdit: (customer: ListCustomerResponse) => void;
}

export function DropdownMenuContentComponent({
  customer,
  onDetail,
  onEdit,
}: DropdownMenuContentComponentProps) {
  const t = useTranslations();

  return (
    <DropdownMenuContent>
      <DropdownMenuItem onClick={() => onDetail(customer)}>
        {t("common.details")}
      </DropdownMenuItem>
      <DropdownMenuItem onClick={() => onEdit(customer)}>
        {t("common.edit")}
      </DropdownMenuItem>
    </DropdownMenuContent>
  );
}
