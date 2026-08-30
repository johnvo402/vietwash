"use client";

import { DataTable } from "@/components/ui/table/data-table";
import { useBranchProduct } from "../hooks/use-material";
import { useBranchProducts } from "../components/columns";
import { useTranslations } from "next-intl";
import { Button, buttonVariants } from "@/components/ui/button";
import { usePushRouter } from "@/utils/router-utli";
import {
  ROUTE_INVENTORY_MATERIAL_CREATE,
  ROUTE_INVENTORY_MATERIAL_DETAIL,
  ROUTE_INVENTORY_MATERIAL_EDIT,
} from "@/types/router-type";
import { cn } from "@/lib/utils";
import { Plus } from "lucide-react";
import { ActivationStatus, ListBranchProductResponse } from "@/api/generated";
import { MaterialFilter } from "../components/material-filter";
import { useListCategoryResponseQuery } from "@/features/settings/setting-data/category-settings/hooks/use-category-data-query";
import { useState } from "react";
import { Option } from "@/components/core/selects/multi-select";
import { useAuth } from "@/hooks/use-auth";

export default function MaterialListingView() {
  const t = useTranslations();
  const { branchActive } = useAuth();
  const [categoryId, setCategoryId] = useState<number | null>(null);
  const [statusFilter, setStatusFilter] = useState<Option[]>([
    {
      value: ActivationStatus.Active,
      label: t("common.status.active"),
    },
  ]);
  const { error, isLoading, paging, products } = useBranchProduct({
    categoryId,
    statusFilter: statusFilter.map(
      (option) => option.value as ActivationStatus
    ),
    branchId: branchActive?.branchId,
  });
  const pushRouter = usePushRouter();
  const { treeData } = useListCategoryResponseQuery();
  const handleAction = (
    action: "detail" | "edit",
    row: ListBranchProductResponse
  ) => {
    if (action === "detail") {
      pushRouter.pushRouter({
        router: ROUTE_INVENTORY_MATERIAL_DETAIL,
        params: { publicId: row.publicId?.toString()! },
        state: {
          [row.publicId?.toString()!]: row.id,
        },
      });
    } else {
      pushRouter.pushRouter({
        router: ROUTE_INVENTORY_MATERIAL_EDIT,
        params: { publicId: row.publicId?.toString()! },
        state: {
          [row.publicId?.toString()!]: row.id,
        },
      });
    }
  };
  const { columns } = useBranchProducts(handleAction);
  return (
    <div className="w-full">
      <div className="flex flex-wrap mb-5 items-stretch justify-between">
        <MaterialFilter
          categoryId={categoryId}
          setCategoryId={setCategoryId}
          setStatusFilter={setStatusFilter}
          statusFilter={statusFilter}
          treeData={treeData}
        />

        <Button
          onClick={() =>
            pushRouter.pushRouter({
              router: ROUTE_INVENTORY_MATERIAL_CREATE,
              query: {
                pageSize: paging.pageSize!,
                page: paging.currentPage!,
              },
            })
          }
          className={cn(buttonVariants(), "text-xs md:text-sm")}
        >
          <Plus className="h-4 w-4" /> {t("common.create")}
        </Button>
      </div>

      <div className="rounded-md border shadow-sm">
        <DataTable
          columns={columns}
          data={products ?? []}
          paging={paging || {}}
          loading={isLoading}
          error={error}
        />
      </div>
    </div>
  );
}
