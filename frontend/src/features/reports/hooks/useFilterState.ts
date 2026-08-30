"use client";

import { useState, useMemo } from "react";
import { useAuth } from "@/hooks/use-auth";
import { DateUtils } from "@/utils/date.utils";
import { FilterState } from "../types/filter.type";
import { useSearchParams } from "next/navigation";

interface FilterErrors {
  dateError?: string;
  branchError?: string;
}

export const useFilterState = () => {
  const { branchActive } = useAuth();
  const searchParams = useSearchParams();

  const getDefaultFilters = useMemo((): FilterState => {
    const from = searchParams.get("from");
    const to = searchParams.get("to");
    const branchIds = searchParams.get("branchIds");
    const time = searchParams.get("time");

    let initialFilters: FilterState = {
      from: DateUtils.getDateRange("thisMonth").from,
      to: DateUtils.getDateRange("thisMonth").to,
      branchIds: branchActive
        ? [
            {
              branchId: branchActive.branchId,
              branchName: branchActive.branchName || "",
            },
          ]
        : [],
      time: "thisMonth",
    };

    if (from && to) {
      try {
        const fromDate = new Date(Number(from) * 1000);
        const toDate = new Date(Number(to) * 1000);
        if (!isNaN(fromDate.getTime()) && !isNaN(toDate.getTime())) {
          initialFilters = {
            ...initialFilters,
            from: fromDate.toISOString(),
            to: toDate.toISOString(),
            time: (time as any) || "custom",
          };
        }
      } catch (error) {
        console.error("Error parsing dates from query:", error);
      }
    }

    if (branchIds) {
      try {
        const parsedBranchIds = JSON.parse(branchIds);
        if (Array.isArray(parsedBranchIds)) {
          initialFilters.branchIds = parsedBranchIds.map((id: string) => ({
            branchId: Number(id),
            branchName: "", // Will be populated in FilterPanel
          }));
        }
      } catch (error) {
        console.error("Error parsing branchIds from query:", error);
      }
    }

    console.log("Initial filters from useFilterState:", initialFilters);
    return initialFilters;
  }, [searchParams, branchActive]);

  const [filters, setFilters] = useState<FilterState>(getDefaultFilters);
  const [appliedFilters, setAppliedFilters] =
    useState<FilterState>(getDefaultFilters);
  const [isLoading, setIsLoading] = useState(false);
  const [errors, setErrors] = useState<FilterErrors>({});

  const updateFilters = (newFilters: Partial<FilterState>) => {
    console.log("updateFilters called with:", newFilters);
    setFilters((prev) => ({ ...prev, ...newFilters }));
    setErrors({});
  };

  const resetFilters = () => {
    console.log("resetFilters called");
    const defaultFilters: FilterState = {
      from: DateUtils.getDateRange("thisMonth").from,
      to: DateUtils.getDateRange("thisMonth").to,
      branchIds: branchActive
        ? [
            {
              branchId: branchActive.branchId,
              branchName: branchActive.branchName || "",
            },
          ]
        : [],
      time: "thisMonth" as FilterState["time"],
    };
    setFilters(defaultFilters);
    setAppliedFilters(defaultFilters);
    setErrors({});
    return defaultFilters;
  };

  const applyFilters = (filtersToApply: FilterState) => {
    console.log("applyFilters called with:", filtersToApply);
    setAppliedFilters(filtersToApply);
    setErrors({});
  };

  return {
    filters,
    appliedFilters,
    isLoading,
    errors,
    setIsLoading,
    setErrors,
    updateFilters,
    resetFilters,
    applyFilters,
  };
};
