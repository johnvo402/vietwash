"use client";

import type * as React from "react";
import { useState, useEffect, useCallback } from "react";
import Image from "next/image";
import { z } from "zod";
import type { ZodIssue } from "zod";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import CustomDateTime from "@/features/cashier/components/booking-receipt-date";
import MultiSelect, { Option } from "@/components/core/selects/multi-select";
import { useTranslations } from "next-intl";
import { CustomerGroup } from "@/api/generated";
import { Customer } from "@/utils/customer-indexedDb";

export interface NewVoucher {
  id?: number;
  code: string;
  title: string;
  img: File | null;
  imgUrl?: string;
  discountFixed: boolean;
  discountValue: number;
  customerGroups: string[];
  description: string;
  startAt: string;
  endAt: string;
  status: "Active" | "Inactive";
  customerIds: number[];
}

interface CreateVoucherFormProps {
  customers: Customer[];
  isOpen: boolean;
  onClose: () => void;
  onCreate: (voucher: NewVoucher) => Promise<void>;
  onUpdate?: (voucher: NewVoucher) => Promise<void>;
  voucher?: NewVoucher;
  viewMode?: boolean;
}

const voucherSchema = z
  .object({
    id: z.number().optional(),
    code: z.string().min(1, "voucher.form.errors.codeRequired"),
    title: z.string().min(1, "voucher.form.errors.titleRequired"),
    img: z
      .instanceof(File, { message: "voucher.form.errors.imageRequired" })
      .nullable()
      .optional(),
    imgUrl: z.string().optional(),
    discountFixed: z.boolean(),
    discountValue: z
      .number()
      .min(0, "voucher.form.errors.discountValueNonNegative"),
    customerGroups: z.array(z.string()).default([]),
    description: z.string().min(1, "voucher.form.errors.descriptionRequired"),
    startAt: z.date({
      required_error: "voucher.form.errors.startDateRequired",
    }),
    endAt: z.date({ required_error: "voucher.form.errors.endDateRequired" }),
    status: z.enum(["Active", "Inactive"], {
      required_error: "voucher.form.errors.statusRequired",
    }),
    customerIds: z.array(z.number()).default([]),
  })
  .superRefine((data, ctx) => {
    if (data.discountFixed) {
      if (data.discountValue <= 0) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: "voucher.form.errors.discountFixedPositive",
          path: ["discountValue"],
        });
      }
    } else {
      if (data.discountValue < 0 || data.discountValue > 100) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: "voucher.form.errors.discountPercentRange",
          path: ["discountValue"],
        });
      }
    }
    if (data.startAt && data.endAt && data.startAt > data.endAt) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "voucher.form.errors.endDateBeforeStart",
        path: ["endAt"],
      });
    }
    // Ensure only one of customerGroups or customerIds is selected in create mode
    if (
      !data.id &&
      data.customerGroups.length > 0 &&
      data.customerIds.length > 0
    ) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "voucher.form.errors.customerOrGroupExclusive",
        path: ["customerGroups"],
      });
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "voucher.form.errors.customerOrGroupExclusive",
        path: ["customerIds"],
      });
    }
  });

const customerGroupOptions: Option[] = [
  { label: CustomerGroup.Loyal, value: CustomerGroup.Loyal },
  { label: CustomerGroup.Normal, value: CustomerGroup.Normal },
];

