// Category-specific form hook
"use client";

import { useEntityForm } from "@/hooks/use-entity-form";
import type { CategoryTreeNode } from "../types/category";
import { CreateCategoryCommand } from "@/api/generated";

export function useCategoryForm() {
  const formHook = useEntityForm<CategoryTreeNode, CreateCategoryCommand>();

  const getInitialFormData = (): any | undefined => {
    if (formHook.mode === "edit" && formHook.editingNode) {
      const parentId = formHook.editingNode.parentId;

      return {
        id: String(formHook.editingNode.id),
        name: formHook.editingNode.name,
        parentId,
        status: formHook.editingNode.status,
      };
    }
    return undefined;
  };

  return {
    ...formHook,
    getInitialFormData,
  };
}
