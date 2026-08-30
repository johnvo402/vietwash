"use client";

import { useMutation, useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import {
  ServiceDialog,
  FormValues,
} from "./components/service-create/create-service-dialog";
import { ROUTE_SERVICE } from "@/types/router-type";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { useAuth } from "@/hooks/use-auth";
import {
  ActivationStatus,
  GetServiceDetailResponse,
  MediaType,
} from "@/api/generated";
import { Loader2 } from "lucide-react";
import { toast } from "react-toastify";

export default function ServicePageUpdate({ publicId }: { publicId: string }) {
  const router = useRouter();
  const t = useTranslations();
  const { branchActive } = useAuth();
  const [image, setImage] = useState<File | null>(null);
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(publicId);
    if (storedId) setId(Number(storedId));
  }, [publicId]);

  const handleError = (title: string) => () => {
    toast.error(title);
  };
  const { data: service, isLoading } = useQuery<GetServiceDetailResponse>({
    queryKey: ["service", id],
    queryFn: async () => {
      if (!id)
        throw new Error(
          t("common.idRequired", {
            Entity: t("common.user").replace(/^./, (c) => c.toUpperCase()),
          })
        );
      const response = await apiClient.ecommerceApiServicesDetailIdGet(id);
      const data = response.data.results;
      return data!;
    },
    enabled: !!id,
  });

  const createServiceMutation = useMutation({
    mutationFn: async (serviceData: FormValues) => {
      if (!image) {
        return null;
      }
      const response = await apiClient.authApiMediaPost(
        [image],
        MediaType.Image
      );
      return response.data.results?.key?.[0];
    },
    onSuccess: async (response, serviceData) => {
      try {
        await apiClient.ecommerceApiServicesIdPut(id!, {
          branchId: branchActive?.branchId!,
          categoryId: serviceData.categoryId,
          description: serviceData.description,
          name: serviceData.name,
          status: serviceData.status as ActivationStatus,
          unitRelations: serviceData.unitRelations as any,
          image: response || service?.image,
        });
      } catch (error: any) {
        handleError(t("toast.update.failed", { entity: t("common.service") }));
      } finally {
        toast.info(t("toast.update.success", { entity: t("common.service") }));
        router.push(ROUTE_SERVICE);
      }
    },
    onError: handleError(
      t("toast.upload.failed", {
        entity: t("common.image").replace(/^./, (c) => c.toLowerCase()),
      })
    ),
  });

  const handleCreateService = async (formData: FormValues) => {
    await createServiceMutation
      .mutateAsync(formData)
      .catch((error) => console.error("Error creating user:", error));
  };

  return service && !isLoading ? (
    <ServiceDialog
      image={async (data) => {
        setImage(data ? data : null);
      }}
      onSubmit={handleCreateService}
      initialData={service as FormValues}
      isUpdate
    />
  ) : (
    <div className="w-full flex justify-center">
      <Loader2 className="animate-spin"></Loader2>
    </div>
  );
}
