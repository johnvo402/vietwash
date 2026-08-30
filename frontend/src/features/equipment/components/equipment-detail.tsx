import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Pencil, ArrowLeft } from "lucide-react";
import { format } from "date-fns";
import {
  ActivationStatus,
  GetEquipmentDetailResponse,
  MediaType,
} from "@/api/generated/api";
import { useTranslations } from "next-intl";
import { useRouter } from "nextjs-toploader/app";
import Image from "next/image";
import { Skeleton } from "@/components/ui/skeleton";
import { formatNumberVN, formatPriceVN } from "@/utils/format";
import { useStringUtil } from "@/lib/stringUtil";
import { useEquipmentMutations } from "../hooks/use-equipment";
import { useState } from "react";
import {
  EquipmentFormData,
  EquipmentFormDialog,
} from "./equipment-update-dialog";
import { apiClient } from "@/api/client";

interface EquipmentInformationProps {
  equipment?: GetEquipmentDetailResponse; // Made optional to handle loading state
  isLoading?: boolean; // New prop to indicate loading state
  refetch: () => void;
}

export const EquipmentInformation = ({
  equipment,
  isLoading = false,
  refetch,
}: EquipmentInformationProps) => {
  const t = useTranslations();
  const route = useRouter();
  const { formatDistance } = useStringUtil();
  const statusTitle = t("common.status.title");
  const priceTitle = t("product.capitalPrice");
  const codeTitle = t("table.accessorKey.code");
  const lastMaintenanceTitle = t("equipment.lastMaintenance");
  const nextMaintenanceTitle = t("equipment.nextMaintenance");
  const { updateEquipment } = useEquipmentMutations();
  const [openEdit, setOpenEdit] = useState<boolean>(false);
  const handleEdit = () => {
    if (equipment?.id) {
      setOpenEdit(true);
    }
  };

  const handleUpdate = async (data: EquipmentFormData) => {
    let image: string | File | null = data.image ?? null;

    if (image != null) {
      if (image instanceof File) {
        const response = await apiClient.authApiMediaPost(
          [image],
          MediaType.Image
        );
        image = response.data.results?.key?.[0] ?? "";
      }
    } else {
      image = null;
    }

    await updateEquipment({
      id: equipment?.id!,
      command: {
        name: data.name,
        description: data.description,
        status: data.status,
        image: image,
      },
    }).then(refetch);
  };

  if (isLoading || !equipment) {
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
          <span className="sr-only">Back</span>
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
              src={equipment?.image || "/logo/favicon.svg"}
              alt={equipment?.name ?? ""}
              width={400}
              height={400}
              className="object-cover mb-2 rounded"
            />
            <h1 className="text-lg font-semibold">{equipment?.name}</h1>
            <p className="text-sm text-muted-foreground text-center">
              {equipment?.description}
            </p>
          </div>

          <div className="space-y-3 text-sm">
            <div className="flex justify-between items-center">
              <span className="text-muted-foreground">{statusTitle}</span>
              <Badge
                variant={
                  equipment?.status === ActivationStatus.Active
                    ? "default"
                    : "destructive"
                }
                className="capitalize text-xs"
              >
                {t(`common.status.${equipment?.status?.toLocaleLowerCase()}`)}
              </Badge>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">{codeTitle}</span>
              <span>{equipment?.code || "--"}</span>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">{priceTitle}</span>
              <span>{formatPriceVN(equipment?.price ?? 0)}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {t("equipment.used")}
              </span>
              <span>{formatNumberVN(equipment?.numberOfUses ?? 0)}</span>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {lastMaintenanceTitle}
              </span>
              <div className="text-right">
                <div>
                  {equipment?.lastMaintenanceOrRepairDate
                    ? format(
                        new Date(equipment.lastMaintenanceOrRepairDate),
                        "dd/MM/yyyy"
                      )
                    : "--"}
                </div>
                <div className="text-xs text-muted-foreground">
                  {equipment?.lastMaintenanceOrRepairDate
                    ? formatDistance(
                        new Date(equipment.lastMaintenanceOrRepairDate)
                      )
                    : "--"}
                </div>
              </div>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {nextMaintenanceTitle}
              </span>
              <div className="text-right">
                <div>
                  {equipment?.nextMaintenanceDate
                    ? format(
                        new Date(equipment.nextMaintenanceDate),
                        "dd/MM/yyyy"
                      )
                    : "--"}
                </div>
                <div className="text-xs text-muted-foreground">
                  {equipment?.nextMaintenanceDate
                    ? formatDistance(new Date(equipment.nextMaintenanceDate))
                    : "--"}
                </div>
              </div>
            </div>

            <div className="flex justify-between">
              <span className="text-muted-foreground">
                {t("common.created")}
              </span>
              <div className="text-right">
                <div>
                  {equipment?.createdAt
                    ? format(new Date(equipment.createdAt), "dd/MM/yyyy")
                    : "--"}
                </div>
                <div className="text-xs text-muted-foreground">
                  {equipment?.createdAt
                    ? formatDistance(new Date(equipment.createdAt))
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
                  {equipment?.updatedAt
                    ? format(new Date(equipment.updatedAt), "dd/MM/yyyy")
                    : "--"}
                </div>
                <div className="text-xs text-muted-foreground">
                  {equipment?.updatedAt
                    ? formatDistance(new Date(equipment.updatedAt))
                    : "--"}
                </div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
      {openEdit && equipment && (
        <EquipmentFormDialog
          isOpen={openEdit}
          initialData={{
            name: equipment.name!,
            description: equipment.description ?? "",
            status: equipment.status!,
            image: equipment.image as any,
          }}
          onSubmit={handleUpdate}
          onClose={() => setOpenEdit(false)}
        />
      )}
    </>
  );
};
