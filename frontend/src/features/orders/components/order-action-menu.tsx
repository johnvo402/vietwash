"use client";

import { useRef, useState } from "react";
import { useTranslations } from "next-intl";
import {
  MoreHorizontal,
  Truck,
  Package,
  Banknote,
  XCircle,
} from "lucide-react";
import { toast } from "react-toastify";
import { ListOrderResponse, OrderStatus, PaymentMethod } from "@/api/generated";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { getOrderActions } from "../order-lifecycle";
import { useOrderTransition } from "../compositions/use-order-transition";
import {
  getPaymentErrorMessage,
  redirectToPayOsCheckout,
} from "../payments/payos";
import { StartOrderDialog } from "./start-order-dialog";
import { CancelOrderDialog } from "./cancel-order-dialog";
import { PaymentMethodSelect } from "./PaymentMethodSelect";

export function OrderActionMenu({
  order,
  onEdit,
  onView,
}: {
  order: ListOrderResponse;
  onEdit?: (id: number) => void;
  onView: () => void;
}) {
  const t = useTranslations();
  const actions = getOrderActions(order.status);
  const [dialog, setDialog] = useState<"start" | "cancel" | "payment" | null>(
    null,
  );
  const [busy, setBusy] = useState(false);
  const submitting = useRef(false);
  const mutation = useOrderTransition();
  const process = async () => {
    if (!actions.process || submitting.current || !order.id) return;
    submitting.current = true;
    setBusy(true);
    try {
      await mutation.mutateAsync({
        id: String(order.id),
        status: OrderStatus.Processed,
      });
    } catch (error) {
      toast.error(
        getPaymentErrorMessage(error, t("order.updateOrderStatusFailed")),
      );
    } finally {
      submitting.current = false;
      setBusy(false);
    }
  };
  const cancel = async (cancellationReason: string) => {
    if (!actions.cancel || !order.id)
      throw new Error(t("order.updateOrderStatusFailed"));
    try {
      await mutation.mutateAsync({
        id: String(order.id),
        status: OrderStatus.Cancelled,
        cancellationReason,
      });
    } catch (error) {
      toast.error(
        getPaymentErrorMessage(error, t("order.errorCancellingOrder")),
      );
      throw error;
    }
  };
  const pay = async (method: PaymentMethod) => {
    if (!actions.complete || !order.id)
      throw new Error(t("order.notProcessed"));
    try {
      if (method === PaymentMethod.Card)
        await redirectToPayOsCheckout(order.id);
      else
        await mutation.mutateAsync({
          id: String(order.id),
          status: OrderStatus.Completed,
          paymentMethod: PaymentMethod.Cash,
        });
    } catch (error) {
      throw new Error(getPaymentErrorMessage(error, t("common.error")));
    }
  };
  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="ghost"
            className="h-8 w-8 p-0"
            disabled={busy || mutation.isPending}
            data-order-status={order.status}
          >
            <span className="sr-only">{t("common.openMenu")}</span>
            <MoreHorizontal className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem onClick={onView}>
            {t("common.viewDetails")}
          </DropdownMenuItem>
          {actions.edit && onEdit && (
            <DropdownMenuItem onClick={() => onEdit(order.id!)}>
              {t("common.update")}
            </DropdownMenuItem>
          )}
          {actions.start && (
            <DropdownMenuItem onClick={() => setDialog("start")}>
              <Truck className="mr-2 h-4 w-4" />
              {t("order.startProcessing")}
            </DropdownMenuItem>
          )}
          {actions.process && (
            <DropdownMenuItem onClick={process}>
              <Package className="mr-2 h-4 w-4" />
              {t("order.markProcessed")}
            </DropdownMenuItem>
          )}
          {actions.complete && (
            <DropdownMenuItem onClick={() => setDialog("payment")}>
              <Banknote className="mr-2 h-4 w-4" />
              {t("order.addPayment")}
            </DropdownMenuItem>
          )}
          {actions.cancel && (
            <>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                className="text-destructive"
                onClick={() => setDialog("cancel")}
              >
                <XCircle className="mr-2 h-4 w-4" />
                {t("common.cancel")}
              </DropdownMenuItem>
            </>
          )}
        </DropdownMenuContent>
      </DropdownMenu>
      <StartOrderDialog
        open={dialog === "start"}
        orderId={order.id ?? 0}
        orderCode={order.code}
        branchId={order.branchId ?? 0}
        onOpenChange={(open) => {
          if (!open) setDialog(null);
        }}
      />
      <CancelOrderDialog
        open={dialog === "cancel"}
        orderCode={order.code}
        isPending={mutation.isPending}
        onOpenChange={(open) => {
          if (!open) setDialog(null);
        }}
        onConfirm={cancel}
      />
      {dialog === "payment" && (
        <PaymentMethodSelect
          isOpen
          onClose={() => setDialog(null)}
          onSubmit={pay}
        />
      )}
    </>
  );
}
