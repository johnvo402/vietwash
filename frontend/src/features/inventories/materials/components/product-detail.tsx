import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Pencil, ArrowLeft } from "lucide-react";
import { format } from "date-fns";
import {
  ActivationStatus,
  DetailBranchProductResponse,
  MediaType,
} from "@/api/generated/api";
import { useTranslations } from "next-intl";
import { useRouter } from "nextjs-toploader/app";
import Image from "next/image";
import { Skeleton } from "@/components/ui/skeleton";
import { formatPriceVN } from "@/utils/format";
import { useStringUtil } from "@/lib/stringUtil";
import { useMemo, useState } from "react";

import { apiClient } from "@/api/client";
import { useAuth } from "@/hooks/use-auth";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_INVENTORY_MATERIAL_EDIT } from "@/types/router-type";

interface BranchProductInformationProps {
  branchProduct?: DetailBranchProductResponse; // Made optional to handle loading state
  isLoading?: boolean; // Indicate loading state
  refetch: () => void;
}

export const BranchProductInformation = ({
  branchProduct,
  isLoading = false,
  refetch,
}: BranchProductInformationProps) => {
  const t = useTranslations();
  const route = useRouter();
  const { formatDistance } = useStringUtil();
  const { user } = useAuth();
  const { pushRouter } = usePushRouter();
  const statusTitle = t("common.status.title"); // Trạng thái
  const priceTitle = t("product.capitalPrice"); // Giá vốn
  const codeTitle = t("table.accessorKey.code"); // Mã
  const categoryTitle = t("common.category"); // danh mục (thay branchProduct.category)
  const branchTitle = t("common.branch"); // chi nhánh (thay branchProduct.branch)
  const stockQuantityTitle = t("product.stockQuantity"); // Số lượng tồn kho (thay branchProduct.stockQuantity)
  const createdByTitle = t("table.accessorKey.createdBy"); // Người tạo
  const updatedByTitle = t("table.accessorKey.updatedBy"); // Người cập nhật
  const branch = useMemo(() => {
    return (
      user?.branchAccounts.find(
        (x) => x.branchId === branchProduct?.branchId
      ) || null
    );
  }, [branchProduct?.branchId, user?.branchAccounts]);
  const handleEdit = () => {
    if (branchProduct?.id) {
      pushRouter({
        router: ROUTE_INVENTORY_MATERIAL_EDIT,
        params: { publicId: branchProduct.publicId?.toString()! },
        state: {
          [branchProduct.publicId?.toString()!]: branchProduct.id,
        },
      });
    }
  };

  if (isLoading || !branchProduct) {
    return (
      <Card className="min-h-[calc(80vh_-_56px)] relative">
        <CardContent className="p-4 h-full flex flex-col">
          <div className="flex flex-col items-center mb-4">
            <Skeleton className="w-[400px] h-[400px] rounded mb-2" />
            <Skeleton className="w-48 h-6 mb-2" />
            <Skeleton className="w-64 h-4" />
          </div>
          <div className="space-y-3 text-sm">
            <div className="flex justify-between">
              <Skeleton className="w-24 h-4" />
              <Skeleton className="w-16 h-4" />
            </div>
            <div className="flex justify-between">
              <Skeleton className="w-24 h-4" />
              <Skeleton className="w-16 h-4" />
            </div>
            <div className="flex justify-between">
              <Skeleton className="w-24 h-4" />
              <Skeleton className="w-16 h-4" />
            </div>
            <div className="flex justify-between">
              <Skeleton className="w-24 h-4" />
              <Skeleton className="w-16 h-4" />
            </div>
            <div className="flex justify-between">
              <Skeleton className="w-24 h-4" />
              <Skeleton className="w-16 h-4" />
            </div>
            <div className="flex justify-between">
              <Skeleton className="w-24 h-4" />
              <div className="text-right space-y-1">
                <Skeleton className="w-16 h-4" />
                <Skeleton className="w-32 h-3" />
              </div>
            </div>
            <div className="flex justify-between">
              <Skeleton className="w-24 h-4" />
              <div className="text-right space-y-1">
                <Skeleton className="w-16 h-4" />
                <Skeleton className="w-32 h-3" />
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <>
      <Card className="min-h-[calc(80vh_-_56px)] relative">
        <Button
          size="icon"
          variant="ghost"
          className="absolute top-1 left-1 h-6 w-6"
          onClick={() => route.back()}
        >
          <ArrowLeft className="h-3 w-3" />
          <span className="sr-only">{t("common.back")}</span>{" "}
          {/* Thêm dịch cho nút Back */}
        </Button>
        <Button
          size="icon"
          variant="ghost"
          className="absolute top-1 right-1 h-6 w-6"
          onClick={handleEdit}
        >
          <Pencil className="h-3 w-3" />
          <span className="sr-only">{t("common.edit")}</span>
        </Button>
        <CardContent className="p-4 h-full flex flex-col">
          <div className="flex flex-col items-center mb-4">
            <Image
              src={branchProduct?.image || "/logo/favicon.svg"}
              alt={t("image.alt", { entity: branchProduct?.name ?? "" })}
              width={400}
              height={400}
              className="object-cover mb-2 rounded"
            />
            <h1 className="text-lg font-semibold">{branchProduct?.name}</h1>
            <div
              className="text-sm text-muted-foreground text-center"
              dangerouslySetInnerHTML={{
                __html: branchProduct?.description || "--",
              }}
            />
          </div>

          <div className="space-y-3 text-sm">
            <div className="flex justify-between items-center">
              <span className="text-muted-foreground">{statusTitle}</span>
              <Badge
                variant={
                  branchProduct?.status === ActivationStatus.Active
                    ? "default"
                    : "destructive"
                }
                className="capitalize text-xs"
              >
                {t(
                  `common.status.${branchProduct?.status?.toLocaleLowerCase()}`
                )}
              </Badge>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">{codeTitle}</span>
              <span>{branchProduct?.sku || "--"}</span>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">{priceTitle}</span>
              <span>{formatPriceVN(branchProduct?.capitalPrice ?? 0)}</span>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {stockQuantityTitle}
              </span>
              <span>{branchProduct?.stockQuantity ?? 0}</span>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">{categoryTitle}</span>
              <span>{branchProduct?.category?.name || "--"}</span>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">{branchTitle}</span>
              <span>{branch?.branchName || "--"}</span>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">{createdByTitle}</span>
              <div className="text-right">
                <div>{branchProduct?.createdUser?.displayName || "--"}</div>
                <div className="text-xs text-muted-foreground">
                  {branchProduct?.createdUser?.email || "--"}
                </div>
              </div>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">{updatedByTitle}</span>
              <div className="text-right">
                <div>{branchProduct?.updatedUser?.displayName || "--"}</div>
                <div className="text-xs text-muted-foreground">
                  {branchProduct?.updatedUser?.email || "--"}
                </div>
              </div>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {t("common.created")}
              </span>
              <div className="text-right">
                <div>
                  {branchProduct?.createdAt
                    ? format(new Date(branchProduct.createdAt), "dd/MM/yyyy")
                    : "--"}
                </div>
                <div className="text-xs text-muted-foreground">
                  {branchProduct?.createdAt
                    ? formatDistance(new Date(branchProduct.createdAt))
                    : "--"}
                </div>
              </div>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {t("common.updated")}
              </span>
              <div className="text-right">
                <div>
                  {branchProduct?.updatedAt
                    ? format(new Date(branchProduct.updatedAt), "dd/MM/yyyy")
                    : "--"}
                </div>
                <div className="text-xs text-muted-foreground">
                  {branchProduct?.updatedAt
                    ? formatDistance(new Date(branchProduct.updatedAt))
                    : "--"}
                </div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
      {/* <BranchProductFormDialog
        isOpen={openEdit}
        initialData={{
          name: branchProduct.name!,
          description: branchProduct.description ?? "",
          status: branchProduct.status!,
          image: branchProduct.image as any,
          sku: branchProduct.sku ?? "", // Added SKU
          capitalPrice: branchProduct.capitalPrice ?? 0, // Added capitalPrice
          stockQuantity: branchProduct.stockQuantity ?? 0, // Added stockQuantity
          branchId: branchProduct.branchId ?? 0, // Added branchId
          categoryId: branchProduct.categoryId ?? 0, // Added categoryId
        }}
        onSubmit={handleUpdate}
        onClose={() => setOpenEdit(false)}
      /> */}
    </>
  );
};
