"use client";

import { Input } from "@/components/ui/input";
import { useTranslations } from "next-intl";
import { useOrdersQuery } from "@/features/orders/compositions/use-order-query";
import { DataTable } from "@/components/ui/table/data-table";
import { useQueryState } from "nuqs";
import { useOrder } from "@/features/orders/components/order-table/columns";
import { OrderStatus } from "@/api/generated";

export default function ServiceOrderView({ serviceId }: { serviceId: number }) {
  const t = useTranslations();
  const [search, setSearch] = useQueryState("search", { defaultValue: "" });
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });

  const { columnOrderServices } = useOrder();

  const { ordersToDisplay, isFetching, isLoading, error, paging } =
    useOrdersQuery({
      search,
      page,
      pageSize,
      viewMode: "list", // <-- giữ đồng bộ với columns
      statusFilter: [OrderStatus.Completed],
      customerGroupFilter: "all",
      dateRange: undefined,
      serviceId,
      enabled: !!serviceId,
    });

  return (
    <div className="w-full mx-auto space-y-6">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <Input
          placeholder={t("search.title") || "Search..."}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="max-w-sm"
        />
      </div>

      <DataTable
        columns={columnOrderServices}
        data={ordersToDisplay}
        paging={paging}
        loading={isLoading || isFetching}
        error={error ? new Error(t("common.error")) : undefined}
      />
    </div>
  );
}
