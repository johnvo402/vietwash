// Search hook with debouncing
"use client";

import { useState, useMemo } from "react";
import { useCategoriesQuery } from "./queries/use-categories-query";
import { useDebounce } from "@/hooks/use-debounce";

export function useCategorySearch() {
  const [searchTerm, setSearchTerm] = useState("");
  const [filters, setFilters] = useState<Record<string, any>>({});

  // Debounce search term
  const debouncedSearchTerm = useDebounce(searchTerm, 300);

  // Build query parameters
  const queryParams = useMemo((): any => {
    const params: any = {
      PageSize: 1000, // Get all for tree building
    };

    if (debouncedSearchTerm) {
      params["Search.Keyword"] = debouncedSearchTerm;
      params["Search.Targets"] = ["name", "path"];
    }

    // Add other filters
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== "") {
        (params as any)[key] = value;
      }
    });

    return params;
  }, [debouncedSearchTerm, filters]);

  // Use query with search parameters
  const query = useCategoriesQuery(queryParams);

  return {
    searchTerm,
    setSearchTerm,
    filters,
    setFilters,
    debouncedSearchTerm,
    ...query,
  };
}
