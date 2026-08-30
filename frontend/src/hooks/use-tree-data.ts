// Generic tree data hook - có thể tái sử dụng
"use client";

import { BaseTreeNode, TreeStats } from "@/types/tree";
import { useState, useMemo } from "react";

export function useTreeData<T, N extends BaseTreeNode<T>>(
  initialData: T[],
  treeBuilder: (data: T[]) => Map<string, N>
) {
  const [data, setData] = useState<T[]>(initialData);

  const treeData = useMemo(() => treeBuilder(data), [data, treeBuilder]);

  const updateData = (updater: (prev: T[]) => T[]): void => {
    setData(updater);
  };

  return {
    data,
    treeData,
    updateData,
  };
}
