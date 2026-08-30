// Enhanced form with React Query
"use client";

import type React from "react";
import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AlertCircle, CheckCircle } from "lucide-react";
import { FormMode, ParentOption } from "@/types/tree";
import { ActivationStatus, CreateCategoryCommand } from "@/api/generated";
import { useTranslations } from "next-intl";

interface CategoryFormQueryProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: any) => Promise<void>;
  initialData?: CreateCategoryCommand;
  mode: FormMode;
  parentOptions: ParentOption[];
  defaultParentId?: number | null;
  isLoading?: boolean;
  error?: Error | null;
  isSuccess?: boolean;
}

export function CategoryFormQuery({
  isOpen,
  onClose,
  onSubmit,
  initialData,
  mode,
  parentOptions,
  defaultParentId,
  isLoading = false,
  error,
  isSuccess,
}: CategoryFormQueryProps) {
  const t = useTranslations();

  const FORM_TITLES = {
    create: t("dialog.create.title", { entity: t("common.category") }),
    edit: t("dialog.edit.title", { entity: t("common.category") }),
    "add-child": t("category.addChild"),
  } as const;
  const [formData, setFormData] = useState<CreateCategoryCommand>(
    initialData || {
      name: "",
      parentId: defaultParentId || null,
      status: ActivationStatus.Active,
    }
  );

  const handleSubmit = async (e: React.FormEvent): Promise<void> => {
    e.preventDefault();

    try {
      await onSubmit(formData);
      // Form will close automatically on success via parent component
    } catch (error) {
      // Error is handled by React Query and passed as prop
      console.error("Form submission failed:", error);
    }
  };

  const updateFormData = (updates: Partial<CreateCategoryCommand>): void => {
    setFormData((prev) => ({ ...prev, ...updates }));
  };

  // Reset form when dialog closes
  const handleClose = () => {
    setFormData(
      initialData || {
        name: "",
        parentId: defaultParentId || null,
        status: ActivationStatus.Active,
      }
    );
    onClose();
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>{FORM_TITLES[mode]}</DialogTitle>
        </DialogHeader>

        {/* Success Message */}
        {isSuccess && (
          <Alert className="border-green-200 bg-green-50">
            <CheckCircle className="h-4 w-4 text-green-600" />
            <AlertDescription className="text-green-800">
              {mode === "edit"
                ? t("toast.update.success", { entity: t("common.category") })
                : t("toast.create.success", { entity: t("common.category") })}
            </AlertDescription>
          </Alert>
        )}

        {/* Error Message */}
        {error && (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>
              {error.message || t("common.error")}
            </AlertDescription>
          </Alert>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">{t("common.entityName", { Entity: t("common.category").replace(/^./, (c) => c.toUpperCase()) })} *</Label>
            <Input
              id="name"
              value={formData.name ?? ""}
              onChange={(e) => updateFormData({ name: e.target.value })}
              placeholder={t("dialog.placeholder", { entity: t("common.category") })}
              required
              disabled={isLoading}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="parent">{t("category.parent")}</Label>
            <Select
              value={formData.parentId?.toString() || ""}
              onValueChange={(value) =>
                updateFormData({ parentId: Number(value) || null })
              }
              disabled={isLoading}
            >
              <SelectTrigger>
                <SelectValue placeholder={t("dialog.placeholder", { entity: t("category.parent").toLowerCase() })} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="none">
                  -- {t("category.noParent")} --
                </SelectItem>
                {parentOptions.map((option) => (
                  <SelectItem key={option.id} value={option.id.toString()!}>
                    {option.name} ({option.code})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="flex items-center space-x-2">
            <Switch
              id="status"
              checked={formData.status === ActivationStatus.Active}
              onCheckedChange={(checked) =>
                updateFormData({
                  status: checked
                    ? ActivationStatus.Active
                    : ActivationStatus.Inactive,
                })
              }
              disabled={isLoading}
            />
            <Label htmlFor="status">{t("common.activate")}</Label>
          </div>

          <div className="flex justify-end space-x-2 pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={handleClose}
              disabled={isLoading}
            >
              {t("common.cancel")}
            </Button>
            <Button
              type="submit"
              disabled={isLoading || !formData.name!.trim()}
            >
              {isLoading ? (
                <>
                  <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                  {t("common.status.handling")}...
                </>
              ) : mode === "edit" ? (
                t("common.update")
              ) : (
                t("common.create")
              )}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
