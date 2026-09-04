import { ContentLayout } from "@/components/admin-panel/content-layout";
import FundDetailView from "@/features/fund/fund-detail-view";

interface DetailProps {
  params: Promise<{ publicId: string }>;
}
export default async function Page({ params: paramsPromise }: DetailProps) {
  const params = await paramsPromise;
  return (
    <ContentLayout scrollable={false}>
      <FundDetailView params={params} />
    </ContentLayout>
  );
}
