// Generic tree stats component - có thể tái sử dụng
import type { TreeStats } from "../../types/tree";

interface TreeStatsProps {
  stats: TreeStats;
  labels?: {
    total?: string;
    active?: string;
  };
}

export function TreeStatsComponent({
  stats,
  labels = { total: "Tổng số", active: "Hoạt động" },
}: TreeStatsProps) {
  return (
    <div className="flex gap-6 text-sm text-gray-600 mt-2">
      <span className="flex items-center gap-1">
        <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
        {labels.total}: {stats.total}
      </span>
      <span className="flex items-center gap-1">
        <div className="w-2 h-2 bg-green-500 rounded-full"></div>
        {labels.active}: {stats.active}
      </span>
    </div>
  );
}
