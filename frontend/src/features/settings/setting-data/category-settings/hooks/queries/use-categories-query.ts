// React Query hooks for categories
"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

import { apiClient } from "@/api/client";
import {
  CategoryModel,
  CreateCategoryCommand,
  ListCategoryResponse,
} from "@/api/generated";
import { useTranslations } from "next-intl";
import { toast } from "react-toastify";

// Query keys
export const categoryKeys = {
  all: ["categories"] as const,
  lists: () => [...categoryKeys.all, "list"] as const,
  list: (params: any) => [...categoryKeys.lists(), params] as const,
  details: () => [...categoryKeys.all, "detail"] as const,
  detail: (id: string) => [...categoryKeys.details(), id] as const,
};

// Fetch categories query
export function useCategoriesQuery(params: any = {}) {
  return useQuery({
    queryKey: categoryKeys.list(params),
    queryFn: async (): Promise<ListCategoryResponse[] | null | undefined> => {
      const response = await apiClient.ecommerceApiCategoriesGet(params);

      return response.data.results!.data;
    },
    select: (data) => data || [],
  });
}

// Create category mutation
export function useCreateCategoryMutation() {
  const queryClient = useQueryClient();
  const t = useTranslations();
  return useMutation({
    mutationFn: async (command: CreateCategoryCommand) => {
      const response = await apiClient.ecommerceApiCategoriesPost(command);

      if (response.data.status !== 201) {
        throw new Error(response.data.message || "Failed to create category");
      }

      return response;
    },
    onSuccess: () => {
      // Invalidate and refetch categories
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
      toast.info(
        t("toast.create.success", {
          entity: t("common.category").toLowerCase(),
        })
      );
    },
    onError: (error) => {
      console.error("Create category failed:", error);
      toast.error(
        t("toast.create.failed", {
          entity: t("common.category").toLowerCase(),
        })
      );
    },
  });
}

// Update category mutation
export function useUpdateCategoryMutation() {
  const queryClient = useQueryClient();
  const t = useTranslations();
  return useMutation({
    mutationFn: async ({
      id,
      command,
    }: {
      id: string;
      command: CategoryModel;
    }) => {
      const response = await apiClient.ecommerceApiCategoriesIdPut(id, command);

      if (response.data.status !== 200) {
        throw new Error(response.data.message || "Failed to update category");
      }

      return response;
    },
    onSuccess: (_, { id }) => {
      // Invalidate specific category and lists
      queryClient.invalidateQueries({ queryKey: categoryKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
      toast.info(
        t("toast.update.success", {
          entity: t("common.category").toLowerCase(),
        })
      );
    },
    onError: (error) => {
      console.error("Update category failed:", error);
      toast.error(
        t("toast.update.failed", { entity: t("common.category").toLowerCase() })
      );
    },
  });
}

// Delete category mutation
export function useDeleteCategoryMutation() {
  const queryClient = useQueryClient();
  const t = useTranslations();
  return useMutation({
    mutationFn: async (id: string) => {
      const response = await apiClient.ecommerceApiCategoriesIdDelete(id);

      if (response.status !== 204) {
        throw new Error("Failed to delete category");
      }

      return response;
    },
    onSuccess: (_, id) => {
      // Remove from cache and invalidate lists
      queryClient.removeQueries({ queryKey: categoryKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
      toast.info(
        t("toast.delete.success", {
          entity: t("common.category").toLowerCase(),
        })
      );
    },
    onError: (error) => {
      console.error("Delete category failed:", error);
    },
  });
}

// Optimistic update for better UX
export function useOptimisticCategoryMutation() {
  const queryClient = useQueryClient();
  const t = useTranslations();

  const createMutation = useMutation({
    mutationFn: async (command: CreateCategoryCommand) => {
      const response = await apiClient.ecommerceApiCategoriesPost(command);

      if (response.data.status !== 201) {
        throw new Error(
          response.data.message ||
            t("toast.create.failed", { entity: t("common.category") })
        );
      }

      return response;
    },
    onMutate: async (newCategory) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: categoryKeys.lists() });

      // Snapshot previous value
      const previousCategories = queryClient.getQueryData(
        categoryKeys.list({})
      );

      // Optimistically update
      if (previousCategories) {
        const optimisticCategory: ListCategoryResponse = {
          name: newCategory.name || "",
          parentId: newCategory.parentId || null,
          status: newCategory.status,
          createdAt: new Date().toISOString(),
        };

        queryClient.setQueryData(categoryKeys.list({}), [
          ...(previousCategories as ListCategoryResponse[]),
          optimisticCategory,
        ]);
      }

      return { previousCategories };
    },
    onError: (err, newCategory, context) => {
      // Rollback on error
      if (context?.previousCategories) {
        queryClient.setQueryData(
          categoryKeys.list({}),
          context.previousCategories
        );
      }
    },
    onSettled: () => {
      // Always refetch after error or success
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
    },
  });

  return { createMutation };
}
