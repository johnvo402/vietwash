"use client";

import {
  PieChart as RechartsPieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
} from "recharts";
import { useAuth } from "@/hooks/use-auth";
import { GetNetRevenueBranchResponse } from "@/api/generated";
import { formatPriceVN } from "@/utils/format";
import { useIsMobile } from "@/hooks/use-mobile";

interface PieChartProps {
  data: Array<GetNetRevenueBranchResponse>;
}

const COLORS = [
  "#1E3A8A", // Dark Blue
  "#2563EB", // Bright Blue
  "#0EA5E9", // Sky Blue
  "#14B8A6", // Teal
  "#22D3EE", // Cyan
  "#3B82F6", // Standard Blue
];

// Custom hook to detect mobile

export function PieChart({ data }: PieChartProps) {
  const { user } = useAuth();
  const isMobile = useIsMobile();

  const chartData = data.map((item) => ({
    name:
      user?.branchAccounts?.find((b) => b.branchId === item.branchId)
        ?.branchName || `Branch ${item.branchId}`,
    value: item.percentage,
    totalNetRevenue: item.totalNetRevenue,
  }));

  const chartWidth = isMobile ? 300 : 500; // Giảm width cho mobile
  const chartHeight = isMobile ? 200 : 300; // Giảm height cho mobile
  const outerRadius = isMobile ? 70 : 100; // Điều chỉnh radius cho mobile
  const innerRadius = isMobile ? 30 : 0; // Thêm innerRadius cho donut trên mobile

  return (
    <div className="w-full max-w-[500px] mx-auto overflow-hidden p-2">
      <RechartsPieChart width={chartWidth} height={chartHeight}>
        <Pie
          data={chartData}
          dataKey="value"
          nameKey="name"
          cx="50%"
          cy="50%"
          outerRadius={outerRadius}
          innerRadius={innerRadius}
        >
          {chartData.map((_, index) => (
            <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
          ))}
        </Pie>
        <Tooltip
          formatter={(value: number, name: string, props: any) => [
            `${value}% (${formatPriceVN(props.payload.totalNetRevenue)})`,
            name,
          ]}
          contentStyle={{
            fontSize: isMobile ? "10px" : "14px",
            padding: isMobile ? "4px" : "8px",
            borderRadius: "4px",
          }}
        />
        <Legend
          layout={isMobile ? "vertical" : "horizontal"}
          align="center"
          verticalAlign={isMobile ? "bottom" : "bottom"}
          wrapperStyle={{
            fontSize: isMobile ? "10px" : "14px",
            paddingTop: isMobile ? 5 : 20,
          }}
        />
      </RechartsPieChart>
    </div>
  );
}
