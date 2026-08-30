"use client";

import { ROUTE_SUPPLIER } from "@/types/router-type";
import { usePushRouter } from "@/utils/router-utli";
import { useSearchParams } from "next/navigation";
import { CreateSupplierCommand, ActivationStatus } from "@/api/generated";
import { useSupplierMutations } from "./hooks/use-supplier";
import { CreateSupplierDialog } from "./components/create-supplier-dialog";

export default function SupplierPageCreate() {
  const pushRouter = usePushRouter();
  const { createSupplier, isLoading } = useSupplierMutations();
  const searchParams = useSearchParams();
  const params = Object.fromEntries(searchParams.entries());

  async function handleCreateSupplier(data: { supplier: FormData }) {
    try {
      const formData = Object.fromEntries(data.supplier);
      const createAccountCommand: CreateSupplierCommand = {
        name: formData.name as string,
        email: formData.email as string,
        phone: formData.phone as string | undefined,
        address: formData.address as string | undefined,
        status: formData.status as ActivationStatus,
        description: formData.description as string | undefined,
        code: formData.code as string | undefined,
      };
      await createSupplier(createAccountCommand);
      onClose();
    } catch (error) {
      console.error("Error creating user:", error);
      throw error;
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
      onCreateSupplier={handleCreateSupplier}
      isLoading={isLoading}
    />
  );
}
