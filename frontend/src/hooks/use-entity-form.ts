// Generic form hook - có thể tái sử dụng
"use client";

import { BaseTreeNode, FormMode } from "@/types/tree";
import { useState } from "react";

export function useEntityForm<T extends BaseTreeNode<any>, F>() {
  const [isOpen, setIsOpen] = useState(false);
  const [mode, setMode] = useState<FormMode>("create");
  const [editingNode, setEditingNode] = useState<T | null>(null);
  const [defaultParentId, setDefaultParentId] = useState<
    number | null | undefined
  >(null);

  const openCreateForm = (): void => {
    setMode("create");
    setEditingNode(null);
    setDefaultParentId(null);
    setIsOpen(true);
  };

  const openEditForm = (node: T): void => {
    setMode("edit");
    setEditingNode(node);
    setIsOpen(true);
  };

  const openAddChildForm = (node: T): void => {
    setMode("add-child");
    setEditingNode(node);
    setDefaultParentId(node.id!);
    setIsOpen(true);
  };

  const closeForm = (): void => {
    setIsOpen(false);
    setEditingNode(null);
    setDefaultParentId(null);
  };

  return {
    isOpen,
    mode,
    editingNode,
    defaultParentId,
    openCreateForm,
    openEditForm,
    openAddChildForm,
    closeForm,
  };
}
