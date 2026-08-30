/* eslint-disable react-hooks/exhaustive-deps */
"use client";

import { useCallback, useState } from "react";
import { DateRange } from "react-day-picker";
import { DateRangePicker } from "@/components/date-range-picker";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  endOfDay,
  endOfMonth,
  endOfWeek,
  startOfDay,
  startOfMonth,
  startOfWeek,
  subDays,
  subMonths,
} from "date-fns";
import { useTranslations } from "next-intl";
import { debounce } from "lodash";

interface DateFiltersProps {
  dateRange: DateRange;
  datePreset: string;
  setDateRange: (range: DateRange) => void;
  setDatePreset: (preset: string) => void;
  setDate: (date: { from: string; to: string }) => void;
  showDateRange?: boolean;
}

export function DateFilters({
  dateRange,
  datePreset,
  setDateRange,
  setDatePreset,
  showDateRange = true,
}: Omit<DateFiltersProps, "setDate">) {
  const debouncedSetDateRange = useCallback(
    debounce((range: DateRange) => {
      setDateRange(range);
    }, 300),
    [setDateRange]
  );

  const debouncedSetDatePreset = useCallback(
    debounce((preset: string) => {
      setDatePreset(preset);
    }, 300),
    [setDatePreset]
  );

  const handlePresetChange = (value: string) => {
    debouncedSetDatePreset(value);
    const now = new Date();
    let fromDate: Date;
    let toDate: Date;

    switch (value) {
      case "today":
        fromDate = startOfDay(now);
        toDate = endOfDay(now);
        break;
      case "yesterday":
        const yesterday = subDays(now, 1);
        fromDate = startOfDay(yesterday);
        toDate = endOfDay(yesterday);
        break;
      case "thisWeek":
        fromDate = startOfWeek(now, { weekStartsOn: 0 });
        toDate = endOfDay(now);
        break;
      case "lastWeek":
        const lastWeekStart = startOfWeek(subDays(now, 7), { weekStartsOn: 0 });
        const lastWeekEnd = endOfWeek(subDays(now, 7), { weekStartsOn: 0 });
        fromDate = lastWeekStart;
        toDate = lastWeekEnd;
        break;
      case "thisMonth":
        fromDate = startOfMonth(now);
        toDate = endOfDay(now);
        break;
      case "lastMonth":
        const lastMonth = subMonths(now, 1);
        fromDate = startOfMonth(lastMonth);
        toDate = endOfMonth(lastMonth);
        break;
      default:
        fromDate = startOfDay(now);
        toDate = endOfDay(now);
    }

    debouncedSetDateRange({ from: fromDate, to: toDate });
  };

  const handleDateRangeChange = (range: DateRange | undefined) => {
    if (!range || !range.from || !range.to) return;

    const fromDate = startOfDay(range.from);
    const toDate = endOfDay(range.to);

    debouncedSetDateRange({ from: fromDate, to: toDate });
    debouncedSetDatePreset("");
  };

  const t = useTranslations();
  return (
    <div className="flex flex-col 2xl:flex-row gap-4 mb-4">
      {showDateRange && (
        <DateRangePicker date={dateRange} setDate={handleDateRangeChange} />
      )}

      <Select value={datePreset} onValueChange={handlePresetChange}>
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder="Select date range" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="today">{t("dateAndTime.today")}</SelectItem>
          <SelectItem value="yesterday">
            {t("dateAndTime.yesterday")}
          </SelectItem>
          <SelectItem value="thisWeek">{t("dateAndTime.thisWeek")}</SelectItem>
          <SelectItem value="lastWeek">{t("dateAndTime.lastWeek")}</SelectItem>
          <SelectItem value="thisMonth">
            {t("dateAndTime.thisMonth")}
          </SelectItem>
          <SelectItem value="lastMonth">
            {t("dateAndTime.lastMonth")}
          </SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
}
