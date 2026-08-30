"use client";

import { useState } from "react";
import { DataTable } from "@/components/ui/table/data-table";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTranslations } from "next-intl";
import { Button, buttonVariants } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { ListSupplierResponse } from "@/api/generated";

import { ROUTE_SUPPLIER_CREATE } from "@/types/router-type";
import { cn } from "@/lib/utils";
import { usePushRouter } from "@/utils/router-utli";
import { useSupplier } from "./hooks/use-supplier";
import { useSupplierTable } from "./components/columns";

export default function SupplierListingPage() {
  const t = useTranslations();
  const { suppliers, paging, isLoading, error } = useSupplier();
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();

  const pushRouter = usePushRouter();
  const { columns } = useSupplierTable();

  return (
    <>
      <div className="flex flex-wrap items-stretch justify-between mb-6">
        <DataTableSearch
          placeholder={t("search.searchBy", {
            entity: (
              t("table.accessorKey.name") +
              " " +
              t("user.and") +
              " " +
              t("table.accessorKey.code")
            ).toLowerCase(),
          })}
          searchQuery={searchQuery}
          setSearchQuery={setSearchQuery}
          setPage={setPage}
        />

        <Button
          onClick={() =>
            pushRouter.pushRouter({
              router: ROUTE_SUPPLIER_CREATE,
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

      {/* Data Table */}
      <div className="rounded-md border shadow-sm">
        <DataTable
          columns={columns}
          data={suppliers}
          paging={paging}
          loading={isLoading}
          error={error}
        />
      </div>

      {/* {supplierToDelete && (
        <DeleteConfirmationDialog
          open={isDeleteDialogOpen}
          onClose={() => {
            setIsDeleteDialogOpen(false);
            setSupplierToDelete(undefined);
          }}
          onConfirm={async () => {
            await deleteSupplier({ id: supplierToDelete.id! });
            setIsDeleteDialogOpen(false);
            setSupplierToDelete(undefined);
          }}
          name={supplierToDelete.displayName ?? ""}
        />
      )} */}
    </>
  );
}
