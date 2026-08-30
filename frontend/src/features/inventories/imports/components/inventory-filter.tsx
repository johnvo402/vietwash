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
import { InventoryStatus } from "@/api/generated";
import { Filter } from "lucide-react";
import DateBadgeSelector from "@/components/core/date-badge-selector";
import { DateRange } from "@/features/reports/types/filter.type";
import { DateUtils } from "@/utils/date.utils";
import MultiSelect, { Option } from "@/components/core/selects/multi-select";
import { useAuth } from "@/hooks/use-auth";

interface InventoryFilterProps {
  statusFilter: Option[];
  setStatusFilter: (value: Option[]) => void;
  dateRange: DateRange | undefined;
  setDateRange: (value: DateRange | undefined) => void;
  branchId: string | null;
  setBranchId: (value: string | null) => void;
}

export function InventoryFilter({
  statusFilter,
  setStatusFilter,
  dateRange,
  setDateRange,
  branchId,
  setBranchId,
}: InventoryFilterProps) {
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const t = useTranslations();
  const { user } = useAuth();
  const [open, setOpen] = useState(false);
  const [tempStatusFilter, setTempStatusFilter] =
    useState<Option[]>(statusFilter);
  const [tempDateRange, setTempDateRange] = useState<DateRange | undefined>(
    dateRange || DateUtils.getDateRange("thisMonth")
  );
  const [tempBranchId, setTempBranchId] = useState<string | null>(branchId);

  const statusOptions: Option[] = [
    { value: InventoryStatus.Pending, label: t("common.status.pending") },
    { value: InventoryStatus.Completed, label: t("common.status.completed") },
    { value: InventoryStatus.Canceled, label: t("common.status.canceled") },
  ];

  const defaultDateRange = DateUtils.getDateRange("thisMonth");

  const handleReset = () => {
    const defaultStatuses = statusOptions.filter(
      (option) => option.value !== InventoryStatus.Canceled
    );
    setTempStatusFilter(defaultStatuses);
    setTempDateRange(defaultDateRange);
    setTempBranchId("");
    setStatusFilter(defaultStatuses);
    setDateRange(defaultDateRange);
    setBranchId("");
    setPage(1);
  };

  const handleApply = () => {
    setStatusFilter(tempStatusFilter);
    setDateRange(tempDateRange);
    setBranchId(tempBranchId);
    setPage(1);
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
          entity: t("inventory.code").toLowerCase(),
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
            defaultPreset={
              tempDateRange?.time || (defaultDateRange.time as any)
            }
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
          <DropdownMenuLabel>{t("common.branch")}</DropdownMenuLabel>
          <Select value={tempBranchId ?? ""} onValueChange={setTempBranchId}>
            <SelectTrigger className="mb-2">
              <SelectValue
                placeholder={t("common.placeholderSelect", {
                  entity: t("common.branch"),
                })}
              />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">{t("common.allBranches")}</SelectItem>
              {user?.branchAccounts.map((branch) => (
                <SelectItem
                  key={branch.branchId}
                  value={String(branch.branchId)}
                >
                  <div className="flex items-center gap-2">
                    <span>{branch.branchName}</span>
                  </div>
                </SelectItem>
              ))}
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
