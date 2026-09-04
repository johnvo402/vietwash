"use client";

import { useState, useEffect, useRef } from "react";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Search, Loader2, MoreVertical, WashingMachine } from "lucide-react";
import {
  useEquipmentMutations,
  useFormEquipments,
} from "../hooks/use-equipment";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { useTranslations } from "next-intl";
import Image from "next/image";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_EQUIPMENT_DETAIL } from "@/types/router-type";
import {
  EquipmentFormData,
  EquipmentFormDialog,
} from "./equipment-update-dialog";
import {
  EquipmentStatus,
  ListEquipmentResponse,
  MediaType,
} from "@/api/generated";
import { apiClient } from "@/api/client";
import { EquipmentFilter } from "./equipment-filter";
import { Option } from "@/components/core/selects/multi-select";
import { useAuth } from "@/hooks/use-auth";

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case "active":
      return "bg-green-100 text-green-800 hover:bg-green-200";
    case "inactive":
      return "bg-red-100 text-red-800 hover:bg-red-200";
    case "pending":
      return "bg-yellow-100 text-yellow-800 hover:bg-yellow-200";
    default:
      return "bg-gray-100 text-gray-800 hover:bg-gray-200";
  }
};

export default function EquipmentCardList() {
  const t = useTranslations();
  const { branchActive } = useAuth();
  const [searchTerm, setSearchTerm] = useState("");
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState("");
  const loadMoreRef = useRef<HTMLDivElement>(null);
  const { pushRouter } = usePushRouter();
  const { updateEquipment } = useEquipmentMutations();
  const [equipmentSelected, setEquipmentSelected] =
    useState<ListEquipmentResponse | null>(null);
  const [statusFilter, setStatusFilter] = useState<Option>({
    value: EquipmentStatus.Active,
    label: t("common.status.active"),
  });

  // Debounce search term
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 500);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const {
    equipments,
    isLoading,
    error,
    refetch,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useFormEquipments({
    searchTerm: debouncedSearchTerm,
    query: {
      filter: {
        status: {
          $eq: statusFilter.value as EquipmentStatus,
        },
        ...(branchActive ? { branchId: { $eq: branchActive.branchId } } : {}),
      },
    },
  });

  // Infinite scroll observer
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
          fetchNextPage();
        }
      },
      { threshold: 0.1 },
    );

    if (loadMoreRef.current) {
      observer.observe(loadMoreRef.current);
    }

    return () => observer.disconnect();
  }, [fetchNextPage, hasNextPage, isFetchingNextPage]);

  if (error) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center text-red-600 font-medium">
          {t("equipment.equipmentList.errorLoading")}: {error.message}
        </div>
      </div>
    );
  }

  const handleUpdate = async (data: EquipmentFormData) => {
    let image: string | File | null = data.image ?? null;

    if (image != null) {
      if (image instanceof File) {
        const response = await apiClient.authApiMediaPost(
          [image],
          MediaType.Image,
        );
        image = response.data.results?.key?.[0] ?? "";
      }
    } else {
      image = null;
    }

    await updateEquipment({
      id: equipmentSelected?.id!,
      command: {
        name: data.name,
        description: data.description,
        status: data.status,
        image: image,
      },
    }).then(refetch);
  };

  return (
    <>
      <div className="mx-auto min-h-screen w-full">
        <div className="mb-8">
          <EquipmentFilter
            setStatusFilter={setStatusFilter}
            statusFilter={statusFilter}
            searchQuery={searchTerm}
            setSearchQuery={setSearchTerm}
          />
        </div>

        {/* Loading state for initial load */}
        {isLoading && equipments.length === 0 ? (
          <div className="flex justify-center items-center py-12">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
          </div>
        ) : (
          <>
            {/* Equipment Cards Grid */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {equipments.map((equipment, index) => (
                <Card
                  key={`${equipment.id}-${index}`}
                  className="overflow-hidden hover:shadow-xl transition-all duration-300 rounded-xl bg-background border border-border"
                >
                  <CardHeader className="p-0">
                    <div className="relative">
                      <Image
                        src={equipment.image ?? "/logo/favicon.svg"}
                        alt={equipment.name!}
                        width={400}
                        height={400}
                        className="w-full h-48 object-cover rounded-t-xl"
                      />
                      {/* Thêm lớp phủ icon nếu equipment.using là true */}
                      {equipment.using && (
                        <div className="absolute top-2 left-2 bg-primary/80 rounded-full p-2">
                          <WashingMachine className="h-6 w-6 text-background animate-shake" />
                        </div>
                      )}
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button
                            variant="ghost"
                            className="absolute top-2 right-2 h-11 w-11 p-1 rounded-full bg-border hover:bg-background"
                            aria-label={t("common.openMenu")}
                          >
                            <MoreVertical className="h-5 w-5 text-gray-600" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end" className="rounded-lg">
                          <DropdownMenuItem
                            className="cursor-pointer"
                            onClick={() =>
                              pushRouter({
                                router: ROUTE_EQUIPMENT_DETAIL,
                                params: {
                                  publicId: equipment.publicId!.toString(),
                                },
                                state: {
                                  [equipment.publicId!.toString()]:
                                    equipment.id,
                                },
                              })
                            }
                          >
                            {t("common.details")}
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            className="cursor-pointer"
                            onClick={() => setEquipmentSelected(equipment)}
                          >
                            {t("common.edit")}
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </div>
                  </CardHeader>
                  <CardContent className="p-5">
                    <div className="space-y-3">
                      <h3 className="font-semibold text-lg text-gray-800 line-clamp-2">
                        {equipment.name}
                      </h3>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-gray-500">
                          {t("table.accessorKey.code")}
                        </span>
                        <code className="text-sm font-mono bg-gray-100 px-2 py-1 rounded">
                          {equipment.code}
                        </code>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-gray-500">
                          {t("common.status.title")}
                        </span>
                        <Badge
                          className={`px-3 py-1 ${getStatusColor(equipment.status!)} font-medium`}
                        >
                          {t(
                            `common.status.${equipment.status!.toLowerCase()}`,
                          )}
                        </Badge>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>

            {/* Load More Trigger */}
            <div ref={loadMoreRef} className="py-8">
              {isFetchingNextPage && (
                <div className="flex justify-center items-center">
                  <Loader2 className="h-6 w-6 animate-spin text-primary" />
                  <span className="ml-2 ">{t("common.loading")}</span>
                </div>
              )}
            </div>

            {/* No Results */}
            {equipments.length === 0 && !isLoading && (
              <div className="text-center py-12">
                <p className="text-gray-500 text-lg">{t("common.noResult")}</p>
                {debouncedSearchTerm && (
                  <p className="text-sm text-gray-400 mt-2">
                    {t("equipment.equipmentList.adjustSearch")}
                  </p>
                )}
              </div>
            )}
          </>
        )}
      </div>
      {equipmentSelected && (
        <EquipmentFormDialog
          isOpen={equipmentSelected != null}
          initialData={{
            name: equipmentSelected.name!,
            description: equipmentSelected.description ?? "",
            status: equipmentSelected.status!,
            image: equipmentSelected.image as any,
          }}
          onSubmit={handleUpdate}
          onClose={() => setEquipmentSelected(null)}
        />
      )}
    </>
  );
}
