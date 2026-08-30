import { DateRange } from "@/features/reports/types/filter.type";
import { startOfDay, endOfDay, format } from "date-fns";

export class DateUtils {
  private static formatDateTime = (date: Date): string =>
    format(date, "yyyy-MM-dd'T'HH:mm");

  private static formatDisplayDate = (date: Date): string =>
    format(date, "dd/MM/yyyy HH:mm");

  static getDateRange(preset: string): DateRange {
    const today = new Date();

    const presetMap: Record<string, () => DateRange> = {
      today: () => ({
        from: this.formatDateTime(startOfDay(today)),
        to: this.formatDateTime(endOfDay(today)),
        time: "today",
      }),
      yesterday: () => {
        const yesterday = new Date(today);
        yesterday.setDate(today.getDate() - 1);
        return {
          from: this.formatDateTime(startOfDay(yesterday)),
          to: this.formatDateTime(endOfDay(yesterday)),
          time: "yesterday",
        };
      },
      last3Days: () => {
        const threeDaysAgo = new Date(today);
        threeDaysAgo.setDate(today.getDate() - 3);
        return {
          from: this.formatDateTime(startOfDay(threeDaysAgo)),
          to: this.formatDateTime(endOfDay(today)),
          time: "last3Days",
        };
      },
      thisWeek: () => {
        const startOfWeek = new Date(today);
        startOfWeek.setDate(today.getDate() - today.getDay());
        return {
          from: this.formatDateTime(startOfDay(startOfWeek)),
          to: this.formatDateTime(endOfDay(today)),
          time: "thisWeek",
        };
      },
      lastWeek: () => {
        const lastWeekStart = new Date(today);
        lastWeekStart.setDate(today.getDate() - today.getDay() - 7);
        const lastWeekEnd = new Date(lastWeekStart);
        lastWeekEnd.setDate(lastWeekStart.getDate() + 6);
        return {
          from: this.formatDateTime(startOfDay(lastWeekStart)),
          to: this.formatDateTime(endOfDay(lastWeekEnd)),
          time: "lastWeek",
        };
      },
      last2Weeks: () => {
        const twoWeeksAgo = new Date(today);
        twoWeeksAgo.setDate(today.getDate() - 14);
        return {
          from: this.formatDateTime(startOfDay(twoWeeksAgo)),
          to: this.formatDateTime(endOfDay(today)),
          time: "last2Weeks",
        };
      },
      thisMonth: () => {
        const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
        return {
          from: this.formatDateTime(startOfDay(startOfMonth)),
          to: this.formatDateTime(endOfDay(today)),
          time: "thisMonth",
        };
      },
      lastMonth: () => {
        const lastMonthStart = new Date(
          today.getFullYear(),
          today.getMonth() - 1,
          1
        );
        const lastMonthEnd = new Date(today.getFullYear(), today.getMonth(), 0);
        return {
          from: this.formatDateTime(startOfDay(lastMonthStart)),
          to: this.formatDateTime(endOfDay(lastMonthEnd)),
          time: "lastMonth",
        };
      },
      last3Months: () => {
        const threeMonthsAgo = new Date(today);
        threeMonthsAgo.setMonth(today.getMonth() - 3);
        return {
          from: this.formatDateTime(startOfDay(threeMonthsAgo)),
          to: this.formatDateTime(endOfDay(today)),
          time: "last3Months",
        };
      },
      last6Months: () => {
        const sixMonthsAgo = new Date(today);
        sixMonthsAgo.setMonth(today.getMonth() - 6);
        return {
          from: this.formatDateTime(startOfDay(sixMonthsAgo)),
          to: this.formatDateTime(endOfDay(today)),
          time: "last6Months",
        };
      },
      thisYear: () => {
        const startOfYear = new Date(today.getFullYear(), 0, 1);
        return {
          from: this.formatDateTime(startOfDay(startOfYear)),
          to: this.formatDateTime(endOfDay(today)),
          time: "thisYear",
        };
      },
      lastYear: () => {
        const lastYearStart = new Date(today.getFullYear() - 1, 0, 1);
        const lastYearEnd = new Date(today.getFullYear() - 1, 11, 31);
        return {
          from: this.formatDateTime(startOfDay(lastYearStart)),
          to: this.formatDateTime(endOfDay(lastYearEnd)),
          time: "lastYear",
        };
      },
    };

    return presetMap[preset]?.() || { from: "", to: "" };
  }

  static getDefaultPreset(maxDays: number): string {
    if (maxDays <= 1) return "today";
    if (maxDays <= 7) return "thisWeek";
    if (maxDays <= 31) return "thisMonth";
    if (maxDays <= 365) return "last3Months";
    return "thisYear";
  }

  static getMinMaxDateTime(maxDays: number): { min: string; max: string } {
    const today = new Date();
    const minDate = new Date(today);
    const maxDate = new Date(today);

    minDate.setDate(today.getDate() - maxDays);
    maxDate.setDate(today.getDate() + maxDays);

    return {
      min: this.formatDateTime(startOfDay(minDate)),
      max: this.formatDateTime(endOfDay(maxDate)),
    };
  }

  static formatForDisplay(dateString: string): string {
    return dateString
      ? this.formatDisplayDate(new Date(dateString))
      : "Chưa chọn";
  }

  static processCustomDateTime(value: string, field: "from" | "to"): string {
    if (!value) return value;

    if (!value.includes("T")) {
      const date = new Date(value);
      return field === "from"
        ? this.formatDateTime(startOfDay(date))
        : this.formatDateTime(endOfDay(date));
    }

    return value;
  }
  static getDaysBetween(start: Date, end: Date): number {
    const diffTime = Math.abs(end.getTime() - start.getTime());
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }
}
