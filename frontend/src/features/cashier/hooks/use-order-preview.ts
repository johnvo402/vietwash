import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import type { PreviewOrderQuery } from "@/api/generated";

export function useOrderPreview(input: PreviewOrderQuery | null) {
  const key = input ? JSON.stringify(input) : "";
  const [debouncedKey, setDebouncedKey] = useState("");
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedKey(key), 300);
    return () => clearTimeout(timer);
  }, [key]);
  const query = useQuery({
    queryKey: ["order-preview", debouncedKey],
    enabled: !!key && key === debouncedKey,
    retry: false,
    staleTime: 0,
    queryFn: async ({ signal }) => {
      const response = await apiClient.ecommerceApiOrdersPreviewPost(
        JSON.parse(debouncedKey) as PreviewOrderQuery,
        { signal },
      );
      if (!response.data.results) throw new Error("Missing pricing preview");
      return response.data.results;
    },
  });
  const calculating =
    !!key && (key !== debouncedKey || query.isFetching || query.isPending);
  return {
    preview:
      key && key === debouncedKey && !calculating && !query.isError
        ? query.data
        : undefined,
    calculating,
    error: key && key === debouncedKey ? query.error : null,
    errorMessage: (
      query.error as { response?: { data?: { title?: string } } } | null
    )?.response?.data?.title,
    retry: () => query.refetch(),
  };
}
