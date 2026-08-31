import { ContentLayout } from "@/components/admin-panel/content-layout";
import CustomerListingPage from "@/features/customer/UserPageList";

export default async function Page() {
  return (
    <ContentLayout scrollable={false}>
      <div className="flex flex-1 flex-col space-y-4">
        <CustomerListingPage />
      </div>
    </ContentLayout>
  );
}
