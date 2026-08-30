import { apiClient } from "@/api/client";
import { useQueryState } from "nuqs";
import { useCallback, useState } from "react";

export function useInvoiceSearch() {
  const [invoiceCode, setInvoiceCode] = useQueryState("code", {
    defaultValue: "",
    shallow: true,
  });
  const [searchResult, setSearchResult] = useState<string | null>(null);

  const handleSearch = useCallback(async () => {
    if (invoiceCode.trim()) {
      // Simulate invoice URL generation (replace with actual API call in production)
      const response =
        await apiClient.financeApiEInvoiceGetByCodeCodeGet(invoiceCode);
      const invoiceUrl = response.data.results?.url ?? null;
      setSearchResult(invoiceUrl);
    }
  }, [invoiceCode]);

  return { invoiceCode, setInvoiceCode, searchResult, handleSearch };
}
