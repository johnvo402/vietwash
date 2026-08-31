import { ContentLayout } from "@/components/admin-panel/content-layout";
import ReportServiceView from "@/features/reports/services/report-report-view";

export default async function Page() {
  return (
    <ContentLayout scrollable={false}>
      <ReportServiceView />
    </ContentLayout>
  );
}
