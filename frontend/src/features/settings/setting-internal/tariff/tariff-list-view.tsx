"use client";

import { useState } from "react";
import { DataTable } from "@/components/ui/table/data-table";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTranslations } from "next-intl";
import { ListTariffResponse } from "@/api/generated";
import { useTariffTable } from "./components/column";
import { useTariff } from "./hooks/use-tariff-hook";
import CreatePriceListDialog from "./components/create-tariff-form";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import TariffDetail from "./tariff-detail-view";
import TariffEdit from "./tariff-edit-view";
import { useAuth } from "@/hooks/use-auth";

export default function TariffSettingListPage() {
  const t = useTranslations();
  const { branchActive } = useAuth();
  const { tariffs, paging, isLoading, error } = useTariff({
    branchId: branchActive?.branchId,
  });
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);

  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isDetailDialogOpen, setIsDetailDialogOpen] = useState(false);
  const [selectedTariff, setSelectedTariff] = useState<
    ListTariffResponse | undefined
  >(undefined);

  const { columns } = useTariffTable({
    openEditDialog: (tariff: ListTariffResponse) => {
      setSelectedTariff(tariff);
      setIsEditDialogOpen(true);
    },
    openDetailDialog: (tariff: ListTariffResponse) => {
      setSelectedTariff(tariff);
      setIsDetailDialogOpen(true);
    },
  });

  return (
    <div className="mx-auto space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">
          {t("common.tariff").replace(/^./, (c) => c.toUpperCase())}
        </h1>
      </div>
      <div className="flex justify-between">
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
          onClick={() => {
            setIsCreateDialogOpen(true);
          }}
          className="flex items-center gap-2 bg-primary text-primary-foreground hover:bg-primary/90"
        >
          <Plus className="h-4 w-4" />
          {t("common.create")}
        </Button>
      </div>
      {/* Search Bar */}

      {/* Data Table */}
      <div className="rounded-md border shadow-sm">
        <DataTable
          columns={columns}
          data={tariffs}
          paging={paging}
          loading={isLoading}
          error={error}
        />
      </div>

      {isCreateDialogOpen && (
        <CreatePriceListDialog
          isOpen={isCreateDialogOpen}
          onClose={() => setIsCreateDialogOpen(false)}
        />
      )}
      {isDetailDialogOpen && selectedTariff && (
        <TariffDetail
          id={selectedTariff?.id!}
          isOpen={isDetailDialogOpen}
          onClose={() => setIsDetailDialogOpen(false)}
        />
      )}
      {isEditDialogOpen && selectedTariff && (
        <TariffEdit
          id={selectedTariff?.id!}
          isOpen={isEditDialogOpen}
          onClose={() => setIsEditDialogOpen(false)}
        />
      )}
    </div>
  );
}
