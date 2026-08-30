"use client";

import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Form } from "@/components/ui/form";
import { Loader2, Undo2 } from "lucide-react";
import { useRouter } from "next/navigation";
import { ActivationStatus } from "@/api/generated";
import { useTranslations } from "next-intl";
import { useAuth } from "@/hooks/use-auth";
import { useListCategoryResponseQuery } from "@/features/settings/setting-data/category-settings/hooks/use-category-data-query";
import { useUnitSettings } from "@/features/settings/setting-data/unit-settings/hooks/use-unit-hook";
import { ProductFormFields } from "./product-form";
import { UnitRelationsForm } from "./unit-relation-form";
import { ImageUploadField } from "./iamge-upload";

const MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB
const ALLOWED_IMAGE_TYPES = ["image/jpeg", "image/png", "image/gif"];

// Schema for unit relation
const unitRelationSchema = (t: any) =>
  z.object({
    name: z.string().min(2, t("product.dialog.unit.validation")),
    status: z.nativeEnum(ActivationStatus).default(ActivationStatus.Active),
    baseUnit: z.boolean().default(false),
    price: z.coerce.number().min(0, t("cashier.priceMustBePositive")),
    multiple: z.coerce
      .number()
      .min(1, t("product.dialog.unit.multiple.validation"))
      .default(1),
    processingTime: z.coerce
      .number()
      .min(0, t("product.processingTime.validation")),
    unitId: z.number().optional(),
  });

// Main form schema
const formSchema = (t: any) =>
  z.object({
    branchId: z
      .number()
      .min(1, t("common.entityRequired", { Entity: t("common.branch") })),
    name: z.string().min(2, t("product.nameValidation")),
    description: z.string().min(10, t("product.desValidation")),
    sku: z.string().optional(),
    capitalPrice: z.coerce
      .number()
      .min(0, t("product.capitalPriceMustBePositive")),
    image: z
      .union([z.instanceof(File), z.string()])
      .optional()
      .refine(
        (file) =>
          !file || typeof file === "string" || file.size <= MAX_FILE_SIZE,
        t("product.imageSizeValidation", { max: "50MB" })
      )
      .refine(
        (file) =>
          !file ||
          typeof file === "string" ||
          ALLOWED_IMAGE_TYPES.includes(file.type),
        t("product.imageTypeValidation", { allowed: "JPEG, PNG, GIF" })
      ),
    categoryId: z.number({
      required_error: t("common.entityRequired", {
        Entity: t("common.category").replace(/^./, (c: string) =>
          c.toUpperCase()
        ),
      }),
    }),
    status: z.nativeEnum(ActivationStatus).default(ActivationStatus.Active),
    unitRelations: z
      .array(unitRelationSchema(t))
      .min(1, t("product.dialog.unit.relation.validation")),
    baseUnitName: z.string().min(2, t("product.dialog.unit.validation")),
  });

const newCategorySchema = (t: any) =>
  z.object({
    name: z.string().min(2, t("product.categoryValidation")),
  });

const newUnitSchema = (t: any) =>
  z.object({
    name: z.string().min(2, t("product.dialog.unit.validation")),
  });

// Types
export type FormValues = z.infer<ReturnType<typeof formSchema>>;
export type CategoryFormValues = z.infer<ReturnType<typeof newCategorySchema>>;
export type UnitFormValues = z.infer<ReturnType<typeof newUnitSchema>>;

interface PageProps {
  onSubmit: (data: FormValues) => Promise<void>;
  initialData?: FormValues | undefined;
  isUpdate?: boolean;
}

