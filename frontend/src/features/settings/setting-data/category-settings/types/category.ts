// Updated category types to match API
import { ActivationStatus, ListCategoryResponse } from "@/api/generated";
import type { BaseTreeNode } from "@/types/tree";
import type { TreeStats } from "@/types/tree";

export interface CategoryTreeNode extends BaseTreeNode<ListCategoryResponse> {
  status: ActivationStatus;
  code: string | null;
  parentId: any | null;
}

export interface CategoryStats extends TreeStats {
  totalCategories: number;
  activeCategories: number;
}
