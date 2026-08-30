// Generic tree types - có thể tái sử dụng cho bất kỳ loại tree nào
export interface BaseTreeNode<T = any> {
  id: any;
  name: string | null | undefined;
  path: string | null | undefined;
  isLeaf: boolean;
  children: Map<string, BaseTreeNode<T>>;
  originalData?: T;
}

export interface TreeFilters {
  searchTerm: string;
  showDisabled: boolean;
}

export interface TreeStats {
  total: number;
  active: number;
}

export interface ParentOption {
  id: any;
  code?: string | null;
  name: string | null | undefined;
  path: string | null | undefined;
}

export type FormMode = "create" | "edit" | "add-child";
