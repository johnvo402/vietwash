// Category-specific tree node
"use client";

import { CategoryNodeIcon } from "./category-node-icon";
import { CategoryNodeContent } from "./category-node-content";
import { CategoryNodeActions } from "./category-node-actions";
import { TreeNodeBase } from "@/components/tree/tree-node-base";
import { CategoryTreeNode } from "../types/category";
import { TreeFilters } from "@/types/tree";
import { CategoryUtils } from "../utils/category-utils";
import { useTranslations } from "next-intl";

interface CategoryTreeNodeProps {
  node: CategoryTreeNode;
  level?: number;
  filters: TreeFilters;
  onEdit: (node: CategoryTreeNode) => void;
  onDelete: (node: CategoryTreeNode) => void;
  onAddChild: (node: CategoryTreeNode) => void;
}

export function CategoryTreeNodeComponent(props: CategoryTreeNodeProps) {
  const t = useTranslations();
  return (
    <TreeNodeBase
      {...props}
      renderIcon={(node, isExpanded, onToggle) => (
        <CategoryNodeIcon
          node={node}
          isExpanded={isExpanded}
          onToggle={onToggle}
        />
      )}
      renderContent={(node, level) => (
        <CategoryNodeContent node={node} level={level} />
      )}
      renderActions={(node, onEdit, onDelete, onAddChild) => (
        <CategoryNodeActions
          node={node}
          onEdit={onEdit}
          onDelete={onDelete}
          onAddChild={onAddChild}
        />
      )}
      matchesSearch={CategoryUtils.matchesSearch}
      matchesFilters={(node, filters) =>
        CategoryUtils.matchesFilters(node, filters.showDisabled)
      }
      confirmDelete={(name) =>
        confirm(`${t("common.deleteConfirm.description", { entity: t("common.category"), entityName: name })}`)
      }
    />
  );
}
