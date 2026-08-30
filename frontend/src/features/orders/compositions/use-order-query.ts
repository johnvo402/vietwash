import { useEffect, useRef } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { useQueryFilter } from "@/lib/filter";
import { OrderStatus } from "@/api/generated";
import { DateRange } from "@/features/reports/types/filter.type";

interface OrdersQueryParams {
  search: string | null;
  statusFilter?: OrderStatus[] | undefined;
  customerGroupFilter: string;
  dateRange?: DateRange | undefined;
  viewMode: string;
  page: string;
  pageSize: string;
  serviceId?: number | undefined;
  customerId?: number | undefined;
  branchId?: number | undefined;
  staffId?: number | undefined;
  enabled?: boolean;
}

export function useOrdersQuery({
  search,
  statusFilter,
  customerGroupFilter,
  dateRange,
  viewMode,
  page,
  pageSize,
  serviceId = undefined,
  customerId = undefined,
  branchId = undefined,
  staffId = undefined,
  enabled = true,
}: OrdersQueryParams) {
  const { flattenQueryObject, prepareApiParams } = useQueryFilter();
  const observerRef = useRef<HTMLDivElement | null>(null);

  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeyword: search || undefined,
    searchTargets: ["code"],
    sort: "status:asc,createdAt:desc",
    filter: flattenQueryObject({
      ...(statusFilter ? { status: { $in: statusFilter } } : {}),
      ...(customerGroupFilter !== "all"
        ? { customer: { customerGroup: { $eq: customerGroupFilter } } }
        : {}),
      ...(customerId ? { customerId: { $eq: customerId } } : {}),
      ...(staffId ? { staff: { id: { $eq: staffId } } } : {}),
      ...(dateRange
        ? {
            $and: [
              {
                createdAt: {
                  $gte: dateRange?.from,
                },
              },
              {
                createdAt: {
                  $lte: dateRange?.to,
                },
              },
            ],
          }
        : {}),
    }),
    branchId,
    serviceId,
  };

  const ecommerceApiOrdersParamKeys = [
    "from",
    "to",
    "branchId",
    "serviceId",
    "page",
    "pageSize",
    "cursorBefore",
    "cursorAfter",
    "searchKeyword",
    "searchTargets",
    "sort",
    "filter",
    "dynamicFilter",
    "originFilters",
    "options",
  ] as const;

  const {
    data,
    isFetching,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
    refetch,
  } = useInfiniteQuery({
    queryKey: [
      "orders",
      {
        page,
        pageSize,
        search,
        statusFilter,
        customerGroupFilter,
        dateRange,
        viewMode,
        branchId,
        serviceId,
        customerId,
      },
    ],
    queryFn: async ({ pageParam = 1 }) => {
      const updatedParams = {
        ...params,
        page: viewMode === "list" ? parseInt(page) || 1 : pageParam,
      };
      const args = prepareApiParams(
        ecommerceApiOrdersParamKeys,
        updatedParams,
        {
          page: 1,
          pageSize: 10,
        }
      );
      const response = await apiClient.ecommerceApiOrdersGet(...args);
      return {
        results: response.data.results?.data || [],
        paging: response.data.results?.paging,
      };
    },
    getNextPageParam: (lastPage) => {
      const paging = lastPage.paging!;
      const currentPage = paging.currentPage ?? 1;
      const totalPages = paging.totalPage ?? 1;
      const result = currentPage < totalPages ? currentPage + 1 : undefined;
      return result;
    },
    initialPageParam: 1,
    enabled: enabled,
  });

  // Flatten orders from all pages
  const ordersToDisplay = data?.pages.flatMap((page) => page.results) || [];
  // Set up IntersectionObserver for infinite scroll in card view
  useEffect(() => {
    if (viewMode !== "card" || !hasNextPage || isFetching) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting) {
          fetchNextPage();
        }
      },
      {
        root: null,
        rootMargin: "100px",
        threshold: 0.1,
      }
    );

    if (observerRef.current) {
      observer.observe(observerRef.current);
    }

    return () => {
      if (observerRef.current) {
        // eslint-disable-next-line react-hooks/exhaustive-deps
        observer.unobserve(observerRef.current);
      }
    };
  }, [viewMode, hasNextPage, isFetching, fetchNextPage]);

  return {
    ordersToDisplay,
    isFetching,
    isLoading,
    error,
    observerRef,
    refetch,
    paging: data?.pages[data.pages.length - 1]?.paging || {},
  };
}
