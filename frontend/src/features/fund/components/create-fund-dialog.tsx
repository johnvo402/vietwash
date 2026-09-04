/* eslint-disable react-hooks/exhaustive-deps */
"use client";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { useTranslations } from "next-intl";
import { useStringUtil } from "@/lib/stringUtil";
import { FundType, ListFundBehaviorResponse } from "@/api/generated/api";
import { FormEvent, useCallback, useState } from "react";
import { z } from "zod";
import { ZodError } from "zod";
import dynamic from "next/dynamic";
import { useAuth } from "@/hooks/use-auth";
import { formatNumberVN, parseNumberVN } from "@/utils/format";

const TextEditor = dynamic(() => import("@/components/ui/text-editor"), {
  ssr: false,
});

interface FundCreatePopupProps {
  isOpen: boolean;
  onClose: () => void;
  fundBehaviors: ListFundBehaviorResponse[];
  onSubmit: (data: {
    type: FundType;
    amount: number;
    fundBehaviorId: number;
    note: string;
    paymentMethod: string;
    branchId: number;
  }) => Promise<void>;
  isUpdate?: boolean;
  initialData?: {
    type: FundType;
    amount: number;
    fundBehaviorId: number;
    note: string;
    paymentMethod: string;
    branchId: number;
  };
  loading: boolean; // Added loading prop
}

