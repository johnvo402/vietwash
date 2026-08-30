/* eslint-disable react-hooks/exhaustive-deps */
"use client";

import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { Loader2 } from "lucide-react";
import { DateRange } from "react-day-picker";
import { useAuth } from "@/hooks/use-auth";
import { RevenueCards } from "./components/revenue-cards";
import dynamic from "next/dynamic";
import { endOfDay, startOfMonth } from "date-fns";
import { StatisticsHeader } from "./components/statistic-header";
import { DateFilters } from "./components/date-filter";
import { useTranslations } from "next-intl";
import { useNotification } from "../notification/hooks/use-notification";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";

// Dynamically import chart components
const MonthlyRevenueChart = dynamic(
  () =>
    import("./components/monthly-revenue-chart").then(
      (mod) => mod.MonthlyRevenueChart
    ),
  {
    ssr: false,
    loading: () => (
      <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
    ),
  }
);
const TopServicesChart = dynamic(
  () =>
    import("./components/top-services-chart").then(
      (mod) => mod.TopServicesChart
    ),
  {
    ssr: false,
    loading: () => (
      <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
    ),
  }
);
const PieChart = dynamic(
  () => import("./components/pie-chart").then((mod) => mod.PieChart),
  {
    ssr: false,
    loading: () => (
      <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
    ),
  }
);
const RecentActivitiesCard = dynamic(
  () =>
    import("./components/recent-activities").then(
      (mod) => mod.RecentActivitiesCard
    ),
  {
    ssr: false,
    loading: () => (
      <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
    ),
  }
);

