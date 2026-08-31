import { ContentLayout } from "@/components/admin-panel/content-layout";
import OrdersPageView from "@/features/orders/order-view";

export default function OrdersPage() {
  return (
    <ContentLayout scrollable={false}>
      <OrdersPageView />
    </ContentLayout>
  );
}
