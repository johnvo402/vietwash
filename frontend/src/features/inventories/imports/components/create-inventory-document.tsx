"use client";

import { useEffect, useState, useCallback, useMemo } from "react";
import { useForm, useFieldArray, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Loader2, Plus, Undo2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useTranslations } from "next-intl";
import { Textarea } from "@/components/ui/textarea";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { formatNumberVN, formatPriceVN } from "@/utils/format";
import {
  useFormProducts,
  useFormSuppliers,
} from "../hooks/use-inventory-document";
import { useAuth } from "@/hooks/use-auth";
import {
  InventoryDocumentDetailResponse,
  InventoryType,
} from "@/api/generated";
import ProductSupplyingRow from "./product-supplying-row";
import EquipmentSupplyingRow from "./equipment-supplying-row";
import { useStringUtil } from "@/lib/stringUtil";
import { Input } from "@/components/ui/input";
import { format, parseISO } from "date-fns";
import { usePushRouter } from "@/utils/router-utli";

// Schema definitions
const productInventoryDocumentSchema = (t: any) =>
  z.object({
    productId: z.number().min(1, {
      message: t("common.entityRequired", { Entity: t("common.product") }),
    }),
    supplierId: z.number().min(1, {
      message: t("common.entityRequired", { Entity: t("common.supplier") }),
    }),
    quantity: z.coerce.number().min(1, {
      message: t("inventory.productSupplyings.validation.quantity"),
    }),
    price: z.coerce
      .number()
      .min(1, { message: t("inventory.productSupplyings.validation.price") }),
    unitRelationId: z.number().min(1, {
      message: t("common.entityRequired", { Entity: t("common.unit") }),
    }),
  });

const equipmentInventoryDocumentSchema = (t: any) =>
  z.object({
    name: z
      .string()
      .min(2, { message: t("inventory.equipmentSupplyings.validation.name") }),
    code: z
      .string()
      .min(1, { message: t("inventory.equipmentSupplyings.validation.code") })
      .optional(),
    quantity: z.coerce.number().min(1, {
      message: t("inventory.equipmentSupplyings.validation.quantity"),
    }),
    price: z.coerce
      .number()
      .min(0, { message: t("inventory.equipmentSupplyings.validation.price") }),
    supplierId: z.number().min(1, {
      message: t("common.entityRequired", { Entity: t("common.supplier") }),
    }),
    image: z
      .union([z.instanceof(File), z.string()])
      .optional()
      .refine(
        (file) =>
          !file || typeof file === "string" || file.size <= 50 * 1024 * 1024,
        t("inventory.equipmentSupplyings.validation.imageSize", { max: "50MB" })
      ),
  });

const formSchema = (t: any) =>
  z
    .object({
      branchId: z.number().min(1, {
        message: t("common.entityRequired", { Entity: t("common.branch") }),
      }),
      transactionAt: z
        .string()
        .refine((val) => (val ? !isNaN(Date.parse(val)) : true), {
          message: t("common.entityRequired", {
            Entity: t("inventory.transactionAt"),
          }),
        })
        .optional(),
      note: z.string().optional(),
      productSupplyings: z.array(productInventoryDocumentSchema(t)).min(0, {
        message: t("inventory.productSupplyings.min"),
      }),
      equipmentSupplyings: z.array(equipmentInventoryDocumentSchema(t)).min(0),
    })
    .refine(
      (data) => {
        const totalImageSize = data.equipmentSupplyings.reduce(
          (sum: number, equipment: any) => {
            const size =
              equipment.image instanceof File ? equipment.image.size : 0;
            return sum + size;
          },
          0
        );
        return totalImageSize <= 200 * 1024 * 1024;
      },
      {
        message: t("inventory.equipmentSupplyings.validation.totalImageSize", {
          max: "200MB",
        }),
        path: ["equipmentSupplyings"],
      }
    );

export type FormValues = z.infer<ReturnType<typeof formSchema>>;

interface PageProps {
  onSubmit: (data: FormValues, isDraft: boolean) => Promise<void>;
  initialData?: InventoryDocumentDetailResponse;
  isLoading?: boolean;
  type: InventoryType;
}

interface BranchOption {
  value: string;
  label: string;
}

interface SupplierOption {
  value: string;
  label: string;
}

interface Totals {
  totalAmount: number;
  totalProductQuantity: number;
  totalEquipmentQuantity: number;
}

