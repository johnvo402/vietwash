"use client";

import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent } from "@/components/ui/card";
import { SupplierInformation } from "./info-section/supplier-info";
import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import ImportExportSupplierPage from "./contents-section/import-export-supplier";

interface SupplierDetailLayoutProps {
  publicId: string;
}

export default function SupplierDetailLayout(props: SupplierDetailLayoutProps) {
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(props.publicId);
    if (storedId) setId(Number(storedId));
  }, [props.publicId]);

  const {
    data: supplier,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["supplier", id],
    queryFn: () =>
      apiClient
        .ecommerceApiSuppliersDetailIdGet(id!)
        .then((res) => res.data.results),
    enabled: !!id,
  });
  const t = useTranslations();

  return (
    <div className="mx-6 h-full py-6">
      <div className="grid grid-cols-12 gap-6 h-full">
        {/* Left side - Supplier information (4 columns) */}
        <div className="h-full col-span-12 md:col-span-4">
          {isLoading ? (
            <SupplierInfoSkeleton />
          ) : error || !supplier ? (
            <Card>
              <CardContent className="p-6">
                <p className="text-sm text-destructive">
                  {error ? t("common.error") : t("common.noData")}
                </p>
              </CardContent>
            </Card>
          ) : (
            <SupplierInformation supplier={supplier} />
          )}
        </div>
        {/* Right side - Placeholder for additional content (8 columns) */}
        <div className="col-span-12 md:col-span-8">
          <Card className="h-full">
            <CardContent className="p-6 h-full flex flex-col">
              <ImportExportSupplierPage id={supplier?.id!} />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

function SupplierInfoSkeleton() {
  return (
    <Card className="min-h-[calc(80vh_-_56px)]">
      <CardContent className="p-4">
        <div className="animate-pulse">
          <div className="flex flex-col items-center mb-6">
            <Skeleton className="h-24 w-24 rounded-full mb-4" />
            <Skeleton className="h-8 w-48 mb-2" />
            <Skeleton className="h-4 w-32" />
          </div>

          <div className="space-y-4">
            {Array(7)
              .fill(0)
              .map((_, i) => (
                <div key={i} className="flex justify-between items-center">
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-4 w-32" />
                </div>
              ))}

            <div className="pt-4 border-t">
              <Skeleton className="h-5 w-32 mb-2" />
              <Skeleton className="h-4 w-48" />
            </div>

            <div className="pt-4 border-t">
              <Skeleton className="h-5 w-32 mb-2" />
              <Skeleton className="h-4 w-48" />
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
