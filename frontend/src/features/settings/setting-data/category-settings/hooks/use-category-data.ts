// Category-specific data hook
"use client";

import { useMemo } from "react";
import type { CategoryTreeNode } from "../types/category";
import { CategoryUtils } from "../utils/category-utils";
import { useTreeData } from "@/hooks/use-tree-data";
import { GenericTreeBuilder } from "@/utils/tree-builder";
import {
  ActivationStatus,
  CategoryModel,
  CreateCategoryCommand,
  ListCategoryResponse,
} from "@/api/generated";

export function useListCategoryResponse(initialData: ListCategoryResponse[]) {
  const treeBuilder = useMemo(
    () => (data: any[]) =>
      GenericTreeBuilder.buildFromPaths(data, CategoryUtils.createCategoryNode),
    []
  );

  const { data, treeData, updateData } = useTreeData(initialData, treeBuilder);

  const parentOptions = useMemo(
    () => CategoryUtils.getParentOptions(data),
    [data]
  );

  const createCategory = (formData: CreateCategoryCommand): void => {
    const newItem: ListCategoryResponse = {
      name: formData.name,
      path: CategoryUtils.buildPath(
        formData.parentId ?? "",
        formData.name ?? ""
      ),
      status: ActivationStatus.Active,
    };
    updateData((prev) => [...prev, newItem]);
  };

  const updateCategory = (nodeId: any, formData: CategoryModel): void => {
    updateData((prev) =>
      prev.map((item) =>
        item.id === nodeId
          ? {
              ...item,
              name: formData.name,
              status: ActivationStatus.Active,
              path: CategoryUtils.buildPath(
                formData.parentId ?? "",
                nodeId ?? ""
              ),
            }
          : item
      )
    );
  };

  const deleteCategory = (nodeId: any): void => {
    updateData((prev) => prev.filter((item) => item.id !== nodeId));
  };

  return {
    data,
    treeData: treeData as unknown as Map<string, CategoryTreeNode>,
    parentOptions,
    createCategory,
    updateCategory,
    deleteCategory,
  };
}
