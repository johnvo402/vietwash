"use client";

import {
  CreateInventoryDocumentCommand,
  InventoryDocumentDetailResponse,
  InventoryStatus,
  InventoryType,
  // PaymentMethod,
} from "@/api/generated/api";
import {
  FormValues,
  InventoryDocumentFormDialog,
} from "../components/create-inventory-document";
import { useInventoryDocumentMutations } from "../hooks/use-inventory-document";
import { useEffect, useState } from "react";
import { apiClient } from "@/api/client";
import { useQuery } from "@tanstack/react-query";
import { format, parseISO } from "date-fns";
interface UpdateProps {
  publicId: string;
  type: InventoryType;
}
export default function UpdateInventoryDocumentPage({
  publicId,
  type,
}: UpdateProps) {
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(publicId);
    if (storedId) setId(Number(storedId));
  }, [publicId]);

  const { data: inventory, isLoading: getLoading } = useQuery<
    InventoryDocumentDetailResponse | undefined
  >({
    queryKey: ["inventory-document", id],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiInventoriesIdGet(id!);
      return response.data.results;
    },
    enabled: !!id,
  });
  const { updateInventoryDocument, isLoading } =
    useInventoryDocumentMutations(type);

  const handleSubmit = async (data: FormValues, isDraft: boolean) => {
    let formattedTransactionAt = data.transactionAt;
    if (formattedTransactionAt) {
      // Parse the input string and append the offset (e.g., +07:00 for your timezone)
      const date = parseISO(formattedTransactionAt);
      formattedTransactionAt = format(date, "yyyy-MM-dd'T'HH:mm:ssXXX"); // Adds offset, e.g., 2025-08-14T13:20:00+07:00
    }
    const command: CreateInventoryDocumentCommand = {
      branchId: data.branchId,
      type: type,
      note: data.note || undefined,
      transactionAt: formattedTransactionAt,
      productSupplyings: data.productSupplyings.map((ps) => ({
        productId: ps.productId,
        supplierId: ps.supplierId,
        quantity: type === InventoryType.Export ? -ps.quantity : ps.quantity,
        price: ps.price,
        unitRelationId: ps.unitRelationId,
      })),
      equipmentSupplyings: data.equipmentSupplyings.map((es) => ({
        name: es.name,
        code: es.code || undefined,
        quantity: es.quantity,
        price: es.price,
        supplierId: es.supplierId,
      })),
    };

    await updateInventoryDocument({
      oldId: id!,
      command: command,
    });
  };

  return (
    inventory && (
      <div className="container mx-auto p-4">
        <InventoryDocumentFormDialog
          onSubmit={handleSubmit}
          initialData={inventory as any}
          isLoading={isLoading || getLoading}
          type={type}
        />
      </div>
    )
  );
}
