import { useState } from "react";

import { DataTable } from "@/components/ui/table/data-table";
import { DateRange } from "react-day-picker";
import { CustomerTransactionFilters } from "./customer-transaction-filter";
import { useCustomerTransactionTable } from "../../customer-table/columns";
import { useCustomerTransaction } from "@/features/customer/hooks/use-customer-hook";

export default function CustomerTransactionPage({ id }: { id: number }) {
  // Trạng thái cho bộ lọc
  const [time, setTime] = useState<DateRange | undefined>(undefined);
  const [typeFilter, setTypeFilter] = useState<string>("all");

  const { columns } = useCustomerTransactionTable();
  const { transactions, isLoading, error, paging } = useCustomerTransaction({
    time: time,
    customerId: id,
  });

  return (
    <>
      {/* Component bộ lọc */}
      <div className="flex justify-between">
        <CustomerTransactionFilters
          time={time}
          typeFilter={typeFilter}
          onApply={(dataTime, dataType) => {
            setTime(dataTime);
            setTypeFilter(dataType);
          }}
        />
      </div>

      <div className="mt-2">
        <DataTable
          columns={columns}
          data={transactions}
          loading={isLoading}
          paging={paging}
          error={error}
        />
      </div>
    </>
  );
}
