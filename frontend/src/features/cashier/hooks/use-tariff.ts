import { useQuery, UseQueryResult } from "@tanstack/react-query";
import { apiClient } from "@/api/client"; // bạn cần điều chỉnh đường dẫn theo dự án của bạn
import {
  getPricesFromIndexedDB,
  PriceItem,
  savePricesToIndexedDB,
} from "@/utils/tariff-db";
import { useEffect, useState } from "react";
import { pricesQueryKey } from "./cashier-draft";

const fetchPrices = async (
  branchId: number,
): Promise<{ prices: PriceItem[] }> => {
  const response =
    await apiClient.ecommerceApiTariffsTariffByBranchGet(branchId);
  const apiData = response.data.results || [];

  const prices: PriceItem[] = apiData.map((item) => ({
    id: item.id!,
    name: item.name!,
  }));

  await savePricesToIndexedDB(prices, branchId);
  return { prices };
};

export const usePrices = (
  branchId: number,
): UseQueryResult<{ prices: PriceItem[] }, Error> => {
  return useQuery({
    queryKey: pricesQueryKey(branchId),
    queryFn: () => fetchPrices(branchId),
    enabled: branchId > 0,
  });
};
export const usePricesFromIndexedDB = (branchId: number) => {
  const [prices, setPrices] = useState<PriceItem[]>([]);

  useEffect(() => {
    let cancelled = false;
    setPrices([]);
    getPricesFromIndexedDB(branchId)
      .then((rows) => {
        if (!cancelled) setPrices(rows);
      })
      .catch(console.error);
    return () => {
      cancelled = true;
    };
  }, [branchId]);

  return prices;
};
