/* eslint-disable react-hooks/exhaustive-deps */
import { useCallback, useEffect, useState, useRef } from "react";
import { useTranslations } from "next-intl";
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
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Plus, Tag, Trash2 } from "lucide-react";
import { ActivationStatus, UnitModel } from "@/api/generated";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { formatNumberVN } from "@/utils/format";
import { UseFormReturn } from "react-hook-form";
import { FormValues, UnitFormValues } from "./create-material-dialog";
import { UseMutateFunction } from "@tanstack/react-query";
import { AxiosResponse } from "axios";

interface UnitRelationsFormProps {
  form: UseFormReturn<FormValues>;
  currentUnits: any[];
  createUnit: UseMutateFunction<
    AxiosResponse<any, any>,
    any,
    UnitModel,
    unknown
  >;
  unitForm: UseFormReturn<UnitFormValues>;
  unitDialogOpen: boolean;
  setUnitDialogOpen: (open: boolean) => void;
}

export function UnitRelationsForm({
  form,
  currentUnits,
  createUnit,
  unitForm,
  unitDialogOpen,
  setUnitDialogOpen,
}: UnitRelationsFormProps) {
  const t = useTranslations();
  const [unitInputModes, setUnitInputModes] = useState<boolean[]>(
    form.getValues("unitRelations").map(() => true),
  );
  const isUpdatingRef = useRef(false); // Prevent recursive updates

  // Add unit relation
  const addUnitRelation = useCallback(() => {
    const currentRelations = form.getValues("unitRelations");
    form.setValue(
      "unitRelations",
      [
        ...currentRelations,
        {
          name: "",
          status: ActivationStatus.Active,
          baseUnit: false,
          price: 0,
          multiple: 1,
          processingTime: 0,
        },
      ],
      { shouldValidate: true },
    );
    setUnitInputModes((prev) => [...prev, true]);
  }, [form]);

  // Remove unit relation
  const removeUnitRelation = useCallback(
    (originalIndex: number) => {
      const currentRelations = form.getValues("unitRelations");
      if (currentRelations.length > 1) {
        form.setValue(
          "unitRelations",
          currentRelations.filter((_, i) => i !== originalIndex),
          { shouldValidate: true },
        );
        setUnitInputModes((prev) => prev.filter((_, i) => i !== originalIndex));
      }
    },
    [form],
  );

  // Handle base unit selection
  const handleBaseUnitChange = useCallback(
    (value: string) => {
      const capitalPrice = form.getValues("capitalPrice");
      const currentRelations = form.getValues("unitRelations");
      const selectedUnit = currentUnits.find((unit) => unit.name === value);

      // Keep only non-base units and add the new base unit
      const newUnitRelations = currentRelations
        .filter((relation) => !relation.baseUnit) // Remove old base unit
        .concat([
          {
            name: value,
            status: ActivationStatus.Active,
            baseUnit: true,
            price: capitalPrice,
            multiple: 1,
            processingTime: 0,
            unitId: selectedUnit?.id, // Set unitId for the base unit
          },
        ]);

      form.setValue("unitRelations", newUnitRelations, {
        shouldValidate: true,
      });
      form.setValue("baseUnitName", value, { shouldValidate: true });

      // Update unitInputModes to match new unitRelations
      setUnitInputModes(newUnitRelations.map(() => true));
    },
    [form, currentUnits],
  );

  // Sync base unit price with capital price and suggest price for non-base units
  useEffect(() => {
    const subscription = form.watch((value, { name }) => {
      if (isUpdatingRef.current) return; // Prevent recursive updates

      const capitalPrice = value.capitalPrice || 0;
      const baseUnitName = value.baseUnitName;
      const unitRelations = value.unitRelations || [];

      if (
        name === "capitalPrice" ||
        name?.startsWith("unitRelations") ||
        name === "baseUnitName"
      ) {
        isUpdatingRef.current = true;

        unitRelations.forEach((relation, index) => {
          if (relation?.name === baseUnitName) {
            // Base unit: set price to capitalPrice and multiple to 1
            const currentPrice = form.getValues(`unitRelations.${index}.price`);
            const currentMultiple = form.getValues(
              `unitRelations.${index}.multiple`,
            );
            if (currentPrice !== capitalPrice) {
              form.setValue(`unitRelations.${index}.price`, capitalPrice, {
                shouldValidate: true,
              });
            }
            if (currentMultiple !== 1) {
              form.setValue(`unitRelations.${index}.multiple`, 1, {
                shouldValidate: true,
              });
            }
          } else if (
            name === `unitRelations.${index}.multiple` &&
            relation?.multiple
          ) {
            // Non-base unit: update price based on capitalPrice * multiple
            const suggestedPrice = capitalPrice * relation.multiple;
            const currentPrice = form.getValues(`unitRelations.${index}.price`);
            if (currentPrice !== suggestedPrice) {
              form.setValue(`unitRelations.${index}.price`, suggestedPrice, {
                shouldValidate: true,
              });
            }
          }
        });

        isUpdatingRef.current = false;
      }
    });
    return () => subscription.unsubscribe();
  }, [form]);

  // Create new unit
  const createNewUnit = useCallback(
    async (data: UnitFormValues) => {
      try {
        await createUnit({ name: data.name, status: ActivationStatus.Active });
        unitForm.reset();
        setUnitDialogOpen(false);
      } catch (error) {
        console.error("Error creating unit:", error);
      }
    },
    [createUnit, unitForm, setUnitDialogOpen],
  );

  // Get available units
  const getAvailableUnits = useCallback(
    (currentIndex: number) => {
      const currentRelations = form.getValues("unitRelations");
      const selectedUnitNames = currentRelations
        .map((relation, index) =>
          index !== currentIndex ? relation.name : null,
        )
        .filter((name) => name !== null && name !== "");

      return (
        currentUnits?.filter(
          (unit: any) => !selectedUnitNames.includes(unit?.name),
        ) || []
      );
    },
    [form, currentUnits],
  );

  // Toggle unit input mode
  const toggleUnitInputMode = useCallback(
    (index: number) => {
      setUnitInputModes((prev) =>
        prev.map((mode, i) => (i === index ? !mode : mode)),
      );
      form.setValue(`unitRelations.${index}.name`, "", {
        shouldValidate: true,
      });
    },
    [form],
  );

  return (
    <div className="space-y-4">
      <div className="flex items-end justify-between">
        <FormField
          control={form.control}
          name="baseUnitName"
          render={({ field }) => (
            <FormItem>
              <FormLabel>
                {t("product.dialog.baseUnit")} (
                {t("product.dialog.primaryUnit")})
              </FormLabel>
              <Select onValueChange={handleBaseUnitChange} value={field.value}>
                <FormControl>
                  <SelectTrigger className="w-full">
                    <SelectValue
                      placeholder={t("common.placeholderSelect", {
                        entity: t("common.unit"),
                      })}
                    />
                  </SelectTrigger>
                </FormControl>
                <SelectContent className="max-h-[200px] overflow-y-auto">
                  {currentUnits.map(
                    (unit, index) =>
                      unit.name && (
                        <SelectItem key={index} value={unit.name}>
                          {unit.name}
                        </SelectItem>
                      ),
                  )}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={addUnitRelation}
        >
          <Plus className="h-4 w-4 mr-2" />
          {t("product.dialog.unit.relation.button")}
        </Button>
      </div>
      {form
        .watch("unitRelations")
        .filter((x) => !x.baseUnit)
        .map((relation, filteredIndex) => {
          // Find the original index in unitRelations
          const originalIndex = form
            .watch("unitRelations")
            .findIndex(
              (r) =>
                r.name === relation.name && r.baseUnit === relation.baseUnit,
            );
          const availableUnits = getAvailableUnits(originalIndex);
          const currentUnitName = relation.name;
          const currentUnitNotAvailable =
            currentUnitName &&
            !availableUnits?.some((unit: any) => unit.name === currentUnitName);

          let displayUnits = availableUnits;
          if (currentUnitNotAvailable && currentUnitName) {
            const currentUnit = currentUnits.find(
              (unit) => unit.name === currentUnitName,
            );
            if (currentUnit) {
              displayUnits = [...(availableUnits || []), currentUnit];
            }
          }

          return (
            <div
              key={originalIndex}
              className="grid grid-cols-1 md:grid-cols-4 gap-4 p-4 border rounded-lg"
            >
              <FormField
                control={form.control}
                name={`unitRelations.${originalIndex}.name`}
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>
                      {t("common.unit").replace(/^./, (c) => c.toUpperCase())}
                    </FormLabel>
                    <div className="flex gap-2">
                      {unitInputModes[originalIndex] ? (
                        <FormControl>
                          <Input
                            placeholder={t("common.placeholderSelect", {
                              entity: t("common.unit"),
                            })}
                            {...field}
                          />
                        </FormControl>
                      ) : (
                        <Select
                          onValueChange={(value) => {
                            const selectedUnit = currentUnits.find(
                              (unit) => unit.name === value,
                            );
                            if (selectedUnit) {
                              field.onChange(selectedUnit.name);
                            }
                          }}
                          value={field.value}
                        >
                          <FormControl>
                            <SelectTrigger className="w-full">
                              <SelectValue
                                placeholder={t("common.placeholderSelect", {
                                  entity: t("common.unit"),
                                })}
                              />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent className="max-h-[200px] overflow-y-auto">
                            {displayUnits?.map((unit: any) => (
                              <SelectItem
                                key={unit?.id?.toString()!}
                                value={unit?.name!}
                              >
                                {unit.name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      )}
                      <Button
                        type="button"
                        variant="outline"
                        size="icon"
                        onClick={() => toggleUnitInputMode(originalIndex)}
                        className="h-11 w-11"
                        aria-label={t("common.toggleUnitInput")}
                      >
                        {unitInputModes[originalIndex] ? (
                          <Tag className="h-4 w-4" />
                        ) : (
                          <Plus className="h-4 w-4" />
                        )}
                      </Button>
                      <Dialog
                        open={unitDialogOpen}
                        onOpenChange={setUnitDialogOpen}
                      >
                        <DialogTrigger asChild>
                          <Button
                            variant="outline"
                            size="icon"
                            className="h-11 w-11"
                            aria-label={t("common.createUnit")}
                          >
                            <Plus className="h-4 w-4" />
                          </Button>
                        </DialogTrigger>
                        <DialogContent>
                          <DialogHeader>
                            <DialogTitle>
                              {t("dialog.create.title", {
                                entity: t("common.unit"),
                              })}
                            </DialogTitle>
                            <DialogDescription>
                              {t("dialog.create.description", {
                                entity: t("common.unit"),
                              })}
                            </DialogDescription>
                          </DialogHeader>
                          <Form {...unitForm}>
                            <form
                              onSubmit={(e) => {
                                e.preventDefault();
                                unitForm.handleSubmit(createNewUnit)(e);
                              }}
                              className="space-y-4"
                            >
                              <FormField
                                control={unitForm.control}
                                name="name"
                                render={({ field }) => (
                                  <FormItem>
                                    <FormLabel>
                                      {t("common.entityName", {
                                        entity: t("common.unit").replace(
                                          /^./,
                                          (c) => c.toUpperCase(),
                                        ),
                                      })}
                                    </FormLabel>
                                    <FormControl>
                                      <Input
                                        placeholder={t("dialog.placeholder", {
                                          entity:
                                            t("common.unit").toLowerCase(),
                                        })}
                                        {...field}
                                      />
                                    </FormControl>
                                    <FormMessage />
                                  </FormItem>
                                )}
                              />
                              <DialogFooter>
                                <Button type="submit">
                                  {t("common.create")}
                                </Button>
                              </DialogFooter>
                            </form>
                          </Form>
                        </DialogContent>
                      </Dialog>
                    </div>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name={`unitRelations.${originalIndex}.multiple`}
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>
                      {t("product.dialog.unit.multiple.title")}
                    </FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        min="1"
                        step="1"
                        value={field.value}
                        placeholder="1"
                        onChange={(e) => {
                          const val = Number(e.target.value);
                          if (val >= 1) {
                            field.onChange(val);
                          }
                        }}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name={`unitRelations.${originalIndex}.price`}
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("common.price")}</FormLabel>
                    <FormControl>
                      <Input
                        type="text"
                        min="0"
                        step="1"
                        value={formatNumberVN(field.value)}
                        placeholder="0.00"
                        onChange={(e) => {
                          const val = e.target.value.replace(/\D/g, "");
                          field.onChange(val);
                        }}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <div className="flex items-end space-x-2">
                <FormField
                  control={form.control}
                  name={`unitRelations.${originalIndex}.status`}
                  render={({ field }) => (
                    <FormItem className="space-y-3">
                      <FormLabel>{t("common.status.title")}</FormLabel>
                      <FormControl>
                        <RadioGroup
                          onValueChange={(value) => field.onChange(value)}
                          defaultValue={field.value.toString()}
                          className="flex flex-col space-y-1"
                        >
                          <FormItem className="flex items-center space-x-3 space-y-0">
                            <FormControl>
                              <RadioGroupItem value={ActivationStatus.Active} />
                            </FormControl>
                            <FormLabel className="font-normal text-green-500">
                              {t("common.status.active")}
                            </FormLabel>
                          </FormItem>
                          <FormItem className="flex items-center space-x-3 space-y-0">
                            <FormControl>
                              <RadioGroupItem
                                value={ActivationStatus.Inactive}
                              />
                            </FormControl>
                            <FormLabel className="font-normal text-destructive">
                              {t("common.status.inactive")}
                            </FormLabel>
                          </FormItem>
                        </RadioGroup>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  onClick={() => removeUnitRelation(originalIndex)}
                  disabled={form.watch("unitRelations").length <= 1}
                  className="ml-auto h-11 w-11"
                  aria-label={t("common.removeItem", {
                    item: t("common.unit"),
                  })}
                >
                  <Trash2 className="h-4 w-4 text-destructive" />
                </Button>
              </div>
            </div>
          );
        })}
    </div>
  );
}
