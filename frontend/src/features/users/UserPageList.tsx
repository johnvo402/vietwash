"use client";

import { DataTable } from "@/components/ui/table/data-table";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTranslations } from "next-intl";
import { Button, buttonVariants } from "@/components/ui/button";
import { Plus } from "lucide-react";
// import { ListAccountResponse } from "@/api/generated";
import { useUserTable } from "./components/user-table/columns";
import { useUsersQuery } from "./hooks/use-user-hook";
import { ROUTE_USER_CREATE } from "@/types/router-type";
import { cn } from "@/lib/utils";
import { usePushRouter } from "@/utils/router-utli";
// import { DeleteConfirmationDialog } from "./components/user-table/delete-dialog";

export default function UserListingPage() {
  const t = useTranslations();
  const { users, paging, isLoading, error } = useUsersQuery();
  // const { deleteAccount } = useUserMutations();
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  // const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  // const [userToDelete, setUserToDelete] = useState<
  //   ListAccountResponse | undefined
  // >(undefined);
  const pushRouter = usePushRouter();
  const { columns } = useUserTable();

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-stretch justify-between">
        <DataTableSearch
          placeholder={t("search.searchBy", {
            entity: (
              t("user.email.title") +
              ", " +
              t("user.firstName.title") +
              ", " +
              t("user.lastName.title")
            ).toLowerCase(),
          })}
          searchQuery={searchQuery}
          setSearchQuery={setSearchQuery}
          setPage={setPage}
        />

        <Button
          onClick={() =>
            pushRouter.pushRouter({
              router: ROUTE_USER_CREATE,
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
          data={users}
          paging={paging}
          loading={isLoading}
          error={error}
        />
      </div>

      {/* {userToDelete && (
        <DeleteConfirmationDialog
          open={isDeleteDialogOpen}
          onClose={() => {
            setIsDeleteDialogOpen(false);
            setUserToDelete(undefined);
          }}
          onConfirm={async () => {
            deleteAccount({ id: userToDelete.id! });
            setIsDeleteDialogOpen(false);
            setUserToDelete(undefined);
          }}
          name={userToDelete.displayName ?? ""}
        />
      )} */}
    </div>
  );
}
