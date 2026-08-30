"use client";

import * as React from "react";
import Image from "next/image";
import { useTranslations } from "next-intl";
import { ActivationStatus } from "@/api/generated";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Loader2 } from "lucide-react";
import { formatNumberVN } from "@/utils/format";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useAuth } from "@/hooks/use-auth";

interface Tariff {
  id: number;
  branchId: number;
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

interface TariffDetailDialogProps {
  tariff: Tariff | null;
  isLoading: boolean;
  error: unknown;
  isOpen: boolean;
  onClose: () => void;
}

export default function TariffDetailDialog({
  tariff,
  isLoading,
  error,
  isOpen,
  onClose,
}: TariffDetailDialogProps) {
  const t = useTranslations();
  const { user } = useAuth();
  const branches = React.useMemo(
    () =>
      user?.branchAccounts.find((x) => x.branchId === tariff?.branchId) || null,
    [tariff?.branchId, user?.branchAccounts]
  );
  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-full w-full max-h-[100vh] h-full overflow-y-auto bg-card p-6">
        <DialogHeader>
          <DialogTitle className="text-2xl text-center">
            {t("dialog.detail.title", { entity: t("common.tariff") })}
          </DialogTitle>
        </DialogHeader>

        {/* Handle loading and error states */}
        {isLoading ? (
          <div className="flex justify-center items-center py-10">
            <Loader2 className="h-8 w-8 animate-spin" />
          </div>
        ) : error || !tariff ? (
          <div className="flex justify-center items-center py-10">
            <p className="text-destructive">{t("common.errorLoading")}</p>
          </div>
        ) : (
          <div className="flex flex-col gap-6">
            {/* General Information */}
            <div className="bg-card p-6 rounded-md shadow-md">
              <h2 className="text-lg font-semibold mb-4">{t("tariff.info")}</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>
                    {t("dialog.name", { Entity: t("common.tariff") })}
                  </Label>
                  <p className="text-sm text-muted-foreground">{tariff.name}</p>
                </div>
                <div className="space-y-2">
                  <Label>{t("common.branch")}</Label>
                  <p className="text-sm text-muted-foreground">
                    {branches?.branchName || t("common.unknown")}
                  </p>
                </div>
                <div className="space-y-2">
                  <Label>{t("common.status.title")}</Label>
                  <p className="text-sm text-muted-foreground">
                    {t(`common.status.${tariff.status.toLowerCase()}`)}
                  </p>
                </div>
                <div className="space-y-2">
                  <Label>{t("table.accessorKey.startAt")}</Label>
                  <p className="text-sm text-muted-foreground">
                    {new Date(tariff.startAt).toLocaleString()}
                  </p>
                </div>
                <div className="space-y-2">
                  <Label>{t("table.accessorKey.endAt")}</Label>
                  <p className="text-sm text-muted-foreground">
                    {new Date(tariff.endAt).toLocaleString()}
                  </p>
                </div>
              </div>
            </div>

            {/* Service Tariffs */}
            <div className="bg-card p-6 rounded-md shadow-md">
              <h2 className="text-lg font-semibold mb-4">
                {t("tariff.list_service")}
              </h2>
              <div className="border rounded-md">
                <table className="w-full table-auto">
                  <thead className="bg-muted">
                    <tr>
                      <th className="px-4 py-2 text-left w-10">
                        {t("table.accessorKey.index")}
                      </th>
                      <th className="px-4 py-2 text-left">
                        {t("common.service")}
                      </th>
                      <th className="px-4 py-2 text-left">
                        {t("common.unit")}
                      </th>
                      <th className="px-4 py-2 text-left">
                        {t("common.price")}
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {tariff.serviceTariffs.map((serviceTariff, index) => (
                      <tr key={index} className="border-b">
                        <td className="px-4 py-2">{index + 1}</td>
                        <td className="px-4 py-2">
                          <div className="flex items-center gap-2">
                            <Image
                              src={
                                serviceTariff.serviceImageUrl ||
                                "/logo/favicon.svg"
                              }
                              alt={
                                serviceTariff.serviceName || t("common.unknown")
                              }
                              width={24}
                              height={24}
                              className="rounded-full object-cover"
                              placeholder="blur"
                              blurDataURL="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAC9AFp1D3b2wAAAABJRU5ErkJggg=="
                            />
                            <span>
                              {serviceTariff.serviceName || t("common.unknown")}
                            </span>
                          </div>
                        </td>
                        <td className="px-4 py-2">
                          {serviceTariff.unitName || t("common.unknown")}
                        </td>
                        <td className="px-4 py-2">
                          {formatNumberVN(serviceTariff.price)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Close Button */}
            <div className="flex justify-end">
              <Button variant="outline" onClick={onClose}>
                {t("common.close")}
              </Button>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
