// filter-service.ts
import { FilterState } from "../types/filter.type";
import { DateUtils } from "@/utils/date.utils";

export class FilterService {
  static validateFilters(
    filters: FilterState,
    t: any
  ): {
    isValid: boolean;
    dateError?: string;
    branchError?: string;
  } {
    const errors: {
      isValid: boolean;
      dateError?: string;
      branchError?: string;
    } = {
      isValid: true,
    };

    // Validate date range
    if (!filters.from || !filters.to) {
      errors.isValid = false;
      errors.dateError = t("errors.dateRequired");
    } else {
      const fromDate = new Date(filters.from);
      const toDate = new Date(filters.to);
      if (isNaN(fromDate.getTime()) || isNaN(toDate.getTime())) {
        errors.isValid = false;
        errors.dateError = t("errors.invalidDate");
      } else if (fromDate > toDate) {
        errors.isValid = false;
        errors.dateError = t("errors.dateOrder");
      }
    }

    // Validate branch selection
    if (!filters.branchIds || filters.branchIds.length === 0) {
      errors.isValid = false;
      errors.branchError = t("errors.branchRequired");
    }

    return errors;
  }

  static async applyFilters(filters: FilterState): Promise<void> {
    // Simulate API call or filter application logic
    return Promise.resolve();
  }

  static hasChanges(
    filters: FilterState,
    appliedFilters: FilterState
  ): boolean {
    return (
      filters.from !== appliedFilters.from ||
      filters.to !== appliedFilters.to ||
      JSON.stringify(filters.branchIds) !==
        JSON.stringify(appliedFilters.branchIds)
    );
  }
}
