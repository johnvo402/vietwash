"use client";
import { ContentLayout } from "@/components/admin-panel/content-layout";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import OrdersPageView from "@/features/orders/order-view";
import { ROUTE_CASHIER } from "@/types/router-type";
import { usePushRouter } from "@/utils/router-utli";

export default function OrdersPage() {
  const { pushRouter } = usePushRouter();

  const handleClose = () => {
    pushRouter({
      router: ROUTE_CASHIER,
    });
  };

  return (
    <Dialog open={true} onOpenChange={handleClose}>
      <DialogContent className="w-screen h-screen max-w-none rounded-none overflow-y-auto p-0">
        <ContentLayout scrollable={true}>
          <OrdersPageView />
        </ContentLayout>
      </DialogContent>
    </Dialog>
  );
}
