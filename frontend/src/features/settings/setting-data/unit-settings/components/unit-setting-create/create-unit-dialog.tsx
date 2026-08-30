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
import { ActivationStatus, ListUnitResponse, UnitModel } from "@/api/generated";
import { useEffect } from "react";

interface PageProps {
  name: string;
  onClose: () => void;
  onCreateUnit: (data: UnitModel) => Promise<void>;
  onUpdateUnit?: (data: { id: number; unitData: UnitModel }) => Promise<void>;
  unit?: ListUnitResponse;
  open: boolean;
}
export function CreateUnitDialog({
  name,
  onClose,
  onCreateUnit,
  onUpdateUnit,
  unit,
  open,
}: PageProps) {
  const t = useTranslations();
  const formSchema = z.object({
    name: z.string().min(1, {
      message: t("common.nameRequired", {
        Entity: t("common.unit").replace(/^./, (c) => c.toUpperCase()),
      }),
    }),
    status: z.nativeEnum(ActivationStatus).default(ActivationStatus.Active),
  });
  const isEditMode = !!unit;

  const form = useForm<UnitModel | any>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: "",
      status: ActivationStatus.Active,
    },
  });

  // Sync form with unit prop changes
  useEffect(() => {
    if (isEditMode && unit) {
      form.reset({
        name: unit.name || "",
        status: unit.status,
      });
    } else {
      form.reset({
        name: "",
        status: ActivationStatus.Active,
      });
    }
  }, [unit, isEditMode, form]);

  const onSubmit = async (data: UnitModel) => {
    try {
      if (isEditMode && unit?.id && onUpdateUnit) {
        await onUpdateUnit({ id: unit.id, unitData: data });
      } else {
        await onCreateUnit(data);
      }
      form.reset();
    } catch (error) {
      // Error handled in mutation
    }
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>
            {isEditMode
              ? t("dialog.edit.title", { entity: t("common.unit") })
              : t("dialog.create.title", { entity: t("common.unit") })}
          </DialogTitle>
        </DialogHeader>
        <Form {...form}>
          <form
            id={name}
            onSubmit={form.handleSubmit(onSubmit)}
            className="space-y-6"
          >
            <div className="grid grid-cols-1 gap-4">
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>
                      {t("common.entityName", {
                        Entity: t("common.unit").replace(/^./, (c) =>
                          c.toUpperCase()
                        ),
                      })}
                    </FormLabel>
                    <FormControl>
                      <Input
                        placeholder={t("dialog.placeholder", {
                          entity: t("common.unit"),
                        })}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <div className="flex items-center space-x-2">
                <Switch
                  id="status"
                  checked={form.watch("status") === ActivationStatus.Active}
                  onCheckedChange={(checked) =>
                    form.setValue(
                      "status",
                      checked
                        ? ActivationStatus.Active
                        : ActivationStatus.Inactive
                    )
                  }
                />
                <Label htmlFor="status">{t("common.activate")}</Label>
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
