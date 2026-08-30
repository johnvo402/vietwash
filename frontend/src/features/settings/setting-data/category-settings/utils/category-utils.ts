// Category-specific utilities
import { BaseTreeNode, ParentOption } from "@/types/tree";
import type { CategoryTreeNode } from "../types/category";
import { GenericTreeUtils } from "@/utils/tree-utils";
import { ActivationStatus, ListCategoryResponse } from "@/api/generated";

export class CategoryUtils extends GenericTreeUtils {
  static getParentOptions(data: ListCategoryResponse[]): ParentOption[] {
    return data
      .filter((item) => item.status === ActivationStatus.Active)
      .map((item) => ({
        id: item.id,
        code: item.code || null,
        name: item.name,
        path: item.path || (item.id !== undefined ? String(item.id) : null),
      }));
  }

  static matchesSearch<T>(
    node: BaseTreeNode<T>,
    searchTerm: string,
    searchFields: (keyof T)[]
  ): boolean {
    if (!searchTerm) return true;

    const term = searchTerm.toLowerCase();

    return searchFields.some((field) => {
      const val = node.originalData?.[field];
      return typeof val === "string" && val.toLowerCase().includes(term);
    });
  }
  static matchesFilters(
    node: CategoryTreeNode,
    showInactive: boolean
  ): boolean {
    return showInactive || node.status === ActivationStatus.Active;
  }

  static createCategoryNode(
    item: ListCategoryResponse,
    path: string,
    isLeaf: boolean,
    allData: ListCategoryResponse[]
  ): CategoryTreeNode {
    return {
      id: item.id,
      name: item.name,
      code: item.code || null,
      path,
      isLeaf,
      status: item.status || ActivationStatus.Active,
      parentId: item.parentId || null,
      children: new Map(),
      originalData: {
        id: item.id,
        name: item.name,
      },
    };
  }

  static getStatusText(status: ActivationStatus): string {
    return status === ActivationStatus.Active ? "Active" : "Inactive";
  }

  static getStatusColor(status: ActivationStatus): string {
    return status === ActivationStatus.Active
      ? "bg-green-100 text-green-800 border-green-200"
      : "bg-red-100 text-red-800 border-red-200";
  }
}
