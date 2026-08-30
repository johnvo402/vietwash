"use client";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTranslations } from "next-intl";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { useState } from "react";
import { ActivationStatus, OrderStatus } from "@/api/generated";
import { Filter } from "lucide-react";
import { Option } from "@/components/core/selects/multi-select";
import CategorySelect from "@/features/services/components/category-select";

interface MaterialFilterProps {
  statusFilter: Option[];
  setStatusFilter: (value: Option[]) => void;
  categoryId: number | null;
  setCategoryId: (value: number | null) => void;
  treeData: any; // Replace with proper type for your category tree data
  refetch?: () => void;
}

export function MaterialFilter({
  statusFilter,
  setStatusFilter,
  categoryId,
  setCategoryId,
  treeData,
  refetch,
}: MaterialFilterProps) {
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const t = useTranslations();
  const [open, setOpen] = useState(false);
  const [tempStatusFilter, setTempStatusFilter] =
    useState<Option[]>(statusFilter);
  const [tempCategoryId, setTempCategoryId] = useState<number | null>(
    categoryId
  );

  const statusOptions: Option[] = [
    { value: ActivationStatus.Active, label: t("common.status.active") },
    { value: ActivationStatus.Inactive, label: t("common.status.inactive") },
  ];

  const handleReset = () => {
    const defaultStatuses = statusOptions.filter(
      (option) => option.value !== OrderStatus.Cancelled
    );
    setTempStatusFilter(defaultStatuses);
    setTempCategoryId(null);
    setStatusFilter(defaultStatuses);
    setCategoryId(null);
    setPage(1);
    refetch?.();
  };

  const handleApply = () => {
    setStatusFilter(tempStatusFilter);
    setCategoryId(tempCategoryId);
    setPage(1);
    refetch?.();
    setOpen(false);
  };

  return (
    <div className="flex gap-4">
      <DataTableSearch
        placeholder={t("search.searchBy", {
          entity: t("product.productName").toLowerCase() + ", sku",
        })}
        searchQuery={searchQuery}
        setSearchQuery={setSearchQuery}
        setPage={setPage}
      />

      <DropdownMenu open={open} onOpenChange={setOpen}>
        <DropdownMenuTrigger asChild>
          <Button variant="outline">
            <Filter className="h-5 w-5" />
            {t("table.accessorKey.filter")}
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent className="w-80 p-4">
          <DropdownMenuLabel>{t("common.status.title")}</DropdownMenuLabel>
          <Select
            value={tempStatusFilter[0]?.value || ""}
            onValueChange={(value) => {
              const selected = statusOptions.find(
                (option) => option.value === value
              );
              setTempStatusFilter(selected ? [selected] : []);
            }}
          >
            <SelectTrigger className="mb-2">
              <SelectValue
                placeholder={t("common.filterBy", {
                  entity: t("common.status.title").toLowerCase(),
                })}
              />
            </SelectTrigger>
            <SelectContent>
              {statusOptions.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <DropdownMenuSeparator />
          <DropdownMenuLabel>{t("common.category")}</DropdownMenuLabel>
          <CategorySelect
            treeData={treeData}
            value={tempCategoryId!}
            onValueChange={(value) => setTempCategoryId(Number(value))}
            placeholder={t("common.placeholderSelect", {
              entity: t("common.category"),
            })}
            className="mb-2"
          />
          <DropdownMenuSeparator />
          <div className="flex justify-end gap-2 p-2">
            <Button variant="outline" size="sm" onClick={handleReset}>
              {t("common.reset")}
            </Button>
            <Button size="sm" onClick={handleApply}>
              {t("common.apply")}
            </Button>
          </div>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}
