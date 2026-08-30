// Generic tree node component - có thể tái sử dụng
"use client";

import { useState, type ReactNode } from "react";
import type { BaseTreeNode, TreeFilters } from "@/types/tree";

interface TreeNodeBaseProps<T extends BaseTreeNode<any>> {
  node: T;
  level?: number;
  filters: TreeFilters;
  onEdit: (node: T) => void;
  onDelete: (node: T) => void;
  onAddChild: (node: T) => void;
  renderIcon: (node: T, isExpanded: boolean, onToggle: () => void) => ReactNode;
  renderContent: (node: T, level: number) => ReactNode;
  renderActions: (
    node: T,
    onEdit: () => void,
    onDelete: () => void,
    onAddChild: () => void
  ) => ReactNode;
  matchesSearch: (
    node: BaseTreeNode<T>,
    searchTerm: string,
    searchField: (keyof T)[]
  ) => boolean;
  matchesFilters: (node: T, filters: TreeFilters) => boolean;
  confirmDelete?: (nodeName: string) => boolean;
}

export function TreeNodeBase<T extends BaseTreeNode<any>>({
  node,
  level = 0,
  filters,
  onEdit,
  onDelete,
  onAddChild,
  renderIcon,
  renderContent,
  renderActions,
  matchesSearch,
  matchesFilters,
  confirmDelete = (name) => confirm(`Bạn có chắc chắn muốn xóa "${name}"?`),
}: TreeNodeBaseProps<T>) {
  const [isExpanded, setIsExpanded] = useState(false);

  const hasChildren = node.children.size > 0;
  const indent = level * 20;

  if (
    !matchesSearch(node, filters.searchTerm, ["name"]) ||
    !matchesFilters(node, filters)
  ) {
    return null;
  }

  const handleToggle = (): void => {
    setIsExpanded(!isExpanded);
  };

  const handleDelete = (): void => {
    if (confirmDelete(node.name ?? "")) {
      onDelete(node);
    }
  };

  return (
    <div className="select-none">
      <div
        className={`group flex items-center py-2 px-3 rounded-md transition-all duration-200`}
        style={{ marginLeft: `${indent}px` }}
      >
        <div className="flex items-center min-w-0 flex-1 gap-2">
          {renderIcon(node, isExpanded, handleToggle)}
          {renderContent(node, level)}
        </div>

        {renderActions(
          node,
          () => onEdit(node),
          handleDelete,
          () => onAddChild(node)
        )}
      </div>

      {hasChildren && isExpanded && (
        <div className="ml-2 border-l">
          {Array.from(node.children.values()).map((child) => (
            <TreeNodeBase
              key={child.path}
              node={child as T}
              level={level + 1}
              filters={filters}
              onEdit={onEdit}
              onDelete={onDelete}
              onAddChild={onAddChild}
              renderIcon={renderIcon}
              renderContent={renderContent}
              renderActions={renderActions}
              matchesSearch={matchesSearch}
              matchesFilters={matchesFilters}
              confirmDelete={confirmDelete}
            />
          ))}
        </div>
      )}
    </div>
  );
}
