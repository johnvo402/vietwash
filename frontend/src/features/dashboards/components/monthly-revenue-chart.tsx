import { GetRevenueStatistic } from "@/api/generated/api";
import { useLanguage } from "@/hooks/use-language";
import { formatPriceVN } from "@/utils/format";
import {
  Bar,
  BarChart,
  CartesianGrid,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import { useIsMobile } from "@/hooks/use-mobile";
import React, { memo, useMemo } from "react";

interface MonthlyRevenueChartProps {
  data: Array<GetRevenueStatistic>;
}

const MonthlyRevenueChartComponent = ({ data }: MonthlyRevenueChartProps) => {
  const { lang } = useLanguage();
  const isMobile = useIsMobile();

  const formattedData = useMemo(() => {
    return (
      data &&
      data.map((item) => ({
        ...item,
        date: new Date(item.revenueDate!)
          .toLocaleString(lang, {
            day: "2-digit",
            month: "2-digit",
          })
          .toString(),
      }))
    );
  }, [data, lang]);

  const dynamicHeight = isMobile ? 200 : 300;
  const barMargin = isMobile
    ? { top: 5, right: 5, left: 30, bottom: 10 }
    : { top: 10, right: 10, left: 50, bottom: 20 };
  const maxBarSize = isMobile ? 40 : 60;
  const tickFontSize = isMobile ? 10 : 12;

  return (
    <div className="w-full">
      <ResponsiveContainer width="100%" height={dynamicHeight}>
        <BarChart data={formattedData} margin={barMargin}>
          <CartesianGrid vertical={false} strokeDasharray="3 3" />
          <XAxis
            dataKey="date"
            tickLine={false}
            axisLine={false}
            tickMargin={isMobile ? 5 : 10}
            tick={{
              fontSize: tickFontSize,
              textAnchor: "end",
              dx: isMobile ? -5 : 0,
            }}
            interval={isMobile ? "preserveStartEnd" : "preserveStart"}
          />
          <YAxis
            tickFormatter={(value) => `${formatPriceVN(Number(value))}`}
            tickLine={false}
            axisLine={false}
            tickMargin={isMobile ? 5 : 10}
            tick={{ fontSize: tickFontSize }}
          />
          <Tooltip
            formatter={(value) => [`${formatPriceVN(Number(value))}`]}
            cursor={{ fill: "rgba(0, 0, 0, 0.05)" }}
            contentStyle={{
              fontSize: isMobile ? "10px" : "12px",
              padding: "2px",
            }}
          />
          <Bar
            dataKey="totalRevenue"
            fill="hsl(var(--primary))"
            radius={[4, 4, 0, 0]}
            maxBarSize={maxBarSize}
          />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
};

export const MonthlyRevenueChart = memo(MonthlyRevenueChartComponent);
