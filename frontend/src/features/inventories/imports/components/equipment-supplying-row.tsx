import {
  FormControl,
  FormField,
  FormItem,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import {
  Trash2,
  PlusCircle,
  MinusCircle,
  Image as ImageIcon,
} from "lucide-react";
import { useTranslations } from "next-intl";
import SearchableSelect from "./search-product";
import { formatNumberVN } from "@/utils/format";
import Image from "next/image";
import { useState, useCallback, useEffect } from "react";
import { useWatch } from "react-hook-form";

interface EquipmentSupplyingRowProps {
  index: number;
  form: any;
  supplierOptions: any[];
  isSuppliersLoading: boolean;
  suppliersError: any;
  fetchNextSuppliers: () => void;
  hasMoreSuppliers: boolean;
  setSupplierSearch: (value: string) => void;
  removeEquipment: (index: number) => void;
}

export default function EquipmentSupplyingRow({
  index,
  form,
  supplierOptions,
  isSuppliersLoading,
  suppliersError,
  fetchNextSuppliers,
  hasMoreSuppliers,
  setSupplierSearch,
  removeEquipment,
}: EquipmentSupplyingRowProps) {
  const t = useTranslations();
  const [imagePreview, setImagePreview] = useState<string | null>(null);

  // Theo dõi giá trị code và image
  const code = useWatch({
    control: form.control,
    name: `equipmentSupplyings.${index}.code`,
    defaultValue: "",
  });

  const currentImage = useWatch({
    control: form.control,
    name: `equipmentSupplyings.${index}.image`,
  });

  const handleIncrement = (field: any) => {
    const currentValue = Number(field.value) || 0;
    field.onChange(currentValue + 1);
  };

  const handleDecrement = (field: any, min: number = 0) => {
    const currentValue = Number(field.value) || 0;
    if (currentValue > min) field.onChange(currentValue - 1);
  };

  const sanitizeFileName = (name: string) => {
    // Loại bỏ ký tự không hợp lệ cho tên file và thay khoảng trắng bằng dấu gạch dưới
    return name
      .replace(/[^a-zA-Z0-9._-]/g, "")
      .replace(/\s+/g, "_")
      .trim();
  };

  // Xử lý khi tải ảnh lên
  const handleImageChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>, field: any) => {
      const file = e.target.files?.[0];
      if (file) {
        // Lấy code từ form
        const fileExtension = file.name.split(".").pop() || "jpg";
        const newFileName = code
          ? `${sanitizeFileName(code)}.${fileExtension}`
          : `equipment_${index + 1}.${fileExtension}`;

        // Tạo đối tượng File mới với tên dựa trên code
        const renamedFile = new File([file], newFileName, { type: file.type });

        // Tạo URL xem trước
        const previewUrl = URL.createObjectURL(renamedFile);
        setImagePreview(previewUrl);
        field.onChange(renamedFile);

        // Kiểm tra tổng kích thước ảnh
        const totalSize = form
          .getValues("equipmentSupplyings")
          .reduce((sum: number, equipment: any) => {
            const size =
              equipment.image instanceof File ? equipment.image.size : 0;
            return sum + size;
          }, renamedFile.size);

        // Kiểm tra giới hạn tổng kích thước
        if (totalSize > 200 * 1024 * 1024) {
          form.setError(`equipmentSupplyings.${index}.image`, {
            type: "manual",
            message: t(
              "inventory.equipmentSupplyings.validation.totalImageSize",
              {
                max: "200MB",
              },
            ),
          });
          setImagePreview(null);
          field.onChange(null);
        }
      } else {
        setImagePreview(null);
        field.onChange(null);
      }
    },
    [form, index, code, t],
  );

  // Xử lý khi xóa ảnh
  const handleRemoveImage = useCallback((field: any) => {
    setImagePreview(null);
    field.onChange(null);
  }, []);

  // Cập nhật tên file ảnh khi code thay đổi
  useEffect(() => {
    if (currentImage instanceof File) {
      const fileExtension = currentImage.name.split(".").pop() || "jpg";
      const newFileName = code
        ? `${sanitizeFileName(code)}.${fileExtension}`
        : `equipment_${index + 1}.${fileExtension}`;

      // Chỉ cập nhật nếu tên file mới khác với tên file hiện tại
      if (newFileName !== currentImage.name) {
        // Tạo đối tượng File mới với tên mới
        const renamedFile = new File([currentImage], newFileName, {
          type: currentImage.type,
        });

        // Cập nhật giá trị image trong form
        form.setValue(`equipmentSupplyings.${index}.image`, renamedFile, {
          shouldValidate: true,
        });

        // Cập nhật URL xem trước
        const newPreviewUrl = URL.createObjectURL(renamedFile);
        setImagePreview((prev) => {
          if (prev) URL.revokeObjectURL(prev);
          return newPreviewUrl;
        });
      }
    } else if (currentImage != null) {
      setImagePreview(currentImage);
    }
  }, [code, currentImage, form, index]);

  // Dọn dẹp URL xem trước khi component unmount
  useEffect(() => {
    return () => {
      if (imagePreview) {
        URL.revokeObjectURL(imagePreview);
      }
    };
  }, [imagePreview]);

  return (
    <tr className="border-b">
      <td className="p-2">
        <FormField
          control={form.control}
          name={`equipmentSupplyings.${index}.name`}
          render={({ field }) => (
            <FormItem>
              <FormControl>
                <Input
                  placeholder={t(
                    "inventory.equipmentSupplyings.placeholder.name",
                  )}
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </td>
      <td className="p-2">
        <FormField
          control={form.control}
          name={`equipmentSupplyings.${index}.code`}
          render={({ field }) => (
            <FormItem>
              <FormControl>
                <Input
                  placeholder={t(
                    "inventory.equipmentSupplyings.placeholder.code",
                  )}
                  {...field}
                  value={field.value || ""}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </td>
      <td className="p-2">
        <FormField
          control={form.control}
          name={`equipmentSupplyings.${index}.image`}
          render={({ field }) => (
            <FormItem>
              <FormControl>
                <div className="flex items-center space-x-2">
                  {imagePreview ? (
                    <div className="relative w-16 h-16">
                      <Image
                        src={imagePreview}
                        alt="Xem trước thiết bị"
                        layout="fill"
                        objectFit="cover"
                        className="rounded"
                      />
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className="absolute h-8 w-8 -top-4 -right-4 bg-destructive text-white after:absolute after:-inset-2"
                        onClick={() => handleRemoveImage(field)}
                        aria-label={t("common.removeItem", {
                          item: t("common.image"),
                        })}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  ) : (
                    <Button
                      type="button"
                      variant="outline"
                      size="icon"
                      className="bg-background hover:bg-primary-foreground rounded-lg"
                      asChild
                      aria-label={t("common.upload")}
                    >
                      <label>
                        <ImageIcon className="h-4 w-4" />
                        <Input
                          type="file"
                          accept="image/*"
                          className="hidden"
                          onChange={(e) => handleImageChange(e, field)}
                        />
                      </label>
                    </Button>
                  )}
                </div>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </td>
      <td className="p-2 flex justify-center">
        <FormField
          control={form.control}
          name={`equipmentSupplyings.${index}.quantity`}
          render={({ field }) => (
            <FormItem>
              <FormControl>
                <div className="flex items-center space-x-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="icon"
                    onClick={() => handleDecrement(field, 1)}
                    className="h-11 w-11 bg-background hover:bg-primary-foreground rounded-lg"
                    aria-label={t("common.decreaseQuantity", {
                      item: t("common.equipment"),
                    })}
                  >
                    <MinusCircle className="h-4 w-4" />
                  </Button>
                  <Input
                    type="number"
                    className="w-24 text-center"
                    min="1"
                    step="1"
                    placeholder="0"
                    {...field}
                    onChange={(e) =>
                      field.onChange(parseInt(e.target.value) || 0)
                    }
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="icon"
                    onClick={() => handleIncrement(field)}
                    className="h-11 w-11 bg-background hover:bg-primary-foreground rounded-lg"
                    aria-label={t("common.increaseQuantity", {
                      item: t("common.equipment"),
                    })}
                  >
                    <PlusCircle className="h-4 w-4" />
                  </Button>
                </div>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </td>
      <td className="p-2">
        <FormField
          control={form.control}
          name={`equipmentSupplyings.${index}.price`}
          render={({ field }) => (
            <FormItem>
              <FormControl>
                <Input
                  type="text"
                  className="text-center"
                  value={formatNumberVN(field.value)}
                  placeholder="0.00"
                  onChange={(e) => {
                    const val = e.target.value.replace(/\D/g, "");
                    field.onChange(Number(val));
                  }}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </td>
      <td className="p-2">
        <FormField
          control={form.control}
          name={`equipmentSupplyings.${index}.supplierId`}
          render={({ field }) => (
            <FormItem>
              <FormControl>
                <SearchableSelect
                  options={supplierOptions}
                  value={field.value ? field.value.toString() : "0"}
                  onChange={(value: string) => field.onChange(Number(value))}
                  onSearch={setSupplierSearch}
                  placeholder={t("common.placeholderSelect", {
                    entity: t("common.supplier"),
                  })}
                  isLoading={isSuppliersLoading}
                  error={
                    suppliersError
                      ? "Không thể tải danh sách nhà cung cấp"
                      : undefined
                  }
                  fetchNextPage={fetchNextSuppliers}
                  hasNextPage={hasMoreSuppliers}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </td>
      <td className="p-2">
        <Button
          type="button"
          variant="ghost"
          size="icon"
          onClick={() => removeEquipment(index)}
          className="h-11 w-11"
          aria-label={t("common.removeItem", {
            item: t("common.equipment"),
          })}
        >
          <Trash2 className="h-4 w-4 text-destructive" />
        </Button>
      </td>
    </tr>
  );
}
