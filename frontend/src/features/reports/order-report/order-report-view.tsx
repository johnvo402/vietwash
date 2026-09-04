"use client";

import { useEffect, useState } from "react";
import FilterPanel from "../components/filter-report-order";
import { useOrderReport } from "./hooks/use-order-report";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { DataTable } from "@/components/ui/table/data-table";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { useOrderReportTable } from "./components/column";
import { useFilterState } from "../hooks/useFilterState";
import { useTranslations } from "next-intl";
import { useIsMobile } from "@/hooks/use-mobile";
import { Button } from "@/components/ui/button";
import type { ExcelCell } from "@/shared/utils/excel";
import { excelHeaderStyle } from "@/shared/themes/excel";
import { toast } from "react-toastify";
import { format, formatDate } from "date-fns";
import { useAuth } from "@/hooks/use-auth";

export default function ReportOrderView() {
  const {
    orderReport,
    isLoading,
    paging,
    error,
    from,
    to,
    branchIds,
    fetchAllData,
  } = useOrderReport();
  const { setIsLoading } = useFilterState();
  const isMobile = useIsMobile();
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const { columns } = useOrderReportTable();
  const { user } = useAuth();
  const t = useTranslations();
  const [exportLoading, setExportLoading] = useState(false);

  // Get branch names for export
  const branchNames =
    user?.branchAccounts
      ?.filter((x) => branchIds?.includes(x.branchId))
      .map((x) => x.branchName)
      .join(", ") || t("common.allBranches");

  useEffect(() => {
    setIsLoading(isLoading);
  }, [isLoading, setIsLoading]);

  const handleExportExcel = async () => {
    if (exportLoading) return;
    setExportLoading(true);

    try {
      const { convertObjectsToRows, createExcelBuilder, exportFile } =
        await import("@/shared/utils/excel");
      toast.loading(t("common.excel_downloading"));

      const allData = await fetchAllData();

      if (!allData || allData.length === 0) {
        throw new Error(t("common.noDataToExport"));
      }

      // Define Excel headers
      const headerCells: ExcelCell[] = [
        {
          start: { c: 1, r: 8 },
          end: { c: 1, r: 9 },
          value: t("table.accessorKey.index"),
        },
        {
          start: { c: 2, r: 8 },
          end: { c: 2, r: 9 },
          value: t("order.orderCode").replace(/^./, (c) => c.toUpperCase()),
        },
        {
          start: { c: 3, r: 8 },
          end: { c: 3, r: 9 },
          value: t("common.branch").replace(/^./, (c) => c.toUpperCase()),
        },
        {
          start: { c: 4, r: 8 },
          end: { c: 4, r: 9 },
          value: t("order.customerName"),
        },
        {
          start: { c: 5, r: 8 },
          end: { c: 5, r: 9 },
          value: t("service.numberOfServices"),
          isNumFmt: true,
        },
        {
          start: { c: 6, r: 8 },
          end: { c: 6, r: 9 },
          value: t("table.accessorKey.amount"),
          isCurrency: true,
        },
        {
          start: { c: 7, r: 8 },
          end: { c: 7, r: 9 },
          value: t("order.orderDate"),
        },
      ];

      // Prepare table rows
      const tableRows = allData.map((row: any, index: number) => ({
        stt: index + 1,
        code: row.code,
        branch:
          user?.branchAccounts?.find(
            (account) => account.branchId === row.branchId
          )?.branchName || String(row.branchId),
        customerName: row.customerName,
        orderItemCount: row.orderItemCount,
        amount: row.amount,
        orderDate: formatDate(new Date(row.orderDate), "dd/MM/yyyy HH:mm"),
      }));

      // Prepare statistics
      const statisticLabel = [
        t("report.totalOrders"),
        t("report.totalServices"),
        t("report.totalAmount"),
      ];
      const statisticValue: any[] = [
        allData.length,
        allData.reduce((sum: number, row: any) => sum + row.orderItemCount, 0),
        allData.reduce((sum: number, row: any) => sum + row.amount, 0),
      ];

      // Create Excel builder
      const excelBuilder = await createExcelBuilder({
        sheetName: "Order Report",
      });

      // Add info and statistic rows
      excelBuilder.addRowsCustom(
        [
          [
            t("dateAndTime.fromDateAndTime"),
            from
              ? formatDate(new Date(Number(from) * 1000), "dd/MM/yyyy")
              : "--",
          ],
          [
            t("dateAndTime.toDateAndTime"),
            to ? formatDate(new Date(Number(to) * 1000), "dd/MM/yyyy") : "--",
          ],
          [t("common.branch"), branchNames],
        ],
        {
          alignment: {
            vertical: "middle",
            horizontal: "right",
            wrapText: true,
          },
        }
      );
      excelBuilder.addRowsCustom(
        [
          [], // Empty row
          statisticLabel,
          statisticValue,
        ],
        {
          alignment: {
            vertical: "middle",
            horizontal: "right",
            wrapText: true,
          },
        },
        true
      );

      excelBuilder
        .addCells(headerCells, excelHeaderStyle)
        .addRows(
          convertObjectsToRows(tableRows, [
            "stt",
            "code",
            "branch",
            "customerName",
            "orderItemCount",
            "amount",
            "orderDate",
          ])
        );
      excelBuilder.rowsCustomStyle([5], { font: { bold: true } }); // Bold statistic labels
      excelBuilder.rowsCustomCurrent([6]); // Format numbers for statistic values
      excelBuilder.autoAdjustColumnWidth(excelBuilder.worksheet, 4); // Auto-adjust columns from row 8

      const bytes = await excelBuilder.write();
      const result = exportFile(
        `ORDER_REPORT_${format(new Date(), "ddMMyyyy")}.xlsx`,
        bytes,
        {
          mimeType:
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        }
      );

      if (result instanceof Error) {
        throw result;
      }
    } catch (error) {
      console.error("Export failed:", error);
      toast.error(t("common.excel_export_failed"));
    } finally {
      setExportLoading(false);
      toast.dismiss();
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <div
        className={`${
          isMobile
            ? "grid grid-cols-1 gap-2 h-auto"
            : "grid grid-cols-8 gap-2 h-screen"
        }`}
      >
        {/* Filter Panel */}
        <div
          className={`${
            isMobile ? "col-span-1 w-full" : "col-span-2"
          } bg-background rounded-lg shadow-md p-${isMobile ? 4 : 6}`}
        >
          <FilterPanel maxDays={365} title="report.orderReport" />
        </div>

        {/* Main Content */}
        <div
          className={`${
            isMobile ? "col-span-1 w-full" : "col-span-6"
          } bg-background rounded-lg shadow-md p-${isMobile ? 2 : 6}`}
        >
          <div className="flex justify-between items-center mb-4">
            <DataTableSearch
              placeholder={t("search.searchBy", {
                entity: t("order.orderCode").toLowerCase(),
              })}
              searchQuery={searchQuery}
              setSearchQuery={setSearchQuery}
              setPage={setPage}
            />
            <Button
              onClick={handleExportExcel}
              disabled={exportLoading}
              className="ml-4"
            >
              {exportLoading ? t("common.loading") : t("common.downloadExcel")}
            </Button>
          </div>

          {/* Data Table */}
          <div className={`rounded-md border shadow-sm mt-${isMobile ? 2 : 3}`}>
            <DataTable
              columns={columns}
              data={orderReport}
              paging={paging}
              loading={isLoading}
              error={error}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
