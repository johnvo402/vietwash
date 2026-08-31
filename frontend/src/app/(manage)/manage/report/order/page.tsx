import { ContentLayout } from "@/components/admin-panel/content-layout";
import ReportOrderView from "@/features/reports/order-report/order-report-view";

export default async function Page() {
  return (
    <ContentLayout scrollable={false}>
      <ReportOrderView />
    </ContentLayout>
  );
}