export function CreateVoucherForm({
  isOpen,
  onClose,
  onCreate,
  onUpdate,
  customers,
  voucher,
  viewMode = false,
}: CreateVoucherFormProps) {
  const t = useTranslations();
  const isEditing = !!voucher && !viewMode;

  // Initialize state with voucher data if editing or viewing
  const [code, setCode] = useState(voucher?.code || "");
  const [title, setTitle] = useState(voucher?.title || "");
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [localImgPreviewUrl, setLocalImgPreviewUrl] = useState<string | null>(
    voucher?.imgUrl || null
  );
  const [discountFixed, setDiscountFixed] = useState(
    voucher?.discountFixed ?? true
  );
  const [discountValue, setDiscountValue] = useState<number>(
    voucher?.discountValue || 0
  );
  const [customerGroups, setCustomerGroups] = useState<Option[]>(
    voucher?.customerGroups.map((group) => ({
      label: group,
      value: group,
    })) || []
  );
  const [description, setDescription] = useState(voucher?.description || "");
  const [startAt, setStartAt] = useState<Date | undefined>(
    voucher?.startAt ? new Date(voucher.startAt) : undefined
  );
  const [endAt, setEndAt] = useState<Date | undefined>(
    voucher?.endAt ? new Date(voucher.endAt) : undefined
  );
  const [status, setStatus] = useState<"Active" | "Inactive">(
    voucher?.status || "Active"
  );
  const [customerIds, setCustomerIds] = useState<Option[]>(
    voucher?.customerIds.map((id) => {
      const customer = customers.find((c) => c.id === id);
      return {
        label: customer?.displayName || id.toString(),
        value: id.toString(),
      };
    }) || []
  );
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formErrors, setFormErrors] = useState<ZodIssue[]>([]);

  // Memoize onCreate and onUpdate with useCallback
  const handleCreate = useCallback(
    async (voucher: NewVoucher) => {
      await onCreate(voucher);
    },
    [onCreate]
  );

  const handleUpdate = useCallback(
    async (voucher: NewVoucher) => {
      if (onUpdate) {
        await onUpdate(voucher);
      }
    },
    [onUpdate]
  );

  // Update form state when voucher prop changes
  useEffect(() => {
    if (voucher) {
      setCode(voucher.code || "");
      setTitle(voucher.title || "");
      setImageFile(voucher.img || null);
      setLocalImgPreviewUrl(
        voucher.imgUrl ||
          (voucher.img ? URL.createObjectURL(voucher.img) : null)
      );
      setDiscountFixed(voucher.discountFixed ?? true);
      setDiscountValue(voucher.discountValue || 0);
      setCustomerGroups(
        voucher.customerGroups.map((group) => ({
          label: group,
          value: group,
        })) || []
      );
      setDescription(voucher.description || "");
      setStartAt(voucher.startAt ? new Date(voucher.startAt) : undefined);
      setEndAt(voucher.endAt ? new Date(voucher.endAt) : undefined);
      setStatus(voucher.status || "Active");
      setCustomerIds(
        voucher.customerIds.map((id) => {
          const customer = customers.find((c) => c.id === id);
          return {
            label: customer?.displayName || id.toString(),
            value: id.toString(),
          };
        }) || []
      );
    }
  }, [voucher, customers]);

  useEffect(() => {
    return () => {
      if (localImgPreviewUrl && !voucher?.imgUrl) {
        URL.revokeObjectURL(localImgPreviewUrl);
      }
    };
  }, [localImgPreviewUrl, voucher?.imgUrl]);

  const getErrorMessage = (path: string) => {
    const error = formErrors.find((err) => err.path[0] === path);
    return error ? t(error.message) : undefined;
  };

  const customerOptions: Option[] = customers.map((item) => ({
    label: item.displayName,
    value: item.id.toString(),
  }));

  const handleImageFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (viewMode) return;
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      setImageFile(file);
      if (localImgPreviewUrl && !voucher?.imgUrl) {
        URL.revokeObjectURL(localImgPreviewUrl);
      }
      setLocalImgPreviewUrl(URL.createObjectURL(file));
      setFormErrors((prev) => prev.filter((err) => err.path[0] !== "img"));
    } else {
      setImageFile(null);
      if (localImgPreviewUrl && !voucher?.imgUrl) {
        URL.revokeObjectURL(localImgPreviewUrl);
      }
      setLocalImgPreviewUrl(null);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    if (viewMode) return;
    e.preventDefault();
    setIsSubmitting(true);
    setFormErrors([]);

    const voucherData = {
      id: voucher?.id,
      code,
      title,
      img: imageFile,
      imgUrl: localImgPreviewUrl || voucher?.imgUrl,
      discountFixed,
      discountValue,
      customerGroups: isEditing
        ? voucher?.customerGroups || []
        : customerGroups.map((opt) => opt.value),
      description,
      startAt,
      endAt,
      status,
      customerIds: isEditing
        ? voucher?.customerIds || []
        : customerIds.map((opt) => Number(opt.value)),
    };

    const validationResult = voucherSchema.safeParse(voucherData);

    if (!validationResult.success) {
      setFormErrors(validationResult.error.issues);
      setIsSubmitting(false);
      return;
    }

    try {
      const formattedData = {
        ...validationResult.data,
        startAt: validationResult.data.startAt.toISOString(),
        endAt: validationResult.data.endAt.toISOString(),
      };

      if (isEditing && onUpdate) {
        await handleUpdate(formattedData as NewVoucher);
      } else {
        await handleCreate(formattedData as NewVoucher);
      }
      resetForm();
      onClose();
    } catch (error) {
    } finally {
      setIsSubmitting(false);
    }
  };

  const resetForm = () => {
    if (viewMode || isEditing) return;
    setCode("");
    setTitle("");
    setImageFile(null);
    if (localImgPreviewUrl && !voucher?.imgUrl) {
      URL.revokeObjectURL(localImgPreviewUrl);
    }
    setLocalImgPreviewUrl(null);
    setDiscountFixed(true);
    setDiscountValue(0);
    setCustomerGroups([]);
    setDescription("");
    setStartAt(undefined);
    setEndAt(undefined);
    setStatus("Active");
    setCustomerIds([]);
    setIsSubmitting(false);
    setFormErrors([]);
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="!w-screen !h-screen max-w-none max-h-none overflow-y-auto p-0">
        <DialogHeader className="sticky top-0 z-10 bg-background p-6 text-primary">
          <DialogTitle>
            {viewMode
              ? t("dialog.detail.title", { entity: t("voucher.title") })
              : isEditing
                ? t("dialog.update.title", { entity: t("voucher.title") })
                : t("dialog.create.title", { entity: t("voucher.title") })}
          </DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-6 p-6">
          <div className="space-y-4">
            <h3 className="text-lg font-medium">
              {t("voucher.form.basicInfo")}
            </h3>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="code" className="text-right">
                {t("voucher.form.code")}
              </Label>
              <Input
                id="code"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                className="col-span-3"
                placeholder={t("voucher.form.codePlaceholder")}
                disabled={viewMode}
                readOnly={viewMode}
              />
              {getErrorMessage("code") && !viewMode && (
                <p className="col-start-2 col-span-3 text-red-500 text-sm">
                  {getErrorMessage("code")}
                </p>
              )}
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="title" className="text-right">
                {t("voucher.form.titleLabel")}
              </Label>
              <Input
                id="title"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                className="col-span-3"
                placeholder={t("voucher.form.titlePlaceholder")}
                disabled={viewMode}
                readOnly={viewMode}
              />
              {getErrorMessage("title") && !viewMode && (
                <p className="col-start-2 col-span-3 text-red-500 text-sm">
                  {getErrorMessage("title")}
                </p>
              )}
            </div>
            <div className="grid grid-cols-4 items-start gap-4">
              <Label htmlFor="description" className="text-right pt-2">
                {t("voucher.form.descriptionLabel")}
              </Label>
              <Textarea
                id="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                className="col-span-3"
                placeholder={t("voucher.form.descriptionPlaceholder")}
                disabled={viewMode}
                readOnly={viewMode}
              />
              {getErrorMessage("description") && !viewMode && (
                <p className="col-start-2 col-span-3 text-red-500 text-sm">
                  {getErrorMessage("description")}
                </p>
              )}
            </div>
          </div>

          <div className="space-y-4">
            <h3 className="text-lg font-medium">
              {t("voucher.form.imageSection")}
            </h3>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="image" className="text-right">
                {t("voucher.form.imageLabel")}
              </Label>
              <div className="col-span-3 space-y-2">
                {!viewMode && (
                  <Input
                    id="image"
                    type="file"
                    accept="image/*"
                    onChange={handleImageFileChange}
                    className="flex-grow"
                    disabled={viewMode}
                  />
                )}
                {(localImgPreviewUrl || voucher?.imgUrl) && (
                  <div className="relative h-32 w-32 overflow-hidden rounded-md border">
                    <Image
                      src={localImgPreviewUrl || voucher?.imgUrl || ""}
                      alt={t("voucher.form.imagePreviewAlt")}
                      layout="fill"
                      objectFit="cover"
                    />
                  </div>
                )}
                {getErrorMessage("img") && !viewMode && (
                  <p className="text-red-500 text-sm">
                    {getErrorMessage("img")}
                  </p>
                )}
              </div>
            </div>
          </div>

          <div className="space-y-4">
            <h3 className="text-lg font-medium">
              {t("voucher.form.discountSection")}
            </h3>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="discountFixed" className="text-right">
                {t("voucher.form.discountType")}
              </Label>
              <div className="col-span-3 flex items-center gap-2">
                <Switch
                  id="discountFixed"
                  checked={discountFixed}
                  onCheckedChange={setDiscountFixed}
                  disabled={viewMode}
                />
                <span className="text-sm text-muted-foreground">
                  {discountFixed
                    ? t("voucher.form.fixedDiscount")
                    : t("voucher.form.percentDiscount")}
                </span>
              </div>
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="discountValue" className="text-right">
                {t("voucher.form.discountValue")}
              </Label>
              <Input
                id="discountValue"
                type="number"
                value={discountValue}
                onChange={(e) =>
                  setDiscountValue(Number.parseFloat(e.target.value))
                }
                className="col-span-3"
                placeholder={t("voucher.form.discountValuePlaceholder")}
                disabled={viewMode}
                readOnly={viewMode}
              />
              {getErrorMessage("discountValue") && !viewMode && (
                <p className="col-start-2 col-span-3 text-red-500 text-sm">
                  {getErrorMessage("discountValue")}
                </p>
              )}
            </div>
          </div>

          <div className="space-y-4">
            <h3 className="text-lg font-medium">
              {t("voucher.form.customerSection")}
            </h3>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="customerGroups" className="text-right">
                {t("voucher.form.customerGroups")}
              </Label>
              <MultiSelect
                options={customerGroupOptions}
                value={customerGroups}
                onChange={setCustomerGroups}
                placeholder={t("voucher.form.customerGroupsPlaceholder")}
                className="col-span-3"
                disabled={viewMode || isEditing}
              />
              {getErrorMessage("customerGroups") && !viewMode && !isEditing && (
                <p className="col-start-2 col-span-3 text-red-500 text-sm">
                  {getErrorMessage("customerGroups")}
                </p>
              )}
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label className="text-right">
                {t("voucher.form.customerIds")}
              </Label>
              <MultiSelect
                options={customerOptions}
                value={customerIds}
                onChange={setCustomerIds}
                placeholder={t("voucher.form.customerIdsPlaceholder")}
                className="col-span-3"
                disabled={viewMode || isEditing}
              />
              {getErrorMessage("customerIds") && !viewMode && !isEditing && (
                <p className="col-start-2 col-span-3 text-red-500 text-sm">
                  {getErrorMessage("customerIds")}
                </p>
              )}
            </div>
          </div>

          <div className="space-y-4">
            <h3 className="text-lg font-medium">
              {t("voucher.form.validitySection")}
            </h3>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="startAt" className="text-right">
                {t("voucher.form.startDate")}
              </Label>
              <div className="col-span-3">
                <CustomDateTime
                  date={startAt}
                  onChange={setStartAt}
                  showSeconds
                  placeholder={t("voucher.form.startDatePlaceholder")}
                  disabled={viewMode}
                />
                {getErrorMessage("startAt") && !viewMode && (
                  <p className="text-red-500 text-sm">
                    {getErrorMessage("startAt")}
                  </p>
                )}
              </div>
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="endAt" className="text-right">
                {t("voucher.form.endDate")}
              </Label>
              <div className="col-span-3">
                <CustomDateTime
                  date={endAt}
                  onChange={setEndAt}
                  showSeconds
                  placeholder={t("voucher.form.endDatePlaceholder")}
                  disabled={viewMode}
                />
                {getErrorMessage("endAt") && !viewMode && (
                  <p className="text-red-500 text-sm">
                    {getErrorMessage("endAt")}
                  </p>
                )}
              </div>
            </div>
          </div>

          <div className="space-y-4">
            <h3 className="text-lg font-medium">
              {t("voucher.form.statusSection")}
            </h3>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="status" className="text-right">
                {t("voucher.form.status")}
              </Label>
              <RadioGroup
                value={status}
                onValueChange={(value: "Active" | "Inactive") =>
                  setStatus(value)
                }
                className="col-span-3 flex gap-4"
                disabled={viewMode}
              >
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="Active" id="status-active" />
                  <Label htmlFor="status-active">
                    {t("voucher.form.statusActive")}
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="Inactive" id="status-inactive" />
                  <Label htmlFor="status-inactive">
                    {t("voucher.form.statusInactive")}
                  </Label>
                </div>
              </RadioGroup>
              {getErrorMessage("status") && !viewMode && (
                <p className="col-start-2 col-span-3 text-red-500 text-sm">
                  {getErrorMessage("status")}
                </p>
              )}
            </div>
          </div>

          {!viewMode && (
            <DialogFooter className="sticky bottom-0 z-10 p-6 bg-background border-t border-secondary flex justify-end space-x-2">
              <Button type="button" variant="outline" onClick={onClose}>
                {t("common.cancel")}
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting
                  ? isEditing
                    ? t("common.loading")
                    : t("common.loading")
                  : isEditing
                    ? t("common.update")
                    : t("common.create")}
              </Button>
            </DialogFooter>
          )}
          {viewMode && (
            <DialogFooter className="sticky bottom-0 z-10 p-6 bg-background border-t border-secondary flex justify-end space-x-2">
              <Button type="button" variant="outline" onClick={onClose}>
                {t("common.close")}
              </Button>
            </DialogFooter>
          )}
        </form>
      </DialogContent>
    </Dialog>
  );
}
