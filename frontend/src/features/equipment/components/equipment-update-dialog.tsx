"use client";

import React, { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
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
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { EquipmentStatus } from "@/api/generated/api";
import Image from "next/image";

// Zod schema for validation
const formSchema = (t: any) =>
  z.object({
    name: z
      .string()
      .min(2, {
        message: t("inventory.validation.equipment.name"),
      })
      .nonempty({
        message: t("common.entityRequired", {
          Entity: t("inventory.equipmentSupplyings.name"),
        }),
      }),
    description: z
      .string()
      .max(255, { message: "equipment.validation.descriptionMaxLength" })
      .optional()
      .or(z.literal("")),
    status: z.nativeEnum(EquipmentStatus).default(EquipmentStatus.Active),
    image: z
      .union([z.instanceof(File), z.string()])
      .optional()
      .refine(
        (file) =>
          !file || typeof file === "string" || file.size <= 50 * 1024 * 1024,
        t("inventory.equipmentSupplyings.validation.imageSize", { max: "50MB" })
      ),
  });

export type EquipmentFormData = z.infer<ReturnType<typeof formSchema>>;

interface EquipmentFormDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: EquipmentFormData) => Promise<void>;
  initialData?: Partial<EquipmentFormData> & { image?: string }; // image as string
}

export function EquipmentFormDialog({
  isOpen,
  onClose,
  onSubmit,
  initialData,
}: EquipmentFormDialogProps) {
  const t = useTranslations();
  const [isLoading, setIsLoading] = useState(false);
  const [imagePreview, setImagePreview] = useState<string | null>(
    initialData?.image || null
  ); // Initialize preview with initialData.image (string)

  const form = useForm<EquipmentFormData>({
    resolver: zodResolver(formSchema(t)),
    defaultValues: {
      name: initialData?.name || "",
      description: initialData?.description || "",
      status: initialData?.status || "Active",
      image: initialData?.image, // File input starts empty for new uploads
    },
  });

  // Handle image file change and generate preview
  const handleImageChange = (
    e: React.ChangeEvent<HTMLInputElement>,
    onChange: (file: File | undefined) => void
  ) => {
    const file = e.target.files?.[0];
    if (file) {
      onChange(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    } else {
      onChange(undefined);
      setImagePreview(initialData?.image || null); // Revert to initial image if file is cleared
    }
  };

  const handleSubmit = async (data: EquipmentFormData) => {
    setIsLoading(true);
    try {
      await onSubmit(data);
      onClose();
      setImagePreview(null);
      form.reset();
    } catch (error) {
      console.error("Submission failed:", error);
    } finally {
      setIsLoading(false);
    }
  };
  useEffect(() => {
    if (initialData?.image) {
      setImagePreview(initialData.image);
    }
  }, [initialData?.image]);

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="w-auto min-w-max sm:max-w-[80vw] max-h-[90vh] overflow-y-auto p-0">
        <DialogHeader className="sticky top-0 z-10 bg-primary p-6 text-background">
          <DialogTitle>
            {initialData ? t("common.edit") : t("common.create")}
          </DialogTitle>
        </DialogHeader>
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleSubmit)}
            className="space-y-6 p-4 pb-0"
          >
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>
                    {t("inventory.equipmentSupplyings.name")}
                  </FormLabel>
                  <FormControl>
                    <Input
                      {...field}
                      placeholder={t("dialog.placeholder", {
                        entity: t(
                          "inventory.equipmentSupplyings.name"
                        ).toLowerCase(),
                      })}
                      className="w-full"
                      disabled={isLoading}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("common.description")}</FormLabel>
                  <FormControl>
                    <Textarea
                      {...field}
                      placeholder={t("common.placeholderDes", { entity: "" })}
                      className="w-full min-h-[100px]"
                      disabled={isLoading}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="status"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("common.status.title")}</FormLabel>
                  <FormControl>
                    <Select
                      onValueChange={field.onChange}
                      value={field.value}
                      disabled={isLoading}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder={t("entity.selectStatus")} />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value={EquipmentStatus.Active}>
                          {t("common.status.active")}
                        </SelectItem>
                        <SelectItem value={EquipmentStatus.UnderMaintenance}>
                          {t("common.status.undermaintenance")}
                        </SelectItem>
                        <SelectItem value={EquipmentStatus.UnderRepair}>
                          {t("common.status.underrepair")}
                        </SelectItem>
                      </SelectContent>
                    </Select>
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="image"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("common.image")}</FormLabel>
                  <FormControl>
                    <Input
                      type="file"
                      accept="image/jpeg,image/png"
                      onChange={(e) => handleImageChange(e, field.onChange)}
                      disabled={isLoading}
                      className="w-full"
                    />
                  </FormControl>
                  {imagePreview && (
                    <div className="mt-2">
                      <Image
                        src={imagePreview}
                        alt="Preview"
                        width={400}
                        height={400}
                        className="object-cover mb-2 rounded"
                      />
                    </div>
                  )}
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter className="sticky bottom-0 z-10 p-6 bg-background border-t border-secondary">
              <Button
                type="button"
                variant="outline"
                onClick={onClose}
                className="mr-2"
                disabled={isLoading}
              >
                {t("common.cancel")}
              </Button>
              <Button
                type="submit"
                className="bg-primary hover:bg-primary/90"
                disabled={isLoading}
              >
                {isLoading ? (
                  <span className="flex items-center">
                    <svg
                      className="animate-spin h-5 w-5 mr-2"
                      viewBox="0 0 24 24"
                    >
                      <circle
                        className="opacity-25"
                        cx="12"
                        cy="12"
                        r="10"
                        stroke="currentColor"
                        strokeWidth="4"
                      />
                      <path
                        className="opacity-75"
                        fill="currentColor"
                        d="M4 12a8 8 0 018-8v8h8a8 8 0 11-16 0z"
                      />
                    </svg>
                    {t("common.loading")}
                  </span>
                ) : initialData ? (
                  t("common.save")
                ) : (
                  t("common.create")
                )}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
