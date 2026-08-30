"use client";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { useTranslations } from "next-intl";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { useState, useEffect } from "react";
import { FundStatus, ListFundBehaviorResponse } from "@/api/generated/api";
import { useStringUtil } from "@/lib/stringUtil";
import { DateRange } from "@/features/reports/types/filter.type";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import DateBadgeSelector from "@/components/core/date-badge-selector";
import { Filter } from "lucide-react";
import MultiSelect, { Option } from "@/components/core/selects/multi-select";

interface FundFiltersProps {
  statusFilter: Option[];
  setStatusFilter: (value: Option[]) => void;
  dateRange: DateRange | undefined;
  setDateRange: (value: DateRange | undefined) => void;
  behaviorId: number | undefined;
  setBehaviorId: (value: number | undefined) => void;
  fundBehaviors: ListFundBehaviorResponse[];
  type: string | undefined;
  setType: (value: string | undefined) => void;
  refetch?: () => void;
}

export function FundFilters({
  statusFilter,
  setStatusFilter,
  dateRange,
  setDateRange,
  behaviorId,
  setBehaviorId,
  fundBehaviors,
  type,
  setType,
  refetch,
}: FundFiltersProps) {
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const t = useTranslations();
  const { textByLang } = useStringUtil();
  const [open, setOpen] = useState(false);
  const [tempDateRange, setTempDateRange] = useState<DateRange | undefined>(
    dateRange
  );
  const [tempBehaviorId, setTempBehaviorId] = useState<number | undefined>(
    behaviorId
  );
  const [tempType, setTempType] = useState<string | undefined>(type);
  const [tempStatusFilter, setTempStatusFilter] =
    useState<Option[]>(statusFilter);

  // Chuyển logic reset tempBehaviorId vào useEffect để tránh gọi trong render
  useEffect(() => {
    const filteredFundBehaviors = tempType
      ? fundBehaviors.filter((behavior) => behavior.type === tempType)
      : fundBehaviors;
    const isValidBehaviorId = filteredFundBehaviors.some(
      (behavior) => behavior.id === tempBehaviorId
    );
    if (tempBehaviorId && !isValidBehaviorId) {
      setTempBehaviorId(undefined);
      console.log(
        "FundFilters: Reset tempBehaviorId to undefined due to invalid ID"
      );
    }
  }, [tempType, tempBehaviorId, fundBehaviors]);

  const handleReset = () => {
    const defaultStatuses = statusOptions.filter(
      (option) => option.value !== FundStatus.Cancelled
    );
    setTempStatusFilter(defaultStatuses);
    setTempDateRange(undefined);
    setTempBehaviorId(undefined);
    setTempType(undefined);
    setPage(1);
    setStatusFilter(defaultStatuses);
    setDateRange(undefined);
    setBehaviorId(undefined);
    setType(undefined);
    refetch?.();
    console.log("FundFilters: Reset all filters");
  };

  const handleApply = () => {
    setPage(1);
    if (
      JSON.stringify(tempDateRange) !== JSON.stringify(dateRange) ||
      tempBehaviorId !== behaviorId ||
      tempType !== type ||
      JSON.stringify(tempStatusFilter) !== JSON.stringify(statusFilter)
    ) {
      setDateRange(tempDateRange);
      setBehaviorId(tempBehaviorId);
      setType(tempType);
      setStatusFilter(tempStatusFilter);
      refetch?.();
      console.log("FundFilters: Applied filters", {
        tempDateRange,
        tempBehaviorId,
        tempType,
        tempStatusFilter,
      });
    }
    setOpen(false);
  };

  const handleDateChange = (newRange: DateRange) => {
    if (
      newRange.from !== tempDateRange?.from ||
      newRange.to !== tempDateRange?.to ||
      newRange.time !== tempDateRange?.time
    ) {
      setTempDateRange(newRange);
      console.log("FundFilters: Updated tempDateRange:", newRange);
    }
  };

  const statusOptions: Option[] = [
    {
      value: FundStatus.PendingConfirmation,
      label: t("common.status.pending"),
    },
    {
      value: FundStatus.Confirmed,
      label: t("common.status.confirmed"),
    },
    {
      value: FundStatus.Cancelled,
      label: t("common.status.cancelled"),
    },
  ];

  return (
    <div className="flex gap-4">
      <DataTableSearch
        placeholder={t("fund.searchPlaceholder")}
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
            maxDays={365}
            defaultPreset={"thisMonth"}
            onDateChange={handleDateChange}
            className="m-2"
          />
          <DropdownMenuSeparator />
          <DropdownMenuLabel>{t("fund.type.title")}</DropdownMenuLabel>
          <Select
            value={tempType ?? "all"}
            onValueChange={(value) => {
              const newType = value === "all" ? undefined : value;
              if (newType !== tempType) {
                setTempType(newType);
                console.log("FundFilters: Updated tempType:", newType);
              }
            }}
          >
            <SelectTrigger className="mb-2">
              <SelectValue placeholder={t("fund.selectTransactionType")} />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">{t("common.all")}</SelectItem>
              <SelectItem value="Income">{t("fund.type.income")}</SelectItem>
              <SelectItem value="Spend">{t("fund.type.spend")}</SelectItem>
            </SelectContent>
          </Select>
          <DropdownMenuSeparator />
          <DropdownMenuLabel>{t("fund.behavior")}</DropdownMenuLabel>
          <Select
            value={tempBehaviorId?.toString() ?? "all"}
            onValueChange={(value) => {
              const newBehaviorId = value === "all" ? undefined : Number(value);
              if (newBehaviorId !== tempBehaviorId) {
                setTempBehaviorId(newBehaviorId);
                console.log(
                  "FundFilters: Updated tempBehaviorId:",
                  newBehaviorId
                );
              }
            }}
          >
            <SelectTrigger className="mb-2">
              <SelectValue placeholder={t("fund.selectPlacehodelrBehavior")} />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">{t("fund.allBehavior")}</SelectItem>
              {fundBehaviors
                .filter((behavior) => !tempType || behavior.type === tempType)
                .map((behavior) => (
                  <SelectItem key={behavior.id} value={behavior.id!.toString()}>
                    {textByLang(JSON.parse(behavior.name)) || t("common.nA")}
                  </SelectItem>
                ))}
            </SelectContent>
          </Select>
          <DropdownMenuSeparator />
          <DropdownMenuLabel>{t("common.status.title")}</DropdownMenuLabel>
          <MultiSelect
            options={statusOptions}
            value={tempStatusFilter}
            onChange={(newValue) => {
              if (
                JSON.stringify(newValue) !== JSON.stringify(tempStatusFilter)
              ) {
                setTempStatusFilter(newValue);
                console.log("FundFilters: Updated tempStatusFilter:", newValue);
              }
            }}
            placeholder={t("common.filterBy", {
              entity: t("common.status.title").replace(/^./, (c) =>
                c.toLowerCase()
              ),
            })}
            className="mb-2"
          />
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