export default function StatisticsPage() {
  const { branchActive } = useAuth();
  const {
    notification,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
    refetchCountNoti,
    isFetchingNextPage,
  } = useNotification({ sort: "createdAt:desc" }, true);
  const [selectedBranch, setSelectedBranch] = useState<string>(
    String(branchActive?.branchId) || ""
  );

  // Individual date range states
  const [revenueDateRange, setRevenueDateRange] = useState<{
    from: string;
    to: string;
    range: DateRange;
    preset: string;
  }>(() => {
    const now = new Date();
    return {
      from: startOfMonth(now).toISOString().split(".")[0] + "Z",
      to: endOfDay(now).toISOString().split(".")[0] + "Z",
      range: { from: startOfMonth(now), to: endOfDay(now) },
      preset: "thisMonth",
    };
  });

  const [topServicesDateRange, setTopServicesDateRange] = useState<{
    from: string;
    to: string;
    range: DateRange;
    preset: string;
  }>(() => {
    const now = new Date();
    return {
      from: startOfMonth(now).toISOString().split(".")[0] + "Z",
      to: endOfDay(now).toISOString().split(".")[0] + "Z",
      range: { from: startOfMonth(now), to: endOfDay(now) },
      preset: "thisMonth",
    };
  });

  const [pieChartDateRange, setPieChartDateRange] = useState<{
    from: string;
    to: string;
    range: DateRange;
    preset: string;
  }>(() => {
    const now = new Date();
    return {
      from: startOfMonth(now).toISOString().split(".")[0] + "Z",
      to: endOfDay(now).toISOString().split(".")[0] + "Z",
      range: { from: startOfMonth(now), to: endOfDay(now) },
      preset: "thisMonth",
    };
  });

  // Individual queries for each API
  const revenueQuery = useQuery({
    queryKey: [
      "revenue",
      selectedBranch,
      revenueDateRange.from,
      revenueDateRange.to,
    ],
    queryFn: () =>
      apiClient.ecommerceApiRevenueStatisticGet(
        selectedBranch,
        revenueDateRange.from,
        revenueDateRange.to
      ),
    enabled: !!selectedBranch,
    staleTime: 30000,
    gcTime: 60000,
    refetchOnWindowFocus: false,
  });

  const salesQuery = useQuery({
    queryKey: ["sales", selectedBranch],
    queryFn: () =>
      apiClient.ecommerceApiDashboardCardGet(Number(selectedBranch)),
    enabled: !!selectedBranch,
    staleTime: 30000,
    gcTime: 60000,
    refetchOnWindowFocus: false,
  });

  const pieChartQuery = useQuery({
    queryKey: [
      "branchRevenue",
      selectedBranch,
      pieChartDateRange.from,
      pieChartDateRange.to,
    ],
    queryFn: () =>
      apiClient.ecommerceApiNetRevenueBranchGet(
        pieChartDateRange.from,
        pieChartDateRange.to
      ),
    enabled: !!selectedBranch,
    staleTime: 30000,
    gcTime: 60000,
    refetchOnWindowFocus: false,
  });

  const topServicesQuery = useQuery({
    queryKey: [
      "topServices",
      selectedBranch,
      topServicesDateRange.from,
      topServicesDateRange.to,
    ],
    queryFn: () => {
      console.log("Fetching topServices API", {
        selectedBranch,
        from: topServicesDateRange.from,
        to: topServicesDateRange.to,
      });
      return apiClient.ecommerceApiTopServiceGet(
        topServicesDateRange.from,
        topServicesDateRange.to,
        selectedBranch
      );
    },
    enabled: !!selectedBranch,
    staleTime: 30000,
    gcTime: 60000,
    refetchOnWindowFocus: false,
  });

  // Individual date range handlers
  const handleRevenueDateRange = (
    newRange: Partial<{
      range: DateRange;
      preset: string;
    }>
  ) => {
    const normalizeISO = (date: Date) =>
      new Date(
        Date.UTC(
          date.getFullYear(),
          date.getMonth(),
          date.getDate(),
          date.getHours(),
          date.getMinutes(),
          date.getSeconds()
        )
      )
        .toISOString()
        .split(".")[0] + "Z";

    setRevenueDateRange((prev) => {
      const merged = {
        ...prev,
        ...newRange,
        from: newRange.range?.from
          ? normalizeISO(newRange.range.from)
          : prev.from,
        to: newRange.range?.to ? normalizeISO(newRange.range.to) : prev.to,
      };

      if (
        prev.from !== merged.from ||
        prev.to !== merged.to ||
        prev.preset !== merged.preset ||
        prev.range?.from?.getTime() !== merged.range?.from?.getTime() ||
        prev.range?.to?.getTime() !== merged.range?.to?.getTime()
      ) {
        console.log("Updating revenue dateRange", merged);
        revenueQuery.refetch();
      }

      return merged;
    });
  };

  const handleTopServicesDateRange = (
    newRange: Partial<{
      range: DateRange;
      preset: string;
    }>
  ) => {
    const normalizeISO = (date: Date) =>
      new Date(
        Date.UTC(
          date.getFullYear(),
          date.getMonth(),
          date.getDate(),
          date.getHours(),
          date.getMinutes(),
          date.getSeconds()
        )
      )
        .toISOString()
        .split(".")[0] + "Z";

    setTopServicesDateRange((prev) => {
      const merged = {
        ...prev,
        ...newRange,
        from: newRange.range?.from
          ? normalizeISO(newRange.range.from)
          : prev.from,
        to: newRange.range?.to ? normalizeISO(newRange.range.to) : prev.to,
      };

      if (
        prev.from !== merged.from ||
        prev.to !== merged.to ||
        prev.preset !== merged.preset ||
        prev.range?.from?.getTime() !== merged.range?.from?.getTime() ||
        prev.range?.to?.getTime() !== merged.range?.to?.getTime()
      ) {
        console.log("Updating topServices dateRange", merged);
        topServicesQuery.refetch();
      }

      return merged;
    });
  };

  const handlePieChartDateRange = (
    newRange: Partial<{
      range: DateRange;
      preset: string;
    }>
  ) => {
    const normalizeISO = (date: Date) =>
      new Date(
        Date.UTC(
          date.getFullYear(),
          date.getMonth(),
          date.getDate(),
          date.getHours(),
          date.getMinutes(),
          date.getSeconds()
        )
      )
        .toISOString()
        .split(".")[0] + "Z";

    setPieChartDateRange((prev) => {
      const merged = {
        ...prev,
        ...newRange,
        from: newRange.range?.from
          ? normalizeISO(newRange.range.from)
          : prev.from,
        to: newRange.range?.to ? normalizeISO(newRange.range.to) : prev.to,
      };

      if (
        prev.from !== merged.from ||
        prev.to !== merged.to ||
        prev.preset !== merged.preset ||
        prev.range?.from?.getTime() !== merged.range?.from?.getTime() ||
        prev.range?.to?.getTime() !== merged.range?.to?.getTime()
      ) {
        console.log("Updating pieChart dateRange", merged);
        pieChartQuery.refetch();
      }

      return merged;
    });
  };

  // Refetch when selectedBranch changes
  useEffect(() => {
    if (!selectedBranch) return;

    revenueQuery.refetch();
    salesQuery.refetch();
    pieChartQuery.refetch();
    topServicesQuery.refetch();
  }, [selectedBranch]);

  const t = useTranslations();

  return (
    <div className="w-full py-8">
      <div className="grid grid-cols-1 md:grid-cols-6 gap-6">
        <div className="md:col-span-4 space-y-8">
          <StatisticsHeader
            selectedBranch={selectedBranch}
            setSelectedBranch={setSelectedBranch}
          />
          <Card>
            <CardHeader>
              <CardTitle>{t("dashboard.saleOverview")}</CardTitle>
            </CardHeader>
            <CardContent>
              <RevenueCards data={salesQuery.data?.data?.results ?? {}} />
            </CardContent>
          </Card>
          <div className="grid grid-cols-1 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>{t("revenue.month")}</CardTitle>
              </CardHeader>
              <CardContent>
                <DateFilters
                  dateRange={revenueDateRange.range}
                  datePreset={revenueDateRange.preset}
                  setDateRange={(range) => handleRevenueDateRange({ range })}
                  setDatePreset={(preset) => handleRevenueDateRange({ preset })}
                />
                {revenueQuery.isLoading ? (
                  <div className="flex items-center justify-center h-[300px]">
                    <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                  </div>
                ) : (
                  <MonthlyRevenueChart
                    data={revenueQuery.data?.data?.results ?? []}
                  />
                )}
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>
                  {t("dashboard.branchRevenueDistribution")}
                </CardTitle>
              </CardHeader>
              <CardContent>
                <DateFilters
                  dateRange={pieChartDateRange.range}
                  datePreset={pieChartDateRange.preset}
                  setDateRange={(range) => handlePieChartDateRange({ range })}
                  setDatePreset={(preset) =>
                    handlePieChartDateRange({ preset })
                  }
                  showDateRange={false}
                />
                {pieChartQuery.isLoading ? (
                  <div className="flex items-center justify-center h-[300px]">
                    <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                  </div>
                ) : (
                  <PieChart data={pieChartQuery.data?.data?.results ?? []} />
                )}
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>{t("dashboard.topService")}</CardTitle>
              </CardHeader>
              <CardContent>
                <DateFilters
                  dateRange={topServicesDateRange.range}
                  datePreset={topServicesDateRange.preset}
                  setDateRange={(range) =>
                    handleTopServicesDateRange({ range })
                  }
                  setDatePreset={(preset) =>
                    handleTopServicesDateRange({ preset })
                  }
                  showDateRange={false}
                />
                {topServicesQuery.isLoading ? (
                  <div className="flex items-center justify-center h-[300px]">
                    <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                  </div>
                ) : (
                  <TopServicesChart
                    data={topServicesQuery.data?.data?.results ?? []}
                  />
                )}
              </CardContent>
            </Card>
          </div>
        </div>
        <div className="md:col-span-2 h-full">
          <RecentActivitiesCard
            notifications={notification}
            isLoading={isLoading}
            error={error}
            fetchNextPage={fetchNextPage}
            hasNextPage={hasNextPage}
            isFetchingNextPage={isFetchingNextPage}
            refetch={refetchCountNoti}
          />
        </div>
      </div>
    </div>
  );
}
