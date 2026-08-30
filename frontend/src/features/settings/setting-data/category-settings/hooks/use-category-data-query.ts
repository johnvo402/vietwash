// High-level hook using React Query
"use client";

import { useMemo } from "react";
import type { CategoryTreeNode } from "../types/category";

import { CategoryUtils } from "../utils/category-utils";
import {
  useCategoriesQuery,
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useDeleteCategoryMutation,
} from "./queries/use-categories-query";
import { GenericTreeBuilder, TreeBuildable } from "@/utils/tree-builder";
import { CategoryModel, CreateCategoryCommand } from "@/api/generated";
import { BaseTreeNode } from "@/types/tree";

export function useListCategoryResponseQuery() {
  // Fetch categories
  const {
    data: categories = [],
    isLoading,
    isError,
    error,
    refetch,
    isFetching,
  } = useCategoriesQuery();

  // Mutations
  const createMutation = useCreateCategoryMutation();
  const updateMutation = useUpdateCategoryMutation();
  const deleteMutation = useDeleteCategoryMutation();

  // Build tree data
  const treeData = useMemo(() => {
    if (!categories.length) return new Map<string, CategoryTreeNode>();
    // Convert categories to TreeBuildable[] by mapping id to string
    const treeBuildableCategories: TreeBuildable[] = categories.map(
      (cat: any) => ({
        ...cat,
        id: String(cat.id),
      })
    );

    // Use a compatible createCategoryNode function that accepts TreeBuildable and returns BaseTreeNode<TreeBuildable>
    const createCategoryNode = (
      item: TreeBuildable,
      path: string,
      isLeaf: boolean,
      allItems: TreeBuildable[]
    ): BaseTreeNode<TreeBuildable> => {
      // Always ensure id is a string and cast result to unknown first if needed
      return CategoryUtils.createCategoryNode(
        {
          ...item,
          id: typeof item.id === "string" ? Number(item.id) : item.id,
        },
        path,
        isLeaf,
        allItems.map((i) => ({
          ...i,
          id: typeof i.id === "string" ? Number(i.id) : i.id,
        }))
      ) as unknown as BaseTreeNode<TreeBuildable>;
    };

    const baseTreeMap = GenericTreeBuilder.buildFromPaths(
      treeBuildableCategories,
      createCategoryNode
    ) as Map<string, BaseTreeNode<TreeBuildable>>;

    // Convert BaseTreeNode<TreeBuildable> to CategoryTreeNode
    function convertNode(node: BaseTreeNode<TreeBuildable>): CategoryTreeNode {
      // Recursively convert children
      const convertedChildren = new Map<string, CategoryTreeNode>();
      node.children.forEach((childNode, childKey) => {
        convertedChildren.set(childKey, convertNode(childNode));
      });

      // Map id to number if possible, otherwise undefined
      const id =
        typeof node.id === "string" && !isNaN(Number(node.id))
          ? Number(node.id)
          : node.id;

      return {
        id: id as number | null | undefined,
        name: node.name,
        code: (node as any).code ?? null,
        path: node.path,
        isLeaf: node.isLeaf,
        children: convertedChildren,
        status: (node as any).status ?? (node as any).data?.status,
        parentId: (node as any).parentId ?? (node as any).data?.parentId,
        originalData: node.originalData
          ? {
              ...node.originalData,
              id:
                typeof node.originalData.id === "string" &&
                !isNaN(Number(node.originalData.id))
                  ? Number(node.originalData.id)
                  : typeof node.originalData.id === "number"
                    ? node.originalData.id
                    : undefined,
            }
          : undefined,
      };
    }

    const categoryTreeMap = new Map<string, CategoryTreeNode>();
    baseTreeMap.forEach((node, key) => {
      categoryTreeMap.set(key, convertNode(node));
    });
    return categoryTreeMap;
  }, [categories]);

  // Calculate stats

  // Parent options for form
  const parentOptions = useMemo(
    () => CategoryUtils.getParentOptions(categories),
    [categories]
  );

  // CRUD operations
  const createCategory = async (
    formData: CreateCategoryCommand
  ): Promise<void> => {
    const command: CreateCategoryCommand = {
      name: formData.name,
      parentId: formData.parentId,
      status: formData.status,
    };

    await createMutation.mutateAsync(command);
  };

  const updateCategory = async (
    nodeId: string,
    formData: CreateCategoryCommand
  ): Promise<void> => {
    const command: CategoryModel = {
      name: formData.name,
      parentId: formData.parentId,
      status: formData.status,
    };

    await updateMutation.mutateAsync({ id: nodeId, command });
  };

  const deleteCategory = async (nodeId: string): Promise<void> => {
    await deleteMutation.mutateAsync(nodeId);
  };

  // Combined loading state
  const loading =
    isLoading ||
    createMutation.isPending ||
    updateMutation.isPending ||
    deleteMutation.isPending;

  // Combined error state
  const combinedError =
    error ||
    createMutation.error ||
    updateMutation.error ||
    deleteMutation.error;

  return {
    // Data
    data: categories,
    treeData: treeData as Map<string, CategoryTreeNode>,
    parentOptions,

    // Loading states
    loading,
    isLoading,
    isFetching,
    isRefetching: isFetching && !isLoading,

    // Error states
    error: combinedError,
    isError:
      isError ||
      createMutation.isError ||
      updateMutation.isError ||
      deleteMutation.isError,

    // Mutation states
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,

    // Operations
    createCategory,
    updateCategory,
    deleteCategory,
    refetch,

    // Mutation objects for advanced usage
    mutations: {
      create: createMutation,
      update: updateMutation,
      delete: deleteMutation,
    },
  };
}
