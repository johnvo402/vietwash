"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { useQueryState } from "nuqs";
import { useTranslations } from "next-intl";
import { CreateTariffCommand, TariffModel } from "@/api/generated";
import { toast } from "react-toastify";
import { useQueryFilter } from "@/lib/filter";

export const useTariff = ({
  branchId = null,
}: {
  branchId?: number | null;
}) => {
  const t = useTranslations();
  const queryClient = useQueryClient();
  // Query state for pagination and search
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");
  const { prepareApiParams, flattenQueryObject } = useQueryFilter();
  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeyword: search || undefined,
    searchTargets: search ? ["name", "code"] : undefined,
    filter: flattenQueryObject({
      ...(branchId ? { branchId: { $eq: branchId } } : {}),
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
  // Fetch tariffs query
  const {
    data,
    isFetching,
    isLoading: isQueryLoading,
    error: queryError,
  } = useQuery({
    queryKey: ["tariffs", { page, pageSize, search, branchId }],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiTariffsGet(...args);
      return {
        tariffs: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
  });

  // Update tariff mutation
  const updateTariffMutation = useMutation({
    mutationFn: async ({
      id,
      tariffData,
    }: {
      id: number;
      tariffData: TariffModel;
    }) => {
      return apiClient.ecommerceApiTariffsIdPut(id, tariffData);
    },
    onSuccess: () => {
      toast.info(t("toast.update.success", { entity: t("common.tariff") }));
      queryClient.invalidateQueries({ queryKey: ["tariffs"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.update.failed", { entity: t("common.tariff") }));
    },
  });
  const createTariff = useMutation({
    mutationFn: async ({ tariffData }: { tariffData: CreateTariffCommand }) => {
      return apiClient.ecommerceApiTariffsPost(tariffData);
    },
    onSuccess: () => {
      toast.info(t("toast.create.success", { entity: t("common.tariff") }));
      queryClient.invalidateQueries({ queryKey: ["tariffs"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.create.failed", { entity: t("common.tariff") }));
    },
  });
  const isLoading =
    isQueryLoading ||
    isFetching ||
    updateTariffMutation.isPending ||
    createTariff.isPending;
  const error = updateTariffMutation.error || queryError;

  return {
    tariffs: data?.tariffs || [],
    paging: data?.paging || {},
    isLoading,
    error,
    updateTariff: updateTariffMutation.mutateAsync,
    createTariff,
  };
};
