"use client";

import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useTranslations } from "next-intl";
import { useEffect } from "react";
import { AddressSelector } from "@/features/profile/components/address-selector";
import {
  ActivationStatus,
  BranchModel,
  ListBranchResponse,
} from "@/api/generated";
import { RadioGroupItem, RadioGroup } from "@/components/ui/radio-group";

interface PageProps {
  name: string;
  onClose: () => void;
  onCreateBranch: (data: BranchModel) => Promise<void>;
  onUpdateBranch?: (data: {
    id: number;
    branchData: BranchModel;
  }) => Promise<void>;
  branch?: ListBranchResponse;
  open: boolean;
}

export function CreateBranchDialog({
  name,
  onClose,
  onCreateBranch,
  onUpdateBranch,
  branch,
  open,
}: PageProps) {
  const t = useTranslations();
  const formSchema = z.object({
    name: z.string().min(1, {
      message: t("common.nameRequired", {
        Entity: t("common.branch").replace(/^./, (c) => c.toUpperCase()),
      }),
    }),
    code: z.string().optional(),
    main: z.boolean().optional(),
    email: z
      .string()
      .email({ message: t("table.accessorKey.invalidEmail") })
      .optional(),
    phoneCode: z.string().optional(),
    phoneNumber: z
      .string()
      .refine(
        (value) => value === "" || /^(0[0-9]{8,10}|\+[0-9]{9,12})$/.test(value),
        {
          message: t("common.entityInvalid", {
            Entity: t("user.phoneNumber.title"),
          }),
        }
      )
      .optional(),
    status: z.nativeEnum(ActivationStatus).default(ActivationStatus.Active),
    addressName: z.string().optional(),
    communeName: z.string().optional(),
    communeCode: z.string().optional(),
    districtName: z.string().optional(),
    districtCode: z.string().optional(),
    provinceName: z.string().optional(),
    provinceCode: z.string().optional(),
    street: z.string().optional(),
  });
  const isEditMode = !!branch;

  const form = useForm<BranchModel | any>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: "",
      code: "",
      main: false,
      email: "",
      phoneCode: "",
      phoneNumber: "",
      addressName: "",
      communeName: "",
      communeCode: "",
      districtName: "",
      districtCode: "",
      provinceName: "",
      provinceCode: "",
      street: "",
      status: ActivationStatus.Active,
    },
  });

  // Sync form with branch prop changes
  useEffect(() => {
    if (isEditMode && branch) {
      form.reset({
        name: branch.name || "",
        code: branch.code || "",
        main: branch.main || false,
        email: branch.email || "",
        phoneCode: branch.phoneCode || "",
        phoneNumber: branch.phoneNumber || "",
        addressName: branch.addressName || "",
        communeName: branch.communeName || "",
        communeCode: branch.communeCode || "",
        districtName: branch.districtName || "",
        districtCode: branch.districtCode || "",
        provinceName: branch.provinceName || "",
        provinceCode: branch.provinceCode || "",
        street: branch.street || "",
        status: branch.status || ActivationStatus.Active,
      });
    } else {
      form.reset({
        name: "",
        code: "",
        main: false,
        email: "",
        phoneCode: "",
        phoneNumber: "",
        addressName: "",
        communeName: "",
        communeCode: "",
        districtName: "",
        districtCode: "",
        provinceName: "",
        provinceCode: "",
        street: "",
        status: ActivationStatus.Active,
      });
    }
  }, [branch, isEditMode, form]);

  // Compute addressName when dependent fields change
  const street = form.watch("street");
  const communeName = form.watch("communeName");
  const districtName = form.watch("districtName");
  const provinceName = form.watch("provinceName");

  useEffect(() => {
    const addressName = [street, communeName, districtName, provinceName]
      .filter(Boolean)
      .join(", ");
    const currentValue = form.getValues("addressName");
    if (currentValue !== addressName) {
      form.setValue("addressName", addressName ?? "");
    }
  }, [street, communeName, districtName, provinceName, form]);

  const onSubmit = async (data: BranchModel) => {
    try {
      if (isEditMode && branch?.id && onUpdateBranch) {
        await onUpdateBranch({ id: branch.id, branchData: data });
      } else {
        await onCreateBranch(data);
      }
      form.reset();
    } catch (error) {
      console.log(error);
    }
  };

  const handleAddressChange = (field: keyof any, value: string) => {
    const fieldMap: { [key in keyof any]?: keyof BranchModel } = {
      province: "provinceName",
      provinceCode: "provinceCode",
      district: "districtName",
      districtCode: "districtCode",
      commune: "communeName",
      communeCode: "communeCode",
      address: "addressName",
      street: "street",
    };

    const branchField = fieldMap[field.toString()];
    if (branchField) {
      form.setValue(branchField, value);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>
            {isEditMode
              ? t("dialog.edit.title", {
                  entity: t("common.branch").toLowerCase(),
                })
              : t("dialog.create.title", {
                  entity: t("common.branch").toLowerCase(),
                })}
          </DialogTitle>
        </DialogHeader>
        <Form {...form}>
          <form
            id={name}
            onSubmit={form.handleSubmit(onSubmit)}
            className="space-y-6"
          >
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>
                      {t("dialog.name", {
                        Entity: t("common.branch").replace(/^./, (c) =>
                          c.toUpperCase()
                        ),
                      })}
                    </FormLabel>
                    <FormControl>
                      <Input
                        placeholder={t("dialog.placeholder", {
                          entity: t("common.branch"),
                        })}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="code"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("table.accessorKey.code")}</FormLabel>
                    <FormControl>
                      <Input
                        placeholder={t("search.searchBy", {
                          entity:
                            t("table.accessorKey.name") +
                            " " +
                            t("user.and") +
                            " " +
                            t("table.accessorKey.code"),
                        })}
                        {...field}
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
                        placeholder={t("branch.placeholderEmail")}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <div className="grid grid-cols-1 gap-2">
                <FormField
                  control={form.control}
                  name="phoneNumber"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t("user.phoneNumber.title")}</FormLabel>
                      <FormControl>
                        <Input
                          placeholder={t("user.phoneNumberPlaceholder")}
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="col-span-2 space-y-2">
                <Label className="text-md">{t("user.address.title")}</Label>
                <AddressSelector
                  contact={{
                    province: form.watch("provinceName") ?? "",
                    provinceCode: form.watch("provinceCode") ?? "",
                    district: form.watch("districtName") ?? "",
                    districtCode: form.watch("districtCode") ?? "",
                    commune: form.watch("communeName") ?? "",
                    communeCode: form.watch("communeCode") ?? "",
                    address: form.watch("addressName") ?? "",
                    street: form.watch("street") ?? "",
                  }}
                  isEditing={true}
                  onChange={handleAddressChange}
                />
                <FormField
                  control={form.control}
                  name="street"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-xs font-normal">
                        {t("AddressSelector.street")}
                      </FormLabel>
                      <FormControl>
                        <Input
                          placeholder={t("dialog.placeholder", {
                            entity: t("AddressSelector.street").toLowerCase(),
                          })}
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
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
                          <RadioGroupItem value="Active" id="status-active" />
                          <FormLabel htmlFor="status-active">
                            {t("common.status.active")}
                          </FormLabel>
                        </div>
                        <div className="flex items-center space-x-2">
                          <RadioGroupItem
                            value="Inactive"
                            id="status-inactive"
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
              <div className="flex justify-end items-end space-x-2">
                <Switch
                  id="main"
                  checked={form.watch("main")}
                  onCheckedChange={(checked) => form.setValue("main", checked)}
                />
                <Label htmlFor="main">{t("branch.setMain")}</Label>
              </div>
            </div>
            <div className="flex justify-end space-x-2">
              <Button type="button" variant="outline" onClick={onClose}>
                {t("common.cancel")}
              </Button>
              <Button type="submit">
                {isEditMode ? t("common.update") : t("common.create")}
              </Button>
            </div>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
