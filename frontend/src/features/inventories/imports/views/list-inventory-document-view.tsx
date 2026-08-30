"use client";

import { DataTable } from "@/components/ui/table/data-table";

import { useTranslations } from "next-intl";
import { Button, buttonVariants } from "@/components/ui/button";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_INVENTORY_DOC_CREATE } from "@/types/router-type";
import { cn } from "@/lib/utils";
import { Plus } from "lucide-react";
import { useInventoryDocuments } from "../components/column";
import {
  useInventoryDocumentMutations,
  useInventoryDocumentsQuery,
} from "../hooks/use-inventory-document";
import { InventoryStatus, InventoryType } from "@/api/generated/api";
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogCancel,
  AlertDialogAction,
} from "@/components/ui/alert-dialog";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { InventoryFilter } from "../components/inventory-filter";
import { useState } from "react";
import { Option } from "@/components/core/selects/multi-select";
import { DateUtils } from "@/utils/date.utils";
import { DateRange } from "@/features/reports/types/filter.type";

export default function InventoryListingView({
  type,
}: {
  type: InventoryType;
}) {
  const t = useTranslations();
  const [branchId, setBranchId] = useState<string | null>("all");
  const [statusFilter, setStatusFilter] = useState<Option[]>([
    {
      value: InventoryStatus.Pending,
      label: t("common.status.pending"),
    },
    {
      value: InventoryStatus.Completed,
      label: t("common.status.completed"),
    },
  ]);
  const [dateRange, setDateRange] = useState<DateRange>(() =>
    DateUtils.getDateRange("thisMonth")
  );
  const { updateStatusInventoryDocument } = useInventoryDocumentMutations(type);

  const {
    columns,
    isCancelDialogOpen,
    setIsCancelDialogOpen,
    setSelectedDocumentId,
    cancelReason,
    setCancelReason,
    handleCancelConfirm,
  } = useInventoryDocuments({
    type: type,
    onUpdateStatus: async (id, status, reason) => {
      await updateStatusInventoryDocument({
        id: id,
        command: { status: status, cancelReason: reason },
      });
    },
  });
  const { error, isLoading, paging, documents } = useInventoryDocumentsQuery({
    type: type,
    status: statusFilter.map((option) => option.value as InventoryStatus),
    branchId: branchId !== "all" ? Number(branchId) : undefined,
    dateRange,
  });
  const pushRouter = usePushRouter();
  return (
    <div className="w-full">
      <div className="flex flex-wrap mb-5 items-stretch justify-between">
        <InventoryFilter
          branchId={String(branchId)}
          setBranchId={(branchId) => setBranchId(branchId)}
          setStatusFilter={setStatusFilter}
          statusFilter={statusFilter}
          setDateRange={(value) => value && setDateRange(value)}
          dateRange={dateRange}
        />
        <Button
          onClick={() =>
            pushRouter.pushRouter({
              router: ROUTE_INVENTORY_DOC_CREATE,
              params: {
                type: type.toLowerCase(),
              },
              query: {
                pageSize: paging.pageSize!,
                page: paging.currentPage!,
              },
            })
          }
          className={cn(buttonVariants(), "text-xs md:text-sm")}
        >
          <Plus className="h-4 w-4" /> {t("common.create")}
        </Button>
      </div>

      <div className="rounded-md border shadow-sm">
        <DataTable
          columns={columns}
          data={documents ?? []}
          paging={paging || {}}
          loading={isLoading}
          error={error}
        />
      </div>
      <AlertDialog open={isCancelDialogOpen}>
        <AlertDialogContent className="sm:max-w-[425px] bg-white dark:bg-gray-800">
          <AlertDialogHeader>
            <AlertDialogTitle className="text-gray-900 dark:text-gray-100">
              {t("common.cancelReasonTitle")}
            </AlertDialogTitle>
            <AlertDialogDescription className="text-gray-600 dark:text-gray-400">
              {t("common.cancelReasonLabel")}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-4 items-center gap-4">
              <Label
                htmlFor="cancel-reason"
                className="text-right text-gray-700 dark:text-gray-300"
              >
                {t("common.cancelReasonLabel")}
              </Label>
              <Input
                id="cancel-reason"
                value={cancelReason}
                onChange={(e) => setCancelReason(e.target.value)}
                className="col-span-3 border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100"
                placeholder={t("common.cancelReasonPlaceholder")}
                autoFocus
              />
            </div>
          </div>
          <AlertDialogFooter>
            <AlertDialogCancel
              onClick={() => {
                setIsCancelDialogOpen(false);
                setCancelReason("");
                setSelectedDocumentId(null);
              }}
              className="text-gray-600 dark:text-gray-400 border-gray-300 dark:border-gray-600"
            >
              {t("common.cancel")}
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleCancelConfirm}
              className="bg-red-600 hover:bg-red-700 dark:bg-red-700 dark:hover:bg-red-800 text-white"
              disabled={!cancelReason.trim()}
            >
              {t("common.status.confirm")}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
