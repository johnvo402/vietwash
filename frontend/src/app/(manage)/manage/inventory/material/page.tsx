import { ContentLayout } from "@/components/admin-panel/content-layout";
import MaterialListingView from "@/features/inventories/materials/view/list-material-view";

export default async function Page() {
  return (
    <ContentLayout scrollable={false}>
      <MaterialListingView />
    </ContentLayout>
  );
}