export function FundCreatePopup({
  isOpen,
  onClose,
  fundBehaviors,
  onSubmit,
  isUpdate = false,
  initialData,
  loading,
}: FundCreatePopupProps) {
  const t = useTranslations();
  const { textByLang } = useStringUtil();
  const { user, branchActive } = useAuth();

  // Zod schema for validation
  const fundSchema = z.object({
    type: z.nativeEnum(FundType, {
      errorMap: () => ({ message: t("fund.type.title") }), // "Loại giao dịch"
    }),
    amount: z.number().positive({
      message: t("common.entityInvalid").replace(
        "{Entity}",
        t("table.accessorKey.amount"),
      ),
    }), // "Số tiền không hợp lệ"
    fundBehaviorId: z
      .number()
      .positive({ message: t("fund.selectPlacehodelrBehavior") }), // "Chọn hành vi"
    note: z
      .string()
      .max(255, { message: t("common.maxLength").replace("{max}", "255") }), // "Số ký tự không vượt quá 255"
    paymentMethod: z.enum(["Cash", "Bank", "Card"], {
      errorMap: () => ({ message: t("inventory.validation.paymentMethod") }), // "Vui lòng chọn phương thức thanh toán"
    }),
    branchId: z.number().positive({
      message: t("common.entityInvalid").replace(
        "{Entity}",
        t("common.branch"),
      ),
    }), // "Chi nhánh không hợp lệ"
  });

  const [formData, setFormData] = useState({
    type: initialData?.type || ("Income" as FundType),
    amount: initialData?.amount || 0,
    fundBehaviorId: initialData?.fundBehaviorId || 5,
    note: initialData?.note || "",
    paymentMethod: initialData?.paymentMethod || ("Cash" as "Cash" | "Card"),
    branchId: initialData?.branchId || branchActive?.branchId || 0,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleSubmit = useCallback(
    async (e: FormEvent) => {
      e.preventDefault();
      try {
        const validatedData = fundSchema.parse({
          ...formData,
          amount: Number(formData.amount),
          fundBehaviorId: Number(formData.fundBehaviorId),
          branchId: Number(formData.branchId),
        });
        setErrors({});
        await onSubmit(validatedData);
      } catch (error) {
        if (error instanceof ZodError) {
          const fieldErrors: Record<string, string> = {};
          error.errors.forEach((err) => {
            if (err.path[0]) {
              fieldErrors[err.path[0]] = err.message;
            }
          });
          setErrors(fieldErrors);
        }
      } finally {
        handleClose();
      }
    },
    [formData, onSubmit, t],
  );

  // Handle fundBehaviorId change and update type accordingly
  const handleFundBehaviorChange = (value: string) => {
    const selectedBehavior = fundBehaviors.find(
      (behavior) => behavior.id!.toString() === value,
    );
    setFormData({
      ...formData,
      fundBehaviorId: Number(value),
      type: selectedBehavior?.type ?? "Income", // Fallback to "Income" if type is undefined
    });
  };

  const handleClose = () => {
    setFormData({
      amount: 0,
      branchId: branchActive?.branchId!,
      fundBehaviorId: 5,
      note: "",
      paymentMethod: "Cash",
      type: "Income",
    });
    onClose();
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="max-w-3xl w-full p-6 bg-card text-card-foreground rounded-[var(--radius)] shadow-xl">
        <DialogHeader>
          <DialogTitle className="text-2xl font-bold">
            {isUpdate
              ? t("dialog.update.title", {
                  entity: t("fund.title").toLowerCase(),
                }) // "Cập nhật Quỹ"
              : t("dialog.create.title", {
                  entity: t("fund.title").toLowerCase(),
                })}
            {/* "Tạo mới Quỹ" */}
          </DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
            {/* Left Column */}
            <div className="space-y-6">
              <div>
                <Label className="block text-sm font-medium">
                  {t("common.branch")} {/* "Chi nhánh" */}
                </Label>
                <Select
                  value={formData.branchId.toString()}
                  onValueChange={(value) =>
                    setFormData({ ...formData, branchId: Number(value) })
                  }
                  disabled={isUpdate || loading}
                >
                  <SelectTrigger className="w-full border rounded-[var(--radius)] focus:ring-2 focus:ring-ring">
                    <SelectValue
                      placeholder={t("common.entitySelectPlaceholder").replace(
                        "{entity}",
                        t("common.branch"),
                      )}
                    />{" "}
                    {/* "Chọn chi nhánh" */}
                  </SelectTrigger>
                  <SelectContent>
                    {user?.branchAccounts?.map((branch) => (
                      <SelectItem
                        key={branch.branchId}
                        value={branch.branchId.toString()}
                      >
                        {branch.branchName || t("common.nA")}{" "}
                        {/* "Không áp dụng" */}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {errors.branchId && (
                  <p className="text-destructive text-xs mt-1">
                    {errors.branchId}
                  </p>
                )}
              </div>

              <div>
                <Label className="block text-sm font-medium">
                  {t("table.accessorKey.amount")} {/* "Số tiền" */}
                </Label>
                <Input
                  type="text"
                  value={formatNumberVN(formData.amount)}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      amount: parseNumberVN(e.target.value),
                    })
                  }
                  className="w-full border rounded-[var(--radius)] focus:ring-2 focus:ring-ring"
                  placeholder={t("common.placeholder").replace(
                    "{entity}",
                    t("table.accessorKey.amount").toLowerCase(),
                  )}
                  disabled={isUpdate || loading}
                />
                {errors.amount && (
                  <p className="text-destructive text-xs mt-1">
                    {errors.amount}
                  </p>
                )}
              </div>
            </div>

            {/* Right Column */}
            <div className="space-y-6">
              <div>
                <Label className="block text-sm font-medium">
                  {t("fund.behavior")} {/* "Hành vi" */}
                </Label>
                <Select
                  value={formData.fundBehaviorId?.toString() ?? "5"}
                  onValueChange={handleFundBehaviorChange}
                  disabled={isUpdate || loading}
                >
                  <SelectTrigger className="w-full border rounded-[var(--radius)] focus:ring-2 focus:ring-ring">
                    <SelectValue
                      placeholder={t("fund.selectPlacehodelrBehavior")}
                    />
                  </SelectTrigger>
                  <SelectContent>
                    {(isUpdate
                      ? fundBehaviors
                      : fundBehaviors.filter((x) => [5, 6].includes(x.id!))
                    ).map((behavior) => (
                      <SelectItem
                        key={behavior.id}
                        value={behavior.id!.toString()}
                      >
                        {textByLang(JSON.parse(behavior.name)) || "--"}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {errors.fundBehaviorId && (
                  <p className="text-destructive text-xs mt-1">
                    {errors.fundBehaviorId}
                  </p>
                )}
              </div>

              <div>
                <Label className="block text-sm font-medium">
                  {t("fund.paymentMethod.title")}{" "}
                  {/* "Phương thức thanh toán" */}
                </Label>
                <Select
                  value={formData.paymentMethod}
                  onValueChange={(value) =>
                    setFormData({
                      ...formData,
                      paymentMethod: value as "Cash" | "Card",
                    })
                  }
                  disabled={loading}
                >
                  <SelectTrigger className="w-full border rounded-[var(--radius)] focus:ring-2 focus:ring-ring">
                    <SelectValue placeholder={t("order.selectPaymentMethod")} />{" "}
                    {/* "Chọn phương thức thanh toán" */}
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Cash">
                      {t("fund.paymentMethod.cash")} {/* "Tiền mặt" */}
                    </SelectItem>
                    <SelectItem value="Card">
                      {t("fund.paymentMethod.card")} {/* "Thẻ" */}
                    </SelectItem>
                  </SelectContent>
                </Select>
                {errors.paymentMethod && (
                  <p className="text-destructive text-xs mt-1">
                    {errors.paymentMethod}
                  </p>
                )}
              </div>
            </div>
          </div>

          <div className="mt-6">
            <Label className="block text-sm font-medium">
              {t("common.note")} {/* "Ghi chú" */}
            </Label>
            <TextEditor
              value={formData.note}
              onChange={(value) => setFormData({ ...formData, note: value })}
              className="w-full border rounded-[var(--radius)] focus-within:ring-2 focus-within:ring-ring min-h-[100px]"
              placeholder={t("common.placeholderDes").replace(
                "{entity}",
                t("common.note"),
              )}
            />
            {errors.note && (
              <p className="text-destructive text-xs mt-1">{errors.note}</p>
            )}
          </div>

          <DialogFooter className="mt-6 flex justify-end space-x-4">
            <Button
              type="button"
              variant="outline"
              onClick={onClose}
              className="border rounded-[var(--radius)] text-foreground hover:bg-muted"
              disabled={loading}
            >
              {t("common.cancel")} {/* "Hủy" */}
            </Button>
            <Button
              type="submit"
              className="bg-primary text-primary-foreground hover:bg-primary/90 rounded-[var(--radius)]"
              disabled={loading}
            >
              {loading
                ? t("common.loading") // "Đang xử lý"
                : isUpdate
                  ? t("common.update") // "Cập nhật"
                  : t("common.create")}{" "}
              {/* "Tạo mới" */}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
