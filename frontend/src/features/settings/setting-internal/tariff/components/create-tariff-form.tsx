"use client";

import * as React from "react";
import { useForm, useFieldArray } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { useTranslations } from "next-intl";
import { useAuth } from "@/hooks/use-auth";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import {
  ActivationStatus,
  CreateTariffCommand,
  TariffModel,
} from "@/api/generated";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { Loader2, Plus, Trash2 } from "lucide-react";
import { useTariff } from "../hooks/use-tariff-hook";
import { formatNumberVN } from "@/utils/format";
import { Combobox } from "@/components/ui/combobox";
import Image from "next/image";

const tariffSchema = z.object({
  branchId: z.string().min(1, { message: "common.entityRequired" }),
  name: z.string().min(1, { message: "common.nameRequired" }),
  status: z.nativeEnum(ActivationStatus),
  startAt: z.string().min(1, { message: "common.entityRequired" }),
  endAt: z.string().min(1, { message: "common.entityRequired" }),
  serviceTariffs: z
    .array(
      z.object({
        serviceId: z.number({
          required_error: "common.entityRequired",
        }),
        unitRelationId: z.number({ required_error: "common.entityRequired" }),
        price: z.number().min(0, { message: "common.priceMustBePositive" }),
      }),
    )
    .min(1, { message: "common.entityRequired" }),
});

type FormData = z.infer<typeof tariffSchema>;

interface Tariff {
  id: number;
  branchId: number;
  branchName: string;
  name: string;
  status: ActivationStatus;
  startAt: string;
  endAt: string;
  serviceTariffs: Array<{
    serviceId: number;
    serviceName: string;
    serviceImageUrl?: string;
    unitRelationId: number;
    unitName: string;
    price: number;
  }>;
}

interface TariffDialogProps {
  isOpen: boolean;
  onClose: () => void;
  tariff?: Tariff | null; // Optional tariff for edit mode
}

