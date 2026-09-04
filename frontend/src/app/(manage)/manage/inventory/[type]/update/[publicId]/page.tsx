import { InventoryType } from "@/api/generated/api";
import { ContentLayout } from "@/components/admin-panel/content-layout";
import UpdateInventoryDocumentPage from "@/features/inventories/imports/views/update-inventory-view";
interface DetailProps {
  params: Promise<{
    type: string;
    publicId: string;
  }>;
}

export default async function Page({ params: paramsPromise }: DetailProps) {
  const params = await paramsPromise;
  const getType = (): InventoryType => {
    const type = params.type?.toLowerCase();
    if (type === "import") return InventoryType.Import;
    return InventoryType.Export;
  };
  return (
    <ContentLayout scrollable={false}>
      <UpdateInventoryDocumentPage
        publicId={params.publicId}
        type={getType()}
      />
    </ContentLayout>
  );
}
