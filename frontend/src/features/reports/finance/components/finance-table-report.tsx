"use client";

import { formatPriceVN } from "@/utils/format";
import { Loader2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useState, useEffect } from "react";
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
import { useFilterState } from "../../hooks/useFilterState";
import { useIsMobile } from "@/hooks/use-mobile";

export interface RevenueResponse {
  totalRevenue: number;
  cancelValue: number;
  totalDiscount: number;
  totalPoint: number;
  totalNetRevenue: number;
}

export interface ExpenseResponse {
  totalStockImport: number;
  totalStockExport: number;
  totalOtherIncome: number;
  totalOtherSpend: number;
}

export interface ReportItem {
  id: string;
  description: string;
  amount: string;
  isChild?: boolean;
}

interface ReportFinanceTableProps {
  revenue?: RevenueResponse;
  expense?: ExpenseResponse;
  from?: string;
  to?: string;
  branchIds?: number[];
}

export default function ReportFinanceTable({
  revenue,
  expense,
  from,
  to,
  branchIds,
}: ReportFinanceTableProps) {
  const t = useTranslations();
  const { user } = useAuth();
  const { isLoading: filterLoading, setIsLoading } = useFilterState();
  const [exportLoading, setExportLoading] = useState(false);
  const isMobile = useIsMobile();
  // Get branch names for export
  const branchNames =
    user?.branchAccounts
      ?.filter((x) => branchIds?.includes(x.branchId))
      .map((x) => x.branchName)
      .join(", ") || t("report.common.allBranches");

  useEffect(() => {
    if (revenue && expense) {
      setIsLoading(false);
    } else {
      setIsLoading(true);
    }
  }, [revenue, expense, setIsLoading]);

  // Safely calculate values with fallback
  const discountAndPoint =
    (revenue?.totalDiscount ?? 0) + (revenue?.totalPoint ?? 0);
  const totalReduction = discountAndPoint + (revenue?.cancelValue ?? 0);
  const totalExpenses =
    (expense?.totalStockExport ?? 0) - (expense?.totalStockImport ?? 0);
  const totalNetRevenue =
    (revenue?.totalRevenue ?? 0) - (totalReduction ?? 0);
  const profit =
    (totalNetRevenue ?? 0) +
    (expense?.totalOtherIncome ?? 0) +
    totalExpenses -
    (expense?.totalOtherSpend ?? 0);

  const reportItems: ReportItem[] = [
    {
      id: "1",
      description: t("report.salesRevenue"),
      amount: formatPriceVN(revenue?.totalRevenue),
    },
    {
      id: "2",
      description: t("report.revenueDeduction", { part: "(2.1 + 2.2)" }),
      amount: formatPriceVN(totalReduction),
    },
    {
      id: "2.1",
      description: t("report.invoiceDiscount"),
      amount: formatPriceVN(discountAndPoint),
      isChild: true,
    },
    {
      id: "2.2",
      description: t("report.refundAmount"),
      amount: formatPriceVN(revenue?.cancelValue),
      isChild: true,
    },
    {
      id: "3",
      description: t("report.finalRevenue", { part: "(1 - 2)" }),
      amount: formatPriceVN(totalNetRevenue),
    },
    {
      id: "4",
      description: t("report.imExInventorty", { part: "(4.2 - 4.1)" }),
      amount: formatPriceVN(totalExpenses),
    },
    {
      id: "4.1",
      description: t("report.stockImport"),
      amount: formatPriceVN(expense?.totalStockImport),
      isChild: true,
    },
    {
      id: "4.2",
      description: t("report.stockExport"),
      amount: formatPriceVN(expense?.totalStockExport),
      isChild: true,
    },
    {
      id: "5",
      description: t("report.incomeOther"),
      amount: formatPriceVN(expense?.totalOtherIncome),
    },
    {
      id: "6",
      description: t("report.spendOther"),
      amount: formatPriceVN(expense?.totalOtherSpend),
    },
    {
      id: "7",
      description: t("report.profit", { part: "(3 + 4 + 5 - 6)" }),
      amount: formatPriceVN(profit),
    },
  ];

  const handleExportExcel = async () => {
    if (exportLoading || filterLoading) return;
    setExportLoading(true);

    try {
      toast.loading(t("common.excel_downloading"));

      if (!revenue || !expense) {
        throw new Error(t("common.noDataToExport"));
      }

      // Define Excel headers
      const headerCells: ExcelCell[] = [
        {
          start: { c: 1, r: 6 },
          end: { c: 1, r: 7 },
          value: t("report.description"),
        },
        {
          start: { c: 2, r: 6 },
          end: { c: 2, r: 7 },
          value: t("report.amount"),
          isCurrency: true,
        },
      ];

      // Prepare table rows (strip formatting for numeric values in Excel)
      const tableRows = reportItems.map((item) => ({
        description: item.isChild
          ? `    (${item.id})${item.description}`
          : `(${item.id})${item.description}`, // Indent child items
        amount: parseFloat(item.amount.replace(/[^\d,-]/g, "") || "0"), // Convert formatted string to number
      }));

      // Create Excel builder
      const excelBuilder = await createExcelBuilder({
        sheetName: "Finance Report",
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

      excelBuilder
        .addCells(headerCells, excelHeaderStyle)
        .addRows(convertObjectsToRows(tableRows, ["description", "amount"]));
      excelBuilder.rowsCustomStyle([5], { font: { bold: true } }); // Bold statistic labels
      excelBuilder.rowsCustomCurrent([6]); // Format numbers for statistic values
      excelBuilder.autoAdjustColumnWidth(excelBuilder.worksheet, 4); // Auto-adjust columns from row 8

      const bytes = await excelBuilder.write();
      const result = exportFile(
        `FINANCE_REPORT_${format(new Date(), "ddMMyyyy")}.xlsx`,
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

  if (filterLoading) {
    return (
      <div className="w-full mx-auto flex items-center justify-center py-8">
        <div className="flex flex-col items-center gap-2">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
          <span>{t("loading")}</span>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full mx-auto">
      <div className="flex justify-end mb-4">
        <Button
          onClick={handleExportExcel}
          disabled={exportLoading}
          className="ml-4"
        >
          {exportLoading ? t("common.loading") : t("common.downloadExcel")}
        </Button>
      </div>
      <table className="w-full border-collapse">
        <thead>
          <tr className="bg-primary-foreground">
            <th className="border-b p-2 text-right font-bold text-lg"></th>
            <th className="border-b p-2 text-right font-bold text-lg sm:col-span-3">
              {t("report.total")}
            </th>
            {!isMobile && (
              <>
                <th className="border-b p-2 text-right font-bold text-lg"></th>
                <th className="border-b p-2 text-right font-bold text-lg"></th>
              </>
            )}
          </tr>
        </thead>
        <tbody>
          {reportItems.map((item) => (
            <tr key={item.id} className="border-b">
              <td className={`p-2 ${item.isChild ? "pl-6" : "font-bold"}`}>
                {item.id}. {item.description}
              </td>
              <td className="p-2 text-right sm:col-span-3">{item.amount}</td>
              {!isMobile && (
                <>
                  <td className="p-2 w-[20%] text-right"></td>
                  <td className="p-2 w-[20%] text-right"></td>
                </>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
