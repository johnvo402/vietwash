import { apiClient } from "@/api/client";
import { GetByTariffResponse } from "@/api/generated";
import { useQueryFilter } from "@/lib/filter";
import { PropsQuery } from "@/types/props";
import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { useMemo } from "react";
interface ServiceCashierResult {
  serviceCashiers: GetByTariffResponse[];
  isLoading: boolean;
  error: any;
  fetchNextPage: () => void;
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
}
export const useServiceCashier = ({
  tariffId,
  query,
}: {
  query?: PropsQuery;
  tariffId: number;
}): ServiceCashierResult => {
  const { prepareApiParams, flattenQueryObject } = useQueryFilter();
  const params = {
    tariffId,
    page: 1,
    pageSize: 20,
    sort: query?.sort ? query?.sort : undefined,
    filter: flattenQueryObject(query?.filter),
    searchKeyword: query?.searchKeywords,
    searchTargets: query?.searchTarget,
  };

  const searchApiParamsKeys = [
    "tariffId",
    "page",
    "pageSize",
    "before",
    "after",
    "searchKeyword",
    "searchTargets",
    "sort",
    "filter",
  ] as const;

  const {
    data,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useInfiniteQuery({
    queryKey: ["service-cashier", query, tariffId],
    queryFn: async ({ pageParam = 1 }) => {
      const args = prepareApiParams(searchApiParamsKeys, {
        ...params,
        page: pageParam,
      });
      const response = await apiClient.ecommerceApiServicesServicesByTariffGet(
        ...args
      );
      return {
        results: response.data.results?.data || [],
        paging: response.data.results?.paging,
      };
    },
    getNextPageParam: (lastPage) => {
      const paging = lastPage.paging;
      const current_page = paging?.currentPage ?? 1;
      const total_pages = paging?.totalPage ?? 1;
      return current_page < total_pages ? current_page + 1 : undefined;
    },
    initialPageParam: 1,
    enabled: !!tariffId, // Only run query when enabled is true
  });

  const serviceCashiers = useMemo(
    () => data?.pages.flatMap((page) => page.results) || [],
    [data]
  );
  return {
    serviceCashiers,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading,
  };
};