// Component for form header
const FormHeader = ({
  form,
  branchOptions,
  t,
}: {
  form: any;
  branchOptions: BranchOption[];
  t: any;
}) => (
  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
    <FormField
      control={form.control}
      name="branchId"
      render={({ field }) => (
        <FormItem>
          <FormLabel>{t("common.branch")}</FormLabel>
          <FormControl>
            <Select
              onValueChange={(value) => field.onChange(Number(value))}
              value={field.value ? field.value.toString() : undefined}
            >
              <SelectTrigger aria-label={t("common.branch")}>
                <SelectValue
                  placeholder={t("common.placeholderSelect", {
                    entity: t("common.branch"),
                  })}
                />
              </SelectTrigger>
              <SelectContent>
                {branchOptions.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FormControl>
          <FormMessage />
        </FormItem>
      )}
    />
    <FormField
      control={form.control}
      name="transactionAt"
      render={({ field }) => (
        <FormItem>
          <FormLabel>{t("inventory.transactionAt")}</FormLabel>
          <FormControl>
            <Input
              type="datetime-local"
              max={format(new Date(), "yyyy-MM-dd'T'HH:mm")}
              {...field}
              value={field.value ? field.value.split(".")[0] : ""}
              onChange={(e) => field.onChange(e.target.value)}
              aria-label={t("inventory.transactionAt")}
            />
          </FormControl>
          <FormMessage />
        </FormItem>
      )}
    />
    <FormField
      control={form.control}
      name="note"
      render={({ field }) => (
        <FormItem>
          <FormLabel>{t("common.note")}</FormLabel>
          <FormControl>
            <Textarea
              placeholder={t("common.placeholderDes", {
                entity: t("common.note"),
              })}
              className="min-h-[100px]"
              {...field}
              aria-label={t("common.note")}
            />
          </FormControl>
          <FormMessage />
        </FormItem>
      )}
    />
  </div>
);

// Component for totals summary
const TotalsSummary = ({ totals, t }: { totals: Totals; t: any }) => (
  <div className="flex flex-col gap-4 w-full md:w-1/2">
    <div className="flex justify-between items-center">
      <h3 className="text-sm font-medium">
        {t("inventory.totalProductQuantity")}
      </h3>
      <p className="text-base font-bold">
        {formatNumberVN(totals.totalProductQuantity)}
      </p>
    </div>
    <div className="flex justify-between items-center">
      <h3 className="text-sm font-medium">
        {t("inventory.totalEquipmentQuantity")}
      </h3>
      <p className="text-base font-bold">
        {formatNumberVN(totals.totalEquipmentQuantity)}
      </p>
    </div>
    <div className="flex justify-between items-center">
      <h3 className="text-sm font-medium">{t("inventory.totalAmount")}</h3>
      <p className="text-base font-bold">{formatPriceVN(totals.totalAmount)}</p>
    </div>
  </div>
);

export function InventoryDocumentFormDialog({
  onSubmit,
  initialData,
  isLoading = false,
  type,
}: PageProps) {
  const t = useTranslations();
  const { user } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDialogOpen, setIsDialogOpen] = useState(true);
  const [supplierSearch, setSupplierSearch] = useState("");
  const [productSearch, setProductSearch] = useState("");
  const router = usePushRouter();
  const { textByLang } = useStringUtil();

  const branches = useMemo(
    () => user?.branchAccounts || [],
    [user?.branchAccounts]
  );

  const {
    suppliers,
    isLoading: isSuppliersLoading,
    error: suppliersError,
    fetchNextPage: fetchNextSuppliers,
    hasNextPage: hasMoreSuppliers,
  } = useFormSuppliers(supplierSearch);

  const {
    products,
    isLoading: isProductsLoading,
    error: productsError,
    fetchNextPage: fetchNextProducts,
    hasNextPage: hasMoreProducts,
  } = useFormProducts(productSearch);

  const loading = isSuppliersLoading || isProductsLoading || isLoading;

  const transformedInitialData = useMemo(() => {
    if (!initialData) return null;

    return {
      branchId:
        initialData.branchId ||
        (branches.length > 0 ? branches[0].branchId : 0),
      note: initialData.note || "",
      transactionAt: initialData.transactionAt
        ? format(parseISO(initialData.transactionAt), "yyyy-MM-dd'T'HH:mm")
        : format(new Date(), "yyyy-MM-dd'T'HH:mm"),
      productSupplyings:
        initialData.productSupplyings?.map((ps) => ({
          productId: ps.productId || 0,
          supplierId: ps.supplierId || 0,
          quantity: Math.abs(ps.quantity || 0),
          price: ps.price || 0,
          unitRelationId: ps.unitRelationId || 0,
        })) || [],
      equipmentSupplyings:
        initialData.equipmentSupplyings?.map((es) => ({
          name: es.name || "",
          code: es.code || "",
          quantity: es.quantity || 0,
          price: es.price || 0,
          supplierId: es.supplierId || 0,
          image: es.image || "",
        })) || [],
    };
  }, [initialData, branches]);

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema(t)),
    defaultValues: transformedInitialData || {
      branchId: branches.length > 0 ? branches[0].branchId : 0,
      transactionAt: format(new Date(), "yyyy-MM-dd'T'HH:mm"),
      note: "",
      productSupplyings: [],
      equipmentSupplyings: [],
    },
  });

  useEffect(() => {
    if (!loading && transformedInitialData) {
      form.reset(transformedInitialData);
    }
  }, [loading, transformedInitialData, form]);

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: "productSupplyings",
  });

  const {
    fields: equipmentFields,
    prepend: prependEquipment,
    remove: removeEquipment,
  } = useFieldArray({
    control: form.control,
    name: "equipmentSupplyings",
  });

  const productSupplyings = useWatch({
    control: form.control,
    name: "productSupplyings",
  });
  const equipmentSupplyings = useWatch({
    control: form.control,
    name: "equipmentSupplyings",
  });

  const branchOptions = useMemo<BranchOption[]>(
    () =>
      branches.map((branch: any) => ({
        value: branch.branchId?.toString() || "0",
        label: branch.branchName ?? "",
      })),
    [branches]
  );

  const supplierOptions = useMemo<SupplierOption[]>(
    () =>
      suppliers.map((supplier: any) => ({
        value: supplier.id?.toString() || "0",
        label: supplier.name ?? "",
      })),
    [suppliers]
  );

  const totals = useMemo<Totals>(() => {
    const productTotal = productSupplyings.reduce(
      (sum: number, ps: { price: number; quantity: number }) =>
        sum + ps.price * ps.quantity,
      0
    );
    const equipmentTotal = equipmentSupplyings.reduce(
      (sum: number, es: { price: number; quantity: number }) =>
        sum + es.price * es.quantity,
      0
    );
    const totalProductQuantity = productSupplyings.reduce(
      (sum: number, ps: { quantity: number }) => sum + ps.quantity,
      0
    );
    const totalEquipmentQuantity = equipmentSupplyings.reduce(
      (sum: number, es: { quantity: number }) => sum + es.quantity,
      0
    );
    return {
      totalAmount: productTotal + equipmentTotal,
      totalProductQuantity,
      totalEquipmentQuantity,
    };
  }, [productSupplyings, equipmentSupplyings]);

  const onSubmitHandler = useCallback(
    async (data: FormValues, isDraft: boolean) => {
      setIsSubmitting(true);
      try {
        await onSubmit(data, isDraft);
        setIsDialogOpen(false);
      } catch (error) {
        console.error("Error submitting form:", error);
      } finally {
        setIsSubmitting(false);
      }
    },
    [onSubmit]
  );

  useEffect(() => {
    if (!isDialogOpen) {
      setProductSearch("");
      setSupplierSearch("");
      router.back();
    }
  }, [isDialogOpen, router]);

  const addProductInventoryDocument = useCallback(() => {
    append({
      productId: 0,
      supplierId: 0,
      quantity: 0,
      price: 0,
      unitRelationId: 0,
    });
  }, [append]);

  const removeProductInventoryDocument = useCallback(
    (index: number) => {
      remove(index);
    },
    [remove]
  );

  const addEquipmentInventoryDocument = useCallback(() => {
    prependEquipment({
      name: "",
      code: "",
      quantity: 0,
      price: 0,
      supplierId: 0,
    });
  }, [prependEquipment]);

  const removeEquipmentInventoryDocument = useCallback(
    (index: number) => {
      removeEquipment(index);
    },
    [removeEquipment]
  );

  if (branches.length === 0) {
    return (
      <div className="flex items-center justify-center h-screen">
        <p className="text-destructive">{t("inventory.noBranchesAvailable")}</p>
      </div>
    );
  }

  if (suppliersError || productsError) {
    return (
      <div className="flex items-center justify-center h-screen">
        <p className="text-destructive">
          {textByLang(
            productsError?.ErrorDetail || suppliersError?.ErrorDetail
          )}
        </p>
      </div>
    );
  }

  return (
    <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
      <DialogContent
        className="!w-screen !h-screen max-w-none max-h-none overflow-y-auto p-0"
        aria-describedby="inventory-dialog-description"
      >
        <DialogHeader className="sticky top-0 z-10 bg-background p-6 text-primary">
          <DialogTitle>
            {t(`dialog.${initialData ? "update" : "create"}.title`, {
              entity: t(`inventory.detail.${type.toLowerCase()}`).toLowerCase(),
            })}
          </DialogTitle>
          <p id="inventory-dialog-description" className="sr-only">
            {t("inventory.dialogDescription")}
          </p>
          <Button
            variant="ghost"
            size="icon"
            className="absolute right-4 top-4"
            onClick={() => setIsDialogOpen(false)}
            aria-label={t("common.close")}
          >
            <Undo2 className="h-4 w-4" />
          </Button>
        </DialogHeader>
        {loading ? (
          <div className="flex items-center justify-center h-screen">
            <Loader2 className="h-8 w-8 animate-spin" />
          </div>
        ) : (
          <>
            <div className="p-6">
              <Form {...form}>
                <form id="formInventoryDocument" className="space-y-6">
                  <FormHeader form={form} branchOptions={branchOptions} t={t} />

                  {type === InventoryType.Import ? (
                    <Tabs defaultValue="products" className="w-full">
                      <TabsList className="grid w-full grid-cols-2">
                        <TabsTrigger value="products">
                          {t("inventory.productSupplyings.title")}
                        </TabsTrigger>
                        <TabsTrigger value="equipment">
                          {t("inventory.equipmentSupplyings.title")}
                        </TabsTrigger>
                      </TabsList>
                      <TabsContent value="products">
                        <div className="space-y-4">
                          <div className="flex items-center justify-between">
                            <Button
                              type="button"
                              variant="outline"
                              size="sm"
                              onClick={addProductInventoryDocument}
                              aria-label={t("inventory.addProduct")}
                            >
                              <Plus className="h-4 w-4 mr-2" />
                              {t("inventory.addProduct")}
                            </Button>
                          </div>
                          <div className="max-h-[50vh] overflow-auto border rounded text-xs">
                            <table className="w-full">
                              <thead className="bg-secondary sticky top-0 z-10">
                                <tr className="bg-secondary">
                                  <th className="p-2">
                                    {t("inventory.productSupplyings.product")}
                                  </th>
                                  <th className="p-2">
                                    {t("inventory.productSupplyings.unit")}
                                  </th>
                                  <th className="p-2">
                                    {t(
                                      "inventory.productSupplyings.stockQuantity"
                                    )}
                                  </th>
                                  <th className="p-2">
                                    {t("inventory.productSupplyings.supplier")}
                                  </th>
                                  <th className="p-2">
                                    {t("inventory.quantity")}
                                  </th>
                                  <th className="p-2">
                                    {t("inventory.price")}
                                  </th>
                                  <th className="p-2"></th>
                                </tr>
                              </thead>
                              <tbody className="divide-y">
                                {fields.map((field, index) => (
                                  <ProductSupplyingRow
                                    key={field.id}
                                    index={index}
                                    form={form}
                                    products={products}
                                    supplierOptions={supplierOptions}
                                    isProductsLoading={isProductsLoading}
                                    productsError={productsError}
                                    fetchNextProducts={fetchNextProducts}
                                    hasMoreProducts={hasMoreProducts ?? false}
                                    setProductSearch={setProductSearch}
                                    setSupplierSearch={setSupplierSearch}
                                    isSuppliersLoading={isSuppliersLoading}
                                    suppliersError={suppliersError}
                                    fetchNextSuppliers={fetchNextSuppliers}
                                    hasMoreSuppliers={hasMoreSuppliers ?? false}
                                    removeProduct={
                                      removeProductInventoryDocument
                                    }
                                  />
                                ))}
                              </tbody>
                            </table>
                          </div>
                        </div>
                      </TabsContent>
                      <TabsContent value="equipment">
                        <div className="space-y-4">
                          <div className="flex items-center justify-between">
                            <Button
                              type="button"
                              variant="outline"
                              size="sm"
                              onClick={addEquipmentInventoryDocument}
                              aria-label={t("inventory.addEquipment")}
                            >
                              <Plus className="h-4 w-4 mr-2" />
                              {t("inventory.addEquipment")}
                            </Button>
                          </div>
                          <div className="max-h-[50vh] overflow-auto border rounded text-xs">
                            <table className="w-full">
                              <thead className="bg-secondary sticky top-0 z-10">
                                <tr>
                                  <th className="p-2">
                                    {t("inventory.equipmentSupplyings.name")}
                                  </th>
                                  <th className="p-2">
                                    {t("inventory.equipmentSupplyings.code")}
                                  </th>
                                  <th className="p-2">
                                    {t("inventory.equipmentSupplyings.image")}
                                  </th>
                                  <th className="p-2">
                                    {t("inventory.quantity")}
                                  </th>
                                  <th className="p-2">
                                    {t("inventory.price")}
                                  </th>
                                  <th className="p-2">
                                    {t("common.supplier")}
                                  </th>
                                  <th className="p-2"></th>
                                </tr>
                              </thead>
                              <tbody className="divide-y">
                                {equipmentFields.map((field, index) => (
                                  <EquipmentSupplyingRow
                                    key={field.id}
                                    index={index}
                                    form={form}
                                    supplierOptions={supplierOptions}
                                    isSuppliersLoading={isSuppliersLoading}
                                    suppliersError={suppliersError}
                                    fetchNextSuppliers={fetchNextSuppliers}
                                    hasMoreSuppliers={hasMoreSuppliers ?? false}
                                    setSupplierSearch={setSupplierSearch}
                                    removeEquipment={
                                      removeEquipmentInventoryDocument
                                    }
                                  />
                                ))}
                              </tbody>
                            </table>
                          </div>
                        </div>
                      </TabsContent>
                    </Tabs>
                  ) : (
                    <div className="space-y-4">
                      <div className="flex items-center justify-between">
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={addProductInventoryDocument}
                          aria-label={t("inventory.addProduct")}
                        >
                          <Plus className="h-4 w-4 mr-2" />
                          {t("inventory.addProduct")}
                        </Button>
                      </div>
                      <div className="max-h-[50vh] overflow-auto border rounded text-xs">
                        <table className="w-full">
                          <thead className="bg-secondary sticky top-0 z-10">
                            <tr className="bg-secondary">
                              <th className="p-2">
                                {t("inventory.productSupplyings.product")}
                              </th>
                              <th className="p-2">
                                {t("inventory.productSupplyings.unit")}
                              </th>
                              <th className="p-2">
                                {t("inventory.productSupplyings.stockQuantity")}
                              </th>
                              <th className="p-2">
                                {t("inventory.productSupplyings.supplier")}
                              </th>
                              <th className="p-2">{t("inventory.quantity")}</th>
                              <th className="p-2">{t("inventory.price")}</th>

                              <th className="p-2"></th>
                            </tr>
                          </thead>
                          <tbody className="divide-y">
                            {fields.map((field, index) => (
                              <ProductSupplyingRow
                                key={field.id}
                                index={index}
                                form={form}
                                products={products}
                                supplierOptions={supplierOptions}
                                isProductsLoading={isProductsLoading}
                                productsError={productsError}
                                fetchNextProducts={fetchNextProducts}
                                hasMoreProducts={hasMoreProducts ?? false}
                                setProductSearch={setProductSearch}
                                setSupplierSearch={setSupplierSearch}
                                isSuppliersLoading={isSuppliersLoading}
                                suppliersError={suppliersError}
                                fetchNextSuppliers={fetchNextSuppliers}
                                hasMoreSuppliers={hasMoreSuppliers ?? false}
                                removeProduct={removeProductInventoryDocument}
                              />
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  )}

                  <div className="mt-6 flex flex-col gap-6 md:flex-row md:justify-between">
                    <div className="flex flex-col gap-4 w-full md:w-1/2"></div>
                    <TotalsSummary totals={totals} t={t} />
                  </div>
                </form>
              </Form>
            </div>
            <DialogFooter className="sticky bottom-0 z-10 p-6 bg-background border-t border-secondary flex justify-end space-x-2">
              <Button
                type="button"
                variant="destructive"
                className="rounded-lg"
                onClick={() => setIsDialogOpen(false)}
                aria-label={t("common.close")}
              >
                {t("common.close")}
              </Button>
              {!transformedInitialData && (
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-lg"
                  disabled={isSubmitting}
                  onClick={form.handleSubmit((data) =>
                    onSubmitHandler(data, true)
                  )}
                  aria-label={t("inventory.saveDraft")}
                >
                  {isSubmitting && (
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  )}
                  {t("inventory.saveDraft")}
                </Button>
              )}
              <Button
                form="formInventoryDocument"
                type="submit"
                className="rounded-lg"
                disabled={isSubmitting}
                onClick={form.handleSubmit((data) =>
                  onSubmitHandler(data, false)
                )}
                aria-label={
                  transformedInitialData
                    ? t("common.update")
                    : t("common.submit")
                }
              >
                {isSubmitting && (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                {transformedInitialData
                  ? t("common.update")
                  : t("common.submit")}
              </Button>
            </DialogFooter>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}
