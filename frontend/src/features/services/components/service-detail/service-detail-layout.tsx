"use client";

import { useQuery } from "@tanstack/react-query";
import { ServiceDetailTab } from "./content-section/service-detail-tab";
import { apiClient } from "@/api/client";
import { ServiceInformation } from "./infomation-section/service-information";
import ServiceDetailTabContent from "./content-section/service-detail-tab-content";
import { Card, CardContent } from "@/components/ui/card";
import { useEffect, useState } from "react";
interface ServiceDetailLayoutProps {
  publicId: string;
}
export default function ServiceDetailLayout(props: ServiceDetailLayoutProps) {
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(props.publicId);
    if (storedId) setId(Number(storedId));
  }, [props.publicId]);
  const { data: service } = useQuery({
    queryKey: ["service", id],
    queryFn: () =>
      apiClient
        .ecommerceApiServicesDetailIdGet(id!)
        .then((res) => res.data.results),
    enabled: !!id,
  });
  return (
    <div className="mx-6 h-full py-6">
      <div className="grid grid-cols-12 gap-6 h-full">
        {/* Left side - Product information (4 columns) */}
        <div className="h-full col-span-12 md:col-span-4">
          <ServiceInformation service={service!} />
        </div>
        <div className="col-span-12 md:col-span-8">
          <Card className="h-full">
            <CardContent className="p-6 h-full flex flex-col">
              <ServiceDetailTab />
              <ServiceDetailTabContent
                unitRelations={service?.unitRelations ?? []}
                id={service?.id!}
              />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
