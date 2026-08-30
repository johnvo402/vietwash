import { ActivationStatus } from "@/api/generated";
import { SelectItem } from "@/components/ui/select";
import { CategoryTreeNode } from "@/features/settings/setting-data/category-settings/types/category";
import { cn } from "@/lib/utils";
import { Folder, FolderOpen, Minus, Plus, Tag } from "lucide-react";
import { useState } from "react";

export default function CategoryTreeNodeComponent({
  node,
}: {
  node: CategoryTreeNode;
}) {
  const [isExpanded, setIsExpanded] = useState(false);
  const hasChildren = node.children?.size > 0;

  const handleToggleExpand = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsExpanded(!isExpanded);
  };

  const nodeId = node.id?.toString() || "";

  if (!node.name || !nodeId) return null;

  return (
    <div>
      <div className="flex items-center w-full hover:bg-accent/50 transition-colors">
        {hasChildren ? (
          <div
            className="flex items-center cursor-pointer p-1 hover:bg-accent rounded mr-1"
            onClick={handleToggleExpand}
          >
            {isExpanded ? (
              <Minus className="h-3 w-3 text-muted-foreground hover:text-foreground" />
            ) : (
              <Plus className="h-3 w-3 text-muted-foreground hover:text-foreground" />
            )}
          </div>
        ) : (
          <div className="w-5" />
        )}

        <SelectItem
          value={nodeId}
          className={cn(
            "flex-1 cursor-pointer transition-colors border-0 focus:bg-transparent",
            "data-[state=checked]:bg-primary/10 data-[state=checked]:text-primary",
            node.status === ActivationStatus.Inactive &&
              "opacity-50 text-muted-foreground"
          )}
          disabled={node.status === ActivationStatus.Inactive}
        >
          <div className="flex items-center w-full">
            <div className="mr-2 flex-shrink-0">
              {hasChildren ? (
                isExpanded ? (
                  <FolderOpen className="h-4 w-4 text-blue-500" />
                ) : (
                  <Folder className="h-4 w-4 text-blue-600" />
                )
              ) : (
                <Tag className="h-4 w-4 text-gray-500" />
              )}
            </div>

            <span
              className={cn(
                "flex-1 truncate",
                hasChildren && "font-medium",
                node.status === ActivationStatus.Inactive && "line-through"
              )}
            >
              {node.name}
            </span>

            <div className="flex items-center gap-1 ml-2">
              <div
                className={cn(
                  "w-2 h-2 rounded-full",
                  node.status === ActivationStatus.Active
                    ? "bg-green-500"
                    : "bg-red-500"
                )}
              />
              {hasChildren && (
                <span className="text-xs text-muted-foreground bg-muted px-1.5 py-0.5 rounded">
                  {node.children?.size || 0}
                </span>
              )}
            </div>
          </div>
        </SelectItem>
      </div>

      {hasChildren && isExpanded && (
        <div className="ml-4">
          {Array.from(node.children.values()).map((childNode) => (
            <CategoryTreeNodeComponent
              key={childNode.path}
              node={childNode as CategoryTreeNode}
            />
          ))}
        </div>
      )}
    </div>
  );
}
