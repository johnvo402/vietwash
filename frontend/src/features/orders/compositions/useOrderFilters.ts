/* eslint-disable react-hooks/exhaustive-deps */
"use client";

import { useState, useEffect } from "react";

import { useTranslations } from "next-intl";
import { OrderStatus } from "@/api/generated";
import { DateRange } from "@/features/reports/types/filter.type";
import { Option } from "@/components/core/selects/multi-select";
import { DateUtils } from "@/utils/date.utils";
import { useQueryState } from "nuqs";

export function useOrderFilters() {
  const t = useTranslations();
  const [search, setSearch] = useQueryState("search", { defaultValue: "" });
  const [statusFilter, setStatusFilter] = useState<Option[]>([
    { value: OrderStatus.Pending, label: t("common.status.pending") },
    { value: OrderStatus.InProgress, label: t("common.status.handling") },
    { value: OrderStatus.Processed, label: t("common.status.handled") },
    { value: OrderStatus.Completed, label: t("common.status.completed") },
  ]);
  const [customerGroupFilter, setCustomerGroupFilter] = useState<string>("all");
  const [page, setPage] = useQueryState("page", { defaultValue: "1" });
  const [pageSize, setPageSize] = useQueryState("pageSize", {
    defaultValue: "10",
  });
  const [viewMode, setViewMode] = useQueryState("viewMode", {
    defaultValue: "list",
  });
  const [dateRange, setDateRange] = useState<DateRange>(() =>
    DateUtils.getDateRange("thisMonth")
  );

  // Adjust pageSize based on viewMode
  useEffect(() => {
    setPageSize(viewMode === "card" ? "9" : "10");
  }, [viewMode]);

  return {
    search,
    setSearch,
    statusFilter,
    setStatusFilter,
    customerGroupFilter,
    setCustomerGroupFilter,

    dateRange,
    setDateRange,
    page,
    setPage,
    pageSize,
    setPageSize,
    viewMode,
    setViewMode,
  };
}
