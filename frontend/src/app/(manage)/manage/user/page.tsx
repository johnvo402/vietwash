import { ContentLayout } from "@/components/admin-panel/content-layout";
import UserListingPage from "@/features/users/UserPageList";

export default async function Page() {
  return (
    <ContentLayout scrollable={false}>
      <div className="flex flex-1 flex-col space-y-4">
        <UserListingPage />
      </div>
    </ContentLayout>
  );
}
