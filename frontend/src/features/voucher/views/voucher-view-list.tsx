"use client";

import { useTranslations } from "next-intl";
import { useVouchers, useVoucherMutations } from "../hooks/use-voucher";
import { VoucherCard } from "../components/voucher-card";
import { useEffect, useRef, useState } from "react";
import { Loader2, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  CreateVoucherForm,
  NewVoucher,
} from "../components/create-voucher-form";
import { apiClient } from "@/api/client";
import {
  CreateVoucherCommand,
  MediaType,
  ListVoucherResponse,
  GetVoucherDetailResponse,
} from "@/api/generated";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { useQuery } from "@tanstack/react-query";

// Utility function to convert ListVoucherResponse to NewVoucher
const mapToNewVoucher = (voucher: GetVoucherDetailResponse): NewVoucher => ({
  id: voucher.id,
  code: voucher.code || "",
  title: voucher.title || "",
  img: null, // No File object in viewMode or editMode; handled by form input
  imgUrl: voucher.imgUrl || "", // Use imgUrl from API
  discountFixed: voucher.discountFixed ?? true,
  discountValue: voucher.discountValue || 0,
  customerGroups: voucher.customerGroups || [],
  description: voucher.description || "",
  startAt: voucher.startAt || "",
  endAt: voucher.endAt || "",
  status: voucher.status || "Active",
  customerIds: voucher.customerIds || [],
});

export default function VoucherView() {
  const t = useTranslations();
  const loadMoreRef = useRef<HTMLDivElement>(null);
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [voucherSelectedId, setVoucherSelectedId] = useState<number | null>(
    null
  );
  const [viewDetail, setViewDetail] = useState(false);

  const { searchQuery, setPage, setSearchQuery } = useTableFilters();

  const {
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading,
    vouchers,
    customers,
  } = useVouchers(searchQuery);

  const { createVoucher, updateVoucher } = useVoucherMutations();

  const { data: voucher, isLoading: isVoucherLoading } = useQuery({
    queryKey: ["voucher", voucherSelectedId],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiVouchersDetailIdGet(
        voucherSelectedId!
      );
      return response.data.results;
    },
    enabled: !!voucherSelectedId,
  });

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

    return () => observer.disconnect();
  }, [fetchNextPage, hasNextPage, isFetchingNextPage]);

  const handleCreateVoucher = async (voucher: NewVoucher) => {
    let imageKey: string = voucher.imgUrl || "";
    if (voucher.img && voucher.img.size > 0) {
      const response = await apiClient.authApiMediaPost(
        [voucher.img],
        MediaType.Image
      );
      imageKey = response.data.results?.key?.[0] ?? "";
    }

    const voucherForm: CreateVoucherCommand = {
      ...voucher,
      customerGroups: voucher.customerGroups.map((group) => ({
        group,
      })) as any,
      imgUrl: imageKey,
    };
    await createVoucher({ command: voucherForm as any });
    setIsCreateDialogOpen(false);
  };

  const handleUpdateVoucher = async (voucher: NewVoucher) => {
    let imageKey: string = voucher.imgUrl || "";
    if (voucher.img && voucher.img.size > 0) {
      const response = await apiClient.authApiMediaPost(
        [voucher.img],
        MediaType.Image
      );
      imageKey = response.data.results?.key?.[0] ?? "";
    }

    const voucherForm: CreateVoucherCommand = {
      ...voucher,
      customerGroups: voucher.customerGroups.map((group) => ({
        id: group,
      })) as any,
      imgUrl: imageKey,
    };
    await updateVoucher({
      id: voucherSelectedId!,
      command: voucherForm as any,
    });
    setIsCreateDialogOpen(false);
    setVoucherSelectedId(null);
    setViewDetail(false);
  };

  const handleOpenEdit = (id: number) => {
    setVoucherSelectedId(id);
    setViewDetail(false);
    setIsCreateDialogOpen(true);
  };

  const handleViewDetail = (id: number) => {
    setVoucherSelectedId(id);
    setViewDetail(true);
    setIsCreateDialogOpen(true);
  };

  if (error) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center text-red-600 font-medium">
          {error.message}
        </div>
      </div>
    );
  }

  return (
    <div className="w-full">
      <div className="flex justify-between items-center mb-6">
        <DataTableSearch
          placeholder={t("search.searchBy", {
            entity: t("table.accessorKey.code"),
          })}
          searchQuery={searchQuery}
          setSearchQuery={setSearchQuery}
          setPage={setPage}
        />
        <Button onClick={() => setIsCreateDialogOpen(true)}>
          <Plus className="h-4 w-4" />
          {t("common.create")}
        </Button>
      </div>

      {isCreateDialogOpen && (!viewDetail || (viewDetail && voucher)) && (
        <CreateVoucherForm
          isOpen={true}
          onClose={() => {
            setIsCreateDialogOpen(false);
            setVoucherSelectedId(null);
            setViewDetail(false);
          }}
          onCreate={handleCreateVoucher}
          onUpdate={handleUpdateVoucher}
          customers={customers ?? []}
          viewMode={viewDetail}
          voucher={voucher ? mapToNewVoucher(voucher) : undefined}
        />
      )}

      <div className="grid w-full grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-4">
        {vouchers.map((voucher, index: number) => (
          <VoucherCard
            openEdit={handleOpenEdit}
            viewDetail={handleViewDetail}
            key={index}
            voucher={voucher}
          />
        ))}
      </div>
      <div ref={loadMoreRef} className="py-8">
        {isFetchingNextPage && (
          <div className="flex justify-center items-center">
            <Loader2 className="h-6 w-6 animate-spin text-primary" />
            <span className="ml-2">{t("common.loading")}</span>
          </div>
        )}
      </div>
      {isLoading && vouchers.length === 0 && (
        <div className="flex h-full justify-center items-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      )}
      {vouchers.length === 0 && !isLoading && (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">{t("common.noResult")}</p>
        </div>
      )}
    </div>
  );
}
