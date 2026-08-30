"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { useQueryState } from "nuqs";
import { useTranslations } from "next-intl";
import { CreateAccountCommand, UpdateAccount } from "@/api/generated";
import { useQueryFilter } from "@/lib/filter";
import { toast } from "react-toastify";

// Hook chỉ để fetch danh sách users với paging, search
export const useUsersQuery = () => {
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");
  const { prepareApiParams } = useQueryFilter();
  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeyword: search || undefined,
    searchTargets: search ? ["displayName", "email"] : undefined,
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
    queryKey: ["users", { page, pageSize, search }],
    queryFn: async () => {
      const response = await apiClient.authApiAccountsGet(...args);
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

// Hook chỉ để thực hiện các mutation create/update/delete
export const useUserMutations = () => {
  const t = useTranslations();
  const queryClient = useQueryClient();

  const createAccountMutation = useMutation({
    mutationFn: async (userData: CreateAccountCommand) => {
      return apiClient.authApiAccountsPost(userData);
    },
    onSuccess: () => {
      toast.info(t("toast.create.success", { entity: t("common.branch") }));
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.create.failed", { entity: t("common.user") }));
    },
  });

  const updateAccountMutation = useMutation({
    mutationFn: async ({
      id,
      accountData,
    }: {
      id: number;
      accountData: UpdateAccount;
    }) => {
      return apiClient.authApiAccountsIdPut(id, accountData);
    },
    onSuccess: () => {
      toast.info(t("toast.update.success", { entity: t("common.branch") }));
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.update.failed", { entity: t("common.branch") }));
    },
  });

  const deleteAccountMutation = useMutation({
    mutationFn: async ({ id }: { id: number }) => {
      return apiClient.authApiAccountsIdDelete(id);
    },
    onSuccess: () => {
      toast.info(t("toast.delete.success", { entity: t("common.branch") }));
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.delete.failed", { entity: t("common.branch") }));
    },
  });

  const isLoading =
    createAccountMutation.isPending ||
    updateAccountMutation.isPending ||
    deleteAccountMutation.isPending;

  const error =
    createAccountMutation.error ||
    updateAccountMutation.error ||
    deleteAccountMutation.error;

  return {
    createAccount: createAccountMutation.mutate,
    updateAccount: updateAccountMutation.mutate,
    deleteAccount: deleteAccountMutation.mutate,
    isLoading,
    error,
  };
};
