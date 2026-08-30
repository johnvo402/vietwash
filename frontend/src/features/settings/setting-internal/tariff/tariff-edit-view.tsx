"use client";

import * as React from "react";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { ActivationStatus } from "@/api/generated";
import TariffDialog from "./components/create-tariff-form";
import { Loader2 } from "lucide-react";
import { useTranslations } from "next-intl";

interface Tariff {
  id: number;
  branchId: number;
  branchName: string;
  name: string;
  status: ActivationStatus;
  startAt: string;
  endAt: string;
  serviceTariffs: Array<{
    serviceId: number;
    serviceName: string;
    serviceImageUrl?: string;
    unitRelationId: number;
    unitName: string;
    price: number;
  }>;
}
interface TariffEditProps {
  isOpen: boolean;
  onClose: () => void;
  id: number;
}
export default function TariffEdit({ isOpen, onClose, id }: TariffEditProps) {
  const t = useTranslations();
  const {
    data: tariff,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["tariff", id],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiTariffsDetailIdGet(
        Number(id)
      );
      return response.data.results as Tariff;
    },
    enabled: !!id,
  });
  if (isLoading) {
    return (
      <div className="flex justify-center items-center py-10">
        <Loader2 className="h-8 w-8 animate-spin" />
      </div>
    );
  } else if (error || !tariff) {
    return (
      <div className="flex justify-center items-center py-10">
        <p className="text-destructive">{t("common.errorLoading")}</p>
      </div>
    );
  }
  return (
    tariff && (
      <TariffDialog tariff={tariff || null} isOpen={isOpen} onClose={onClose} />
    )
  );
}
