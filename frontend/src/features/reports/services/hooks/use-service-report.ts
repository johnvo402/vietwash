import { apiClient } from "@/api/client";
import { ServiceRevenueReportResponse } from "@/api/generated";
import { useAuth } from "@/hooks/use-auth";
import { useQueryFilter } from "@/lib/filter";
import { useQuery } from "@tanstack/react-query";
import { useQueryState, useQueryStates } from "nuqs";

export const useServiceReport = () => {
  const { branchActive } = useAuth();
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");
  const [from] = useQueryState("from");
  const [to] = useQueryState("to");
  const [{ branchIds }] = useQueryStates({
    branchIds: {
      defaultValue: branchActive?.branchId
        ? [branchActive.branchId]
        : undefined,
      parse: (value: string) => {
        try {
          const parsed = JSON.parse(value);
          const ids = Array.isArray(parsed)
            ? parsed.map(Number).filter((id) => !isNaN(id))
            : [];
          return ids.length > 0 ? ids : undefined;
        } catch {
          return undefined;
        }
      },
      serialize: (value: number[]) => JSON.stringify(value),
    },
  });
  const { prepareApiParams } = useQueryFilter();

  const params = {
    from: from ? from : undefined,
    to: to ? to : undefined,
    BranchIds: branchIds,
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeywords: search || undefined,
    searchKeyword: search || undefined,
    searchTargets: ["serviceName"],
  };

  const searchApiParamsKeys = [
    "from",
    "to",
    "BranchIds",
    "searchKeywords",
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
    queryKey: [
      "serviceReport",
      { page, pageSize, search, branchIds, from, to },
    ],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiReportServiceOrderGet(
        ...args
      );
      return {
        serviceReport: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
  });
  const fetchAllData = async () => {
    let allData: ServiceRevenueReportResponse[] = [];
    let currentPage = 1;
    const pageSize = 5000;

    while (true) {
      const fetchArgs = prepareApiParams(
        searchApiParamsKeys,
        { ...params, page: currentPage, pageSize },
        { page: 1, pageSize }
      );

      const response = await apiClient.ecommerceApiReportServiceOrderGet(
        ...fetchArgs
      );
      const results = response.data.results?.data || [];
      allData = [...allData, ...results];

      if (!response.data.results?.paging?.hasNextPage || results.length === 0) {
        break;
      }
      currentPage++;
    }

    return allData;
  };
  return {
    serviceReport: data?.serviceReport || [],
    fetchAllData,
    paging: data?.paging || {},
    isLoading: isQueryLoading || isFetching,
    error: queryError,
    from,
    to,
    branchIds,
  };
};
