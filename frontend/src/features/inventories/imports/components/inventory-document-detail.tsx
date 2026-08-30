"use client";

import { useTranslations } from "next-intl";
import { useRouter } from "next/navigation";
import { formatPriceVN, formatNumberVN } from "@/utils/format";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Undo2 } from "lucide-react";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import {
  InventoryDocumentDetailResponse,
  InventoryStatus,
} from "@/api/generated/api";
import { ColumnDef } from "@tanstack/react-table";
import { useQueryState, parseAsInteger } from "nuqs";
import { DataTable, Paging } from "@/components/ui/table/data-table";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { useAuth } from "@/hooks/use-auth";
import { format } from "date-fns";
import DownloadButton from "./export-pdf";

interface SupplyDetailPageProps {
  supply: InventoryDocumentDetailResponse;
}

export function SupplyDetailPage({ supply }: SupplyDetailPageProps) {
  const t = useTranslations();
  const router = useRouter();
  const { setPage } = useTableFilters();
  const { user } = useAuth();
  const branch = user?.branchAccounts.find(
    (x) => x.branchId == supply.branchId
  );

  const pageSizeOption = [5, 10, 20, 40, 50];
  // State for productSupplyings pagination
  const [page] = useQueryState(
    "page",
    parseAsInteger
      .withOptions({ shallow: false, history: "push" })
      .withDefault(1)
  );
  const [pageSize] = useQueryState(
    "pageSize",
    parseAsInteger
      .withOptions({ shallow: false, history: "push" })
      .withDefault(pageSizeOption[0])
  );

  // Mock server-side pagination data for productSupplyings
  const productTotalItems = (supply.productSupplyings || []).length;
  const productTotalPages = Math.max(
    1,
    Math.ceil(productTotalItems / pageSize)
  );
  const productPaging: Paging = {
    currentPage: page,
    pageSize: pageSize,
    totalPage: productTotalPages,
  };
  const paginatedProductSupplyings = (supply.productSupplyings || []).slice(
    (page - 1) * pageSize,
    page * pageSize
  );

  // Mock server-side pagination data for equipmentSupplyings
  const equipmentTotalItems = (supply.equipmentSupplyings || []).length;
  const equipmentTotalPages = Math.max(
    1,
    Math.ceil(equipmentTotalItems / pageSize)
  );
  const equipmentPaging: Paging = {
    currentPage: page,
    pageSize: pageSize,
    totalPage: equipmentTotalPages,
  };
  const paginatedEquipmentSupplyings = (supply.equipmentSupplyings || []).slice(
    (page - 1) * pageSize,
    page * pageSize
  );

  // Define columns for productSupplyings
  const productColumns: ColumnDef<any>[] = [
    {
      accessorKey: "index",
      header: t("table.accessorKey.index"),
      cell: ({ row, table }) => {
        const pageIndex = table.getState().pagination.pageIndex;
        const pageSize = table.getState().pagination.pageSize;
        return pageIndex * pageSize + row.index + 1;
      },
      meta: {
        header: { className: "w-1 text-center" },
        body: { className: "text-center" },
      },
    },
    {
      accessorKey: "supplierName",
      header: t("inventory.productSupplyings.supplier"),
      meta: {
        header: { className: "w-48 text-center" },
      },
    },
    {
      accessorKey: "productName",
      header: t("product.productName"),
      meta: {
        header: { className: "w-48 text-center" },
      },
    },

    {
      accessorKey: "unitName",
      header: t("inventory.productSupplyings.unit"),
      meta: {
        header: { className: "w-16 text-center" },
      },
    },

    {
      accessorKey: "quantity",
      header: t("inventory.quantity"),
      cell: ({ row }) => formatNumberVN(row.original.quantity ?? 0),
      meta: {
        header: { className: "w-24 text-center" },
        body: { className: "text-right" },
      },
    },
    {
      accessorKey: "price",
      header: t("inventory.price"),
      cell: ({ row }) => formatPriceVN(row.original.price ?? 0),
      meta: {
        header: { className: "w-24 text-center" },
        body: { className: "text-right" },
      },
    },
  ];

  const equipmentColumns: ColumnDef<any>[] = [
    {
      accessorKey: "index",
      header: t("table.accessorKey.index"),
      cell: ({ row, table }) => {
        const pageIndex = table.getState().pagination.pageIndex;
        const pageSize = table.getState().pagination.pageSize;
        return pageIndex * pageSize + row.index + 1;
      },
      meta: {
        header: { className: "w-1 text-center" },
        body: { className: "text-center" },
      },
    },
    {
      accessorKey: "name",
      header: t("inventory.equipmentSupplyings.name"),
      meta: {
        header: { className: "w-48 text-center" },
      },
    },
    {
      accessorKey: "code",
      header: t("inventory.equipmentSupplyings.code"),
      meta: {
        header: { className: "w-24 text-center" },
      },
    },
    {
      accessorKey: "quantity",
      header: t("inventory.quantity"),
      cell: ({ row }) => formatNumberVN(row.original.quantity ?? 0),
      meta: {
        header: { className: "w-24 text-center" },
        body: { className: "text-right" },
      },
    },
    {
      accessorKey: "price",
      header: t("inventory.price"),
      cell: ({ row }) => formatPriceVN(row.original.price ?? 0),
      meta: {
        header: { className: "w-24 text-center" },
        body: { className: "text-right" },
      },
    },
    {
      accessorKey: "supplierName",
      header: t("common.supplier"),
      meta: {
        header: { className: "w-40 text-center" },
      },
    },
  ];
  const handleTabChange = (value: string) => {
    setPage(1);
  };
  return (
    <Dialog open={true} onOpenChange={() => router.back()}>
      <DialogContent className="!w-screen !h-screen max-w-none max-h-none overflow-y-auto p-0">
        <DialogHeader className="sticky top-0 z-10 bg-background p-6 text-primary">
          <DialogTitle>
            {t(`inventory.detail.${supply.type?.toLocaleLowerCase()}`)}{" "}
            {supply.code}
          </DialogTitle>
          <div className="absolute right-4 top-4 flex gap-6 justify-between">
            {supply.status === InventoryStatus.Completed && (
              <DownloadButton invId={supply.id!} supply={supply} />
            )}
            <Button
              variant="ghost"
              size="icon"
              onClick={() => router.back()}
              className="ml-6"
            >
              <Undo2 className="h-4 w-4" />
              <span className="sr-only">{t("common.close")}</span>
            </Button>
          </div>
        </DialogHeader>
        <div className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            <div>
              <p className="text-sm font-medium">{t("common.branch")}</p>
              <p className="text-base">{branch?.branchName}</p>
            </div>
            <div>
              <p className="text-sm font-medium">{t("common.note")}</p>
              <p className="text-base">{supply.note || "--"}</p>
            </div>
            <div>
              <p className="text-sm font-medium">{t("common.status.title")}</p>
              <p className="text-base">
                {t(`common.status.${supply.status?.toLocaleLowerCase()}`)}
              </p>
            </div>
            <div>
              <p className="text-sm font-medium">
                {t("inventory.transactionAt")}
              </p>
              <p className="text-base">
                {supply.transactionAt
                  ? format(new Date(supply.transactionAt), "dd/MM/yy HH:mm:ss")
                  : "--"}
              </p>
            </div>
          </div>

          <Tabs
            defaultValue="products"
            className="w-full"
            onValueChange={handleTabChange}
          >
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="products">
                {t("inventory.productSupplyings.title")}
              </TabsTrigger>
              <TabsTrigger value="equipment">
                {t("inventory.equipmentSupplyings.title")}
              </TabsTrigger>
            </TabsList>
            <TabsContent value="products">
              <div className="space-y-4">
                <DataTable
                  columns={productColumns}
                  data={paginatedProductSupplyings}
                  paging={productPaging}
                  pageSizeOptions={pageSizeOption}
                />
              </div>
            </TabsContent>
            <TabsContent value="equipment">
              <div className="space-y-4">
                <DataTable
                  columns={equipmentColumns}
                  data={paginatedEquipmentSupplyings}
                  paging={equipmentPaging}
                  pageSizeOptions={pageSizeOption}
                />
              </div>
            </TabsContent>
          </Tabs>

          <div className="mt-6 flex flex-col gap-6 md:flex-row md:justify-between">
            <div className="flex flex-col gap-4 w-full md:w-1/2"></div>
            <div className="flex flex-col gap-4 w-full md:w-1/2">
              <div className="flex justify-between items-center">
                <h3 className="text-sm font-medium">
                  {t("inventory.totalProductQuantity")}
                </h3>
                <p className="text-base font-bold">
                  {formatNumberVN(
                    supply.productSupplyings?.reduce(
                      (sum, item) => sum + (item.quantity ?? 0),
                      0
                    ) ?? 0
                  )}
                </p>
              </div>
              <div className="flex justify-between items-center">
                <h3 className="text-sm font-medium">
                  {t("inventory.totalEquipmentQuantity")}
                </h3>
                <p className="text-base font-bold">
                  {formatNumberVN(
                    supply.equipmentSupplyings?.reduce(
                      (sum, item) => sum + (item.quantity ?? 0),
                      0
                    ) ?? 0
                  )}
                </p>
              </div>
              <div className="flex justify-between items-center">
                <h3 className="text-sm font-medium">
                  {t("inventory.totalAmount")}
                </h3>
                <p className="text-base font-bold">
                  {formatPriceVN(supply.amount ?? 0)}
                </p>
              </div>
            </div>
          </div>
        </div>
        <DialogFooter className="sticky bottom-0 z-10 p-6 bg-background border-t border-secondary flex justify-end space-x-2">
          <Button
            type="button"
            variant="destructive"
            className="rounded-lg"
            onClick={() => router.back()}
          >
            {t("common.close")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
