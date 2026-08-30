"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Loader2, Check, X, Search } from "lucide-react";
import Image from "next/image";
import { useTranslations } from "next-intl";
import { EquipmentStatus, ListEquipmentResponse } from "@/api/generated";
import { useFormEquipments } from "@/features/equipment/hooks/use-equipment";
import { OrderEquipment } from "../../cashier/types";

interface EquipmentPickerProps {
  selected: OrderEquipment[];
  onToggle: (equipment: OrderEquipment) => void;
  disabledIds?: number[];
  searchTerm: string;
}

export default function CashierEquipmentPicker({
  selected,
  onToggle,
  disabledIds = [],
  searchTerm = "",
}: EquipmentPickerProps) {
  const t = useTranslations();
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState("");

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearchTerm(searchTerm), 350);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const {
    equipments,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useFormEquipments({
    searchTerm: debouncedSearchTerm,
    query: {
      filter: {
        $and: [
          {
            status: {
              $eq: EquipmentStatus.Active,
            },
          },
          {
            using: {
              $eq: false,
            },
          },
        ],
      },
    },
  });

  const loadMoreRef = useRef<HTMLDivElement>(null);
  useEffect(() => {
    const io = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage)
          fetchNextPage();
      },
      { threshold: 0.1 }
    );
    if (loadMoreRef.current) io.observe(loadMoreRef.current);
    return () => io.disconnect();
  }, [fetchNextPage, hasNextPage, isFetchingNextPage]);

  const selectedIds = useMemo(
    () => new Set(selected.map((s) => s.equipmentId)),
    [selected]
  );

  if (error) {
    return (
      <div className="text-center text-red-600">
        {t("equipment.equipmentList.errorLoading")}:{" "}
        {String((error as any).message)}
      </div>
    );
  }

  return (
    <div className="w-full">
      {/* Selected summary */}
      <div className="mb-2 flex flex-wrap gap-1.5">
        {selected.map((s) => (
          <Badge
            key={s.equipmentId}
            variant="secondary"
            className="flex items-center gap-1.5 px-2 py-0.5 text-xs"
          >
            {s.equipmentName}
            <button
              type="button"
              onClick={() => onToggle(s)}
              className="ml-0.5 rounded hover:bg-muted p-0.5"
              aria-label={t("common.remove")}
            >
              <X className="h-3 w-3" />
            </button>
          </Badge>
        ))}
        {selected.length === 0 && (
          <span className="text-xs text-muted-foreground">
            {t("common.placeholderSelect", {
              entity: t("equipment.title").toLowerCase(),
            })}
          </span>
        )}
      </div>

      {/* Cards (compact) */}
      {isLoading && equipments.length === 0 ? (
        <div className="py-8 flex items-center justify-center">
          <Loader2 className="h-5 w-5 animate-spin" />
        </div>
      ) : (
        <div className="grid grid-cols-2 md:grid-cols-2 xl:grid-cols-3 gap-3">
          {equipments.map((eq: ListEquipmentResponse) => {
            const id = eq.id!;
            const isSelected = selectedIds.has(id);
            const isDisabled = disabledIds.includes(id);
            return (
              <Card
                key={id}
                className={`overflow-hidden rounded-lg border p-2 transition-colors ${
                  isSelected ? "ring-2 ring-primary" : "hover:bg-muted/30"
                } ${isDisabled ? "opacity-50 pointer-events-none" : "cursor-pointer"}`}
                onClick={() =>
                  onToggle({ equipmentId: id, equipmentName: eq.name! })
                }
              >
                <div className="relative w-full h-20 mb-2">
                  <Image
                    src={eq.image ?? "/logo/favicon.svg"}
                    alt={eq.name!}
                    fill
                    className="object-cover rounded"
                    sizes="(max-width: 768px) 50vw, 25vw"
                  />
                  {isSelected && (
                    <div className="absolute top-1 left-1 flex items-center gap-1 rounded bg-primary text-primary-foreground px-1.5 py-0.5 text-[10px]">
                      <Check className="h-3 w-3" />
                    </div>
                  )}
                </div>
                <div className="text-sm font-medium leading-tight line-clamp-2">
                  {eq.name}
                </div>
                <div className="text-[11px] text-muted-foreground mt-0.5">
                  {eq.code}
                </div>
              </Card>
            );
          })}
        </div>
      )}

      <div ref={loadMoreRef} className="h-8 flex items-center justify-center">
        {isFetchingNextPage && (
          <div className="flex items-center gap-2 text-xs text-muted-foreground">
            <Loader2 className="h-4 w-4 animate-spin" /> {t("common.loading")}
          </div>
        )}
      </div>
    </div>
  );
}
