import VoucherView from "@/features/voucher/views/voucher-view-list";
import { ContentLayout } from "@/components/admin-panel/content-layout";

export default function Page() {
  return (
    <ContentLayout scrollable={false}>
      <VoucherView />
    </ContentLayout>
  );
}
