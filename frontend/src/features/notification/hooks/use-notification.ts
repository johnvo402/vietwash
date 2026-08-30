import { useMemo } from "react";
import { apiClient } from "@/api/client";
import { ListNotificationResponse } from "@/api/generated";
import { useQueryFilter } from "@/lib/filter";
import { PropsQuery } from "@/types/props";
import {
  useInfiniteQuery,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";

interface Notification {
  id: string;
  title: string;
  contentHtml: string;
}

interface UseNotificationResult {
  notification: ListNotificationResponse[];
  countNotification: number;
  refetchCountNoti: () => void;
  isLoading: boolean;
  error: any;
  fetchNextPage: () => void;
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
}

export const useNotification = (
  query: PropsQuery,
  enabled: boolean
): UseNotificationResult => {
  const queryClient = useQueryClient();
  const { prepareApiParams, flattenQueryObject } = useQueryFilter();
  const params = {
    page: 1,
    pageSize: 10,
    sort: query.sort,
    filter: flattenQueryObject(query.filter),
    searchKeyword: query.searchKeywords,
    searchTargets: query.searchTarget,
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

  const {
    data,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useInfiniteQuery({
    queryKey: ["notification", query.filter],
    queryFn: async ({ pageParam = 1 }) => {
      const args = prepareApiParams(
        searchApiParamsKeys,
        { ...params, page: pageParam },
        { page: 1, pageSize: 10 }
      );
      const response = await apiClient.notificationApiListNotifyGet(...args);
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
    enabled, // Only run query when enabled is true
  });

  const notification = useMemo(
    () => data?.pages.flatMap((page) => page.results) || [],
    [data]
  );

  const { data: countNotify, refetch: refetchCount } = useQuery<
    number | undefined
  >({
    queryKey: ["countNotify"],
    queryFn: async () => {
      const response = await apiClient.notificationApiCountNotifyGet();
      return response.data.results?.numberNotify;
    },
  });

  const refetch = () => {
    refetchCount();
    queryClient.invalidateQueries({ queryKey: ["notification"] });
  };

  return {
    notification,
    countNotification: countNotify ?? 0,
    refetchCountNoti: refetch,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  };
};

export const useNotificationMutations = () => {
  const queryClient = useQueryClient();

  const readAllNoti = useMutation({
    mutationFn: async () => {
      return await apiClient.notificationApiReadAllNotifyPut();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        predicate: (query) =>
          query.queryKey[0] === "notification" ||
          query.queryKey[0] === "countNotify",
      });
    },
  });
  const readOne = useMutation({
    mutationFn: ({ id }: { id: number }) =>
      apiClient.notificationApiReadOneNotifyIdPut(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        predicate: (query) =>
          query.queryKey[0] === "notification" ||
          query.queryKey[0] === "countNotify",
      });
    },
  });
  return {
    readAllNoti: readAllNoti.mutate,
    readOne: readOne.mutateAsync,
    isLoading: readAllNoti.isPending,
    error: readAllNoti.error,
  };
};
