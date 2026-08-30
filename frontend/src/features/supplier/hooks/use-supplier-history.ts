import { apiClient } from "@/api/client";
import { useQueryFilter } from "@/lib/filter";
import { useQuery } from "@tanstack/react-query";
import { useQueryState } from "nuqs";

export const useSupplierHistory = ({ supplierId }: { supplierId: number }) => {
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");
  const { prepareApiParams, flattenQueryObject } = useQueryFilter();
  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeyword: search || undefined,
    searchTargets: search ? ["documentCode"] : undefined,
    filter: flattenQueryObject({
      supplierId: {
        $eq: supplierId,
      },
    }),
  };

  const searchApiParamsKeys = [
    "page",
    "pageSize",
    "before",
    "after",
    "searchKeyword",
    "searchTargets",
    "sort",
    "filter",
  ] as const;

  const args = prepareApiParams(searchApiParamsKeys, params, {
    page: 1,
    pageSize: 10,
  });
  const {
    data,
    isFetching,
    isLoading: isQueryLoading,
    error: queryError,
  } = useQuery({
    queryKey: ["suppliers-history", { page, pageSize, search, supplierId }],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiImportExportHistoriesGet(
        ...args
      );
      return {
        suppliers: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
    enabled: !!supplierId,
  });

  return {
    supplierHistories: data?.suppliers || [],
    paging: data?.paging || {},
    isLoading: isQueryLoading || isFetching,
    error: queryError,
  };
};
