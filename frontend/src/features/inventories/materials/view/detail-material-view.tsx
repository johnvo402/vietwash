"use client";

import { useEffect, useState } from "react";
import { apiClient } from "@/api/client";
import { useQuery } from "@tanstack/react-query";
import { Card, CardContent } from "@/components/ui/card";
import { BranchProductInformation } from "../components/product-detail";
import { BranchProductDetailTab } from "../components/material-detail-tab";
import BranchProductDetailTabContent from "../components/material-tab-content";
interface DetailProps {
  params: { publicId: string };
}
export default function DetailBranchProductPage({ params }: DetailProps) {
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(params.publicId);
    if (storedId) setId(Number(storedId));
  }, [params.publicId]);

  const { data, isLoading, refetch } = useQuery({
    queryKey: ["branchProduct", id],
    queryFn: async () => {
      if (id === null) return undefined;
      const response = await apiClient.ecommerceApiBranchProductsIdGet(id);
      return response.data.results;
    },
    enabled: id !== null,
  });
  return (
    <div className="mx-6 h-full py-6">
      <div className="grid grid-cols-12 gap-6 h-full">
        {/* Left side - Product information (4 columns) */}
        <div className="h-full col-span-12 md:col-span-4">
          <BranchProductInformation
            refetch={refetch}
            branchProduct={data}
            isLoading={isLoading}
          />
        </div>
        <div className="col-span-12 md:col-span-8">
          <Card className="h-full">
            <CardContent className="p-4 h-full flex flex-col">
              <BranchProductDetailTab />
              <BranchProductDetailTabContent
                unitRelations={data?.unitRelations ?? []}
                id={data?.id!}
              />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
