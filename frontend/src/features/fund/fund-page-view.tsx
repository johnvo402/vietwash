"use client";

import { apiClient } from "@/api/client";
import { useQueryFilter } from "@/lib/filter";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useFundTable } from "./components/fund-table/column";
import { DataTable } from "@/components/ui/table/data-table";
import {
  useQueryState,
  parseAsArrayOf,
  parseAsIsoDateTime,
  parseAsInteger,
  parseAsStringEnum,
} from "nuqs";
import { FundFilters } from "./components/fund-table/fund-filter";
import { useState, useEffect, useRef } from "react";
import {
  subDays,
  startOfWeek,
  endOfWeek,
  startOfMonth,
  endOfMonth,
  subMonths,
  startOfDay,
  endOfDay,
} from "date-fns";
import { useQuery as useQueryFundBehavior } from "@tanstack/react-query";
import { Button, buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { Plus } from "lucide-react";
import { useTranslations } from "next-intl";
import { FundCreatePopup } from "./components/create-fund-dialog";
import { FundStatus, FundType, PaymentMethod } from "@/api/generated";
import { toast } from "react-toastify";
import { DateRange } from "../reports/types/filter.type";
import { DateUtils } from "@/utils/date.utils";
import { Option } from "@/components/core/selects/multi-select";

export const FundPageView = () => {
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [isPopupOpen, setIsPopupOpen] = useState(false);
  const queryClient = useQueryClient();
  const { flattenQueryObject, prepareApiParams } = useQueryFilter();
  const t = useTranslations();

  const { columns } = useFundTable({
    onEdit: (id) => {
      setSelectedId(id);
      setIsPopupOpen(true);
    },
    onStatusChange(id, status) {
      handleUpdateStatus(id, status);
    },
  });

  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search", { defaultValue: "" });
  const [statusFilter, setStatusFilter] = useQueryState<FundStatus[]>(
    "status",
    parseAsArrayOf(parseAsStringEnum(Object.values(FundStatus))).withDefault([
      FundStatus.PendingConfirmation,
      FundStatus.Confirmed,
    ]),
  );
  const [dateFrom, setDateFrom] = useQueryState(
    "from",
    parseAsIsoDateTime.withDefault(new Date()),
  );
  const [dateTo, setDateTo] = useQueryState(
    "to",
    parseAsIsoDateTime.withDefault(new Date()),
  );
  const [time, setTime] = useQueryState("time", { defaultValue: "thisWeek" });
  const [behaviorId, setBehaviorId] = useQueryState(
    "behaviorId",
    parseAsInteger.withDefault(0),
  );
  const [type, setType] = useQueryState("type", { defaultValue: "" });

  const [dateRange, setDateRange] = useState<DateRange | undefined>(() => {
    const defaultRange = DateUtils.getDateRange("thisWeek") || {
      from: startOfWeek(new Date()).toISOString(),
      to: endOfWeek(new Date()).toISOString(),
      time: "thisWeek",
    };
    return defaultRange;
  });

  const isUpdatingRef = useRef(false);

  useEffect(() => {
    if (isUpdatingRef.current) return;

    const newDateRange = {
      from: startOfDay(dateFrom).toISOString(), // Normalize to start of day
      to: endOfDay(dateTo).toISOString(), // Normalize to end of day
      time: time,
    };

    // Only update if the dates are meaningfully different
    if (
      dateRange?.from !== newDateRange.from ||
      dateRange?.to !== newDateRange.to ||
      dateRange?.time !== newDateRange.time
    ) {
      isUpdatingRef.current = true;
      setDateRange(newDateRange as DateRange);
      isUpdatingRef.current = false;
    }
  }, [
    dateFrom,
    dateRange?.from,
    dateRange?.time,
    dateRange?.to,
    dateTo,
    setDateRange,
    time,
  ]);

  const { data: fundBehaviors = [] } = useQueryFundBehavior({
    queryKey: ["fundBehaviors"],
    queryFn: async () => {
      const res = await apiClient.financeApiFundBehaviorsGet();
      return res.data.results ?? [];
    },
  });

  const { data: fund, isLoading: fundLoading } = useQuery({
    queryKey: ["fund", selectedId],
    queryFn: async () => {
      if (selectedId === null) return undefined;
      const response = await apiClient.financeApiFundsId(Number(selectedId));
      return response.data.results;
    },
    enabled: selectedId !== null,
  });

  const handleUpdateStatus = async (
    id: string,
    status: FundStatus,
  ): Promise<void> => {
    try {
      await apiClient.financeApiFundsIdPut(id, { status });
      toast.success(t("toast.update.success", { entity: t("fund.title") }));
      queryClient.invalidateQueries({ queryKey: ["funds"] });
    } catch (error) {
      toast.error(t("toast.update.failed", { entity: t("fund.title") }));
    }
  };

  const handleSubmit = async (data: {
    type: string;
    amount: number;
    fundBehaviorId: number;
    note: string;
    paymentMethod: string;
    branchId: number;
  }): Promise<void> => {
    try {
      if (!selectedId) {
        await apiClient.financeApiFundsPost({
          type: data.type as FundType,
          amount: data.amount,
          fundBehaviorId: data.fundBehaviorId,
          note: data.note,
          paymentMethod: data.paymentMethod as PaymentMethod,
          branchId: data.branchId,
        });
        toast.success(t("toast.create.success", { entity: t("fund.title") }));
      } else {
        await apiClient.financeApiFundsIdPut(selectedId, {
          note: data.note,
          paymentMethod: data.paymentMethod as PaymentMethod,
        });
        toast.success(t("toast.update.success", { entity: t("fund.title") }));
      }
      queryClient.invalidateQueries({ queryKey: ["funds"] });
    } catch (error) {
      if (!selectedId) {
        toast.error(t("toast.create.failed", { entity: t("fund.title") }));
      } else {
        toast.error(t("toast.update.failed", { entity: t("fund.title") }));
      }
    }
  };

  const params = {
    from: dateRange?.from || undefined,
    to: dateRange?.to || undefined,
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeyword: search || undefined,
    searchTargets: ["code"],
    sort: "status:asc,transactionDate:desc",
    filter: flattenQueryObject({
      ...(behaviorId ? { fundBehaviorId: { $eq: behaviorId } } : {}),
      ...(type ? { type: { $eq: type } } : {}),
      ...(statusFilter.length > 0 ? { status: { $in: statusFilter } } : {}),
    }),
  };

  const fundApiParamKeys = [
    "from",
    "to",
    "page",
    "pageSize",
    "cursorBefore",
    "cursorAfter",
    "searchKeyword",
    "searchTargets",
    "sort",
    "filter",
    "dynamicFilter",
    "originFilters",
    "options",
  ] as const;

  const args = prepareApiParams(fundApiParamKeys, params, {
    page: 1,
    pageSize: 10,
  });

  const { data, isFetching, isLoading, error } = useQuery({
    queryKey: ["funds", params],
    queryFn: async () => {
      const response = await apiClient.financeApiFundsGet(...args);
      return {
        fund: response.data.results?.data ?? [],
        paging: response.data.results?.paging ?? {},
      };
    },
  });

  const handleClose = () => {
    setIsPopupOpen(false);
    setSelectedId(null);
  };

  const statusOptions: Option[] = [
    {
      value: FundStatus.PendingConfirmation,
      label: t("common.status.pending"),
    },
    { value: FundStatus.Confirmed, label: t("common.status.confirmed") },
    { value: FundStatus.Cancelled, label: t("common.status.cancelled") },
  ];

  return (
    <>
      <div className="flex items-center justify-between">
        <FundFilters
          dateRange={dateRange}
          setDateRange={(value) => {
            if (isUpdatingRef.current) return;
            isUpdatingRef.current = true;

            if (value === undefined) {
              const defaultRange = DateUtils.getDateRange("thisWeek") || {
                from: startOfWeek(new Date()).toISOString(),
                to: endOfWeek(new Date()).toISOString(),
                time: "thisWeek",
              };
              if (
                dateRange?.from !== defaultRange.from ||
                dateRange?.to !== defaultRange.to ||
                dateRange?.time !== defaultRange.time
              ) {
                setDateFrom(new Date(defaultRange.from));
                setDateTo(new Date(defaultRange.to));
                setTime(defaultRange.time);
                setDateRange(undefined);
                console.log(
                  "FundPageView: Reset dateRange to default:",
                  defaultRange,
                );
              }
            } else {
              if (
                dateRange?.from !== value.from ||
                dateRange?.to !== value.to ||
                dateRange?.time !== value.time
              ) {
                setDateFrom(new Date(value.from));
                setDateTo(new Date(value.to));
                setTime(value.time);
                setDateRange(value);
                console.log("FundPageView: Updated dateRange:", value);
              }
            }
            isUpdatingRef.current = false;
          }}
          behaviorId={behaviorId === 0 ? undefined : behaviorId}
          setBehaviorId={(value) => {
            if (value !== (behaviorId === 0 ? undefined : behaviorId)) {
              setBehaviorId(value ?? 0);
              console.log("FundPageView: Updated behaviorId:", value ?? 0);
            }
          }}
          fundBehaviors={fundBehaviors}
          type={type || undefined}
          setType={(value) => {
            if (value !== (type || undefined)) {
              setType(value || "");
              console.log("FundPageView: Updated type:", value || "");
            }
          }}
          statusFilter={statusOptions.filter((option) =>
            statusFilter.includes(option.value as FundStatus),
          )}
          setStatusFilter={(options) => {
            const newStatusFilter = options.map(
              (option) => option.value as FundStatus,
            );
            if (
              JSON.stringify(statusFilter) !== JSON.stringify(newStatusFilter)
            ) {
              setStatusFilter(newStatusFilter);
              console.log(
                "FundPageView: Updated statusFilter:",
                newStatusFilter,
              );
            }
          }}
          refetch={() => {
            queryClient.invalidateQueries({ queryKey: ["funds"] });
            console.log("FundPageView: Refetch triggered");
          }}
        />
        <Button
          onClick={() => setIsPopupOpen(true)}
          className={cn(buttonVariants(), "text-xs md:text-sm")}
        >
          <Plus className="h-4 w-4" /> {t("common.create")}
        </Button>
      </div>
      <div className="rounded-md border shadow-sm">
        <DataTable
          columns={columns}
          data={data?.fund ?? []}
          paging={data?.paging}
          loading={isLoading || isFetching}
          error={error}
        />
      </div>
      {isPopupOpen && (!selectedId || fund) && (
        <FundCreatePopup
          isOpen={isPopupOpen}
          onClose={() => handleClose()}
          fundBehaviors={fundBehaviors}
          onSubmit={async (data) => await handleSubmit(data)}
          isUpdate={!!selectedId}
          initialData={fund as any}
          loading={fundLoading}
        />
      )}
    </>
  );
};
