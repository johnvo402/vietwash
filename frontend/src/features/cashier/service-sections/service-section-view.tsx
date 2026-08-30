/* eslint-disable react-hooks/exhaustive-deps */
"use client";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Minus, Plus } from "lucide-react";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useState, useRef, useEffect } from "react";
import { formatNumberVN, formatPriceVN, parseNumberVN } from "@/utils/format";
import { ServiceItem } from "../types";
import Image from "next/image";
import { GetByTariffResponse } from "@/api/generated";
import { useServiceCashier } from "../hooks/use-serice-cashier";
import { Input } from "@/components/ui/input";
import { useTranslations } from "next-intl";

interface ServiceSectionViewProps {
  onAddItem: (item: ServiceItem) => void;
  tariffId: number;
  search: string;
}

export const ServiceSectionView = ({
  onAddItem,
  tariffId,
  search = "",
}: ServiceSectionViewProps) => {
  const t = useTranslations();
  const [selectedServiceId, setSelectedServiceId] = useState<number | null>(
    null
  );
  const [selectedUnitRelationId, setSelectedUnitRelationId] = useState<
    number | null
  >(null);
  const [quantity, setQuantity] = useState<number>(1);
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState("");

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearchTerm(search), 350);
    return () => clearTimeout(timer);
  }, [search]);
  const {
    serviceCashiers,
    isLoading,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useServiceCashier({
    tariffId: tariffId,
    query: {
      searchKeywords: debouncedSearchTerm,
      searchTarget: ["services.name", "name"],
    },
  });

  const loadMoreRef = useRef<HTMLDivElement>(null);

  // Intersection Observer to trigger fetchNextPage when reaching the bottom
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
          fetchNextPage();
        }
      },
      { threshold: 0.1 }
    );

    if (loadMoreRef.current) {
      observer.observe(loadMoreRef.current);
    }

    return () => {
      if (loadMoreRef.current) {
        observer.unobserve(loadMoreRef.current);
      }
    };
  }, [fetchNextPage, hasNextPage, isFetchingNextPage]);

  const selectedService = serviceCashiers
    .flatMap((cat: GetByTariffResponse) => cat.services)
    .find((s) => s?.id === selectedServiceId);
  const unitRelations = selectedService?.unitRelations || [];

  const handleQuantityChange = (value: string) => {
    const parsed = parseNumberVN(value);
    if (isNaN(parsed) || parsed <= 0) {
      setQuantity(1); // Reset to 1 if input is invalid, negative, or zero
    } else {
      setQuantity(parsed);
    }
  };

  const handleAddItem = () => {
    if (!selectedServiceId || !selectedService) return;

    const selectedUnitRelation =
      unitRelations.find((ur) => ur.id === selectedUnitRelationId) ||
      unitRelations[0];

    onAddItem({
      id: selectedService.id!,
      name: selectedService.name ?? "",
      price: selectedUnitRelation?.price ?? 0,
      quantity,
      unitRelationId: selectedUnitRelation?.id!,
      unitRelationName: selectedUnitRelation?.name ?? "",
      processingTime: selectedUnitRelation.processingTime,
    });

    setSelectedServiceId(null);
    setSelectedUnitRelationId(null);
    setQuantity(1);
  };

  if (isLoading && serviceCashiers.length === 0) {
    return (
      <div className="space-y-4">
        <div className="animate-pulse space-y-4">
          {Array(3)
            .fill(0)
            .map((_, i) => (
              <div key={i} className="space-y-2">
                <Skeleton className="h-8 w-1/4" />
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  {Array(4)
                    .fill(0)
                    .map((_, j) => (
                      <Skeleton key={j} className="h-32 w-full rounded-md" />
                    ))}
                </div>
              </div>
            ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="max-h-[50vh] p-4 overflow-y-auto border bg-background rounded-md shadow-sm">
        {serviceCashiers.map((category: GetByTariffResponse) => (
          <div key={category.id} className="space-y-3">
            <h2 className="text-xl mt-4 font-semibold text-primary bg-primary-foreground">
              {category.name}
            </h2>
            <div className="grid grid-cols-2 md:grid-cols-6 gap-4">
              {category.services?.map((service) => (
                <Button
                  key={service.id}
                  variant="outline"
                  className="h-auto py-4 flex flex-col items-center justify-center text-center relative"
                  onClick={() => {
                    setSelectedServiceId(service.id!);
                    setSelectedUnitRelationId(
                      service.unitRelations?.[0]?.id || null
                    );
                    setQuantity(1);
                  }}
                >
                  <div className="relative w-full h-24 mb-2">
                    <Image
                      src={service.image || "/logo/favicon.svg"}
                      alt={t("image.alt", { entity: service.name })}
                      fill
                      className="object-cover rounded-md"
                      sizes="(max-width: 768px) 50vw, 25vw"
                      priority={false}
                    />
                  </div>
                  <span className="font-medium whitespace-normal break-words">
                    {service.name}
                  </span>
                  <span className="text-sm text-muted-foreground mt-1">
                    {service.unitRelations && service.unitRelations.length > 1
                      ? `${formatPriceVN(
                          Math.min(
                            ...service.unitRelations.map((x) => x.price ?? 0)
                          )
                        )} - ${formatPriceVN(
                          Math.max(
                            ...service.unitRelations.map((x) => x.price ?? 0)
                          )
                        )}`
                      : service.unitRelations &&
                          service.unitRelations.length === 1
                        ? `${formatPriceVN(
                            service.unitRelations[0]?.price || 0
                          )}/${service.unitRelations[0]?.name}`
                        : t("common.noData")}
                  </span>
                </Button>
              ))}
            </div>
          </div>
        ))}
        <div
          ref={loadMoreRef}
          className="h-10 flex items-center justify-center"
        >
          {isFetchingNextPage && <Skeleton className="h-8 w-1/4" />}
          {!isFetchingNextPage && hasNextPage && (
            <Button
              onClick={() => fetchNextPage()}
              disabled={isFetchingNextPage}
            >
              {t("common.more")}
            </Button>
          )}
          {!hasNextPage && serviceCashiers.length > 0 && (
            <span className="text-sm text-muted-foreground">
              {t("common.noResult")}
            </span>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-9 gap-2 pt-4 border-t items-center">
        <div className="md:col-span-2 md:flex md:flex-col md:justify-center">
          <div id="service" className="w-full flex items-center gap-2">
            <div className="relative h-24 mb-2 w-[20%]">
              <Image
                src={selectedService?.image || "/logo/favicon.svg"}
                alt={t("image.alt", { entity: selectedService?.name || "" })}
                fill
                className="object-contain rounded-md"
                sizes="(max-width: 768px) 100vw, 50vw"
                priority={false}
              />
            </div>

            <p className="text-center text-wrap">
              {selectedService ? selectedService.name : t("common.noData")}
            </p>
          </div>
        </div>

        <div className="md:col-span-2 md:flex md:flex-col md:justify-center">
          <Label htmlFor="unit" className="mb-1">
            {t("common.unit")}
          </Label>
          <Select
            value={selectedUnitRelationId?.toString()}
            onValueChange={(value) => setSelectedUnitRelationId(Number(value))}
            disabled={!selectedService || unitRelations.length === 0}
          >
            <SelectTrigger id="unit" className="w-full">
              <SelectValue
                placeholder={t("common.entitySelectPlaceholder", {
                  entity: t("common.unit"),
                })}
              />
            </SelectTrigger>
            <SelectContent>
              {unitRelations.map((unitRelation) => (
                <SelectItem
                  key={unitRelation.id}
                  value={unitRelation.id?.toString()!}
                  className="pl-6"
                >
                  {unitRelation.name} - {formatPriceVN(unitRelation.price || 0)}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="md:flex md:col-span-2 md:flex-col md:justify-center">
          <Label htmlFor="quantity" className="mb-1">
            {t("table.accessorKey.quantity")}
          </Label>
          <div id="quantity" className="flex items-center gap-2 justify-center">
            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8"
              onClick={() => setQuantity(Math.max(1, quantity - 1))}
            >
              <Minus className="h-4 w-4" />
            </Button>
            <Input
              type="text"
              value={formatNumberVN(quantity)}
              onChange={(e) => handleQuantityChange(e.target.value)}
              className="w-[50%] h-8 border rounded-[var(--radius)] focus:ring-2 focus:ring-ring text-center"
              placeholder={t("common.placeholder", {
                entity: t("table.accessorKey.quantity").toLowerCase(),
              })}
            />
            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8"
              onClick={() => setQuantity(quantity + 1)}
            >
              <Plus className="h-4 w-4" />
            </Button>
          </div>
        </div>
        <div className="md:flex md:flex-col md:justify-center md:col-span-2">
          <Label htmlFor="total" className="mb-1">
            {t("table.accessorKey.total")}
          </Label>
          <div id="total" className="w-full text-center font-medium">
            {formatPriceVN(
              (unitRelations.find((ur) => ur.id === selectedUnitRelationId)
                ?.price || 0) * quantity
            )}
          </div>
        </div>
        <div className="md:flex md:items-center md:justify-center">
          <Button
            onClick={handleAddItem}
            disabled={!selectedServiceId}
            className="w-full h-10"
          >
            <Plus className="mr-2 h-4 w-4" /> {t("common.more")}
          </Button>
        </div>
      </div>
    </div>
  );
};
