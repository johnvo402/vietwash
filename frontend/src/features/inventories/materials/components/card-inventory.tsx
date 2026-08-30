import { DataTable } from "@/components/ui/table/data-table";
import { useBranchProducts } from "./columns";
import { useBranchProductCard } from "../hooks/use-material";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTranslations } from "next-intl";

export default function CarInvList({ id }: { id: number }) {
  const { columnCardInv } = useBranchProducts();
  const t = useTranslations();
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();

  const { productCards, isLoading, error, paging } = useBranchProductCard(id);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-stretch justify-between">
        <DataTableSearch
          placeholder={t("product.searchInvDoc")}
          searchQuery={searchQuery}
          setSearchQuery={setSearchQuery}
          setPage={setPage}
        />
      </div>

      <div className="mt-2">
        <DataTable
          columns={columnCardInv}
          data={productCards}
          loading={isLoading}
          paging={paging}
          error={error}
        />
      </div>
    </div>
  );
}
