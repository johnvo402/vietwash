"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { useQueryState } from "nuqs";
import { useTranslations } from "next-intl";
import { BranchModel, CreateBranchCommand } from "@/api/generated";
import { toast } from "react-toastify";

export const useBranchSettings = () => {
  const t = useTranslations();
  const queryClient = useQueryClient();
  // Query state for pagination and search
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");

  // Fetch branchs query
  const {
    data,
    isFetching,
    isLoading: isQueryLoading,
    error: queryError,
  } = useQuery({
    queryKey: ["branchs", { page, pageSize, search }],
    queryFn: async () => {
      const response = await apiClient.projectApiBranchesGet(
        parseInt(page) || 1,
        parseInt(pageSize) || 10,
        undefined,
        undefined,
        search || undefined,
        ["name", "code"]
      );
      return {
        branchs: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
  });

  // Create branch mutation
  const createBranchMutation = useMutation({
    mutationFn: async (branchData: CreateBranchCommand) => {
      return apiClient.projectApiBranchesPost(branchData);
    },
    onSuccess: () => {
      toast.info(t("toast.create.success", { entity: t("common.branch") }));
      queryClient.invalidateQueries({ queryKey: ["branchs"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.create.failed", { entity: t("common.branch") }));
    },
  });

  // Update branch mutation
  const updateBranchMutation = useMutation({
    mutationFn: async ({
      id,
      branchData,
    }: {
      id: number;
      branchData: BranchModel;
    }) => {
      return apiClient.projectApiBranchesIdPut(id, branchData);
    },
    onSuccess: () => {
      toast.info(t("toast.update.success", { entity: t("common.branch") }));
      queryClient.invalidateQueries({ queryKey: ["branchs"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.update.failed", { entity: t("common.branch") }));
    },
  });

  const deleteBranchMutation = useMutation({
    mutationFn: async ({ id }: { id: number }) => {
      return apiClient.projectApiBranchesIdDelete(id);
    },
    onSuccess: () => {
      toast.info(t("toast.delete.success", { entity: t("common.branch") }));
      queryClient.invalidateQueries({ queryKey: ["branchs"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.delete.failed", { entity: t("common.branch") }));
    },
  });

  const isLoading =
    isQueryLoading ||
    isFetching ||
    createBranchMutation.isPending ||
    updateBranchMutation.isPending ||
    deleteBranchMutation.isPending;

  const error =
    createBranchMutation.error ||
    updateBranchMutation.error ||
    deleteBranchMutation.error ||
    queryError;

  return {
    branchs: data?.branchs || [],
    paging: data?.paging || {},
    isLoading,
    error,
    createBranch: createBranchMutation.mutate,
    updateBranch: updateBranchMutation.mutate,
    deleteBranch: deleteBranchMutation.mutate,
  };
};
