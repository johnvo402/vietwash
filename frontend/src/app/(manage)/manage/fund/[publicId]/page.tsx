import { ContentLayout } from "@/components/admin-panel/content-layout";
import FundDetailView from "@/features/fund/fund-detail-view";

interface DetailProps {
  params: { publicId: string };
}
export default function Page({ params }: DetailProps) {
  return (
    <ContentLayout scrollable={false}>
      <FundDetailView params={params} />
    </ContentLayout>
  );
}
