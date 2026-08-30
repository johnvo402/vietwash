"use client";

import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { DataTable } from "@/components/ui/table/data-table";
import { useQueryState } from "nuqs";
import {
  UnitSelectionProvider,
  useServiceTable,
} from "./components/service-table/columns";
import { useQueryFilter } from "@/lib/filter";
import { useAuth } from "@/hooks/use-auth";

export default function ServiceListingPage() {
  const { columns } = useServiceTable();
  const { branchActive } = useAuth();

  // Use nuqs to manage search params in a Client Component
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", {
    defaultValue: "10",
  });
  const [search] = useQueryState("search");
  const { prepareApiParams, flattenQueryObject } = useQueryFilter();
  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeyword: search || undefined,
    searchTargets: ["name"],
    filter: flattenQueryObject({
      ...(branchActive ? { branchId: { $eq: branchActive.branchId } } : {}),
    }),
    // sort: "name:asc",
  };

  const searchApiParamsKeys = [
    "page",
    "pageSize",
    "Before",
    "After",
    "searchKeyword",
    "searchTargets",
    "sort",
    "filter",
  ] as const;

  const args = prepareApiParams(searchApiParamsKeys, params, {
    page: 1,
    pageSize: 10,
  });
  const { data, isFetching, isLoading, error } = useQuery({
    queryKey: ["services", { page, pageSize }],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiServicesGet(...args);
      return {
        service: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
  });
  return (
    <UnitSelectionProvider>
      <DataTable
        columns={columns}
        data={data?.service ?? []}
        paging={data?.paging || {}}
        loading={isLoading || isFetching}
        error={error}
      />
    </UnitSelectionProvider>
  );
}
