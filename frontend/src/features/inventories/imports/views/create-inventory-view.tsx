"use client";

import {
  CreateInventoryDocumentCommand,
  InventoryStatus,
  InventoryType,
  MediaType,
  // PaymentMethod,
} from "@/api/generated/api";
import {
  FormValues,
  InventoryDocumentFormDialog,
} from "../components/create-inventory-document";
import { useInventoryDocumentMutations } from "../hooks/use-inventory-document";
import { apiClient } from "@/api/client";
import { format, parseISO } from "date-fns";

export default function CreateInventoryDocumentPage({
  type,
}: {
  type: InventoryType;
}) {
  const { createInventoryDocument, updateStatusInventoryDocument, isLoading } =
    useInventoryDocumentMutations(type);

  const handleSubmit = async (data: FormValues, isDraft: boolean) => {
    let formattedTransactionAt = data.transactionAt;
    if (formattedTransactionAt) {
      // Parse the input string and append the offset (e.g., +07:00 for your timezone)
      const date = parseISO(formattedTransactionAt);
      formattedTransactionAt = format(date, "yyyy-MM-dd'T'HH:mm:ssXXX"); // Adds offset, e.g., 2025-08-14T13:20:00+07:00
    }
    const imagesWithCodes = data.equipmentSupplyings
      .map((es, index) => ({
        image: es.image instanceof File ? es.image : null,
        code: es.code || `equipment_${index + 1}`,
        index,
      }))
      .filter((item) => item.image !== null) as {
      image: File;
      code: string;
      index: number;
    }[];

    // Tải ảnh lên API và lấy danh sách khóa
    let imageKeys: string[] = [];
    if (imagesWithCodes.length > 0) {
      const images = imagesWithCodes.map((item) => item.image);
      const response = await apiClient.authApiMediaPost(
        images,
        MediaType.Image
      );
      imageKeys = response.data.results?.key ?? []; // Giả sử API trả về mảng các khóa, ví dụ: ["EQUIP-001", "equipment_2", ...]
    }

    // Ánh xạ khóa với code
    const imageKeyMap = new Map<string, string>();
    imagesWithCodes.forEach((item, idx) => {
      if (imageKeys[idx]) {
        imageKeyMap.set(item.code, imageKeys[idx]);
      }
    });
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
      equipmentSupplyings: data.equipmentSupplyings.map((es, index) => ({
        name: es.name,
        code: es.code || undefined,
        quantity: es.quantity,
        price: es.price,
        supplierId: es.supplierId,
        image:
          es.image instanceof File
            ? imageKeyMap.get(es.code || `equipment_${index + 1}`) || undefined
            : es.image || undefined,
      })),
    };

    const response = await createInventoryDocument({
      command: command,
      isNoti: isDraft,
    });
    if (!isDraft) {
      await updateStatusInventoryDocument({
        id: response.results?.id!,
        command: { status: InventoryStatus.Completed },
      });
    }
  };

  return (
    <div className="container mx-auto p-4">
      <InventoryDocumentFormDialog
        onSubmit={handleSubmit}
        isLoading={isLoading}
        type={type}
      />
    </div>
  );
}
