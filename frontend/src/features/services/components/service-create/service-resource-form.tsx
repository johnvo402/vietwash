import { useCallback } from "react";
import { useTranslations } from "next-intl";
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Trash2, Plus } from "lucide-react";
import { UseFormReturn } from "react-hook-form";
import { FormValues } from "./create-service-dialog";
import ProductSelect from "@/features/inventories/imports/components/product-product";
import {
  ListBranchProductResponse,
  UnitRelationProjection,
} from "@/api/generated";

interface ServiceResourcesFormProps {
  form: UseFormReturn<FormValues>;
  unitIndex: number;
  products: ListBranchProductResponse[];
  isProductsLoading: boolean;
  productsError: any;
  fetchNextProducts: () => void;
  hasMoreProducts: boolean | undefined;
  setProductSearch: (search: string) => void;
  resFields: { id: string }[];
  appendRes: (v: {
    productId: number;
    unitProductId: number;
    quantity: number;
  }) => void;
  removeRes: (index: number) => void;
}

export function ServiceResourcesForm({
  form,
  unitIndex,
  products,
  isProductsLoading,
  productsError,
  fetchNextProducts,
  hasMoreProducts,
  setProductSearch,
  resFields,
  appendRes,
  removeRes,
}: ServiceResourcesFormProps) {
  const t = useTranslations();

  const addServiceResource = useCallback(() => {
    appendRes({ productId: 0, unitProductId: 0, quantity: 1 });
  }, [appendRes]);

  const removeServiceResource = useCallback(
    (resourceIndex: number) => {
      const current = form.getValues(
        `unitRelations.${unitIndex}.serviceResources`
      );
      if (current && current.length > 1) removeRes(resourceIndex);
    },
    [form, unitIndex, removeRes]
  );

  const handleProductChange = (value: string, resourceIndex: number) => {
    const productId = Number(value);
    form.setValue(
      `unitRelations.${unitIndex}.serviceResources.${resourceIndex}.productId`,
      productId,
      { shouldValidate: false, shouldDirty: true }
    );
    form.setValue(
      `unitRelations.${unitIndex}.serviceResources.${resourceIndex}.unitProductId`,
      0,
      { shouldValidate: false, shouldDirty: true }
    );
  };

  return (
    <div>
      <div className="flex items-center justify-between my-2">
        <FormLabel>{t("service.resources.title")}</FormLabel>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={addServiceResource}
        >
          <Plus className="h-4 w-4 mr-2" />
          {t("service.resources.add")}
        </Button>
      </div>

      {resFields.map((rf, resourceIndex) => {
        const resource = form.getValues(
          `unitRelations.${unitIndex}.serviceResources.${resourceIndex}`
        );
        const selectedProduct = products.find(
          (p) => p.id === resource?.productId
        );
        const unitOptions: UnitRelationProjection[] =
          (selectedProduct?.unitRelations as any) ?? [];
        const unitDisabled = !selectedProduct || unitOptions.length === 0;

        // Làm sạch unitProductId để loại bỏ ký tự "~" hoặc định dạng không mong muốn
        const cleanUnitProductId = String(
          resource?.unitProductId || ""
        ).replace(/[~]/g, "");
        // Tìm đơn vị được chọn từ unitOptions dựa trên unitProductId đã làm sạch
        const selectedUnit = unitOptions.find(
          (u) => String(u.id) === cleanUnitProductId
        );

        // Debug log để kiểm tra dữ liệu
        console.log("Resource:", resource);
        console.log("Selected Product:", selectedProduct);
        console.log("Unit Options:", unitOptions);
        console.log("Clean Unit Product ID:", cleanUnitProductId);
        console.log("Selected Unit:", selectedUnit);

        return (
          <div
            key={rf.id}
            className="flex flex-col gap-2 border p-2 md:flex-row md:items-center md:gap-3"
          >
            {/* Product */}
            <FormField
              control={form.control}
              name={`unitRelations.${unitIndex}.serviceResources.${resourceIndex}.productId`}
              render={({ field }) => (
                <FormControl className="flex-1 min-w-[200px]">
                  <ProductSelect
                    options={products}
                    value={field.value ? String(field.value) : "0"}
                    onChange={(val) => handleProductChange(val, resourceIndex)}
                    onSearch={setProductSearch}
                    placeholder={t("common.placeholderSelect", {
                      entity: t("common.product"),
                    })}
                    isLoading={isProductsLoading}
                    error={
                      productsError ? "Failed to load products" : undefined
                    }
                    fetchNextPage={fetchNextProducts}
                    hasNextPage={hasMoreProducts}
                  />
                </FormControl>
              )}
            />

            {/* Unit (dependent on selected product) */}
            <FormField
              control={form.control}
              name={`unitRelations.${unitIndex}.serviceResources.${resourceIndex}.unitProductId`}
              render={({ field }) => (
                <FormItem className="min-w-[180px]">
                  <FormControl>
                    <Select
                      onValueChange={(value) => field.onChange(Number(value))}
                      value={field.value ? String(field.value) : ""}
                      disabled={unitDisabled}
                    >
                      <SelectTrigger>
                        <SelectValue
                          placeholder={t("common.placeholderSelect", {
                            entity: t("common.unit"),
                          })}
                        >
                          {field.value && !selectedUnit
                            ? isProductsLoading
                              ? t("common.loadingUnit") // Hiển thị khi đang tải
                              : t("common.unitNotFound") // Hiển thị khi không tìm thấy unit
                            : selectedUnit?.name}{" "}
                          {/* Hiển thị tên unit nếu tìm thấy */}
                        </SelectValue>
                      </SelectTrigger>
                      <SelectContent className="max-h-64">
                        {unitOptions.map((u) => (
                          <SelectItem key={u.id} value={String(u.id)}>
                            {u.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Quantity */}
            <FormField
              control={form.control}
              name={`unitRelations.${unitIndex}.serviceResources.${resourceIndex}.quantity`}
              render={({ field }) => (
                <FormItem className="w-[140px] space-y-0">
                  <FormLabel className="sr-only">
                    {t("table.accessorKey.quantity")}
                  </FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      min={0.001}
                      step="any" // <-- quan trọng
                      value={field.value}
                      onChange={(e) => {
                        const raw = e.target.value.trim();
                        if (raw === "") {
                          field.onChange("");
                          return;
                        }
                        const val = parseFloat(raw);
                        if (!Number.isNaN(val))
                          field.onChange(Math.max(0.001, val));
                      }}
                      onBlur={(e) => {
                        const val = parseFloat(e.target.value || "0.001");
                        field.onChange(
                          Number.isNaN(val) ? 0.001 : Math.max(0.001, val)
                        );
                      }}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Delete */}
            <div className="flex items-center md:ml-auto">
              <Button
                type="button"
                variant="ghost"
                size="icon"
                onClick={() => removeServiceResource(resourceIndex)}
                disabled={
                  (
                    form.getValues(
                      `unitRelations.${unitIndex}.serviceResources`
                    ) ?? []
                  ).length <= 1
                }
              >
                <Trash2 className="h-4 w-4 text-destructive" />
              </Button>
            </div>
          </div>
        );
      })}
    </div>
  );
}
