import { ContentLayout } from "@/components/admin-panel/content-layout";
import DetailInventoryDocumentPage from "@/features/inventories/imports/views/detail-inventory-view";
interface DetailProps {
  params: Promise<{
    type: string;
    publicId: string;
  }>;
}

export default async function Page({ params: paramsPromise }: DetailProps) {
  const params = await paramsPromise;
  return (
    <ContentLayout scrollable={false}>
      <DetailInventoryDocumentPage params={params} />
    </ContentLayout>
  );
}
