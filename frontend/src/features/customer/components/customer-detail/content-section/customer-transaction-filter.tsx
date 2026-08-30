import { DateRangePicker } from "@/components/date-range-picker";
import type { DateRange } from "react-day-picker";
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
import { useState } from "react";
import { Filter } from "lucide-react";

interface CustomerTransactionFiltersProps {
  time: DateRange | undefined;
  typeFilter: string;
  onApply: (time: DateRange | undefined, typeFilter: string) => void;
}

export function CustomerTransactionFilters({
  time,
  typeFilter,
  onApply,
}: CustomerTransactionFiltersProps) {
  const { setPage } = useTableFilters();
  const t = useTranslations();
  const [open, setOpen] = useState(false);
  const [timeFilter, setTimeFilter] = useState<DateRange | undefined>(time);
  const [type, setType] = useState<string>(typeFilter ?? "all");

  const handleReset = () => {
    setTimeFilter(undefined);
    setType("all");
    onApply(undefined, "all");
    setPage(1);
    setOpen(false);
  };

  const handleApply = () => {
    setPage(1);
    onApply(timeFilter, type);
    setOpen(false);
  };

  return (
    <div className="flex gap-4">
      <DropdownMenu open={open} onOpenChange={setOpen}>
        <DropdownMenuTrigger asChild>
          <Button variant="outline">
            <Filter className="h-5 w-5" />
            {t("table.accessorKey.filter")}
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent className="w-64">
          <DropdownMenuLabel>
            {t("dateAndTime.dateRange")} {/* Ví dụ: "Khoảng thời gian" */}
          </DropdownMenuLabel>
          <DateRangePicker date={timeFilter} setDate={setTimeFilter} />
          <DropdownMenuSeparator />
          <div className="flex justify-end gap-2 p-2">
            <Button variant="outline" size="sm" onClick={handleReset}>
              {t("common.reset")} {/* Ví dụ: "Đặt lại" */}
            </Button>
            <Button size="sm" onClick={handleApply}>
              {t("common.apply")} {/* Ví dụ: "Áp dụng" */}
            </Button>
          </div>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}
