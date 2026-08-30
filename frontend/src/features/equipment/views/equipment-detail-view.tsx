"use client";

import {
  EquipmentStatus,
  GetEquipmentDetailResponse,
} from "@/api/generated/api";
import { useEffect, useState } from "react";
import { apiClient } from "@/api/client";
import { useQuery } from "@tanstack/react-query";
import { EquipmentInformation } from "../components/equipment-detail";
import { Card, CardContent } from "@/components/ui/card";
import EquipmentActivityPage from "../components/equipment-activity";
interface DetailProps {
  params: { publicId: string };
}
export default function DetailEquipmentPage({ params }: DetailProps) {
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(params.publicId);
    if (storedId) setId(Number(storedId));
  }, [params.publicId]);

  const { data, isLoading, refetch } = useQuery<
    GetEquipmentDetailResponse | undefined
  >({
    queryKey: ["equipment", id],
    queryFn: async () => {
      if (id === null) return undefined;
      const response = await apiClient.ecommerceApiEquipmentsDetailIdGet(id);
      return response.data.results;
    },
    enabled: id !== null,
  });
  return (
    <div className="m-6 h-full pb-12">
      <div className="grid grid-cols-12 gap-6 h-full">
        {/* Left side - Product information (4 columns) */}
        <div className="h-full col-span-12 md:col-span-4">
          <EquipmentInformation
            refetch={refetch}
            equipment={data}
            isLoading={isLoading}
          />
        </div>
        <div className="col-span-12 md:col-span-8">
          <Card className="h-full">
            <CardContent className="p-4 h-full flex flex-col">
              <EquipmentActivityPage
                id={id!}
                canCreate={data?.status !== EquipmentStatus.Active}
              />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
