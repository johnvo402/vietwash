import { ContentLayout } from "@/components/admin-panel/content-layout";
import ReportFinanceView from "@/features/reports/finance/finance-report-view";

export default async function Page() {
  return (
    <ContentLayout scrollable={false}>
      <ReportFinanceView />
    </ContentLayout>
  );
}
