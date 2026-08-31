import { ContentLayout } from "@/components/admin-panel/content-layout";
import CustomerRevenueView from "@/features/reports/customer-revenue-report/customer-revenue-report-view";

export default async function Page() {
  return (
    <ContentLayout scrollable={false}>
      <CustomerRevenueView />
    </ContentLayout>
  );
}
