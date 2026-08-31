import { ContentLayout } from "@/components/admin-panel/content-layout";
import SupplierListingPage from "@/features/supplier/supplier-page";

export default async function Page() {
  return (
    <ContentLayout scrollable={false}>
      <div className="flex flex-1 flex-col space-y-4 max-w-full">
        <SupplierListingPage />
      </div>
    </ContentLayout>
  );
}
