import { GetTopServiceResponse } from "@/api/generated/api";
import { formatPriceVN } from "@/utils/format";
import { useTranslations } from "next-intl";
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { useIsMobile } from "@/hooks/use-mobile";
import React, { memo } from "react";

interface TopServicesChartProps {
  data: Array<GetTopServiceResponse>;
}

const TopServicesChartComponent = ({ data }: TopServicesChartProps) => {
  const t = useTranslations();
  const isMobile = useIsMobile();

  const chartData = data.slice(0, isMobile ? 5 : 10).map((item) => ({
    service:
      item.serviceName!.length > 15
        ? `${item.serviceName!.slice(0, 15)}...`
        : item.serviceName || "Unknown Service",
    value: item.totalRevenue || 0,
    fullServiceName: item.serviceName || "Unknown Service",
  }));

  const dynamicHeight = Math.max(200, chartData.length * (isMobile ? 40 : 50));
  const barMargin = isMobile
    ? { top: 5, right: 5, left: 100, bottom: 5 }
    : { top: 10, right: 10, left: 120, bottom: 20 };

  return (
    <div className="w-full">
      <ResponsiveContainer width="100%" height={dynamicHeight}>
        <BarChart data={chartData} layout="vertical" margin={barMargin}>
          <CartesianGrid horizontal={false} strokeDasharray="3 3" />
          <XAxis
            type="number"
            tickFormatter={(value) => `${formatPriceVN(Number(value))}`}
            tickLine={false}
            axisLine={false}
            tickMargin={isMobile ? 2 : 10}
            tick={{ fontSize: isMobile ? 8 : 12 }}
            interval={isMobile ? "preserveStartEnd" : "preserveStart"}
          />
          <YAxis
            type="category"
            dataKey="service"
            tickLine={false}
            axisLine={false}
            width={isMobile ? 120 : 140}
            tick={{
              fontSize: isMobile ? 10 : 12,
              textAnchor: "end",
              dx: isMobile ? -5 : 0,
            }}
            interval={0}
          />
          <Tooltip
            formatter={(value) => [
              `${formatPriceVN(Number(value))}`,
              t("revenue.title"),
            ]}
            labelFormatter={(label) =>
              chartData.find((d) => d.service === label)?.fullServiceName ||
              label
            }
            cursor={{ fill: "rgba(0, 0, 0, 0.05)" }}
            contentStyle={{
              fontSize: isMobile ? "8px" : "12px",
              padding: "2px",
            }}
            itemStyle={{ padding: 0 }}
          />
          <Bar
            dataKey="value"
            fill="hsl(var(--primary))"
            radius={[0, 4, 4, 0]}
            maxBarSize={isMobile ? 15 : 30}
            barSize={isMobile ? 10 : 20}
          />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
};

export const TopServicesChart = memo(TopServicesChartComponent);
