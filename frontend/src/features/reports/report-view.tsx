"use client";
import { Search, FileBarChart } from "lucide-react";
import { Input } from "@/components/ui/input";
import Link from "next/link";
import { useTranslations } from "next-intl";
import { useState } from "react";
import {
  ROUTE_REPORT_CUSTOMER,
  ROUTE_REPORT_FINANCE,
  ROUTE_REPORT_ORDER,
  ROUTE_REPORT_REVENUE,
  ROUTE_REPORT_SERVICE,
  ROUTE_REPORT_SUPPLIER,
} from "@/types/router-type";

export default function ReportView() {
  const t = useTranslations("report");
  const [searchTerm, setSearchTerm] = useState("");

  // Define nested array for report clusters
  const reportClusters = [
    {
      title: t("totalReport"),
      reports: [
        { title: t("orderReport"), href: ROUTE_REPORT_ORDER },
        { title: t("serviceReport"), href: ROUTE_REPORT_SERVICE },
      ],
    },
    {
      title: t("revenueReportsTitle"),
      reports: [
        { title: t("revenueByDayReportsTitle"), href: ROUTE_REPORT_REVENUE },

        { title: t("customerRevenueReport"), href: ROUTE_REPORT_CUSTOMER },
      ],
    },
    {
      title: t("supplier.title"),
      reports: [
        { title: t("importExportReport"), href: ROUTE_REPORT_SUPPLIER },
      ],
    },
    {
      title: t("other"),
      reports: [{ title: t("financeReport"), href: ROUTE_REPORT_FINANCE }],
    },
  ];

  // Filter reports based on search term
  const filteredClusters = reportClusters.map((cluster) => ({
    ...cluster,
    reports: cluster.reports.filter((report) =>
      report.title.toLowerCase().includes(searchTerm.toLowerCase())
    ),
  }));

  return (
    <div className="mx-auto px-4 py-8 max-w-6xl">
      {/* Search Bar */}
      <div className="relative mb-8">
        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
        <Input
          className="pl-10 border-muted rounded-full h-12 bg-background"
          placeholder={t("searchPlaceholder")}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </div>

      {/* Report Clusters */}
      {filteredClusters.map((cluster, clusterIndex) => (
        <div key={clusterIndex} className="mb-10">
          {cluster.reports.length > 0 && (
            <>
              <h2 className="text-2xl font-bold mb-4">{cluster.title}</h2>
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {cluster.reports.map((report, reportIndex) => (
                  <ReportCard
                    key={`${clusterIndex}-${reportIndex}`}
                    title={report.title}
                    href={report.href}
                  />
                ))}
              </div>
            </>
          )}
        </div>
      ))}
    </div>
  );
}
function ReportCard({ title, href }: { title: string; href: string }) {
  return (
    <Link href={href} className="block">
      <div className="h-48 rounded-lg bg-background shadow-sm p-6 flex flex-col items-center justify-center hover:shadow-md transition-shadow cursor-pointer">
        <div className="w-16 h-16 rounded-full bg-primary-foreground flex items-center justify-center mb-4">
          <div className="w-10 h-10 rounded-md bg-secondary flex items-center justify-center">
            <FileBarChart className="w-6 h-6 text-primary" />
          </div>
        </div>
        <h3 className="text-center text-primary font-bold line-clamp-2">
          {title}
        </h3>
      </div>
    </Link>
  );
}
