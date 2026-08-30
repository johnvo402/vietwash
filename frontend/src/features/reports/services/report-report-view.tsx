"use client";

import { useEffect, useState } from "react";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { DataTable } from "@/components/ui/table/data-table";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { useFilterState } from "../hooks/useFilterState";
import { useTranslations } from "next-intl";
import { useIsMobile } from "@/hooks/use-mobile";
import { useServiceReport } from "./hooks/use-service-report";
import { useServiceReportTable } from "./components/column";
import FilterPanel from "../components/filter-report-order";
import { Button } from "@/components/ui/button";
import {
  convertObjectsToRows,
  createExcelBuilder,
  ExcelCell,
  exportFile,
} from "@/shared/utils/excel";
import { excelHeaderStyle } from "@/shared/themes/excel";
import { toast } from "react-toastify";
import { format, formatDate } from "date-fns";
import { useAuth } from "@/hooks/use-auth";
import { apiClient } from "@/api/client";
import { formatNumberVN, formatPriceVN } from "@/utils/format";

export default function ReportServiceView() {
  const {
    serviceReport,
    isLoading,
    paging,
    error,
    from,
    to,
    branchIds,
    fetchAllData,
  } = useServiceReport();
  const { setIsLoading } = useFilterState();
  const isMobile = useIsMobile();
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const { columns } = useServiceReportTable();
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
      toast.loading(t("common.excel_downloading"));

      // Fetch all data for export

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
          value: t("common.entityName", {
            Entity: t("common.service").toLowerCase(),
          }),
        },
        {
          start: { c: 3, r: 8 },
          end: { c: 3, r: 9 },
          value: t("product.unit"),
        },
        {
          start: { c: 4, r: 8 },
          end: { c: 4, r: 9 },
          value: t("common.numberOfOrders"),
          isNumFmt: true,
        },
        {
          start: { c: 5, r: 8 },
          end: { c: 5, r: 9 },
          value: t("report.totalRevenue"),
          isCurrency: true,
        },
        {
          start: { c: 6, r: 8 },
          end: { c: 6, r: 9 },
          value: t("report.totalDiscount"),
          isCurrency: true,
        },
        {
          start: { c: 7, r: 8 },
          end: { c: 7, r: 9 },
          value: t("report.totalNetRevenue"),
          isCurrency: true,
        },
      ];

      // Prepare table rows
      const tableRows = allData.map((row: any, index: number) => ({
        stt: index + 1,
        serviceName: row.serviceName || "",
        unitName: row.unitName || "",
        totalOrderCount: row.totalOrderCount || 0,
        totalRevenue: row.totalRevenue || 0,
        totalDiscount: row.totalDiscount || 0,
        totalNetRevenue: row.totalNetRevenue || 0,
      }));

      // Prepare statistics
      const statisticLabel = [
        t("report.totalServices"),
        t("report.totalOrders"),
        t("report.totalRevenue"),
        t("report.totalDiscount"),
        t("report.totalNetRevenue"),
      ];
      const statisticValue: any[] = [
        allData.length,
        allData.reduce(
          (sum: number, row: any) => sum + (row.totalOrderCount || 0),
          0
        ),
        allData.reduce(
          (sum: number, row: any) => sum + (row.totalRevenue || 0),
          0
        ),
        allData.reduce(
          (sum: number, row: any) => sum + (row.totalDiscount || 0),
          0
        ),
        allData.reduce(
          (sum: number, row: any) => sum + (row.totalNetRevenue || 0),
          0
        ),
      ];

      // Create Excel builder
      const excelBuilder = await createExcelBuilder({
        sheetName: "Service Revenue Report",
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
            "serviceName",
            "unitName",
            "totalOrderCount",
            "totalRevenue",
            "totalDiscount",
            "totalNetRevenue",
          ])
        );
      excelBuilder.rowsCustomStyle([5], { font: { bold: true } }); // Bold statistic labels
      excelBuilder.rowsCustomCurrent([6]); // Format numbers for statistic values
      excelBuilder.autoAdjustColumnWidth(excelBuilder.worksheet, 4); // Auto-adjust columns from row 8

      const bytes = await excelBuilder.write();
      const result = exportFile(
        `SERVICE_REVENUE_REPORT_${format(new Date(), "ddMMyyyy")}.xlsx`,
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
        <div
          className={`${
            isMobile ? "col-span-1 w-full" : "col-span-2"
          } bg-background rounded-lg shadow-md p-${isMobile ? 4 : 6}`}
        >
          <FilterPanel maxDays={365} title="report.serviceReport" />
        </div>

        <div
          className={`${
            isMobile ? "col-span-1 w-full" : "col-span-6"
          } bg-background rounded-lg shadow-md p-${isMobile ? 2 : 6}`}
        >
          <div className="flex justify-between items-center mb-4">
            <DataTableSearch
              placeholder={t("search.searchBy", {
                entity: t("common.entityName", {
                  Entity: t("common.service"),
                }).toLowerCase(),
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
          <div className={`rounded-md border shadow-sm mt-3`}>
            <DataTable
              columns={columns}
              data={serviceReport}
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
