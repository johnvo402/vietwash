"use client";

import { useMutation, useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import {
  ServiceDialog,
  FormValues,
} from "./components/service-create/create-service-dialog";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useTranslations } from "next-intl";
import { useAuth } from "@/hooks/use-auth"; // Adjust the import path
import { MediaType } from "@/api/generated/api";
import { toast } from "react-toastify";

export default function ServicePageCreate() {
  const router = useRouter();
  const t = useTranslations();
  const { branchActive } = useAuth();
  const [image, setImage] = useState<File | null>(null);

  const handleError = (title: string) => () => {
    toast.error(title);
  };

  const createUserMutation = useMutation({
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
        await apiClient.ecommerceApiServicesPost({
          branchId: branchActive?.branchId!,
          categoryId: serviceData.categoryId,
          description: serviceData.description,
          name: serviceData.name,
          unitRelations: serviceData.unitRelations as any,
          status: serviceData.status,
          image: response,
        });
      } catch (error: any) {
        handleError(t("toast.create.failed", { entity: t("common.service") }));
      } finally {
        toast.info(t("toast.create.success", { entity: t("common.service") }));
        router.back();
      }
    },
    onError: handleError(
      t("toast.upload.failed", {
        entity: t("common.image").replace(/^./, (c) => c.toLowerCase()),
      })
    ),
  });

  const handleCreateUser = async (formData: FormValues) => {
    await createUserMutation
      .mutateAsync(formData)
      .catch((error) => console.error("Error creating user:", error));
  };

  return (
    <ServiceDialog
      image={async (data) => {
        setImage(data ? data : null);
      }}
      onSubmit={handleCreateUser}
    />
  );
}
