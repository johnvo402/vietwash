"use client";

import { useState } from "react";
import { DataTable } from "@/components/ui/table/data-table";
import { useUnitSettingTable } from "./components/unit-setting-table/columns";
import { useUnitSettings } from "./hooks/use-unit-hook";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTranslations } from "next-intl";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { ListUnitResponse } from "@/api/generated";
import { CreateUnitDialog } from "./components/unit-setting-create/create-unit-dialog";
import { DeleteConfirmationDialog } from "./components/delete-dialog";

export default function UnitSettingListPage() {
  const t = useTranslations();
  const {
    units,
    paging,
    isLoading,
    error,
    createUnit,
    updateUnit,
    deleteUnit,
  } = useUnitSettings();
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [selectedUnit, setSelectedUnit] = useState<
    ListUnitResponse | undefined
  >(undefined);
  // const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  // const [unitToDelete, setUnitToDelete] = useState<
  //   ListUnitResponse | undefined
  // >(undefined);

  const { columns } = useUnitSettingTable({
    // deleteUnit: (unit: ListUnitResponse) => {
    //   setUnitToDelete(unit);
    //   setIsDeleteDialogOpen(true);
    // },
    openEditDialog: (unit: ListUnitResponse) => {
      setSelectedUnit(unit);
      setIsDialogOpen(true);
    },
  });

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">
          {t("common.unit").charAt(0).toUpperCase() + t("common.unit").slice(1)}
        </h1>
        <Button
          onClick={() => {
            setSelectedUnit(undefined);
            setIsDialogOpen(true);
          }}
          className="flex items-center gap-2 bg-primary text-primary-foreground hover:bg-primary/90"
        >
          <Plus className="h-4 w-4" />
          {t("common.create")}
        </Button>
      </div>

      {/* Search Bar */}
      <DataTableSearch
        placeholder={t("search.searchBy", {
          entity:
            t("table.accessorKey.name") +
            " " +
            t("user.and") +
            " " +
            t("table.accessorKey.code"),
        })}
        searchQuery={searchQuery}
        setSearchQuery={setSearchQuery}
        setPage={setPage}
      />

      {/* Data Table */}
      <div className="rounded-md border shadow-sm">
        <DataTable
          columns={columns}
          data={units}
          paging={paging}
          loading={isLoading}
          error={error}
        />
      </div>

      {/* Create/Edit Unit Dialog */}
      <CreateUnitDialog
        name={selectedUnit ? "edit-unit" : "create-unit"}
        onClose={() => {
          setIsDialogOpen(false);
          setSelectedUnit(undefined);
        }}
        onCreateUnit={async (data) => {
          await createUnit(data);
          setIsDialogOpen(false);
          setSelectedUnit(undefined);
        }}
        onUpdateUnit={async (params) => {
          await updateUnit(params);
          setIsDialogOpen(false);
          setSelectedUnit(undefined);
        }}
        unit={selectedUnit}
        open={isDialogOpen}
      />

      {/* Delete Confirmation Dialog */}
      {/* {unitToDelete && (
        <DeleteConfirmationDialog
          open={isDeleteDialogOpen}
          onClose={() => {
            setIsDeleteDialogOpen(false);
            setUnitToDelete(undefined);
          }}
          onConfirm={async () => {
            await deleteUnit({ id: unitToDelete.id! });
          }}
          unitName={unitToDelete.name ?? ""}
        />
      )} */}
    </div>
  );
}
