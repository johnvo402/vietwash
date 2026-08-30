"use client";

import { useEffect, useState } from "react";
import { DateUtils } from "../../utils/date.utils";
import {
  DatePreset,
  DateRange,
  FilterCallbacks,
} from "@/features/reports/types/filter.type";
import { useTranslations } from "next-intl";

interface DateBadgeSelectorProps extends Pick<FilterCallbacks, "onDateChange"> {
  maxDays: number;
  defaultPreset?:
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
    | "lastYear";
  className?: string;
  showSelectedRange?: boolean;
  value?: DateRange;
}

export const DATE_PRESETS: DatePreset[] = [
  { label: "today", value: "today", maxDays: 1 },
  { label: "yesterday", value: "yesterday", maxDays: 2 },
  { label: "last3Days", value: "last3Days", maxDays: 3 },
  { label: "thisWeek", value: "thisWeek", maxDays: 7 },
  { label: "lastWeek", value: "lastWeek", maxDays: 14 },
  { label: "last2Weeks", value: "last2Weeks", maxDays: 14 },
  { label: "thisMonth", value: "thisMonth", maxDays: 31 },
  { label: "lastMonth", value: "lastMonth", maxDays: 62 },
  { label: "last3Months", value: "last3Months", maxDays: 90 },
  { label: "last6Months", value: "last6Months", maxDays: 180 },
  { label: "thisYear", value: "thisYear", maxDays: 365 },
  { label: "lastYear", value: "lastYear", maxDays: 730 },
];

export default function DateBadgeSelector({
  maxDays,
  defaultPreset,
  onDateChange,
  className = "",
  showSelectedRange = true,
  value,
}: DateBadgeSelectorProps) {
  const t = useTranslations();
  const availablePresets = DATE_PRESETS.filter(
    (preset) => preset.maxDays <= maxDays
  );
  const initialPreset = defaultPreset || DateUtils.getDefaultPreset(maxDays);

  // Initialize with value if provided, otherwise use default
  const initialDateRange = value || DateUtils.getDateRange(initialPreset);
  const [dateRange, setDateRange] = useState<DateRange>(initialDateRange);
  const [selectedPreset, setSelectedPreset] = useState<string>(
    value?.time || initialPreset
  );

  // Sync with value prop changes
  useEffect(() => {
    if (value) {
      console.log("DateBadgeSelector syncing with value:", value); // Debug log
      setDateRange(value);
      setSelectedPreset(value.time || "custom");
    }
  }, [value]);

  const handlePresetChange = (preset: string) => {
    setSelectedPreset(preset);

    if (preset !== "custom") {
      const newRange = DateUtils.getDateRange(preset);
      setDateRange(newRange);
      onDateChange?.(newRange, preset);
    } else {
      // Use current value or default if custom
      const newRange = value || DateUtils.getDateRange("thisMonth");
      setDateRange(newRange);
      onDateChange?.(newRange, "custom");
    }
  };

  const handleCustomDateChange = (field: "from" | "to", value: string) => {
    const processedValue = DateUtils.processCustomDateTime(value, field);
    const newRange: DateRange = {
      ...dateRange,
      [field]: processedValue,
      time: "custom",
    };

    setDateRange(newRange);
    setSelectedPreset("custom");
    onDateChange?.(newRange, "custom");
  };

  const { min: minDateTime, max: maxDateTime } =
    DateUtils.getMinMaxDateTime(maxDays);

  return (
    <div className={`space-y-4 ${className}`}>
      {showSelectedRange && (
        <div className="flex flex-wrap gap-2">
          {availablePresets.map((preset) => (
            <button
              key={preset.value}
              type="button"
              onClick={() => handlePresetChange(preset.value)}
              className={`px-3 py-1 text-xs rounded-full border transition-all duration-200 ${
                selectedPreset === preset.value
                  ? "bg-primary text-background border-primary shadow-md"
                  : "bg-background hover:shadow-sm"
              }`}
            >
              {t(`dateAndTime.${preset.label}`)}
            </button>
          ))}
        </div>
      )}

      {/* Custom DateTime Inputs */}
      <div className="space-y-3">
        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">
            {t("dateAndTime.fromDateAndTime")}
          </label>
          <input
            type="datetime-local"
            value={dateRange.from.slice(0, 16)} // Trim to 'YYYY-MM-DDTHH:MM' format
            max={maxDateTime}
            onChange={(e) => handleCustomDateChange("from", e.target.value)}
            className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary"
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">
            {t("dateAndTime.toDateAndTime")}
          </label>
          <input
            type="datetime-local"
            value={dateRange.to.slice(0, 16)} // Trim to 'YYYY-MM-DDTHH:MM' format
            min={dateRange.from || minDateTime}
            max={maxDateTime}
            onChange={(e) => handleCustomDateChange("to", e.target.value)}
            className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary"
          />
        </div>
      </div>
    </div>
  );
}
