"use client";

import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { useTranslations } from "next-intl";
import { Options } from "nuqs";
import { useEffect, useState, useTransition } from "react";

interface DataTableSearchProps {
  searchQuery: string;
  placeholder?: string;
  setSearchQuery: (
    value: string | ((old: string) => string | null) | null,
    options?: Options | undefined
  ) => Promise<URLSearchParams>;
  setPage: <Shallow>(
    value: number | ((old: number) => number | null) | null,
    options?: Options | undefined
  ) => Promise<URLSearchParams>;
}

/*************  ✨ Command ⭐  *************/
/**
 * A search input for a DataTable.
 *
 * @remarks
 *
 * This component allows users to search the table by keyword. It uses the
 * `useTransition` hook from `react` to prevent the component from re-rendering
 * while the search is in progress.
 *
 * The component expects the following props:
 *
 * - `searchKey`: A string that describes what the user is searching for.
 * - `searchQuery`: The current search query.
 * - `setSearchQuery`: A function that sets the search query.
 * - `setPage`: A function that sets the page number.
 *
 * The component returns an `Input` component with a placeholder that includes
 * the `searchKey`. The component is wrapped in a `div` with a class of
 * `"data-table-search"`.
 ***/

export function DataTableSearch({
  placeholder,
  searchQuery,
  setSearchQuery,
  setPage,
}: DataTableSearchProps) {
  const t = useTranslations();
  const [isLoading, startTransition] = useTransition();
  const [debouncedQuery, setDebouncedQuery] = useState(searchQuery);

  useEffect(() => {
    const handler = setTimeout(() => {
      if (debouncedQuery !== searchQuery) {
        startTransition(() => {
          setSearchQuery(debouncedQuery, { startTransition });
        });
        setPage(1); // Chỉ set lại page nếu query thay đổi
      }
    }, 300);

    return () => clearTimeout(handler);
  }, [debouncedQuery, searchQuery, setSearchQuery, setPage]);

  return (
    <Input
      placeholder={`${placeholder ? placeholder : t(`search.title`)}`}
      value={debouncedQuery ?? ""}
      onChange={(e) => setDebouncedQuery(e.target.value)}
      className={cn("w-full md:max-w-sm", isLoading && "animate-pulse")}
    />
  );
}
