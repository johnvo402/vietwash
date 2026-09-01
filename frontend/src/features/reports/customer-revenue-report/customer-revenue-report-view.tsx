"use client";

import { useEffect, useState } from "react";
import FilterPanel from "../components/filter-report-order";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { DataTable } from "@/components/ui/table/data-table";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { useFilterState } from "../hooks/useFilterState";
import { useCustomerRevenueReport } from "./hooks/use-customer-revenue-report";
import { useCustomerRevenueTable } from "./components/column";
import { useTranslations } from "next-intl";
import { useIsMobile } from "@/hooks/use-mobile";
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

export default function CustomerRevenueView() {
  const {
    customerRevenue,
    isLoading,
    paging,
    error,
    fetchAllData,
    from,
    to,
    branchIds,
  } = useCustomerRevenueReport();
  const { user } = useAuth();
  const t = useTranslations();
  const { setIsLoading } = useFilterState();
  const isMobile = useIsMobile();
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const [exportLoading, setExportLoading] = useState(false);

  const { columns } = useCustomerRevenueTable();
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

      // Lấy toàn bộ dữ liệu
      const allData = await fetchAllData();
      if (!allData || allData.length === 0) {
        throw new Error(t("common.noDataToExport"));
      }

      // Định nghĩa tiêu đề cho Excel
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
            Entity: t("common.customer").replace(/^./, (c) => c.toUpperCase()),
          }),
        },
        {
          start: { c: 3, r: 8 },
          end: { c: 3, r: 9 },
          value: t("user.phoneNumber.title"),
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
          value: t("common.numberOfCancelOrders"),
          isNumFmt: true,
        },
        {
          start: { c: 6, r: 8 },
          end: { c: 6, r: 9 },
          value: t("revenue.grossRevenue"),
          isCurrency: true,
        },
        {
          start: { c: 7, r: 8 },
          end: { c: 7, r: 9 },
          value: t("common.cancelValue"),
          isCurrency: true,
        },
        {
          start: { c: 8, r: 8 },
          end: { c: 8, r: 9 },
          value: t("revenue.netRevenue"),
          isCurrency: true,
        },
      ];

      // Chuẩn bị dữ liệu cho các hàng
      const tableRows = allData.map((row: any, index: number) => ({
        stt: index + 1,
        customer: `${row.displayName} (${row.customerCode})`,
        phoneNumber: row.phoneNumber,
        orderSaleQuantity: row.orderSaleQuantity,
        orderCancelQuantity: row.orderCancelQuantity,
        revenue: row.revenue,
        cancelValue: row.cancelValue,
        netRevenue: row.netRevenue,
      }));

      // Chuẩn bị dữ liệu thống kê
      const statisticLabel = [
        t("report.totalRegisteredCustomers"),
        t("report.totalRevenue"),
        t("report.totalCancelValue"),
        t("report.totalNetRevenue"),
      ];
      const statisticValue: any[] = [
        allData.filter((row: any) => row.customerId != null).length,
        allData.reduce((sum: number, row: any) => sum + row.revenue, 0),
        allData.reduce((sum: number, row: any) => sum + row.cancelValue, 0),
        allData.reduce((sum: number, row: any) => sum + row.netRevenue, 0),
      ];

      // Tạo Excel builder
      const excelBuilder = await createExcelBuilder({
        sheetName: "Customer Revenue Report",
      });

      // Thêm các hàng thông tin và thống kê
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
          [], // Hàng trống
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
            "customer",
            "phoneNumber",
            "orderSaleQuantity",
            "orderCancelQuantity",
            "revenue",
            "cancelValue",
            "netRevenue",
          ])
        );
      excelBuilder.rowsCustomStyle([5], { font: { bold: true } }); // In đậm hàng nhãn thống kê
      excelBuilder.rowsCustomCurrent([6]); // Định dạng số cho hàng thống kê
      excelBuilder.autoAdjustColumnWidth(excelBuilder.worksheet, 4); // Tự động điều chỉnh từ hàng 8 trở xuống

      const bytes = await excelBuilder.write();
      const result = exportFile(
        `CUS_REVENUE_${format(new Date(), "ddMMyyyy")}.xlsx`,
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
        {/* Phần bên trái - Filter */}
        <div
          className={`${
            isMobile ? "col-span-1 w-full" : "col-span-2"
          } bg-background rounded-lg shadow-md p-4`}
        >
          <FilterPanel maxDays={365} title="report.customerRevenueReport" />
        </div>

        {/* Phần bên phải - 3 phần */}
        <div
          className={`${
            isMobile ? "col-span-1 w-full" : "col-span-6"
          } bg-background rounded-lg shadow-md p-${isMobile ? 2 : 6}`}
        >
          <div className="flex justify-between items-center mb-4">
            <DataTableSearch
              placeholder={t("search.searchBy", {
                entity:
                  (t("customer.displayName") + ", " + t("order.customerPhone")).toLowerCase(),
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
              columns={columns as any}
              data={customerRevenue}
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
