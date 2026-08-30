"use client";

import { usePushRouter } from "@/utils/router-utli";
import { useSearchParams } from "next/navigation";
import {
  SupplierModel,
  GetSupplierDetailResponse,
  ActivationStatus,
} from "@/api/generated";
import { useSupplierMutations } from "./hooks/use-supplier";
import {
  CreateSupplierDialog,
  FormValues,
} from "./components/create-supplier-dialog";
import { useEffect, useState } from "react";
import { apiClient } from "@/api/client";
import { useQuery } from "@tanstack/react-query";
import { ROUTE_SUPPLIER } from "@/types/router-type";

export default function SupplierPageUpdate({ publicId }: { publicId: string }) {
  const pushRouter = usePushRouter();
  const { updateSupplier, isLoading: updateLoading } = useSupplierMutations();
  const searchParams = useSearchParams();
  const params = Object.fromEntries(searchParams.entries());
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(publicId);
    if (storedId) setId(Number(storedId));
  }, [publicId]);

  const {
    data: supplier,
    isLoading,
    error,
  } = useQuery<GetSupplierDetailResponse | undefined>({
    queryKey: ["supplier", id],
    queryFn: async () => {
      if (!id) throw new Error("Supplier ID is required");
      const response = await apiClient.ecommerceApiSuppliersDetailIdGet(id);
      return response.data.results;
    },
    enabled: !!id,
  });

  async function handleUpdateSupplier(
    data: FormValues & { id: number },
    formData: FormData
  ) {
    try {
      const updateAccountCommand: SupplierModel = {
        name: formData.get("name") as string,
        email: formData.get("email") as string,
        phone: formData.get("phone") as string | undefined,
        address: formData.get("address") as string | undefined,
        status: formData.get("status") as ActivationStatus,
        description: formData.get("description") as string | undefined,
        code: formData.get("code") as string | undefined,
      };
      await updateSupplier({ id: data.id, supplierData: updateAccountCommand });
      onClose();
    } catch (error) {
      console.error("Error updating supplier:", error);
      throw error; // Optionally, handle this with a UI notification
    }
  }

  const onClose = () => {
    pushRouter.pushRouter({
      router: ROUTE_SUPPLIER,
      query: params,
    });
  };

  return (
    <CreateSupplierDialog
      onClose={onClose}
      open={true}
      onUpdateSupplier={handleUpdateSupplier}
      supplier={supplier}
      isLoading={isLoading || updateLoading}
    />
  );
}
