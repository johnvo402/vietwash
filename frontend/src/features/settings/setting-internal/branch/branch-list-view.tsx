"use client";

import { useState } from "react";
import { DataTable } from "@/components/ui/table/data-table";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTranslations } from "next-intl";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { ListBranchResponse } from "@/api/generated";
import { useBranchSettingTable } from "./components/column";
import { useBranchSettings } from "./hooks/use-branch";
import { CreateBranchDialog } from "./components/create-branch-dialog";
// import { DeleteConfirmationDialog } from "./components/delete-branch-dialog";

export default function BranchSettingListPage() {
  const t = useTranslations();
  const {
    branchs,
    paging,
    isLoading,
    error,
    createBranch,
    updateBranch,
    // deleteBranch,
  } = useBranchSettings();
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [selectedBranch, setSelectedBranch] = useState<
    ListBranchResponse | undefined
  >(undefined);
  // const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  // const [branchToDelete, setBranchToDelete] = useState<
  //   ListBranchResponse | undefined
  // >(undefined);

  const { columns } = useBranchSettingTable({
    // deleteBranch: (branch: ListBranchResponse) => {
    //   setBranchToDelete(branch);
    //   setIsDeleteDialogOpen(true);
    // },
    openEditDialog: (branch: ListBranchResponse) => {
      setSelectedBranch(branch);
      setIsDialogOpen(true);
    },
  });

  return (
    <div className="mx-auto space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">
          {t("common.branch").replace(/^./, (c) => c.toUpperCase())}
        </h1>
        <Button
          onClick={() => {
            setSelectedBranch(undefined);
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

      {/* Data Table */}
      <div className="rounded-md border shadow-sm">
        <DataTable
          columns={columns}
          data={branchs}
          paging={paging}
          loading={isLoading}
          error={error}
        />
      </div>

      {/* Create/Edit Branch Dialog */}
      <CreateBranchDialog
        name={selectedBranch ? "edit-branch" : "create-branch"}
        onClose={() => {
          setIsDialogOpen(false);
          setSelectedBranch(undefined);
        }}
        onCreateBranch={async (data) => {
          await createBranch(data);
          setIsDialogOpen(false);
          setSelectedBranch(undefined);
        }}
        onUpdateBranch={async (params) => {
          await updateBranch(params);
          setIsDialogOpen(false);
          setSelectedBranch(undefined);
        }}
        branch={selectedBranch}
        open={isDialogOpen}
      />

      {/* Delete Confirmation Dialog */}
      {/* {branchToDelete && (
        <DeleteConfirmationDialog
          open={isDeleteDialogOpen}
          onClose={() => {
            setIsDeleteDialogOpen(false);
            setBranchToDelete(undefined);
          }}
          onConfirm={async () => {
            await deleteBranch({ id: branchToDelete.id! });
          }}
          branchName={branchToDelete.name ?? ""}
        />
      )} */}
    </div>
  );
}
