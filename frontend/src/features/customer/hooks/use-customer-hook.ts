"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { useQueryState } from "nuqs";
import { useTranslations } from "next-intl";
import { CreateCustomerCommand, UpdateCustomerModel } from "@/api/generated";
import { useQueryFilter } from "@/lib/filter";
import { toast } from "react-toastify";
import { DateRange } from "react-day-picker";

// Hook chỉ để fetch danh sách users với paging, search
export const useCustomerQuery = () => {
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");
  const { prepareApiParams } = useQueryFilter();
  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeyword: search || undefined,
    searchTargets: search ? ["displayName", "phoneNumber"] : undefined,
  };

  const userApiParamsKeys = [
    "page",
    "pageSize",
    "before",
    "after",
    "searchKeyword",
    "searchTargets",
    "sort",
    "filter",
  ] as const;

  const args = prepareApiParams(userApiParamsKeys, params, {
    page: 1,
    pageSize: 10,
  });
  const {
    data,
    isFetching,
    isLoading: isQueryLoading,
    error: queryError,
  } = useQuery({
    queryKey: ["customers", { page, pageSize, search }],
    queryFn: async () => {
      const response = await apiClient.authApiCustomersGet(...args);
      return {
        users: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
  });

  return {
    users: data?.users || [],
    paging: data?.paging || {},
    isLoading: isQueryLoading || isFetching,
    error: queryError,
  };
};

export const useCustomerMutations = () => {
  const t = useTranslations();
  const queryClient = useQueryClient();

  const createCustomerMutation = useMutation({
    mutationFn: async (userData: CreateCustomerCommand) => {
      return apiClient.authApiCustomersPost(userData);
    },
    onSuccess: () => {
      toast.info(
        t("toast.create.success", {
          entity: t("user.customerInformation").toLowerCase(),
        })
      );
      queryClient.invalidateQueries({ queryKey: ["customer"] });
    },
    onError: () => {
      toast.error(
        t("toast.create.failed", {
          entity: t("user.customerInformation").toLowerCase(),
        })
      );
    },
  });

  const updateAccountMutation = useMutation({
    mutationFn: async ({
      id,
      accountData,
    }: {
      id: number;
      accountData: UpdateCustomerModel;
    }) => {
      return apiClient.authApiCustomersIdPut(id, accountData);
    },
    onSuccess: () => {
      toast.info(
        t("toast.update.success", {
          entity: t("user.customerInformation").toLowerCase(),
        })
      );
      queryClient.invalidateQueries({ queryKey: ["customers"] });
    },
    onError: () => {
      toast.error(
        t("toast.update.failed", {
          entity: t("user.customerInformation").toLowerCase(),
        })
      );
    },
  });

  const deleteAccountMutation = useMutation({
    mutationFn: async ({ id }: { id: number }) => {
      return apiClient.authApiAccountsIdDelete(id);
    },
    onSuccess: () => {
      toast.info(
        t("toast.delete.success", {
          entity: t("user.customerInformation").toLowerCase(),
        })
      );
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
    onError: () => {
      toast.error(
        t("toast.delete.failed", {
          entity: t("user.customerInformation").toLowerCase(),
        })
      );
    },
  });

  const isLoading =
    createCustomerMutation.isPending ||
    updateAccountMutation.isPending ||
    deleteAccountMutation.isPending;

  const error =
    createCustomerMutation.error ||
    updateAccountMutation.error ||
    deleteAccountMutation.error;

  return {
    createCustomer: createCustomerMutation,
    updateCustomer: updateAccountMutation.mutate,
    deleteAccount: deleteAccountMutation.mutate,
    isLoading,
    error,
  };
};

export const useCustomerTransaction = ({
  customerId,
  time,
}: {
  time?: DateRange;
  customerId: number;
}) => {
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");
  const { prepareApiParams, flattenQueryObject } = useQueryFilter();
  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,

    filter: flattenQueryObject({
      customerId: { $eq: customerId },
      ...(time
        ? {
            $and: [
              {
                transactionAt: {
                  $gte: time.from,
                },
              },
              {
                transactionAt: {
                  $lte: time.to,
                },
              },
            ],
          }
        : {}),
    }),
  };

  const userApiParamsKeys = [
    "page",
    "pageSize",
    "before",
    "after",
    "searchKeyword",
    "searchTargets",
    "sort",
    "filter",
  ] as const;

  const args = prepareApiParams(userApiParamsKeys, params, {
    page: 1,
    pageSize: 10,
  });
  const {
    data,
    isFetching,
    isLoading: isQueryLoading,
    error: queryError,
  } = useQuery({
    queryKey: ["transactions", { page, pageSize, search, customerId, time }],
    queryFn: async () => {
      const response = await apiClient.financeApiTransactionGet(...args);
      return {
        transactions: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
    enabled: !!customerId,
  });

  return {
    transactions: data?.transactions || [],
    paging: data?.paging || {},
    isLoading: isQueryLoading || isFetching,
    error: queryError,
  };
};
