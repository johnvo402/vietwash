import { Badge } from "@/components/ui/badge";
import { CategoryTreeNode } from "../types/category";
import { ActivationStatus } from "@/api/generated";

interface CategoryNodeContentProps {
  node: CategoryTreeNode;
  level: number;
}

export function CategoryNodeContent({ node, level }: CategoryNodeContentProps) {
  const getStatusColor = (): string => {
    return node.status === ActivationStatus.Inactive
      ? "bg-red-100 text-red-800 border-red-200  hover:bg-red-100"
      : "bg-green-100 text-green-800 border-green-200 hover:bg-green-100";
  };

  const getStatusText = (): string => {
    return node.status;
  };

  const getTextStyle = (): string => {
    const baseStyle = "font-medium truncate";
    switch (level) {
      case 0:
        return `${baseStyle} text-lg `;
      case 1:
        return `${baseStyle} text-base`;
      case 2:
        return `${baseStyle} text-sm`;
      default:
        return `${baseStyle} text-sm`;
    }
  };

  return (
    <div className="flex items-center justify-between min-w-0 w-full">
      <div className="flex items-center gap-2 flex-wrap min-w-0">
        <span className="font-mono shrink-0">{`[${node.code!}]`}</span>
        <span className={`${getTextStyle()} truncate`}>{node.name}</span>
      </div>

      <Badge className={`text-xs mr-6 ${getStatusColor()}`}>
        {getStatusText()}
      </Badge>
    </div>
  );
}
