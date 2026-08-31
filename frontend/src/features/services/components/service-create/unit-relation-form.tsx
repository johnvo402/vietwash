import { useCallback, useState } from "react";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Plus, Tag, Trash2, ChevronsUpDown } from "lucide-react";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { ActivationStatus, ListBranchProductResponse } from "@/api/generated";
import { useUnitSettings } from "@/features/settings/setting-data/unit-settings/hooks/use-unit-hook";
import { formatNumberVN, parseNumberVN } from "@/utils/format";
import { UseFormReturn, useFieldArray } from "react-hook-form";
import { FormValues } from "./create-service-dialog";
import { ServiceResourcesForm } from "./service-resource-form";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";

type UnitRelation = FormValues["unitRelations"][number];

interface UnitRelationsFormProps {
  form: UseFormReturn<FormValues>;
  unitDialogOpen: boolean;
  setUnitDialogOpen: (open: boolean) => void;
  products: ListBranchProductResponse[];
  isProductsLoading: boolean;
  productsError: any;
  fetchNextProducts: () => void;
  hasMoreProducts: boolean | undefined;
  setProductSearch: (search: string) => void;
  unitFields: (UnitRelation & { id: string })[];
  appendUnit: (v: Partial<UnitRelation>) => void;
  removeUnit: (index: number) => void;
}