export default function TariffDialog({
  isOpen,
  onClose,
  tariff,
}: TariffDialogProps) {
  const t = useTranslations();
  const { user } = useAuth();
  const { createTariff, updateTariff, isLoading } = useTariff({});
  const isEditMode = !!tariff;
  const branches = React.useMemo(
    () => user?.branchAccounts || [],
    [user?.branchAccounts],
  );
  const branchOptions = React.useMemo(
    () =>
      branches.map((branch: any) => ({
        value: branch.branchId?.toString() || "0",
        label: branch.branchName ?? t("common.unknown"),
      })),
    [branches, t],
  );

  const { data: services, isLoading: isServicesLoading } = useQuery({
    queryKey: ["services"],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiServicesGet(
        1,
        1000,
        undefined,
        undefined,
        undefined,
        ["name"],
      );
      return response.data.results?.data || [];
    },
  });

  const serviceOptions = React.useMemo(
    () =>
      services?.map((service: any) => ({
        value: service.id.toString(),
        label: (
          <div className="flex items-center gap-2">
            <Image
              src={service.image || "/logo/favicon.svg"}
              alt={service.name || t("common.unknown")}
              objectFit="cover"
              width={24}
              height={24}
              className="rounded-full"
            />
            <span>{service.name || t("common.unknown")}</span>
          </div>
        ),
      })) || [],
    [services, t],
  );

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    setValue,
    control,
    watch,
    reset,
  } = useForm<FormData>({
    resolver: zodResolver(tariffSchema),
    defaultValues: {
      branchId: "0",
      name: "",
      status: "Active",
      startAt: new Date().toISOString().slice(0, 16),
      endAt: new Date().toISOString().slice(0, 16),
      serviceTariffs: [{ serviceId: 0, unitRelationId: 0, price: 0 }],
    },
    mode: "onSubmit",
  });

  // Reset form when tariff prop changes (for edit mode)
  React.useEffect(() => {
    if (isEditMode && tariff) {
      reset({
        branchId: tariff.branchId.toString(),
        name: tariff.name,
        status: tariff.status,
        startAt: new Date(tariff.startAt).toISOString().slice(0, 16),
        endAt: new Date(tariff.endAt).toISOString().slice(0, 16),
        serviceTariffs: tariff.serviceTariffs.map((st) => ({
          serviceId: st.serviceId,
          unitRelationId: st.unitRelationId,
          price: st.price,
        })),
      });
    } else {
      reset({
        branchId: "0",
        name: "",
        status: "Active",
        startAt: new Date().toISOString().slice(0, 16),
        endAt: new Date().toISOString().slice(0, 16),
        serviceTariffs: [{ serviceId: 0, unitRelationId: 0, price: 0 }],
      });
    }
  }, [tariff, isEditMode, reset]);

  const { fields, prepend, remove } = useFieldArray({
    control,
    name: "serviceTariffs",
  });

  const formData = watch();

  const getUnitOptions = (serviceId: number) => {
    const selectedService = services?.find((s: any) => s.id === serviceId);
    return (
      selectedService?.unitRelations?.map((unit: any) => ({
        value: unit.id.toString(),
        label: unit.name || t("common.unknown"),
        price: unit.price || 0,
      })) || []
    );
  };

  const handleUnitChange = (index: number, value: string) => {
    const unitId = parseInt(value) || 0;
    const unitOptions = getUnitOptions(
      formData.serviceTariffs[index].serviceId,
    );
    const selectedUnit = unitOptions.find((option) => option.value === value);
    setValue(`serviceTariffs.${index}.unitRelationId`, unitId, {
      shouldValidate: true,
    });
    if (selectedUnit?.price) {
      setValue(`serviceTariffs.${index}.price`, selectedUnit.price, {
        shouldValidate: true,
      });
    }
  };

  const addTariff = () => {
    prepend({ serviceId: 0, unitRelationId: 0, price: 0 }, {
      shouldValidate: false,
    } as any); // Tạm thời dùng any để kiểm tra
  };

  const onSubmit = async (data: FormData) => {
    try {
      const tariffData: CreateTariffCommand | TariffModel = {
        branchId: parseInt(data.branchId),
        name: data.name,
        status: data.status,
        startAt: data.startAt,
        endAt: data.endAt,
        serviceTariffs: data.serviceTariffs.map(
          ({ serviceId, unitRelationId, price }) => ({
            serviceId,
            unitRelationId,
            price,
          }),
        ),
      };

      if (isEditMode && tariff) {
        updateTariff({ id: tariff.id, tariffData });
      } else {
        await createTariff.mutateAsync({ tariffData });
      }
      onClose();
    } catch (error) {
      console.error(
        `Error ${isEditMode ? "updating" : "creating"} tariff:`,
        error,
      );
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-full h-screen bg-card p-6 overflow-hidden flex flex-col">
        <DialogHeader className="flex-shrink-0">
          <DialogTitle className="text-2xl text-center">
            {t(`dialog.${isEditMode ? "edit" : "create"}.title`, {
              entity: t("common.tariff"),
            })}
          </DialogTitle>
        </DialogHeader>

        <form
          onSubmit={handleSubmit(onSubmit)}
          className="flex flex-col flex-grow overflow-y-auto"
        >
          {/* General Information */}
          <div className="flex-shrink-0 mb-6">
            <h3 className="text-lg font-semibold mb-4">{t("tariff.info")}</h3>
            <div className="flex flex-wrap gap-4">
              <div className="space-y-2 flex-1 min-w-[200px]">
                <Label htmlFor="name">
                  {t("dialog.name", { Entity: t("common.tariff") })}
                </Label>
                <Input
                  id="name"
                  {...register("name")}
                  placeholder={t("dialog.placeholder", {
                    entity: t("common.tariff").toLowerCase(),
                  })}
                />
                {errors.name?.message && (
                  <p className="text-sm text-destructive">
                    {t(errors.name.message, {
                      Entity: t("common.tariff").toLowerCase(),
                    })}
                  </p>
                )}
              </div>
              <div className="space-y-2 flex-1 min-w-[200px]">
                <Label htmlFor="branchId">{t("Branch")}</Label>
                <Combobox
                  options={branchOptions}
                  value={formData.branchId}
                  onChange={(value) =>
                    setValue("branchId", value, { shouldValidate: true })
                  }
                  placeholder={t("common.entitySelectPlaceholder", {
                    entity: t("common.branch"),
                  })}
                  searchPlaceholder={t("search.searchBy", {
                    entity: t("common.branch"),
                  })}
                  emptyMessage={t("common.noResult")}
                />
                {errors.branchId?.message && (
                  <p className="text-sm text-destructive">
                    {t(errors.branchId.message, {
                      Entity: t("common.branch").toLowerCase(),
                    })}
                  </p>
                )}
              </div>
              <div className="space-y-2 flex-1 min-w-[200px]">
                <Label htmlFor="status">{t("Status")}</Label>
                <Select
                  value={formData.status}
                  onValueChange={(value) =>
                    setValue("status", value as ActivationStatus)
                  }
                >
                  <SelectTrigger>
                    <SelectValue
                      placeholder={t("common.entitySelectPlaceholder", {
                        entity: t("common.status.title"),
                      })}
                    />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Active">
                      {t("common.status.active")}
                    </SelectItem>
                    <SelectItem value="Inactive">
                      {t("common.status.inactive")}
                    </SelectItem>
                  </SelectContent>
                </Select>
                {errors.status?.message && (
                  <p className="text-sm text-destructive">
                    {t(errors.status.message, {
                      Entity: t("common.status.title").toLowerCase(),
                    })}
                  </p>
                )}
              </div>
              <div className="space-y-2 flex-1 min-w-[200px]">
                <Label htmlFor="startAt">
                  {t("table.accessorKey.startAt")}
                </Label>
                <Input
                  id="startAt"
                  type="datetime-local"
                  {...register("startAt")}
                />
                {errors.startAt?.message && (
                  <p className="text-sm text-destructive">
                    {t(errors.startAt.message, {
                      Entity: t("table.accessorKey.startAt").toLowerCase(),
                    })}
                  </p>
                )}
              </div>
              <div className="space-y-2 flex-1 min-w-[200px]">
                <Label htmlFor="endAt">{t("table.accessorKey.endAt")}</Label>
                <Input
                  id="endAt"
                  type="datetime-local"
                  {...register("endAt")}
                />
                {errors.endAt?.message && (
                  <p className="text-sm text-destructive">
                    {t(errors.endAt.message, {
                      Entity: t("table.accessorKey.endAt").toLowerCase(),
                    })}
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* Service Tariffs */}
          <div className="flex-grow overflow-y-auto">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-semibold">
                {t("tariff.list_service")}
              </h3>
              <Button
                variant="outline"
                onClick={addTariff}
                className="flex items-center gap-2"
              >
                <Plus className="h-4 w-4" />
              </Button>
            </div>
            <div className="border rounded-md">
              <table className="w-full table-auto">
                <thead className="sticky top-0 bg-muted z-10">
                  <tr>
                    <th className="px-4 w-10 py-2 text-left">
                      {t("table.accessorKey.index")}
                    </th>
                    <th className="px-4 py-2 text-left">
                      {t("common.service")}
                    </th>
                    <th className="px-4 py-2 text-left">{t("common.unit")}</th>
                    <th className="px-4 w-80 py-2 text-left">
                      {t("common.price")}
                    </th>
                    <th className="px-4 py-2 text-left">{"#"}</th>
                  </tr>
                </thead>
                <tbody>
                  {fields.map((field, index) => (
                    <tr key={field.id} className="border-b">
                      <td className="px-4 py-2">{index + 1}</td>
                      <td className="px-4 py-2">
                        <Combobox
                          options={serviceOptions}
                          value={formData.serviceTariffs[
                            index
                          ].serviceId.toString()}
                          onChange={(value) =>
                            setValue(
                              `serviceTariffs.${index}.serviceId`,
                              parseInt(value) || 0,
                              { shouldValidate: true },
                            )
                          }
                          placeholder={t("common.entitySelectPlaceholder", {
                            entity: t("common.service"),
                          })}
                          searchPlaceholder={t("search.searchBy", {
                            entity: t("common.service"),
                          })}
                          emptyMessage={t("common.noResult")}
                          disabled={isServicesLoading}
                          loading={isServicesLoading}
                        />
                        {errors.serviceTariffs?.[index]?.serviceId?.message && (
                          <p className="text-sm text-destructive mt-1">
                            {t(
                              errors.serviceTariffs[index].serviceId.message!,
                              {
                                Entity: t("common.service"),
                              },
                            )}
                          </p>
                        )}
                      </td>
                      <td className="px-4 py-2">
                        <Combobox
                          options={getUnitOptions(
                            formData.serviceTariffs[index].serviceId,
                          )}
                          value={formData.serviceTariffs[
                            index
                          ].unitRelationId.toString()}
                          onChange={(value) => handleUnitChange(index, value)}
                          placeholder={t("common.entitySelectPlaceholder", {
                            entity: t("common.unit"),
                          })}
                          searchPlaceholder={t("search.searchBy", {
                            entity: t("common.unit"),
                          })}
                          emptyMessage={t("common.noResult")}
                          disabled={
                            isServicesLoading ||
                            !formData.serviceTariffs[index].serviceId
                          }
                        />
                        {errors.serviceTariffs?.[index]?.unitRelationId
                          ?.message && (
                          <p className="text-sm text-destructive mt-1">
                            {t(
                              errors.serviceTariffs[index].unitRelationId
                                .message!,
                              {
                                Entity: t("common.unit"),
                              },
                            )}
                          </p>
                        )}
                      </td>
                      <td className="px-4 py-2">
                        <Input
                          type="text"
                          value={formatNumberVN(
                            formData.serviceTariffs[index].price,
                          )}
                          onChange={(e) => {
                            const value = e.target.value.replace(/[^\d]/g, "");
                            setValue(
                              `serviceTariffs.${index}.price`,
                              parseInt(value) || 0,
                              { shouldValidate: true },
                            );
                          }}
                          placeholder={t("common.placeholderDes", {
                            entity: t("common.price"),
                          })}
                        />
                        {errors.serviceTariffs?.[index]?.price?.message && (
                          <p className="text-sm text-destructive mt-1">
                            {t(errors.serviceTariffs[index].price.message!)}
                          </p>
                        )}
                      </td>
                      <td className="px-4 py-2">
                        <button
                          type="button"
                          onClick={() => remove(index)}
                          disabled={fields.length === 1}
                          className="text-destructive hover:text-destructive/80 disabled:opacity-50"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {errors.serviceTariffs?.message && (
              <p className="text-sm text-destructive mt-2">
                {t(errors.serviceTariffs.message, {
                  Entity: t("common.tariff"),
                })}
              </p>
            )}
          </div>

          {/* Form Actions */}
          <div className="flex justify-end gap-4 sticky bottom-0 bg-card py-4">
            <Button type="button" variant="outline" onClick={onClose}>
              {t("common.cancel")}
            </Button>
            <Button
              type="submit"
              disabled={
                (isEditMode ? isLoading : createTariff.isPending) ||
                isServicesLoading ||
                !isValid
              }
            >
              {(isEditMode ? isLoading : createTariff.isPending) ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                t(isEditMode ? "common.update" : "common.create")
              )}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
