"use client";

import { useAuth } from "@/hooks/use-auth";
import { FilterService } from "./filter-service";
import { DateRange, FilterState } from "../types/filter.type";
import MultiSelect, { Option } from "@/components/core/selects/multi-select";
import FilterActions from "./filter-action";
import DateBadgeSelector from "@/components/core/date-badge-selector";
import { useRouter, useSearchParams } from "next/navigation";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useFilterState } from "../hooks/useFilterState";
import { useTranslations } from "next-intl";
import { DATE_PRESETS } from "@/components/core/date-badge-selector";
import { DateUtils } from "@/utils/date.utils";

interface FilterPanelProps {
  maxDays?: number;
  title?: string;
}

export default function FilterPanel({ maxDays, title = "" }: FilterPanelProps) {
  const {
    filters,
    appliedFilters,
    isLoading,
    errors,
    setIsLoading,
    setErrors,
    updateFilters,
    resetFilters,
    applyFilters,
  } = useFilterState();
  const t = useTranslations();
  const { user } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();
  const [hasInitialized, setHasInitialized] = useState(false);

  const getDefaultThisMonthRange = (): DateRange => {
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    const lastDay = new Date(
      now.getFullYear(),
      now.getMonth() + 1,
      0,
      23,
      59,
      59
    );

    return {
      from: firstDay.toISOString(),
      to: lastDay.toISOString(),
      time: "thisMonth",
    };
  };

  const createQueryString = useCallback((params: Record<string, any>) => {
    const query = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        if (Array.isArray(value)) {
          query.set(key, JSON.stringify(value));
        } else {
          query.set(key, String(value));
        }
      }
    });
    return query.toString();
  }, []);

  const handleSubmit = useCallback(async () => {
    console.log("handleSubmit called with filters:", filters);
    const validation = FilterService.validateFilters(filters, t);

    if (!validation.isValid) {
      setErrors({
        dateError: validation.dateError,
        branchError: validation.branchError,
      });
      return;
    }

    setErrors({});
    setIsLoading(true);

    try {
      await FilterService.applyFilters(filters);
      applyFilters(filters);
      const branchIds = filters.branchIds.map((branch: any) =>
        String(branch.branchId)
      );
      const params = {
        from: new Date(filters.from).getTime() / 1000,
        to: new Date(filters.to).getTime() / 1000,
        branchIds: branchIds,
        time: filters.time,
      };

      const queryString = createQueryString(params);
      if (queryString !== searchParams.toString()) {
        console.log("Pushing new query string:", queryString);
        router.push(`?${queryString}`);
      }
    } catch (error) {
      console.error("Error applying filters:", error);
      setErrors({ dateError: t("search.errorFilter") });
    } finally {
      setIsLoading(false);
    }
  }, [
    filters,
    applyFilters,
    createQueryString,
    router,
    searchParams,
    setIsLoading,
    t,
    setErrors,
  ]);

  const handleReset = () => {
    console.log("handleReset called");
    resetFilters();
    router.push("");
  };

  const handleDateChange = (dateRange: DateRange) => {
    console.log("handleDateChange called with:", dateRange);
    updateFilters({
      from: dateRange.from,
      to: dateRange.to,
      time: dateRange.time,
    });
  };

  const handleBranchChange = (selectedOptions: Option[]) => {
    console.log("handleBranchChange called with:", selectedOptions);
    const updatedBranches = selectedOptions.map((option) => ({
      branchId: Number(option.value),
      branchName: option.label,
    }));
    updateFilters({ branchIds: updatedBranches });
  };

  // Sync filters with query parameters on mount
  useEffect(() => {
    if (hasInitialized || !user?.branchAccounts) {
      console.log("useEffect skipped: already initialized or no user data");
      return;
    }

    console.log(
      "useEffect triggered with searchParams:",
      searchParams.toString()
    );
    const from = searchParams.get("from");
    const to = searchParams.get("to");
    const time = searchParams.get("time") as
      | "today"
      | "yesterday"
      | "last3Days"
      | "thisWeek"
      | "lastWeek"
      | "last2Weeks"
      | "thisMonth"
      | "lastMonth"
      | "last3Months"
      | "last6Months"
      | "thisYear"
      | "lastYear"
      | undefined;
    const branchIds = searchParams.get("branchIds");

    const newFilters: Partial<FilterState> = {};

    // Prioritize time preset if valid
    if (time && DATE_PRESETS.some((preset) => preset.value === time)) {
      const presetRange = DateUtils.getDateRange(time);
      newFilters.from = presetRange.from;
      newFilters.to = presetRange.to;
      newFilters.time = time;
    } else if (from && to) {
      try {
        const fromDate = new Date(Number(from) * 1000);
        const toDate = new Date(Number(to) * 1000);
        if (!isNaN(fromDate.getTime()) && !isNaN(toDate.getTime())) {
          newFilters.from = fromDate.toISOString();
          newFilters.to = toDate.toISOString();
          newFilters.time = "custom";
        }
      } catch (error) {
        console.error("Error parsing dates in useEffect:", error);
      }
    } else {
      // Fallback to default if no valid params
      const defaultRange = getDefaultThisMonthRange();
      newFilters.from = defaultRange.from;
      newFilters.to = defaultRange.to;
      newFilters.time = "thisMonth";
    }

    if (branchIds && user.branchAccounts) {
      try {
        const parsedBranchIds = JSON.parse(branchIds);
        if (Array.isArray(parsedBranchIds)) {
          newFilters.branchIds = parsedBranchIds.map((id: string) => {
            const branch = user.branchAccounts.find(
              (b) => b.branchId === Number(id)
            );
            return {
              branchId: Number(id),
              branchName: branch?.branchName || "",
            };
          });
        }
      } catch (error) {
        console.error("Error parsing branchIds in useEffect:", error);
      }
    }

    if (Object.keys(newFilters).length > 0) {
      console.log("Updating filters from query params:", newFilters);
      updateFilters(newFilters);
      handleSubmit();
    }

    setHasInitialized(true); // Mark as initialized after first run
  }, [
    searchParams,
    user?.branchAccounts,
    updateFilters,
    handleSubmit,
    hasInitialized,
  ]);

  const branchOptions: Option[] = useMemo(
    () =>
      user?.branchAccounts?.map((branch) => ({
        value: String(branch.branchId),
        label: branch.branchName,
      })) ?? [],
    [user?.branchAccounts]
  );

  const selectedBranchOptions: Option[] = useMemo(
    () =>
      filters.branchIds.map((branch: any) => ({
        value: String(branch.branchId),
        label: branch.branchName,
      })),
    [filters.branchIds]
  );

  const hasUnappliedChanges = FilterService.hasChanges(filters, appliedFilters);

  return (
    <>
      {title && (
        <h2 className="text-xl font-semibold mb-6 text-center">{t(title)}</h2>
      )}
      <h3 className="text-md font-semibold mb-6 ">
        {t("table.accessorKey.filter")}
      </h3>
      <div className="flex-1">
        <div>
          <label className="block text-sm font-medium text-secondary-foreground mb-3">
            {t("dateAndTime.pickDate")}
          </label>
          <DateBadgeSelector
            maxDays={maxDays || 31}
            defaultPreset={(filters.time as any) || "thisMonth"}
            onDateChange={handleDateChange}
            showSelectedRange={true}
            value={{ from: filters.from, to: filters.to, time: filters.time }}
          />
          {errors.dateError && (
            <p className="mt-1 text-sm text-red-600">{errors.dateError}</p>
          )}
        </div>
        <div className="mt-4">
          <MultiSelect
            label={t("common.placeholderSelect", {
              entity: t("common.branch"),
            })}
            placeholder={t("common.placeholderSelect", {
              entity: t("common.branch"),
            })}
            options={branchOptions}
            value={selectedBranchOptions}
            onChange={handleBranchChange}
          />
          {errors.branchError && (
            <p className="mt-1 text-sm text-red-600">{errors.branchError}</p>
          )}
        </div>
        <FilterActions
          onSubmit={handleSubmit}
          onReset={handleReset}
          isLoading={isLoading}
          hasChanges={hasUnappliedChanges}
        />
      </div>
    </>
  );
}

export type { FilterPanelProps };
