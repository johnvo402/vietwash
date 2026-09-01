"use client";

import { ArrowDownIcon, ArrowUpIcon, MinusIcon } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatPriceVN } from "@/utils/format";
import { GetDashboardCardResponse } from "@/api/generated";
import { useTranslations } from "next-intl";

interface RevenueCardsProps {
  data: GetDashboardCardResponse;
}

export function RevenueCards({ data }: RevenueCardsProps) {
  // Tính toán sự khác biệt và phần trăm so với hôm qua
  const yesterdayDiff = (data.revenue ?? 0) - (data.revenueYesterday ?? 0);
  const yesterdayPercentage = data.percentageChangeDay ?? 0;

  // Tính toán sự khác biệt và phần trăm so với tháng trước
  const lastMonthDiff = (data.revenue ?? 0) - (data.revenueLastMonth ?? 0);
  const lastMonthPercentage = data.percentageChangeMonth ?? 0;
  const t = useTranslations();
  return (
    <div className="grid gap-4 md:grid-cols-3">
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">{t("revenue.today")}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">
            {formatPriceVN(data.revenue ?? 0)}
          </div>
          <p className="text-xs text-muted-foreground">
            {data.numberOrder} {t("common.orders")}
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">
            {t("revenue.comparedTo",{timePeriod: t("dateAndTime.yesterday")})}
          </CardTitle>
          {yesterdayDiff > 0 ? (
            <ArrowUpIcon className="h-4 w-4 text-green-500" />
          ) : yesterdayDiff < 0 ? (
            <ArrowDownIcon className="h-4 w-4 text-red-500" />
          ) : (
            <MinusIcon className="h-4 w-4 text-muted-foreground" />
          )}
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">
            {formatPriceVN(Math.abs(yesterdayDiff))}
          </div>
          <p
            className={`text-xs ${
              yesterdayDiff > 0
                ? "text-green-500"
                : yesterdayDiff < 0
                  ? "text-red-500"
                  : "text-muted-foreground"
            }`}
          >
            {yesterdayDiff > 0 ? "+" : yesterdayDiff < 0 ? "-" : ""}
            {Math.abs(yesterdayPercentage).toFixed(2)}% {t("revenue.from",{timePeriod: t("dateAndTime.yesterday")})}
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">
            {t("revenue.comparedTo",{timePeriod: t("revenue.samePeriodLastMonth")})}
          </CardTitle>
          {lastMonthDiff > 0 ? (
            <ArrowUpIcon className="h-4 w-4 text-green-500" />
          ) : lastMonthDiff < 0 ? (
            <ArrowDownIcon className="h-4 w-4 text-red-500" />
          ) : (
            <MinusIcon className="h-4 w-4 text-muted-foreground" />
          )}
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">
            {formatPriceVN(Math.abs(lastMonthDiff))}
          </div>
          <p
            className={`text-xs ${
              lastMonthDiff > 0
                ? "text-green-500"
                : lastMonthDiff < 0
                  ? "text-red-500"
                  : "text-muted-foreground"
            }`}
          >
            {lastMonthDiff > 0 ? "+" : lastMonthDiff < 0 ? "-" : ""}
            {Math.abs(lastMonthPercentage).toFixed(2)}% {t("revenue.from",{timePeriod: t("revenue.samePeriodLastMonth")})}
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
