import { useQuery, UseQueryResult } from "@tanstack/react-query";
import { apiClient } from "@/api/client"; // bạn cần điều chỉnh đường dẫn theo dự án của bạn
import {
  getPricesFromIndexedDB,
  PriceItem,
  savePricesToIndexedDB,
} from "@/utils/tariff-db";
import { useEffect, useState } from "react";

const fetchPrices = async (
  branchId: number
): Promise<{ prices: PriceItem[] }> => {
  const response =
    await apiClient.ecommerceApiTariffsTariffByBranchGet(branchId);
  const apiData = response.data.results || [];

  const prices: PriceItem[] = apiData.map((item) => ({
    id: item.id!,
    name: item.name!,
  }));

  await savePricesToIndexedDB(prices);
  return { prices };
};

export const usePrices = (
  branchId: number
): UseQueryResult<{ prices: PriceItem[] }, Error> => {
  return useQuery({
    queryKey: ["prices"],
    queryFn: () => fetchPrices(branchId),
    initialData: { prices: [] },
  });
};
export const usePricesFromIndexedDB = () => {
  const [prices, setPrices] = useState<PriceItem[]>([]);

  useEffect(() => {
    getPricesFromIndexedDB().then(setPrices).catch(console.error);
  }, []);

  return prices;
};
