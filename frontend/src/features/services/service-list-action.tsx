"use client";

import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTableFilters } from "../../compositions/tables/use-table-filters";
import { useTranslations } from "next-intl";
import { ROUTE_SERVICE_CREATE } from "@/types/router-type";
import Link from "next/link";
import { cn } from "@/lib/utils";
import { buttonVariants } from "@/components/ui/button";
import { Plus } from "lucide-react";

export default function ServiceListAction() {
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const t = useTranslations();
  return (
    <div className="flex flex-wrap items-stretch justify-between">
      <DataTableSearch
        placeholder={t("search.searchBy", { entity: t("common.entityName", { Entity: t("common.service") }).toLowerCase() })}
        searchQuery={searchQuery}
        setSearchQuery={setSearchQuery}
        setPage={setPage}
      />

      <Link
        href={ROUTE_SERVICE_CREATE}
        className={cn(buttonVariants(), "text-xs md:text-sm")}
      >
        <Plus className="h-4 w-4" /> {t("common.create")}
      </Link>
    </div>
  );
}
