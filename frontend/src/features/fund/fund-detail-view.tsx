"use client";
import { apiClient } from "@/api/client";
import { GetFundDetailResponse } from "@/api/generated";
import { ContentLayout } from "@/components/admin-panel/content-layout";
import FundDetails from "@/features/fund/components/fund-detail";
import { useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";

interface DetailProps {
  params: { publicId: string };
}
export default function FundDetailView({ params }: DetailProps) {
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(params.publicId);
    if (storedId) setId(Number(storedId));
  }, [params.publicId]);

  const {
    data: fund,
    isLoading,
    error,
  } = useQuery<GetFundDetailResponse | undefined>({
    queryKey: ["fund", id],
    queryFn: async () => {
      if (id === null) return undefined;
      const response = await apiClient.financeApiFundsId(id);
      return response.data.results;
    },
    enabled: id !== null,
  });
  return fund && <FundDetails fund={fund!} />;
}
