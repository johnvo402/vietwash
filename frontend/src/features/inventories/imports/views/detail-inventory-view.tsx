"use client";

import { InventoryDocumentDetailResponse } from "@/api/generated/api";
import { useEffect, useState } from "react";
import { apiClient } from "@/api/client";
import { useQuery } from "@tanstack/react-query";
import { SupplyDetailPage } from "../components/inventory-document-detail";
interface DetailProps {
  params: { publicId: string };
}
export default function DetailInventoryDocumentPage({ params }: DetailProps) {
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(params.publicId);
    if (storedId) setId(Number(storedId));
  }, [params.publicId]);

  const { data } = useQuery<InventoryDocumentDetailResponse | undefined>({
    queryKey: ["inventory-document", id],
    queryFn: async () => {
      if (id === null) return undefined;
      const response = await apiClient.ecommerceApiInventoriesIdGet(id);
      return response.data.results;
    },
    enabled: id !== null,
  });

  return (
    <div className="container mx-auto p-4">
      {data && <SupplyDetailPage supply={data} />}
    </div>
  );
}
