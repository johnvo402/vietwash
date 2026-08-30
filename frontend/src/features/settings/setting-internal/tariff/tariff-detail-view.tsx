"use client";

import * as React from "react";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { ActivationStatus } from "@/api/generated";
import TariffDetailDialog from "./components/tariff-detail-dialog";

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
interface TariffDetailProps {
  isOpen: boolean;
  onClose: () => void;
  id: number;
}
export default function TariffDetail({
  isOpen,
  onClose,
  id,
}: TariffDetailProps) {

  // Fetch tariff data
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

  return (
    <TariffDetailDialog
      tariff={tariff || null}
      isLoading={isLoading}
      error={error}
      isOpen={isOpen}
      onClose={onClose}
    />
  );
}
