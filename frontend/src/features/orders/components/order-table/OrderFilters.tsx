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
import { CustomerGroup, OrderStatus } from "@/api/generated";
import DateBadgeSelector from "@/components/core/date-badge-selector";
import { DateRange } from "@/features/reports/types/filter.type";
import MultiSelect, { Option } from "@/components/core/selects/multi-select";
import { DateUtils } from "@/utils/date.utils";
import { Filter } from "lucide-react";

interface OrderFiltersProps {
  statusFilter: Option[];
  setStatusFilter: (value: Option[]) => void;
  customerGroupFilter: string;
  setCustomerGroupFilter: (value: string) => void;
  dateRange: DateRange | undefined;
  setDateRange: (value: DateRange | undefined) => void;
  refetch?: () => void;
}

export function OrderFilters({
  statusFilter,
  setStatusFilter,
  customerGroupFilter,
  setCustomerGroupFilter,
  dateRange,
  setDateRange,
  refetch,
}: OrderFiltersProps) {
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const t = useTranslations();
  const [open, setOpen] = useState(false);
  const [tempStatusFilter, setTempStatusFilter] =
    useState<Option[]>(statusFilter);
  const [tempCustomerGroupFilter, setTempCustomerGroupFilter] =
    useState<string>(customerGroupFilter);
  const defaultDateRange = DateUtils.getDateRange("thisMonth");
  const [tempDateRange, setTempDateRange] = useState<DateRange | undefined>(
    dateRange || defaultDateRange
  );
  const statusOptions: Option[] = [
    { value: OrderStatus.Pending, label: t("common.status.pending") },
    { value: OrderStatus.InProgress, label: t("common.status.handling") },
    { value: OrderStatus.Processed, label: t("common.status.handled") },
    { value: OrderStatus.Completed, label: t("common.status.completed") },
    { value: OrderStatus.Cancelled, label: t("common.status.cancelled") },
  ];

  const handleReset = () => {
    const defaultStatuses = statusOptions.filter(
      (option) => option.value !== OrderStatus.Cancelled
    );
    setTempStatusFilter(defaultStatuses);
    setTempCustomerGroupFilter("all");
    setTempDateRange(defaultDateRange);
    setStatusFilter(defaultStatuses);
    setCustomerGroupFilter("all");
    setDateRange(defaultDateRange);
    setPage(1);
    refetch?.();
  };

  const handleApply = () => {
    setStatusFilter(tempStatusFilter);
    setCustomerGroupFilter(tempCustomerGroupFilter);
    setDateRange(tempDateRange);
    setPage(1);
    refetch?.();
    setOpen(false);
  };

  const handleDateChange = (newRange: DateRange) => {
    setTempDateRange(newRange);
  };

  const maxDays = 365;

  return (
    <div className="flex gap-4">
      <DataTableSearch
        placeholder={t("search.searchBy", {
          entity: t("order.orderCode").toLowerCase(),
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
        <DropdownMenuContent className="w-80 p-4 pb-0 max-h-[60vh] overflow-auto">
          <DropdownMenuLabel>{t("dateAndTime.dateRange")}</DropdownMenuLabel>
          <DateBadgeSelector
            maxDays={maxDays}
            defaultPreset={defaultDateRange.time as any}
            onDateChange={handleDateChange}
            className="m-2"
          />
          <DropdownMenuSeparator />
          <DropdownMenuLabel>{t("common.status.title")}</DropdownMenuLabel>
          <MultiSelect
            options={statusOptions}
            value={tempStatusFilter}
            onChange={setTempStatusFilter}
            placeholder={t("common.filterBy", {
              entity: t("common.status.title").replace(/^./, (c) =>
                c.toLowerCase()
              ),
            })}
            className="mb-2"
          />
          <DropdownMenuSeparator />
          <DropdownMenuLabel>{t("user.customerGroup.title")}</DropdownMenuLabel>
          <Select
            value={tempCustomerGroupFilter}
            onValueChange={setTempCustomerGroupFilter}
          >
            <SelectTrigger className="mb-2">
              <SelectValue
                placeholder={t("search.searchBy", {
                  entity: t("user.customerGroup.title").toLowerCase(),
                })}
              />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">
                {t("user.customerGroup.allGroups")}
              </SelectItem>
              <SelectItem value={CustomerGroup.Loyal}>
                {t("customer.loyal")}
              </SelectItem>
              <SelectItem value={CustomerGroup.Normal}>
                {t("customer.normal")}
              </SelectItem>
            </SelectContent>
          </Select>
          <div className="sticky bottom-0 w-full bg-background">
            <DropdownMenuSeparator />
            <div className="flex justify-end gap-2 p-2">
              <Button variant="outline" size="sm" onClick={handleReset}>
                {t("common.reset")}
              </Button>
              <Button size="sm" onClick={handleApply}>
                {t("common.apply")}
              </Button>
            </div>
          </div>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}
