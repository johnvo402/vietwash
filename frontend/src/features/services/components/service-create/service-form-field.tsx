import { useTranslations } from "next-intl";
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { ActivationStatus } from "@/api/generated";
import { CategoryFormQuery } from "@/features/settings/setting-data/category-settings/components/category-form";
import { useListCategoryResponseQuery } from "@/features/settings/setting-data/category-settings/hooks/use-category-data-query";
import CategorySelect from "../category-select";
import TextEditor from "@/components/ui/text-editor";
import { CategoryFormValues } from "@/features/inventories/materials/components/create-material-dialog";

interface ServiceFormFieldsProps {
  form: any; // UseFormReturn<FormValues>
  categoryDialogOpen: boolean;
  setCategoryDialogOpen: (open: boolean) => void;
}

export function ServiceFormFields({
  form,
  categoryDialogOpen,
  setCategoryDialogOpen,
}: ServiceFormFieldsProps) {
  const t = useTranslations();
  const { treeData, parentOptions, isCreating, createCategory, mutations } =
    useListCategoryResponseQuery();

  const handleCategorySubmit = async (formData: CategoryFormValues) => {
    try {
      await createCategory(formData);
      setCategoryDialogOpen(false);
    } catch (error) {
      console.error("Error creating category:", error);
    }
  };

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      <div className="space-y-6">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>
                {t("dialog.name", {
                  Entity: t("common.service").replace(/^./, (c) =>
                    c.toUpperCase()
                  ),
                })}
              </FormLabel>
              <FormControl>
                <Input
                  placeholder={t("dialog.placeholder", {
                    entity: t("common.service"),
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
          name="categoryId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>
                {t("common.category").replace(/^./, (c) => c.toUpperCase())}
              </FormLabel>
              <div className="flex gap-2">
                <CategorySelect
                  treeData={treeData}
                  value={Number(field.value)}
                  onValueChange={(value) => field.onChange(Number(value))}
                  placeholder={t("common.placeholderSelect", {
                    entity: t("common.category"),
                  })}
                />
                <Button
                  variant="outline"
                  size="icon"
                  onClick={() => setCategoryDialogOpen(true)}
                >
                  <Plus className="h-4 w-4" />
                </Button>
                {categoryDialogOpen && (
                  <CategoryFormQuery
                    isOpen={categoryDialogOpen}
                    onClose={() => setCategoryDialogOpen(false)}
                    onSubmit={handleCategorySubmit}
                    initialData={undefined}
                    mode={"create"}
                    parentOptions={parentOptions}
                    defaultParentId={null}
                    isLoading={isCreating}
                    error={mutations.create.error}
                    isSuccess={mutations.create.isSuccess}
                  />
                )}
              </div>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="status"
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
                      <RadioGroupItem value={ActivationStatus.Inactive} />
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
      </div>
      <div className="space-y-6">
        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>{t("common.description")}</FormLabel>
              <FormControl>
                <TextEditor
                  value={field.value}
                  onChange={(value) => field.onChange(value)}
                  className="w-full border rounded-[var(--radius)] focus-within:ring-2 focus-within:ring-ring min-h-[100px]"
                  placeholder={t("common.placeholderDes", {
                    entity: t("common.service"),
                  })}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </div>
    </div>
  );
}
