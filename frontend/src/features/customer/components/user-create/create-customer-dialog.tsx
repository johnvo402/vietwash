"use client";

import React, { useEffect, useState } from "react";
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
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Gender } from "@/api/generated";
import { AccountContact } from "@/types/user";
import { ContactForm } from "@/features/profile/components/contact-form";
import { useTranslations } from "next-intl";

interface Customer {
  id?: number;
  displayName: string;
  phoneNumber: string;
  gender: Gender;
  status?: "Active" | "Inactive";
  accountContact?: AccountContact;
}

const formSchema = (t: any, isEditMode: boolean) =>
  z.object({
    displayName: z
      .string()
      .nonempty({
        message: t("common.entityRequired", {
          Entity: t("order.customerName"),
        }),
      })
      .max(100, {
        message: t("common.maxLength", { max: 100 }),
      })
      .regex(/^[a-zA-ZÀ-ỹ\s'-]+$/, {
        message: t("common.entityInvalid", { Entity: t("order.customerName") }),
      }) // Allow letters (including Vietnamese), spaces, hyphens, apostrophes
      .refine((val) => val.trim().length > 0, {
        message: t("common.entityInvalid", { Entity: t("order.customerName") }),
      }), // Prevent only spaces
    phoneNumber: z
      .string()
      .regex(/^(0|\+?[1-9])\d{1,14}$/, {
        message: t("common.entityInvalid", {
          Entity: t("order.customerPhone"),
        }),
      })
      .refine((val) => !/^0+$/.test(val), {
        message: t("common.entityInvalid", {
          Entity: t("order.customerPhone"),
        }),
      }), // Prevent all-zero phone numbers
    gender: z.nativeEnum(Gender, {
      errorMap: () => ({
        message: t("common.entityRequired", {
          Entity: t("user.gender.title"),
        }),
      }),
    }),
    status: isEditMode
      ? z.enum(["Active", "Inactive"], {
          errorMap: () => ({
            message: t("common.entityRequired", {
              Entity: t("common.status"),
            }),
          }),
        })
      : z.enum(["Active", "Inactive"]).default("Active"),
    accountContact: z
      .object({
        address: z.string().optional(),
        commune: z.string().optional(),
        district: z.string().optional(),
        province: z.string().optional(),
        communeCode: z.string().optional(),
        districtCode: z.string().optional(),
        provinceCode: z.string().optional(),
        street: z.string().optional(),
      })
      .optional()
      .nullable(),
  });

export type CustomerFormData = z.infer<ReturnType<typeof formSchema>>;

interface CustomerFormDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CustomerFormData) => Promise<void>;
  customer?: Customer;
  pageType?: "cashier" | "manage";
}

