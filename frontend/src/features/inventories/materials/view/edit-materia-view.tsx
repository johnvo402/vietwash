"use client";

import {
  BranchProductDialog,
  FormValues,
} from "../components/create-material-dialog";
import { useBranchProductMutations } from "../hooks/use-material";
import { useTranslations } from "next-intl";
import { useMutation, useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { MediaType } from "@/api/generated/api";
import { toast } from "react-toastify";
import { useEffect, useState } from "react";

interface DetailProps {
  params: { publicId: string };
}

export default function MaterialPageUpdate({ params }: DetailProps) {
  const { updateBranchProduct } = useBranchProductMutations();
  const t = useTranslations();
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(params.publicId);
    if (storedId) setId(Number(storedId));
  }, [params.publicId]);

  const { data } = useQuery({
    queryKey: ["branchProduct", id],
    queryFn: async () => {
      if (id === null) return undefined;
      const response = await apiClient.ecommerceApiBranchProductsIdGet(id);
      return response.data.results;
    },
    enabled: id !== null,
  });
  const handleError = (title: string) => () => {
    toast.error(title);
  };

  const updateProductMutation = useMutation({
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
    onSuccess: async (imageKey, data) => {
      try {
        await updateBranchProduct({
          id: id!,
          productData: {
            ...data,
            image: imageKey,
          },
        });
      } catch (error: any) {}
    },
    onError: handleError(
      t("toast.upload.failed", {
        entity: t("common.image").replace(/^./, (c) => c.toLowerCase()),
      })
    ),
  });

  async function handleUpdate(data: FormValues) {
    try {
      await updateProductMutation.mutateAsync(data);
    } catch (error) {
      console.error("Error updating product:", error);
    }
  }

  return (
    data && (
      <BranchProductDialog
        onSubmit={handleUpdate}
        initialData={data as any}
        isUpdate
      />
    )
  );
}
