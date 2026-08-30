// Generic tree filters component - có thể tái sử dụng
"use client"

import { Search } from "lucide-react"
import { Input } from "@/components/ui/input"
import { Checkbox } from "@/components/ui/checkbox"
import { TreeFilters } from "@/types/tree"
import { useTranslations } from "next-intl"

interface TreeFiltersProps {
  filters: TreeFilters
  onFiltersChange: (filters: TreeFilters) => void
  searchPlaceholder?: string
  disabledLabel?: string
}

export function TreeFiltersComponent({
  filters,
  onFiltersChange,
  searchPlaceholder ,
  disabledLabel ,
}: TreeFiltersProps) {
  const t = useTranslations()
  const handleSearchChange = (searchTerm: string): void => {
    onFiltersChange({ ...filters, searchTerm })
  }

  const handleShowDisabledChange = (showDisabled: boolean): void => {
    onFiltersChange({ ...filters, showDisabled })
  }
  disabledLabel = disabledLabel || t("common.showDisabled")
  searchPlaceholder = searchPlaceholder || t("common.find") + "..."
  return (
    <div className="flex flex-col sm:flex-row gap-4">
      <div className="relative flex-1">
        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
        <Input
          placeholder={searchPlaceholder}
          value={filters.searchTerm}
          onChange={(e) => handleSearchChange(e.target.value)}
          className="pl-10"
        />
      </div>

      <div className="flex items-center gap-4">
        <div className="flex items-center space-x-2">
          <Checkbox id="showDisabled" checked={filters.showDisabled} onCheckedChange={handleShowDisabledChange} />
          <label htmlFor="showDisabled" className="text-sm">
            {disabledLabel}
          </label>
        </div>
      </div>
    </div>
  )
}
