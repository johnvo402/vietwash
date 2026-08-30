import { DataTable } from "@/components/ui/table/data-table";
import { useTranslations } from "next-intl";
import { useSupplierHistoryImExTable } from "./column";
import { useSupplierHistory } from "@/features/supplier/hooks/use-supplier-history";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTableFilters } from "@/compositions/tables/use-table-filters";

export default function ImportExportSupplierPage({ id }: { id: number }) {
  const t = useTranslations();
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();

  const { columns } = useSupplierHistoryImExTable();
  const { supplierHistories, isLoading, error, paging } = useSupplierHistory({
    supplierId: id,
  });

  return (
    <div className="space-y-4">
      <DataTableSearch
        placeholder={t("search.searchBy", {
          entity: "code",
        })}
        searchQuery={searchQuery}
        setSearchQuery={setSearchQuery}
        setPage={setPage}
      />

      <DataTable
        columns={columns}
        data={supplierHistories}
        loading={isLoading}
        paging={paging}
        error={error}
      />
    </div>
  );
}
