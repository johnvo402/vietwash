"use client";

import React, { useState } from "react";
import { useTranslations } from "next-intl";
import { z } from "zod";
import { useFieldArray, useForm } from "react-hook-form";
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
import { Trash2 } from "lucide-react";
import { formatNumberVN } from "@/utils/format";

// Zod schema for validation
const formSchema = (t: any) =>
  z.object({
    type: z.enum(["Maintenance", "Repair"], {
      errorMap: () => ({
        message: t("common.entityRequired", {
          Entity: t("equipment.activity.type"),
        }),
      }),
    }),
    description: z
      .string()
      .max(255, { message: t("equipment.validation.descriptionMaxLength") })
      .optional()
      .or(z.literal("")),
    laborCost: z
      .number()
      .min(1, { message: t("equipment.validation.laborCostNonNegative") })
      .nonnegative({ message: t("equipment.validation.laborCostNonNegative") }),
    details: z
      .array(
        z.object({
          partName: z.string().nonempty({
            message: t("common.entityRequired", {
              Entity: t("equipment.activity.partName"),
            }),
          }),
          quantity: z
            .number()
            .min(1, { message: t("equipment.validation.quantityPositive") })
            .positive({ message: t("equipment.validation.quantityPositive") }),
          unitPrice: z
            .number()
            .min(1, {
              message: t("equipment.validation.unitPriceNonNegative"),
            })
            .nonnegative({
              message: t("equipment.validation.unitPriceNonNegative"),
            }),
        })
      )
      .min(1, { message: t("equipment.validation.detailsMinLength") }),
  });

export type EquipmentActivityFormData = z.infer<ReturnType<typeof formSchema>>;

interface EquipmentActivityFormDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: EquipmentActivityFormData) => Promise<void>;
  initialData?: Partial<EquipmentActivityFormData> | null;
  viewOnly?: boolean; // New prop for view-only mode
}

export function EquipmentActivityFormDialog({
  isOpen,
  onClose,
  onSubmit,
  initialData,
  viewOnly = false, // Default to false (editable mode)
}: EquipmentActivityFormDialogProps) {
  const t = useTranslations();
  const [isLoading, setIsLoading] = useState(false);

  const form = useForm<EquipmentActivityFormData>({
    resolver: zodResolver(formSchema(t)),
    defaultValues: {
      type: initialData?.type || "Maintenance",
      description: initialData?.description || "",
      laborCost: initialData?.laborCost || 0,
      details: initialData?.details || [
        { partName: "", quantity: 0, unitPrice: 0 },
      ],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: "details",
  });

  const handleSubmit = async (data: EquipmentActivityFormData) => {
    if (viewOnly) return; // Prevent submission in viewOnly mode
    setIsLoading(true);
    try {
      await onSubmit(data);
      onClose();
      form.reset();
    } catch (error) {
      console.error("Submission failed:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="w-full max-w-3xl">
        <DialogHeader>
          <DialogTitle>
            {viewOnly
              ? t("equipment.activity.view")
              : initialData
                ? t("equipment.activity.edit")
                : t("equipment.activity.createActivity")}
          </DialogTitle>
        </DialogHeader>
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleSubmit)}
            className="space-y-6 p-6"
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormField
                control={form.control}
                name="type"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("equipment.activity.type")}</FormLabel>
                    <FormControl>
                      <Select
                        onValueChange={field.onChange}
                        value={field.value}
                        disabled={isLoading || viewOnly} // Disable in viewOnly mode
                      >
                        <SelectTrigger className="w-full">
                          <SelectValue
                            placeholder={t("equipment.activity.selectType")}
                          />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Maintenance">
                            {t("equipment.activity.maintenance")}
                          </SelectItem>
                          <SelectItem value="Repair">
                            {t("equipment.activity.repair")}
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
                name="laborCost"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("equipment.activity.laborCost")}</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        {...field}
                        value={formatNumberVN(field.value)}
                        onChange={(e) => {
                          const val = e.target.value.replace(/\D/g, "");
                          field.onChange(Number(val));
                        }}
                        placeholder={t(
                          "equipment.activity.laborCostPlaceholder"
                        )}
                        className="w-full"
                        disabled={isLoading || viewOnly} // Disable in viewOnly mode
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("equipment.activity.description")}</FormLabel>
                  <FormControl>
                    <Textarea
                      {...field}
                      placeholder={t(
                        "equipment.activity.descriptionPlaceholder"
                      )}
                      className="w-full min-h-[100px]"
                      disabled={isLoading || viewOnly} // Disable in viewOnly mode
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div>
              <FormLabel className="text-lg font-semibold">
                {t("equipment.activity.details")}
              </FormLabel>
              {!viewOnly && ( // Hide "Add Detail" button in viewOnly mode
                <div>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() =>
                      append({ partName: "", quantity: 0, unitPrice: 0 })
                    }
                    disabled={isLoading}
                    className="mt-4 w-full md:w-auto"
                  >
                    {t("equipment.activity.addDetail")}
                  </Button>
                </div>
              )}
              <div className="max-h-64 overflow-auto">
                {fields.map((field, index) => (
                  <div
                    key={field.id}
                    className="mt-4 border p-4 rounded-md bg-background"
                  >
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                      <FormField
                        control={form.control}
                        name={`details.${index}.partName`}
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>
                              {t("equipment.activity.partName")}
                            </FormLabel>
                            <FormControl>
                              <Input
                                {...field}
                                placeholder={t(
                                  "equipment.activity.partNamePlaceholder"
                                )}
                                disabled={isLoading || viewOnly} // Disable in viewOnly mode
                              />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name={`details.${index}.quantity`}
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>
                              {t("equipment.activity.quantity")}
                            </FormLabel>
                            <FormControl>
                              <Input
                                type="number"
                                min={1}
                                {...field}
                                onChange={(e) =>
                                  field.onChange(Number(e.target.value))
                                }
                                placeholder={t(
                                  "equipment.activity.quantityPlaceholder"
                                )}
                                disabled={isLoading || viewOnly} // Disable in viewOnly mode
                              />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name={`details.${index}.unitPrice`}
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>
                              {t("equipment.activity.unitPrice")}
                            </FormLabel>
                            <FormControl>
                              <Input
                                type="number"
                                {...field}
                                value={formatNumberVN(field.value)}
                                onChange={(e) => {
                                  const val = e.target.value.replace(/\D/g, "");
                                  field.onChange(Number(val));
                                }}
                                placeholder={t(
                                  "equipment.activity.unitPricePlaceholder"
                                )}
                                disabled={isLoading || viewOnly} // Disable in viewOnly mode
                              />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      {!viewOnly && ( // Hide delete button in viewOnly mode
                        <div className="flex items-end justify-around">
                          <Button
                            type="button"
                            variant="outline"
                            onClick={() => remove(index)}
                            disabled={
                              isLoading || (fields.length === 1 && !viewOnly)
                            }
                            className="w-full md:w-auto"
                          >
                            <Trash2 className="h-4 w-4 text-destructive" />
                          </Button>
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {!viewOnly && ( // Hide footer in viewOnly mode
              <DialogFooter className="flex justify-end gap-2">
                <Button
                  type="button"
                  variant="outline"
                  onClick={onClose}
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
            )}
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
