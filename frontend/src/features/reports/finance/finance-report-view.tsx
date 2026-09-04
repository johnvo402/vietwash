"use client";

import FilterPanel from "../components/filter-report-order";
import ReportFinanceTable, {
  ExpenseResponse,
  RevenueResponse,
} from "./components/finance-table-report";
import { useFinanceReport } from "./hooks/use-finance-report";

export default function ReportFinanceView() {
  const { expense, revenue, from, to, branchIds } = useFinanceReport();
  return (
    <div className="min-h-screen bg-background">
      <div className="grid h-auto grid-cols-1 gap-2 md:h-screen md:grid-cols-8">
        <div className="col-span-1 w-full rounded-lg bg-background p-4 shadow-md md:col-span-2 md:p-6">
          <FilterPanel maxDays={365} title="report.financeReport" />
        </div>

        <div className="col-span-1 w-full rounded-lg bg-background p-2 shadow-md md:col-span-6 md:p-6">
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
