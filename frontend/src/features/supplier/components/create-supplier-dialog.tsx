"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Loader2, Undo2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
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
import { useTranslations } from "next-intl";
import { Textarea } from "@/components/ui/textarea";
import { ActivationStatus, GetSupplierDetailResponse } from "@/api/generated";

// Form schema with validation
const formSchema = (t: any) =>
  z.object({
    code: z.string().optional(),
    name: z.string().min(
      1,
      t("common.nameRequired", {
        Entity: t("common.supplier").toLowerCase(),
      })
    ),
    email: z.string().email(t("table.accessorKey.invalidEmail")),
    address: z
      .string()
      .min(1, t("common.entityRequired", { Entity: t("user.address.title") })),
    phone: z.string().optional(),
    description: z.string().optional(),
    status: z.nativeEnum(ActivationStatus),
  });

export type FormValues = z.infer<ReturnType<typeof formSchema>>;

interface PageProps {
  open: boolean;
  onClose: () => void;
  onCreateSupplier?: (data: { supplier: FormData }) => Promise<void>;
  onUpdateSupplier?: (
    data: FormValues & { id: number },
    formData: FormData
  ) => Promise<void>;
  supplier?: GetSupplierDetailResponse;
  isLoading?: boolean; // New prop to control loading state
}

// Loading Overlay Component
function LoadingOverlay() {
  const t = useTranslations();
  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="flex flex-col items-center gap-4">
        <Loader2 className="h-8 w-8 animate-spin text-white" />
        <span className="text-white text-lg">{t("supplier.loading")}</span>
      </div>
    </div>
  );
}

export function CreateSupplierDialog({
  open,
  onClose,
  onCreateSupplier,
  onUpdateSupplier,
  supplier: propSupplier,
  isLoading = false, // Default to false if not provided
}: PageProps) {
  const t = useTranslations();
  const isEditMode = !!propSupplier;

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema(t)),
    defaultValues: {
      code: "",
      name: "",
      email: "",
      address: "",
      phone: "",
      description: "",
      status: ActivationStatus.Active,
    },
  });

  // Populate form with fetched supplier data or prop supplier data
  useEffect(() => {
    if (isEditMode && propSupplier) {
      const newValues = {
        code: propSupplier.code ?? "",
        name: propSupplier.name ?? "",
        email: propSupplier.email ?? "",
        address: propSupplier.address ?? "",
        phone: propSupplier.phone ?? "",
        description: propSupplier.description ?? "",
        status: propSupplier.status ?? ActivationStatus.Active,
      };

      if (JSON.stringify(form.getValues()) !== JSON.stringify(newValues)) {
        form.reset(newValues);
      }
    } else {
      const defaultValues = {
        code: "",
        name: "",
        email: "",
        address: "",
        phone: "",
        description: "",
        status: ActivationStatus.Active,
      };

      if (JSON.stringify(form.getValues()) !== JSON.stringify(defaultValues)) {
        form.reset(defaultValues);
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [propSupplier, isEditMode]);

  async function onSubmit(data: FormValues) {
    try {
      const formData = new FormData();
      if (data.code) formData.append("code", data.code);
      formData.append("name", data.name);
      formData.append("email", data.email);
      formData.append("address", data.address);
      if (data.phone) formData.append("phone", data.phone);
      if (data.description) formData.append("description", data.description);
      formData.append("status", String(data.status));

      if (isEditMode) {
        const id = propSupplier?.id!;
        await onUpdateSupplier?.({ ...data, id }, formData);
      } else {
        await onCreateSupplier?.({
          supplier: formData,
        });
      }
      handleClose(); // Close dialog on successful submission
    } catch (error) {
      console.error("Error submitting supplier:", error);
      form.setError("root", { message: t("supplier.submitError") });
    }
  }

  const handleClose = () => {
    form.reset({
      code: "",
      name: "",
      email: "",
      address: "",
      phone: "",
      description: "",
      status: ActivationStatus.Active,
    });
    onClose();
  };

  return (
    <>
      {isLoading && <LoadingOverlay />}
      <Dialog open={open} onOpenChange={handleClose}>
        <DialogContent className="sm:max-w-[80vw] max-h-[90vh] overflow-y-auto p-0">
          <DialogHeader className="sticky top-0 z-10 bg-primary p-6 text-background">
            <DialogTitle>
              {isEditMode
                ? t("dialog.edit.title", {
                    entity: t("common.supplier").toLowerCase(),
                  })
                : t("dialog.create.title", {
                    entity: t("common.supplier").toLowerCase(),
                  })}
            </DialogTitle>
            <DialogDescription className="text-background">
              {isEditMode
                ? t("dialog.edit.description", {
                    entity: t("common.supplier").toLowerCase(),
                  })
                : t("dialog.create.description", {
                    entity: t("common.supplier").toLowerCase(),
                  })}
            </DialogDescription>
            <Button
              variant="ghost"
              size="icon"
              className="absolute right-4 top-4"
              onClick={handleClose}
              disabled={isLoading}
            >
              <Undo2 className="h-4 w-4" />
              <span className="sr-only">{t("common.close")}</span>
            </Button>
          </DialogHeader>
          <div className="p-6">
            <Form {...form}>
              <form
                id="formSupplier"
                onSubmit={form.handleSubmit(onSubmit)}
                className="space-y-6"
              >
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="code"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t("table.accessorKey.code")}</FormLabel>
                        <FormControl>
                          <Input
                            placeholder={t("dialog.placeholder", {
                              entity: t("dialog.name", {
                                Entity: t("common.supplier"),
                              }).toLowerCase(),
                            })}
                            {...field}
                            disabled={isLoading}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="name"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t("table.accessorKey.name")}</FormLabel>
                        <FormControl>
                          <Input
                            placeholder={t("common.entityName", {
                              Entity: t("common.supplier").replace(/^./, (c) =>
                                c.toUpperCase()
                              ),
                            })}
                            {...field}
                            disabled={isLoading}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="email"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t("user.email.title")}</FormLabel>
                        <FormControl>
                          <Input
                            type="email"
                            placeholder="supplier@example.com"
                            {...field}
                            disabled={isLoading}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="address"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t("user.address.title")}</FormLabel>
                        <FormControl>
                          <Input
                            placeholder={t("user.addressPlaceholder")}
                            {...field}
                            disabled={isLoading}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="phone"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t("user.phoneNumber.title")}</FormLabel>
                        <FormControl>
                          <Input
                            placeholder={t("user.phoneNumberPlaceholder")}
                            {...field}
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
                            placeholder={t("common.placeholderDes", {
                              entity: t("common.supplier"),
                            })}
                            {...field}
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
                        <Select
                          onValueChange={(value) => field.onChange(value)}
                          defaultValue={String(field.value)}
                          disabled={isLoading}
                        >
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue
                                placeholder={t("common.status.selectStatus")}
                              />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            {Object.entries(ActivationStatus).map(
                              ([key, value]) => (
                                <SelectItem key={value} value={String(value)}>
                                  {t(`common.status.${key.toLowerCase()}`)}
                                </SelectItem>
                              )
                            )}
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>
              </form>
            </Form>
          </div>
          <DialogFooter className="sticky bottom-0 z-10 p-6 bg-background border-t border-secondary">
            <Button
              type="button"
              variant="outline"
              onClick={handleClose}
              disabled={isLoading}
            >
              {t("common.cancel")}
            </Button>
            <Button form="formSupplier" type="submit" disabled={isLoading}>
              {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {isEditMode ? t("common.update") : t("common.create")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
