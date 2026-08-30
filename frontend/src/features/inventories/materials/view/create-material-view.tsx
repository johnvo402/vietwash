"use client";

import {
  BranchProductDialog,
  FormValues,
} from "../components/create-material-dialog";
import { useBranchProductMutations } from "../hooks/use-material";
import { useTranslations } from "next-intl";
import { useMutation } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { MediaType } from "@/api/generated/api";
import { toast } from "react-toastify";

export default function MaterialPageCreate() {
  const { createBranchProduct } = useBranchProductMutations();
  const t = useTranslations();

  const handleError = (title: string) => () => {
    toast.error(title);
  };
  const createUserMutation = useMutation({
    mutationFn: async (data: FormValues) => {
      if (data.image instanceof File) {
        const response = await apiClient.authApiMediaPost(
          [data.image],
          MediaType.Image
        );
        return response.data.results?.key?.[0];
      }
      return data.image;
    },
    onSuccess: async (response, data) => {
      try {
        await createBranchProduct({
          ...data,
          image: response,
        });
      } catch (error: any) {}
    },
    onError: handleError(
      t("toast.upload.failed", {
        entity: t("common.image").replace(/^./, (c) => c.toLowerCase()),
      })
    ),
  });
  async function handleCreateSupplier(data: FormValues) {
    try {
      await createUserMutation.mutateAsync(data);
    } catch (error) {
      console.error("Error creating user:", error);
      throw error;
    }
  }

  return <BranchProductDialog onSubmit={handleCreateSupplier} />;
}
