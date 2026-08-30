"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { useQueryState } from "nuqs";
import { useTranslations } from "next-intl";
import { UnitModel } from "@/api/generated";
import { toast } from "react-toastify";

export const useUnitSettings = () => {
  const t = useTranslations();
  const queryClient = useQueryClient();
  // Query state for pagination and search
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");

  // Fetch units query
  const {
    data,
    isFetching,
    isLoading: isQueryLoading,
    error: queryError,
  } = useQuery({
    queryKey: ["units", { page, pageSize, search }],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiUnitsGet(
        parseInt(page) || undefined,
        parseInt(pageSize) || undefined,
        undefined,
        undefined,
        search || undefined,
        ["name"]
      );
      return {
        units: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
  });

  // Create unit mutation
  const createUnitMutation = useMutation({
    mutationFn: async (unitData: UnitModel) => {
      return apiClient.ecommerceApiUnitsPost(unitData);
    },
    onSuccess: () => {
      toast.info(t("toast.create.success", { entity: t("common.unit") }));
      queryClient.invalidateQueries({ queryKey: ["units"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.create.failed", { entity: t("common.unit") }));
    },
  });

  // Update unit mutation
  const updateUnitMutation = useMutation({
    mutationFn: async ({
      id,
      unitData,
    }: {
      id: number;
      unitData: UnitModel;
    }) => {
      return apiClient.ecommerceApiUnitsIdPut(id, unitData);
    },
    onSuccess: () => {
      toast.info(t("toast.update.success", { entity: t("common.unit") }));
      queryClient.invalidateQueries({ queryKey: ["units"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.update.failed", { entity: t("common.unit") }));
    },
  });

  const deleteUnitMutation = useMutation({
    mutationFn: async ({ id }: { id: number }) => {
      return apiClient.ecommerceApiUnitsIdDelete(id);
    },
    onSuccess: () => {
      toast.info(t("toast.delete.success", { entity: t("common.unit") }));
      queryClient.invalidateQueries({ queryKey: ["units"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.delete.failed", { entity: t("common.unit") }));
    },
  });

  const isLoading =
    isQueryLoading ||
    isFetching ||
    createUnitMutation.isPending ||
    updateUnitMutation.isPending ||
    deleteUnitMutation.isPending;

  const error =
    createUnitMutation.error ||
    updateUnitMutation.error ||
    deleteUnitMutation.error ||
    queryError;

  return {
    units: data?.units || [],
    paging: data?.paging || {},
    isLoading,
    error,
    createUnit: createUnitMutation.mutate,
    updateUnit: updateUnitMutation.mutate,
    deleteUnit: deleteUnitMutation.mutate,
  };
};
