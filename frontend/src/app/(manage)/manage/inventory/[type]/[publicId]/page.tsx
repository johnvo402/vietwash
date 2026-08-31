import { ContentLayout } from "@/components/admin-panel/content-layout";
import DetailInventoryDocumentPage from "@/features/inventories/imports/views/detail-inventory-view";
interface DetailProps {
  params: {
    type: string;
    publicId: string;
  };
}

export default function Page({ params }: DetailProps) {
  return (
    <ContentLayout scrollable={false}>
      <DetailInventoryDocumentPage params={params} />
    </ContentLayout>
  );
}
