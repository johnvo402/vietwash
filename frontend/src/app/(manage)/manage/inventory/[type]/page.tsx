import { InventoryType } from "@/api/generated";
import { ContentLayout } from "@/components/admin-panel/content-layout";
import InventoryListingView from "@/features/inventories/imports/views/list-inventory-document-view";
interface PageProps {
  params: Promise<{
    type: string;
  }>;
}
export default async function Page({ params: paramsPromise }: PageProps) {
  const params = await paramsPromise;
  const getType = (): InventoryType => {
    const type = params.type?.toLowerCase();
    if (type === "import") return InventoryType.Import;
    return InventoryType.Export;
  };
  return (
    <ContentLayout scrollable={false}>
      <InventoryListingView type={getType()} />
    </ContentLayout>
  );
}
