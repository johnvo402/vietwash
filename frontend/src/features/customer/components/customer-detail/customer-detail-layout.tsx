"use client";

import { useQuery } from "@tanstack/react-query";
import { CustomerDetailTab } from "./content-section/customer-detail-tab";
import { apiClient } from "@/api/client";
import { CustomerInformation } from "./infomation-section/customer-information";
import CustomerDetailTabContent from "./content-section/customer-detail-tab-content";
import { Card, CardContent } from "@/components/ui/card";
import { useEffect, useState } from "react";
interface CustomerDetailLayoutProps {
  publicId: string;
}
export default function CustomerDetailLayout(props: CustomerDetailLayoutProps) {
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(props.publicId);
    if (storedId) setId(Number(storedId));
  }, [props.publicId]);
  const { data: customer, isLoading } = useQuery({
    queryKey: ["customer", id],
    queryFn: () =>
      apiClient
        .authApiCustomersDetailEndpoint(id!)
        .then((res) => res.data.results),
    enabled: !!id,
  });
  return (
    <div className="mx-6 h-full py-6">
      <div className="grid grid-cols-12 gap-6 h-full">
        {/* Left side - Product information (4 columns) */}
        <div className="h-full col-span-12 md:col-span-4">
          <CustomerInformation customer={customer!} isLoading={isLoading} />
        </div>
        <div className="col-span-12 md:col-span-8">
          <Card className="h-full">
            <CardContent className="p-6 h-full flex flex-col">
              <CustomerDetailTab />
              <CustomerDetailTabContent id={customer?.id!} />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
