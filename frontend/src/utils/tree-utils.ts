// Generic tree utilities - có thể tái sử dụng

import { BaseTreeNode } from "@/types/tree";

export class GenericTreeUtils {
  static buildPath(parentId: any | null, childId: string): string {
    return parentId ? `${parentId}.${childId}` : childId;
  }

  static matchesSearch<T>(
    node: BaseTreeNode<T>,
    searchTerm: string,
    searchFields: (keyof T)[]
  ): boolean {
    if (!searchTerm) return true;

    const term = searchTerm.toLowerCase();

    // Search in node name and id
    if (
      node.name?.toLowerCase().includes(term) ||
      String(node.id)?.toLowerCase().includes(term)
    ) {
      return true;
    }

    // Search in original data fields
    if (node.originalData) {
      return searchFields.some((field) => {
        const value = node.originalData![field];
        return typeof value === "string" && value.toLowerCase().includes(term);
      });
    }

    return false;
  }
}