export function BranchProductDialog(props: PageProps) {
  const t = useTranslations();
  const { branchActive } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [categoryDialogOpen, setCategoryDialogOpen] = useState(false);
  const [unitDialogOpen, setUnitDialogOpen] = useState(false);
  const [isDialogOpen, setIsDialogOpen] = useState(true);
  const { units: currentUnits, createUnit } = useUnitSettings();
  const { treeData, parentOptions, isCreating, createCategory, mutations } =
    useListCategoryResponseQuery();
  const router = useRouter();

  // Transform initialData
  const transformedInitialData = useMemo(() => {
    if (!props.initialData) {
      return {
        branchId: branchActive?.branchId || 0,
        name: "",
        description: "",
        sku: "",
        capitalPrice: 0,
        image: undefined,
        categoryId: undefined,
        status: ActivationStatus.Active,
        baseUnitName: "",
        unitRelations: [
          {
            name: "",
            status: ActivationStatus.Active,
            baseUnit: false,
            price: 0,
            multiple: 1,
            processingTime: 0,
            unitId: undefined,
          },
        ],
      };
    }

    const baseUnit = props.initialData.unitRelations?.find(
      (unit) => unit.baseUnit
    );

    return {
      branchId: props.initialData.branchId || branchActive?.branchId || 0,
      name: props.initialData.name || "",
      description: props.initialData.description || "",
      sku: props.initialData.sku || "",
      capitalPrice: Number(props.initialData.capitalPrice) || 0,
      image: props.initialData.image || undefined,
      categoryId: props.initialData.categoryId || undefined,
      status: props.initialData.status || ActivationStatus.Active,
      baseUnitName: baseUnit?.name || "",
      unitRelations: props.initialData.unitRelations?.length
        ? props.initialData.unitRelations.map((unit) => ({
            name: unit.name || "",
            status: unit.status || ActivationStatus.Active,
            baseUnit: Boolean(unit.baseUnit) || false,
            price: Number(unit.price) || 0,
            multiple: Number(unit.multiple) || 1,
            processingTime: Number(unit.processingTime) || 0,
            unitId: undefined,
          }))
        : [
            {
              name: "",
              status: ActivationStatus.Active,
              baseUnit: false,
              price: 0,
              multiple: 1,
              processingTime: 0,
              unitId: undefined,
            },
          ],
    };
  }, [props.initialData, branchActive]);

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema(t)),
    defaultValues: transformedInitialData,
  });

  const categoryForm = useForm<CategoryFormValues>({
    resolver: zodResolver(newCategorySchema(t)),
    defaultValues: { name: "" },
  });

  const unitForm = useForm<UnitFormValues>({
    resolver: zodResolver(newUnitSchema(t)),
    defaultValues: { name: "" },
  });

  // Handle form submit
  const onSubmit = async (data: FormValues) => {
    setIsSubmitting(true);
    try {
      await props.onSubmit(data);
      setIsDialogOpen(false);
    } catch (error) {
      console.error("Error submitting form:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle dialog close
  useEffect(() => {
    if (!isDialogOpen) {
      router.back();
    }
  }, [isDialogOpen, router]);

  return (
    <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
      <DialogContent className="sm:max-w-[80vw] max-h-[90vh] overflow-y-auto p-0">
        <DialogHeader className="sticky top-0 z-10 bg-primary p-6 text-background">
          <DialogTitle>
            {props.isUpdate
              ? t("dialog.update.title", {
                  entity: t("common.product").toLowerCase(),
                })
              : t("dialog.create.title", {
                  entity: t("common.product").toLowerCase(),
                })}
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
              id="formProduct"
              onSubmit={form.handleSubmit(onSubmit)}
              className="space-y-6"
            >
              <ProductFormFields
                form={form}
                treeData={treeData}
                parentOptions={parentOptions}
                isCreating={isCreating}
                createCategory={createCategory}
                categoryForm={categoryForm}
                mutations={mutations}
                categoryDialogOpen={categoryDialogOpen}
                setCategoryDialogOpen={setCategoryDialogOpen}
              />
              <UnitRelationsForm
                form={form}
                currentUnits={currentUnits}
                createUnit={createUnit}
                unitForm={unitForm}
                unitDialogOpen={unitDialogOpen}
                setUnitDialogOpen={setUnitDialogOpen}
              />
              <ImageUploadField
                form={form}
                initialImage={(props.initialData?.image as string) || undefined}
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
          <Button form="formProduct" type="submit" disabled={isSubmitting}>
            {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {props.isUpdate ? t("common.update") : t("common.submit")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
