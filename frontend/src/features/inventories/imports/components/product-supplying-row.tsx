import {
  FormControl,
  FormField,
  FormItem,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Trash2, PlusCircle, MinusCircle } from "lucide-react";
import { useTranslations } from "next-intl";
import SearchableSelect from "./search-product";
import ProductSelect from "./product-product";
import { formatNumberVN } from "@/utils/format";
import {
  ListBranchProductResponse,
  UnitRelationProjection,
} from "@/api/generated";
import { useState } from "react";

interface ProductSupplyingRowProps {
  index: number;
  form: any;
  products: ListBranchProductResponse[];
  supplierOptions: any[];
  isProductsLoading: boolean;
  productsError: any;
  fetchNextProducts: () => void;
  hasMoreProducts: boolean;
  setProductSearch: (value: string) => void;
  setSupplierSearch: (value: string) => void;
  isSuppliersLoading: boolean;
  suppliersError: any;
  fetchNextSuppliers: () => void;
  hasMoreSuppliers: boolean;
  removeProduct: (index: number) => void;
}

export default function ProductSupplyingRow({
  index,
  form,
  products,
  supplierOptions,
  isProductsLoading,
  productsError,
  fetchNextProducts,
  hasMoreProducts,
  setProductSearch,
  setSupplierSearch,
  isSuppliersLoading,
  suppliersError,
  fetchNextSuppliers,
  hasMoreSuppliers,
  removeProduct,
}: ProductSupplyingRowProps) {
  const t = useTranslations();
  const [stock, setStock] = useState(0);
  const getUnitRelationsForProduct = (
    productId: number
  ): UnitRelationProjection[] =>
    products.find((p) => p.id === productId)?.unitRelations || [];

  const units = getUnitRelationsForProduct(
    form.getValues(`productSupplyings.${index}.productId`)
  );

  const handleIncrement = (field: any) => {
    const currentValue = Number(field.value) || 0;
    field.onChange(currentValue + 1);
  };

  const handleDecrement = (field: any, min: number = 0) => {
    const currentValue = Number(field.value) || 0;
    if (currentValue > min) field.onChange(currentValue - 1);
  };

  const handleProductChange = (value: string) => {
    const productId = Number(value);
    const selectedProduct = products.find((p) => p.id === productId);
    const selectedUnits = getUnitRelationsForProduct(productId);
    const defaultUnit =
      selectedUnits.find((unit) => unit.baseUnit) || selectedUnits[0];
    setStock(selectedProduct?.stockQuantity ?? 0);
    // Set productId
    form.setValue(`productSupplyings.${index}.productId`, productId, {
      shouldValidate: false,
    });

    // Only set unitRelationId if a valid defaultUnit exists
    form.setValue(`productSupplyings.${index}.unitRelationId`, defaultUnit.id, {
      shouldValidate: false,
    });

    // Set capitalPrice from the selected product
    form.setValue(
      `productSupplyings.${index}.price`,
      selectedProduct?.capitalPrice || 0,
      { shouldValidate: false }
    );

    // Trigger validation after setting values
    form.trigger(`productSupplyings.${index}`);
  };

  return (
    <tr className="border-b">
      <td className="p-2">
        <FormField
          control={form.control}
          name={`productSupplyings.${index}.productId`}
          render={({ field }) => (
            <FormControl>
              <ProductSelect
                options={products}
                value={field.value ? field.value.toString() : "0"}
                onChange={handleProductChange}
                onSearch={setProductSearch}
                placeholder={t("common.placeholderSelect", {
                  entity: t("common.product"),
                })}
                isLoading={isProductsLoading}
                error={productsError ? "Failed to load products" : undefined}
                fetchNextPage={fetchNextProducts}
                hasNextPage={hasMoreProducts}
              />
            </FormControl>
          )}
        />
      </td>
      <td className="p-2">
        <FormField
          control={form.control}
          name={`productSupplyings.${index}.unitRelationId`}
          render={({ field }) => (
            <FormItem>
              <FormControl>
                <Select
                  onValueChange={(value) => field.onChange(Number(value))}
                  value={field.value ? field.value.toString() : ""}
                  disabled={units.length === 0}
                >
                  <SelectTrigger>
                    <SelectValue
                      placeholder={t("common.placeholderSelect", {
                        entity: t("common.unit"),
                      })}
                    />
                  </SelectTrigger>
                  <SelectContent>
                    {units.map((unit) => (
                      <SelectItem key={unit.id} value={unit.id!.toString()}>
                        {unit.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </td>
      <td className="p-2 text-center">
        <p>{stock}</p>
      </td>
      <td className="p-2">
        <FormField
          control={form.control}
          name={`productSupplyings.${index}.supplierId`}
          render={({ field }) => (
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
                error={suppliersError ? "Failed to load suppliers" : undefined}
                fetchNextPage={fetchNextSuppliers}
                hasNextPage={hasMoreSuppliers}
              />
            </FormControl>
          )}
        />
      </td>

      <td className="p-2 flex justify-center">
        <FormField
          control={form.control}
          name={`productSupplyings.${index}.quantity`}
          render={({ field }) => (
            <FormControl>
              <div className="flex items-center space-x-2">
                <Button
                  type="button"
                  variant="outline"
                  size="icon"
                  className="bg-primary-foreground hover:bg-gray-200 rounded-lg"
                  onClick={() => handleDecrement(field, 1)}
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
                  className="bg-primary-foreground hover:bg-gray-200 rounded-lg"
                  onClick={() => handleIncrement(field)}
                >
                  <PlusCircle className="h-4 w-4" />
                </Button>
              </div>
            </FormControl>
          )}
        />
      </td>
      <td className="p-2">
        <FormField
          control={form.control}
          name={`productSupplyings.${index}.price`}
          render={({ field }) => (
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
          )}
        />
      </td>

      <td className="p-2">
        <Button
          type="button"
          variant="ghost"
          size="icon"
          onClick={() => removeProduct(index)}
        >
          <Trash2 className="h-4 w-4 text-destructive" />
        </Button>
      </td>
    </tr>
  );
}
