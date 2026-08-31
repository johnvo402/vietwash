import { ContentLayout } from "@/components/admin-panel/content-layout";
import StatisticsPage from "@/features/dashboards/dashboard-view";
import { useTranslations } from "next-intl";

export default function Page() {
  const t = useTranslations("route");
  return (
    <ContentLayout scrollable={false}>
      <StatisticsPage />
    </ContentLayout>
  );
}
