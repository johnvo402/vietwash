import { BranchAccount } from "@/types/user";

export interface FilterState {
  from: string;
  to: string;
  branchIds: BranchAccount[];
  time:
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
    | "custom";
}

export interface DateRange {
  from: string;
  to: string;
  time:
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
    | "custom";
}

export interface DatePreset {
  label: string;
  value: string;
  maxDays: number;
}

export interface FilterCallbacks {
  onApplyFilter?: (filters: FilterState) => void;
  onResetFilter?: (filters: FilterState) => void;
  onDateChange?: (dateRange: DateRange, preset: string) => void;
}
