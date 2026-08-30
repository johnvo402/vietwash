"use client";

import { useEffect, useMemo, useState, useCallback } from "react";
import { useForm, useFieldArray } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Loader2, Undo2 } from "lucide-react";
import { useAuth } from "@/hooks/use-auth";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Form } from "@/components/ui/form";
import { useRouter } from "next/navigation";
import { ActivationStatus } from "@/api/generated";
import { useTranslations } from "next-intl";
import { ServiceFormFields } from "./service-form-field";
import { UnitRelationsForm } from "./unit-relation-form";
import { ImageUploadField } from "./image-service";
import { useFormProducts } from "@/features/inventories/imports/hooks/use-inventory-document";

const serviceResourceSchema = (t: any) =>
  z.object({
    productId: z
      .number()
      .min(1, t("common.entityRequired", { Entity: t("common.product") })),
    unitProductId: z
      .number()
      .min(1, t("common.entityRequired", { Entity: t("common.unit") })),
    quantity: z.number().gte(0, t("service.resources.quantityValidation")),
  });

const unitRelationSchema = (t: any) =>
  z.object({
    name: z.string().min(2, t("service.dialog.unit.validation")),
    status: z.nativeEnum(ActivationStatus).default(ActivationStatus.Active),
    baseUnit: z.boolean().default(false),
    price: z.number().min(0, t("cashier.priceMustBePositive")),
    multiple: z.number().default(1),
    processingTime: z.number().min(0, t("service.processingTime.validation")),
    serviceResources: z.array(serviceResourceSchema(t)).optional(),
  });

const formSchema = (t: any) =>
  z.object({
    name: z.string().min(2, t("service.nameValidation")),
    status: z.nativeEnum(ActivationStatus).default(ActivationStatus.Active),
    description: z.string().min(10, t("service.desValidation")),
    image: z.string().optional(),
    categoryId: z.number({
      required_error: t("common.entityRequired", {
        Entity: t("common.category").replace(/^./, (c: string) =>
          c.toUpperCase()
        ),
      }),
    }),
    unitRelations: z
      .array(unitRelationSchema(t))
      .min(1, t("service.dialog.unit.relation.validation")),
  });

export type FormValues = z.infer<ReturnType<typeof formSchema>>;

interface PageProps {
  image: (data?: File) => Promise<void>;
  onSubmit: (data: FormValues, branchActiveId: number) => Promise<void>;
  initialData?: FormValues | undefined;
  isUpdate?: boolean;
}

export function ServiceDialog(props: PageProps) {
  const t = useTranslations();
  const { branchActive } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [categoryDialogOpen, setCategoryDialogOpen] = useState(false);
  const [unitDialogOpen, setUnitDialogOpen] = useState(false);
  const [isDialogOpen, setIsDialogOpen] = useState(true);
  const [productSearch, setProductSearch] = useState("");

  const router = useRouter();

  const productIds = useMemo(() => {
    const ids: number[] = [];
    props.initialData?.unitRelations.map((x) => {
      x.serviceResources?.map((x) => {
        ids.push(x.productId);
      });
    });
    return ids;
  }, [props.initialData]);

  const {
    products,
    isLoading: isProductsLoading,
    error: productsError,
    fetchNextPage: fetchNextProducts,
    hasNextPage: hasMoreProducts,
  } = useFormProducts(productSearch, productIds);

  const transformedInitialData = useMemo(() => {
    if (!props.initialData) {
      return {
        name: "",
        status: ActivationStatus.Active,
        description: "",
        image: "",
        categoryId: undefined as unknown as number,
        unitRelations: [
          {
            name: "",
            status: ActivationStatus.Active,
            baseUnit: true,
            price: 0,
            multiple: 1,
            processingTime: 0,
            serviceResources: [{ productId: 0, unitProductId: 0, quantity: 1 }],
          },
        ],
      } as FormValues;
    }
    return {
      ...props.initialData,
      unitRelations: props.initialData.unitRelations?.length
        ? props.initialData.unitRelations.map((unit) => ({
            ...unit,
            serviceResources: unit.serviceResources?.length
              ? unit.serviceResources
              : [{ productId: 0, unitProductId: 0, quantity: 1 }],
          }))
        : [
            {
              name: "",
              status: ActivationStatus.Active,
              baseUnit: true,
              price: 0,
              multiple: 1,
              processingTime: 0,
              serviceResources: [
                { productId: 0, unitProductId: 0, quantity: 1 },
              ],
            },
          ],
    } as FormValues;
  }, [props.initialData]);

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema(t)),
    defaultValues: transformedInitialData,
    mode: "onSubmit",
    reValidateMode: "onSubmit",
    shouldUseNativeValidation: false,
  });

  // useFieldArray for unitRelations to get stable IDs
  const {
    fields: unitFields,
    append: appendUnit,
    remove: removeUnit,
  } = useFieldArray({ control: form.control, name: "unitRelations" });

  const onSubmit = useCallback(
    async (data: FormValues) => {
      setIsSubmitting(true);
      try {
        await props.onSubmit(data, Number(branchActive?.branchId));
      } catch (error) {
        console.error("Error submitting form:", error);
      } finally {
        setIsSubmitting(false);
        setIsDialogOpen(false);
      }
    },
    [props, branchActive]
  );

  useEffect(() => {
    if (!isDialogOpen) router.back();
  }, [isDialogOpen, router]);

  return (
    <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
      <DialogContent className="sm:max-w-[80vw] max-h-[90vh] overflow-y-auto p-0">
        <DialogHeader className="sticky top-0 z-10 bg-primary p-6 text-background">
          <DialogTitle>
            {props.isUpdate
              ? t("dialog.update.title", { entity: t("common.service") })
              : t("dialog.create.title", { entity: t("common.service") })}
          </DialogTitle>
          <Button
            variant="ghost"
            size="icon"
            className="absolute right-4 top-4"
            onClick={() => setIsDialogOpen(false)}
          >
            <Undo2 className="h-4 w-4" />
            <span className="sr-only">{t("common.close")}</span>
          </Button>
        </DialogHeader>

        <div className="p-6">
          <Form {...form}>
            <form
              id="formService"
              onSubmit={form.handleSubmit(onSubmit)}
              className="space-y-6"
            >
              <ServiceFormFields
                form={form}
                categoryDialogOpen={categoryDialogOpen}
                setCategoryDialogOpen={setCategoryDialogOpen}
              />

              <UnitRelationsForm
                form={form}
                unitDialogOpen={unitDialogOpen}
                setUnitDialogOpen={setUnitDialogOpen}
                products={products || []}
                isProductsLoading={isProductsLoading}
                productsError={productsError}
                fetchNextProducts={fetchNextProducts}
                hasMoreProducts={hasMoreProducts}
                setProductSearch={setProductSearch}
                unitFields={unitFields as any}
                appendUnit={(v) =>
                  appendUnit(
                    v as any // UI append
                  )
                }
                removeUnit={removeUnit}
              />

              <ImageUploadField
                form={form}
                image={props.image}
                initialImage={props.initialData?.image}
              />
            </form>
          </Form>
        </div>

        <DialogFooter className="sticky bottom-0 z-10 p-6 bg-background border-t border-secondary">
          <Button
            type="button"
            variant="destructive"
            onClick={() => setIsDialogOpen(false)}
          >
            {t("common.cancel")}
          </Button>
          <Button form="formService" type="submit" disabled={isSubmitting}>
            {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {props.isUpdate ? t("common.update") : t("common.submit")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
