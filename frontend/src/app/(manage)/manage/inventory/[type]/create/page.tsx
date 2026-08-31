import { InventoryType } from "@/api/generated";
import CreateInventoryDocumentView from "@/features/inventories/imports/views/create-inventory-view";
interface PageProps {
  params: {
    type: string;
  };
}
export default async function Page({ params }: PageProps) {
  const getType = (): InventoryType => {
    const type = params.type?.toLowerCase();
    if (type === "import") return InventoryType.Import;
    return InventoryType.Export;
  };
  return <CreateInventoryDocumentView type={getType()} />;
}
