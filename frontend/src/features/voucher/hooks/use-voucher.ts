import { apiClient } from "@/api/client";
import {
  CreateVoucherCommand,
  ListVoucherResponse,
  VoucherModel,
} from "@/api/generated";
import { useQueryFilter } from "@/lib/filter";
import { Customer, useCustomers } from "@/utils/customer-indexedDb";
import {
  useInfiniteQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import { useTranslations } from "next-intl";
import { toast } from "react-toastify";

interface UseFormVouchersResult {
  vouchers: ListVoucherResponse[];
  isLoading: boolean;
  error: any;
  refetch: () => void;
  fetchNextPage: () => void;
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
  customers?: Customer[];
}

export const useVouchers = (searchTerm = ""): UseFormVouchersResult => {
  const { prepareApiParams } = useQueryFilter();
  const { data: customerData } = useCustomers();

  const params = {
    page: 1,
    pageSize: 9,
    searchKeyword: searchTerm || undefined,
    searchTargets: searchTerm ? ["code"] : undefined,
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
    refetch,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useInfiniteQuery({
    queryKey: ["vouchers", { search: searchTerm }],
    queryFn: async ({ pageParam = 1 }) => {
      const args = prepareApiParams(
        searchApiParamsKeys,
        { ...params, page: pageParam },
        { page: 1, pageSize: 9 }
      );
      const response = await apiClient.ecommerceApiVouchersGet(...args);
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
  });

  const vouchers = data?.pages.flatMap((page) => page.results) || [];

  return {
    vouchers,
    isLoading,
    error,
    refetch,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    customers: customerData?.users,
  };
};

export const useVoucherMutations = () => {
  const queryClient = useQueryClient();
  const t = useTranslations();

  const updateVoucher = useMutation({
    mutationFn: async ({
      id,
      command,
    }: {
      id: number;
      command: VoucherModel;
    }) => {
      const response = await apiClient.ecommerceApiVouchersIdPut(id, command);
      return response.data;
    },
    onSuccess: async () => {
      toast.info(
        t("toast.update.success", {
          entity: t("voucher.title").toLowerCase(),
        })
      );
      queryClient.invalidateQueries({ queryKey: ["vouchers"] });
    },
    onError: (error: any) => {
      toast.error(
        t("toast.update.failed", {
          entity: t("voucher.title").toLowerCase(),
        })
      );
    },
  });

  const createVoucher = useMutation({
    mutationFn: async ({ command }: { command: CreateVoucherCommand }) => {
      const response = await apiClient.ecommerceApiVouchersPost(command);
      return response.data;
    },
    onSuccess: async () => {
      toast.info(
        t("toast.create.success", {
          entity: t("voucher.title").toLowerCase(),
        })
      );
      queryClient.invalidateQueries({ queryKey: ["vouchers"] });
    },
    onError: (error: any) => {
      toast.error(
        t("toast.create.failed", {
          entity: t("voucher.title").toLowerCase(),
        })
      );
    },
  });

  return {
    createVoucher: createVoucher.mutateAsync,
    updateVoucher: updateVoucher.mutateAsync,
    isLoading: updateVoucher.isPending || createVoucher.isPending,
  };
};