export function UnitRelationsForm({
  form,
  unitDialogOpen,
  setUnitDialogOpen,
  products,
  isProductsLoading,
  productsError,
  fetchNextProducts,
  hasMoreProducts,
  setProductSearch,
  unitFields,
  appendUnit,
  removeUnit,
}: UnitRelationsFormProps) {
  const t = useTranslations();
  const { units: currentUnits, createUnit } = useUnitSettings();

  // id-based UI states
  const [unitInputModes, setUnitInputModes] = useState<Record<string, boolean>>(
    () => Object.fromEntries(unitFields.map((f, i) => [f.id, i !== 0])),
  );
  const [openRelations, setOpenRelations] = useState<Record<string, boolean>>(
    () => (unitFields[0] ? { [unitFields[0].id]: true } : {}),
  );

  const addUnitRelation = useCallback(() => {
    appendUnit({
      name: "",
      status: ActivationStatus.Active,
      baseUnit: false,
      price: 0,
      multiple: 1,
      processingTime: 0,
      serviceResources: [{ productId: 0, unitProductId: 0, quantity: 1 }],
    });
  }, [appendUnit]);

  const removeUnitRelation = useCallback(
    (index: number, id: string) => {
      const currentRelations = form.getValues("unitRelations");
      if (currentRelations.length > 1) {
        removeUnit(index);
        setUnitInputModes((prev) => {
          const { [id]: _, ...rest } = prev;
          return rest;
        });
        setOpenRelations((prev) => {
          const { [id]: _, ...rest } = prev;
          return rest;
        });
      }
    },
    [form, removeUnit],
  );

  const createNewUnit = useCallback(
    async (data: { name: string }) => {
      try {
        await createUnit({ name: data.name, status: ActivationStatus.Active });
        setUnitDialogOpen(false);
      } catch (error) {
        console.error("Error creating unit:", error);
      }
    },
    [createUnit, setUnitDialogOpen],
  );

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

  const toggleUnitInputMode = useCallback(
    (id: string, index: number) => {
      setUnitInputModes((prev) => ({ ...prev, [id]: !prev[id] }));
      form.setValue(`unitRelations.${index}.name`, "", {
        shouldValidate: false,
      });
    },
    [form],
  );

  const RelationCard = ({
    relation,
    originalIndex,
    id,
  }: {
    relation: UnitRelation;
    originalIndex: number;
    id: string;
  }) => {
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
      if (currentUnit) displayUnits = [...(availableUnits || []), currentUnit];
    }

    // UseFieldArray for serviceResources
    const {
      fields: resFields,
      append: appendRes,
      remove: removeRes,
    } = useFieldArray({
      control: form.control,
      name: `unitRelations.${originalIndex}.serviceResources`,
    });

    // Calculate totalResourceCost using form.getValues instead of form.watch
    const serviceResources =
      form.getValues(`unitRelations.${originalIndex}.serviceResources`) || [];
    const totalResourceCost =
      serviceResources.reduce((total, resource) => {
        const product = products.find((item) => item.id === resource.productId);
        if (!product) return total;

        const unit = (product.unitRelations || []).find(
          (item) => Number(item.id) === Number(resource.unitProductId),
        );
        return total + (unit?.price ?? 0) * resource.quantity;
      }, 0) ?? 0;

    // Get unitPrice using form.getValues
    const unitPrice =
      form.getValues(`unitRelations.${originalIndex}.price`) || 0;

    // Warning logic
    const showPriceWarning =
      unitPrice <= totalResourceCost && totalResourceCost > 0;

    return (
      <div className="p-4 border rounded-2xl space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* Unit name */}
          <FormField
            control={form.control}
            name={`unitRelations.${originalIndex}.name`}
            render={({ field }) => (
              <FormItem>
                <FormLabel>
                  {t("common.unit")}{" "}
                  {originalIndex === 0 && (
                    <span className="ml-1 text-xs text-muted-foreground">
                      ({t("service.dialog.primaryUnit")})
                    </span>
                  )}
                </FormLabel>
                <div className="flex gap-2">
                  {unitInputModes[id] ? (
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
                        if (selectedUnit) field.onChange(selectedUnit.name);
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
                    onClick={() => toggleUnitInputMode(id, originalIndex)}
                  >
                    {unitInputModes[id] ? (
                      <Tag className="h-4 w-4" />
                    ) : (
                      <Plus className="h-4 w-4" />
                    )}
                  </Button>
                </div>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Price */}
          <FormField
            control={form.control}
            name={`unitRelations.${originalIndex}.price`}
            render={({ field }) => (
              <FormItem>
                <FormLabel>
                  {t("common.price")}
                  {showPriceWarning && (
                    <span className="text-destructive ml-2">
                      {t("service.priceWarning", {
                        totalCost: formatNumberVN(totalResourceCost),
                      })}
                    </span>
                  )}
                </FormLabel>
                <FormControl>
                  <Input
                    type="text"
                    placeholder="0"
                    value={formatNumberVN(field.value)}
                    onChange={(e) => {
                      field.onChange(parseNumberVN(e.target.value));
                    }}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Processing Time */}
          <FormField
            control={form.control}
            name={`unitRelations.${originalIndex}.processingTime`}
            render={({ field }) => (
              <FormItem>
                <FormLabel>{t("service.processingTime.title")}</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min="0"
                    step="1"
                    value={field.value}
                    onChange={(e) => {
                      const val = parseInt(e.target.value, 10);
                      field.onChange(Number.isNaN(val) ? 0 : val);
                    }}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Status + Delete */}
          <div className="flex items-end space-x-2">
            <FormField
              control={form.control}
              name={`unitRelations.${originalIndex}.status`}
              disabled={form.getValues(
                `unitRelations.${originalIndex}.baseUnit`,
              )}
              render={({ field }) => (
                <FormItem className="space-y-3">
                  <FormLabel>{t("common.status.title")}</FormLabel>
                  <FormControl>
                    <RadioGroup
                      onValueChange={(value) => field.onChange(Number(value))}
                      value={field.value?.toString()}
                      className="flex flex-col space-y-1"
                      disabled={form.getValues(
                        `unitRelations.${originalIndex}.baseUnit`,
                      )}
                    >
                      <FormItem className="flex items-center space-x-3 space-y-0">
                        <FormControl>
                          <RadioGroupItem
                            value={ActivationStatus.Active.toString()}
                          />
                        </FormControl>
                        <FormLabel className="font-normal text-green-500">
                          {t("common.status.active")}
                        </FormLabel>
                      </FormItem>
                      <FormItem className="flex items-center space-x-3 space-y-0">
                        <FormControl>
                          <RadioGroupItem
                            value={ActivationStatus.Inactive.toString()}
                          />
                        </FormControl>
                        <FormLabel className="font-normal text-destructive">
                          {t("common.status.inactive")}
                        </FormLabel>
                      </FormItem>
                    </RadioGroup>
                  </FormControl>
                </FormItem>
              )}
            />
            {originalIndex > 0 && (
              <Button
                type="button"
                variant="ghost"
                size="icon"
                onClick={() => removeUnitRelation(originalIndex, id)}
                className="ml-auto"
              >
                <Trash2 className="h-4 w-4 text-destructive" />
              </Button>
            )}
          </div>
        </div>

        {/* Unit-level collapsible for Service Resources */}
        <Collapsible
          open={!!openRelations[id]}
          onOpenChange={(o) =>
            setOpenRelations((prev) => ({ ...prev, [id]: o }))
          }
        >
          <div className="flex items-center justify-between rounded-md border px-3 py-2">
            <div className="text-sm font-medium">
              {t("service.resources.title")} • {t("table.accessorKey.quantity")}
              : <span className="font-semibold">{resFields.length}</span>
            </div>
            <CollapsibleTrigger asChild>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="h-8 w-8"
              >
                <ChevronsUpDown className="h-4 w-4" />
              </Button>
            </CollapsibleTrigger>
          </div>

          <CollapsibleContent>
            <ServiceResourcesForm
              form={form}
              unitIndex={originalIndex}
              products={products}
              isProductsLoading={isProductsLoading}
              productsError={productsError}
              fetchNextProducts={fetchNextProducts}
              hasMoreProducts={hasMoreProducts}
              setProductSearch={setProductSearch}
              resFields={resFields as any}
              appendRes={appendRes}
              removeRes={removeRes}
            />
          </CollapsibleContent>
        </Collapsible>
      </div>
    );
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-base font-semibold">
          {t("service.dialog.unit.relation.title", { fallback: "Units" })}
        </h3>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={addUnitRelation}
        >
          <Plus className="h-4 w-4 mr-2" />
          {t("service.dialog.unit.relation.button")}
        </Button>
      </div>

      {unitFields.map((relation, index) => (
        <RelationCard
          key={relation.id}
          relation={relation}
          originalIndex={index}
          id={relation.id}
        />
      ))}

      {/* Dialog tạo Unit mới */}
      <Dialog open={unitDialogOpen} onOpenChange={setUnitDialogOpen}>
        <DialogTrigger asChild>
          <span />
        </DialogTrigger>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {t("dialog.create.title", {
                entity: t("common.unit").toLowerCase(),
              })}
            </DialogTitle>
            <DialogDescription>
              {t("dialog.create.description", {
                entity: t("common.unit").toLowerCase(),
              })}
            </DialogDescription>
          </DialogHeader>
          <Form {...form}>
            <form
              onSubmit={(e) => {
                e.preventDefault();
                const formData = new FormData(e.currentTarget);
                const name = (formData.get("name") as string) || "";
                createNewUnit({ name });
              }}
              className="space-y-4"
            >
              <FormItem>
                <FormLabel>
                  {t("common.entityName", {
                    Entity: t("common.unit").toLowerCase(),
                  })}
                </FormLabel>
                <FormControl>
                  <Input
                    name="name"
                    placeholder={t("dialog.placeholder", {
                      entity: t("common.unit").toLowerCase(),
                    })}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
              <DialogFooter>
                <Button type="submit">{t("common.create")}</Button>
              </DialogFooter>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