export function CustomerFormDialog({
  isOpen,
  onClose,
  onSubmit,
  customer,
  pageType = "cashier",
}: CustomerFormDialogProps) {
  const t = useTranslations();
  const isEditMode = !!customer;
  const [isLoading, setIsLoading] = useState(false);

  const form = useForm<CustomerFormData>({
    resolver: zodResolver(formSchema(t, isEditMode)),
    defaultValues: {
      displayName: "",
      phoneNumber: "",
      gender: Gender.Male,
      status: "Active",
      accountContact: null, // Initialize as null to indicate no contact info
    },
  });

  useEffect(() => {
    if (isOpen) {
      form.reset({
        displayName: customer?.displayName || "",
        phoneNumber: customer?.phoneNumber || "",
        gender: customer?.gender || Gender.Male,
        status: customer?.status || "Active",
        accountContact: customer?.accountContact || null,
      });
    }
  }, [customer, isOpen, form]);

  const handleContactChange = (field: keyof AccountContact, value: string) => {
    const currentContact = form.getValues("accountContact") || {};
    form.setValue(
      "accountContact",
      { ...currentContact, [field]: value },
      { shouldValidate: true }
    );
    const updatedContact = form.getValues("accountContact") || {};
    const addressParts = [
      updatedContact.street,
      updatedContact.commune,
      updatedContact.district,
      updatedContact.province,
    ].filter(Boolean);
    form.setValue("accountContact.address", addressParts.join(", "), {
      shouldValidate: true,
    });
  };

  const handleSubmit = async (data: CustomerFormData) => {
    setIsLoading(true);
    try {
      await onSubmit({
        ...data,
        accountContact:
          data.accountContact &&
          Object.values(data.accountContact).some(Boolean)
            ? data.accountContact
            : null, // Send null if accountContact is empty
      });
      onClose();
      form.reset();
    } catch (error: any) {
      form.setError("root", {
        type: "manual",
        message: error.message || t("common.error.submissionFailed"),
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent
        className="w-full max-w-lg max-h-[80%] sm:max-w-md overflow-y-auto p-0"
        aria-describedby={undefined}
      >
        <DialogHeader className="sticky top-0 z-10 bg-background p-6 text-primary">
          <DialogTitle>
            {t(isEditMode ? "dialog.edit.title" : "dialog.create.title", {
              entity: t("common.customer"),
            })}
          </DialogTitle>
        </DialogHeader>
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleSubmit)}
            className="space-y-6 p-6 pb-0"
          >
            {form.formState.errors.root && (
              <div className="text-red-500 text-sm">
                {form.formState.errors.root.message}
              </div>
            )}

            <FormField
              control={form.control}
              name="displayName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("order.customerName")}</FormLabel>
                  <FormControl>
                    <Input
                      {...field}
                      placeholder={t("dialog.placeholder", {
                        entity: t("order.customerName").toLowerCase(),
                      })}
                      disabled={isLoading}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="phoneNumber"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("order.customerPhone")}</FormLabel>
                  <FormControl>
                    <Input
                      {...field}
                      placeholder={t("dialog.placeholder", {
                        entity: t("user.phoneNumber.title").toLowerCase(),
                      })}
                      disabled={isLoading}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="gender"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("user.gender.title")}</FormLabel>
                  <FormControl>
                    <Select
                      onValueChange={field.onChange}
                      value={field.value}
                      disabled={isLoading}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue
                          placeholder={t("common.entitySelectPlaceholder", {
                            entity: t("user.gender.title").toLowerCase(),
                          })}
                        />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value={Gender.Male}>
                          {t("user.gender.Male")}
                        </SelectItem>
                        <SelectItem value={Gender.Female}>
                          {t("user.gender.Female")}
                        </SelectItem>
                        <SelectItem value={Gender.Other}>
                          {t("user.gender.Other")}
                        </SelectItem>
                      </SelectContent>
                    </Select>
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {isEditMode && (
              <FormField
                control={form.control}
                name="status"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("common.status.title")}</FormLabel>
                    <FormControl>
                      <RadioGroup
                        onValueChange={field.onChange}
                        value={field.value}
                        className="flex gap-4"
                      >
                        <div className="flex items-center space-x-2">
                          <RadioGroupItem
                            value="Active"
                            id="status-active"
                            disabled={isLoading}
                          />
                          <FormLabel htmlFor="status-active">
                            {t("common.status.active")}
                          </FormLabel>
                        </div>
                        <div className="flex items-center space-x-2">
                          <RadioGroupItem
                            value="Inactive"
                            id="status-inactive"
                            disabled={isLoading}
                          />
                          <FormLabel htmlFor="status-inactive">
                            {t("common.status.inactive")}
                          </FormLabel>
                        </div>
                      </RadioGroup>
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}

            {pageType === "manage" && (
              <div className="mt-4 space-y-2">
                <FormLabel>{t("user.contactInfo")}</FormLabel>
                <ContactForm
                  contact={
                    (form.watch("accountContact") as AccountContact) || {}
                  }
                  isEditing={true}
                  onChange={handleContactChange}
                />
              </div>
            )}

            <DialogFooter className="flex justify-end gap-2 sticky bottom-0 z-10 p-6 bg-background border-t border-secondary space-x-2">
              <Button
                type="button"
                variant="outline"
                onClick={onClose}
                disabled={isLoading}
              >
                {t("common.cancel")}
              </Button>
              <Button type="submit" variant="default" disabled={isLoading}>
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
                ) : (
                  t(isEditMode ? "common.update" : "common.create")
                )}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
