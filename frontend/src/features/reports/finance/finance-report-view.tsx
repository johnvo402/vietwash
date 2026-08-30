"use client";

import { useIsMobile } from "@/hooks/use-mobile"; // Giả định hook này đã có

import FilterPanel from "../components/filter-report-order";
import ReportFinanceTable, {
  ExpenseResponse,
  RevenueResponse,
} from "./components/finance-table-report";
import { useFinanceReport } from "./hooks/use-finance-report";

export default function ReportFinanceView() {
  const isMobile = useIsMobile();
  const { expense, revenue, from, to, branchIds } = useFinanceReport();
  return (
    <div className="min-h-screen bg-background">
      <div
        className={`${
          isMobile
            ? "grid grid-cols-1 gap-2 h-auto"
            : "grid grid-cols-8 gap-2 h-screen"
        }`}
      >
        <div
          className={`${
            isMobile ? "col-span-1 w-full" : "col-span-2"
          } bg-background rounded-lg shadow-md p-${isMobile ? 4 : 6}`}
        >
          <FilterPanel maxDays={365} title="report.financeReport" />
        </div>

        <div
          className={`${
            isMobile ? "col-span-1 w-full" : "col-span-6"
          } bg-background rounded-lg shadow-md p-${isMobile ? 2 : 6}`}
        >
          <ReportFinanceTable
            expense={expense as ExpenseResponse}
            revenue={revenue as RevenueResponse}
            from={from ?? undefined}
            to={to ?? undefined}
            branchIds={branchIds ?? undefined}
          />
        </div>
      </div>
    </div>
  );
}
