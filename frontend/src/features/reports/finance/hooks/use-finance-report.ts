import { apiClient } from "@/api/client";
import { useAuth } from "@/hooks/use-auth";
import { useQueryFilter } from "@/lib/filter";
import { useQuery } from "@tanstack/react-query";
import { useQueryState, useQueryStates } from "nuqs";

export const useFinanceReport = () => {
  const { branchActive } = useAuth();
  const [from] = useQueryState("from");
  const [to] = useQueryState("to");
  const [{ branchIds }] = useQueryStates({
    branchIds: {
      defaultValue: branchActive?.branchId
        ? [branchActive.branchId]
        : undefined,
      parse: (value: string) => {
        try {
          const parsed = JSON.parse(value);
          const ids = Array.isArray(parsed)
            ? parsed.map(Number).filter((id) => !isNaN(id))
            : [];
          return ids.length > 0 ? ids : undefined;
        } catch {
          return undefined;
        }
      },
      serialize: (value: number[]) => JSON.stringify(value),
    },
  });

  const { prepareApiParams } = useQueryFilter();

  const params = {
    from: from || undefined,
    to: to || undefined,
    branchIds: branchIds,
  };

  const searchApiParamsKeys = [
    "from",
    "to",
    "branchIds",
    "searchKeywords",
    "page",
    "pageSize",
    "before",
    "after",
    "searchKeyword",
    "searchTargets",
    "sort",
    "filter",
  ] as const;

  const args = prepareApiParams(searchApiParamsKeys, params);

  const {
    data,
    isFetching,
    isLoading: isQueryLoading,
    error: queryError,
  } = useQuery({
    queryKey: ["financeReport", { branchIds, from, to }],
    queryFn: async () => {
      const [revenueRes, expenseRes] = await Promise.all([
        apiClient.ecommerceApiReportRouteFinancialReportGet(...args),
        apiClient.financeApiReportRouteFinancialReportGet(...args),
      ]);

      return {
        revenue: revenueRes.data.results || null,
        expense: expenseRes.data.results || null,
      };
    },
  });

  return {
    revenue: data?.revenue,
    expense: data?.expense,
    isLoading: isQueryLoading || isFetching,
    error: queryError,
    from,
    to,
    branchIds,
  };
};
