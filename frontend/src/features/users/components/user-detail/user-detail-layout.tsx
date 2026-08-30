"use client";

import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useEffect, useState } from "react";
import UserOrderView from "./content-section/order-staff";
import { UserInformation } from "./infomation-section/user-information";
import { formatNumberVN, formatPriceVN } from "@/utils/format";
import { useTranslations } from "next-intl";
interface UserDetailLayoutProps {
  publicId: string;
}
export default function UserDetailLayout(props: UserDetailLayoutProps) {
  const [id, setId] = useState<number | null>(null);
  const t = useTranslations();
  useEffect(() => {
    const storedId = sessionStorage.getItem(props.publicId);
    if (storedId) setId(Number(storedId));
  }, [props.publicId]);
  const { data: user, isLoading } = useQuery({
    queryKey: ["user", id],
    queryFn: () =>
      apiClient
        .authApiAccountsDetailEndpoint(id!)
        .then((res) => res.data.results),
    enabled: !!id,
  });

  const { data: dataStatistic } = useQuery({
    queryKey: ["user-dataStatistic", id],
    queryFn: () =>
      apiClient
        .ecommerceApiOrdersGetByStaffIdGet(id!)
        .then((res) => res.data.results),
    enabled: !!id,
  });
  return (
    <div className="mx-6 h-full py-6">
      <div className="grid grid-cols-12 gap-6 h-full">
        {/* Left side - Product information (4 columns) */}
        <div className="h-full col-span-12 md:col-span-4">
          <UserInformation user={user!} isLoading={isLoading} />
        </div>
        <div className="col-span-12 md:col-span-8">
          <Card className="h-full">
            <CardHeader>
              <div className="flex gap-4">
                <Card>
                  <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">
                      {t("user.total_order")}
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="text-2xl font-bold">
                      {formatNumberVN(dataStatistic?.totalOrder ?? 0)}
                    </div>
                  </CardContent>
                </Card>
                <Card>
                  <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">
                      {t("user.total_revenue")}
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="text-2xl font-bold">
                      {formatPriceVN(dataStatistic?.totalRevenue ?? 0)}
                    </div>
                  </CardContent>
                </Card>
              </div>
            </CardHeader>
            <CardContent className=" h-full flex flex-col">
              <UserOrderView staffId={user?.id!} />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
