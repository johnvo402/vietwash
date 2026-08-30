import {
  Select,
  SelectContent,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { CategoryTreeNode } from "@/features/settings/setting-data/category-settings/types/category";
import { cn } from "@/lib/utils";
import { useMemo } from "react";
import CategoryTreeNodeComponent from "./category-tree-node";
import { ActivationStatus } from "@/api/generated";
import { useTranslations } from "next-intl";

interface CategorySelectProps {
  treeData?: Map<string, CategoryTreeNode>;
  value?: number;
  onValueChange?: (value: string) => void;
  placeholder?: string;
  className?: string;
}
export default function CategorySelect({
  treeData,
  value,
  onValueChange,
  placeholder,
  className,
}: CategorySelectProps) {
  const t = useTranslations();
  const selectedCategory = useMemo(
    () => (value ? findCategoryById(value, treeData) : null),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [value, treeData]
  );

  function findCategoryById(
    id: number,
    treeNodes?: Map<string, CategoryTreeNode>
  ): CategoryTreeNode | null {
    if (!treeNodes) return null;

    for (const [key, node] of treeNodes) {
      if (String(node.id) === id.toString()) {
        return node;
      }
      if (node.children?.size > 0) {
        const found = findCategoryById(
          id,
          node.children as Map<string, CategoryTreeNode>
        );
        if (found) return found;
      }
    }
    return null;
  }
  return (
    <Select value={value?.toString()} onValueChange={onValueChange}>
      <SelectTrigger className={cn("w-full", className)}>
        <SelectValue placeholder={t("common.placeholderSelect", { entity: t("common.category") })}>
          {selectedCategory && (
            <div className="flex items-center gap-2">
              <span>{selectedCategory.name}</span>
              <div
                className={cn(
                  "w-2 h-2 rounded-full",
                  selectedCategory.status === ActivationStatus.Active
                    ? "bg-green-500"
                    : "bg-red-500"
                )}
              />
            </div>
          )}
        </SelectValue>
      </SelectTrigger>
      <SelectContent className="max-h-80">
        {treeData &&
          Array.from(treeData.values()).map((node) => (
            <CategoryTreeNodeComponent key={node.path} node={node} />
          ))}
      </SelectContent>
    </Select>
  );
}
