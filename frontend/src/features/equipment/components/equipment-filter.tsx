"use client";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTranslations } from "next-intl";
import { Button } from "@/components/ui/button";
import { useMemo } from "react";
import { EquipmentStatus } from "@/api/generated";
import { Option } from "@/components/core/selects/multi-select";
import { RotateCcw, Search } from "lucide-react";
import { Input } from "@/components/ui/input";

interface EquipmentFilterProps {
  // Search is managed by parent now
  searchQuery: string;
  setSearchQuery: (value: string) => void;

  // Status filter (single-select) managed by parent
  statusFilter?: Option;
  setStatusFilter: (value: Option) => void;
}

export function EquipmentFilter({
  searchQuery,
  setSearchQuery,
  statusFilter,
  setStatusFilter,
}: EquipmentFilterProps) {
  const t = useTranslations();

  // Define the available status options
  const statusOptions: Option[] = useMemo(
    () => [
      { value: EquipmentStatus.Active, label: t("common.status.active") },
      {
        value: EquipmentStatus.UnderMaintenance,
        label: t("common.status.undermaintenance"),
      },
      {
        value: EquipmentStatus.UnderRepair,
        label: t("common.status.underrepair"),
      },
    ],
    [t],
  );

  // Current selected value for the single-select (empty string = all)
  const currentValue = statusFilter?.value ? String(statusFilter.value) : "";

  const applyStatus = (value: string) => {
    if (!value) {
      // Empty => clear filter to show all statuses
      setStatusFilter({
        value: EquipmentStatus.Active,
        label: t("common.status.active"),
      });
    } else {
      const found = statusOptions.find((o) => String(o.value) === value);
      setStatusFilter(
        found
          ? found
          : { value: EquipmentStatus.Active, label: t("common.status.active") },
      );
    }
  };

  const handleReset = () => applyStatus("");

  return (
    <div className="flex items-center gap-4">
      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-border h-5 w-5" />
        <Input
          type="text"
          placeholder={t("equipment.equipmentList.searchPlaceholder")}
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-10 rounded-lg border-border focus:ring-2 focus:text-primary transition-all"
        />
      </div>

      {/* Status filter placed inline next to search (no dropdown wrapper) */}
      <div className="flex items-center gap-2">
        <span>{t("common.status.title")}:</span>
        <Select value={currentValue} onValueChange={applyStatus}>
          <SelectTrigger className="w-[220px]">
            <SelectValue
              placeholder={t("common.filterBy", {
                entity: t("common.status.title").toLowerCase(),
              })}
            />
          </SelectTrigger>
          <SelectContent>
            {statusOptions.map((option) => (
              <SelectItem
                key={String(option.value)}
                value={String(option.value)}
              >
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Button
          variant="default"
          size="sm"
          onClick={handleReset}
          className="h-11 min-w-11"
          aria-label={t("common.reset")}
        >
          <RotateCcw size={16} />
        </Button>
      </div>
    </div>
  );
}
