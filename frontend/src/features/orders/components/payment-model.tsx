"use client";

import React, { useEffect } from "react";
import { useTranslations } from "next-intl";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { usePayment } from "../compositions/use-payment";
import { QRScanner } from "@/components/qr-scanner";
import { Order } from "@/features/cashier/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import {
  CalendarIcon,
  ShoppingCartIcon,
  StickyNoteIcon,
  User,
} from "lucide-react";
import { formatPriceVN } from "@/utils/format";
import { useIsMobile } from "@/hooks/use-mobile";
import { usePageType } from "@/hooks/use-page-type"; // Added hook
import { usePushRouter } from "@/utils/router-utli";
import {
  ROUTE_CASHIER_ORDERS_DETAIL,
  ROUTE_ORDERS_DETAIL,
} from "@/types/router-type";
import { OrderStatus } from "@/api/generated/api";

interface PaymentModalProps {
  isOpen: boolean;
  onClose: () => void;
  onPaymentSuccess: (method: "cash" | "card") => void;
  stepDefault?: "barcode" | "paymentMethod";
  orderDefault?: any;
}

export function PaymentModal({
  isOpen,
  onClose,
  onPaymentSuccess,
  stepDefault,
  orderDefault,
}: PaymentModalProps) {
  const t = useTranslations();
  const isMobile = useIsMobile();
  const { isCashierPage } = usePageType();
  const { pushRouter } = usePushRouter();

  const {
    order,
    paymentStep,
    setPaymentStep,
    setOrder,
    message,
    handleBarcodeScan,
    handlePaymentMethod,
    handleGetPaymentLink,
    resetState,
  } = usePayment({ onPaymentSuccess, onClose });

  useEffect(() => {
    if (stepDefault) {
      setPaymentStep(stepDefault);
    }
    if (orderDefault) {
      setOrder(orderDefault);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [stepDefault, orderDefault]);

  const handleScanSuccess = (value: string) => {
    handleBarcodeScan(value);
  };

  const handleScanError = (error: string) => {
    console.error("QR Scan Error:", error);
  };

  const PaymentMethodSelection = () => (
    <div className={`p-4 ${isMobile ? "space-y-3" : "space-y-4"}`}>
      <p className={`mb-4 ${isMobile ? "text-sm" : "text-base"}`}>
        {t("order.selectPaymentMethod")}
      </p>
      <Button
        onClick={() => handlePaymentMethod("cash")}
        className={`w-full ${isMobile ? "text-sm py-2" : "text-base"} bg-green-600 hover:bg-green-700`}
      >
        {t("order.cash")}
      </Button>
      <Button
        onClick={() => {
          handlePaymentMethod("card");
          handleGetPaymentLink();
        }}
        className={`w-full ${isMobile ? "text-sm py-2" : "text-base"} bg-blue-600 hover:bg-blue-700`}
      >
        {t("order.card")}
      </Button>
    </div>
  );

  const getOrderStatusMessage = () => {
    if (!order?.status) return null;

    const status = order.status.toLowerCase();
    const isProcessed = status === OrderStatus.Processed.toLowerCase();
    const isCompleted = status === "completed";
    const isCanceled = status === OrderStatus.Cancelled.toLowerCase();

    const handleViewDetails = () => {
      pushRouter({
        router: isCashierPage
          ? ROUTE_CASHIER_ORDERS_DETAIL
          : ROUTE_ORDERS_DETAIL,
        params: { publicId: order.publicId?.toString()! },
        state: { [order.publicId?.toString()!]: order.id },
        redirect: "blank",
      });
    };

    if (!isProcessed && !isCompleted && !isCanceled) {
      return (
        <>
          <p className="text-yellow-600">{t("order.notProcessed")}</p>
          <Button
            onClick={handleViewDetails}
            className={`mt-4 ${isMobile ? "text-sm py-2" : "text-base"} bg-primary hover:bg-primary/90`}
          >
            {t("common.viewDetails")}
          </Button>
        </>
      );
    } else if (isCompleted) {
      return (
        <>
          <p className="text-green-600">{t("order.completed")}</p>
          <Button
            onClick={handleViewDetails}
            className={`mt-4 ${isMobile ? "text-sm py-2" : "text-base"} bg-primary hover:bg-primary/90`}
          >
            {t("common.viewDetails")}
          </Button>
        </>
      );
    } else if (isCanceled) {
      return (
        <>
          <p className="text-red-600">{t("order.canceled")}</p>
          <Button
            onClick={handleViewDetails}
            className={`mt-4 ${isMobile ? "text-sm py-2" : "text-base"} bg-primary hover:bg-primary/90`}
          >
            {t("common.viewDetails")}
          </Button>
        </>
      );
    }
    return null;
  };

  return (
    <Dialog
      open={isOpen}
      onOpenChange={() => {
        resetState();
        onClose();
      }}
    >
      <DialogContent
        className={isMobile ? "p-2 max-w-[95vw]" : "w-auto min-w-max max-w-4xl"}
      >
        <DialogHeader className={isMobile ? "px-2" : "px-4"}>
          <DialogTitle className={isMobile ? "text-lg" : "text-xl"}>
            {t("order.processPayment")}
          </DialogTitle>
        </DialogHeader>
        {message || getOrderStatusMessage() ? (
          <div className={isMobile ? "space-y-4" : "space-y-6"}>
            {order && <PaymentInfo order={order as Order} />}
            <div
              className={`p-4 text-center font-medium ${isMobile ? "text-sm" : "text-base"}`}
            >
              {message && <p>{message}</p>}
              {getOrderStatusMessage()}
              {message && (
                <Button
                  onClick={() => {
                    resetState();
                    onClose();
                  }}
                  className={`mt-4 ${isMobile ? "text-sm py-2" : "text-base"}`}
                >
                  {t("order.backToPayment")}
                </Button>
              )}
            </div>
          </div>
        ) : (
          <>
            {paymentStep === "barcode" && (
              <QRScanner
                onScanSuccess={handleScanSuccess}
                onScanError={handleScanError}
                autoStart={true}
                className={isMobile ? "w-full" : "w-[25vw]"}
              />
            )}
            {paymentStep === "paymentMethod" && (
              <div
                className={`flex ${isMobile ? "flex-col gap-2" : "flex-row gap-4"} min-w-max`}
              >
                <div className={isMobile ? "w-full" : "flex-1"}>
                  <PaymentMethodSelection />
                </div>
                <div className={isMobile ? "w-full" : "flex-1"}>
                  <PaymentInfo order={order as Order} />
                </div>
              </div>
            )}
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}

function PaymentInfo({ order }: { order: Order }) {
  const t = useTranslations();
  const isMobile = useIsMobile();

  return (
    <div className={`w-full mx-auto ${isMobile ? "space-y-4" : "space-y-6"}`}>
      {/* Header Card */}
      <Card className="border-2 border-primary/10 bg-gradient-to-r from-primary/5 to-primary/10">
        <CardHeader className={isMobile ? "pb-2" : "pb-4"}>
          <CardTitle
            className={`flex items-center justify-center gap-2 ${isMobile ? "text-lg" : "text-2xl"} font-bold text-center`}
          >
            <ShoppingCartIcon className={isMobile ? "h-5 w-5" : "h-6 w-6"} />
            {t("order.orderDetails")}
          </CardTitle>
        </CardHeader>
      </Card>

      {/* Main Content Grid */}
      <div
        className={`grid ${isMobile ? "grid-cols-1 gap-2" : "grid-cols-1 lg:grid-cols-3 gap-4"}`}
      >
        {/* Customer Information */}
        <Card className="hover:shadow-lg transition-shadow duration-300">
          <CardHeader className={isMobile ? "pb-2" : "pb-3"}>
            <CardTitle
              className={`flex items-center gap-2 text-primary ${isMobile ? "text-base" : "text-lg"}`}
            >
              <User className={isMobile ? "h-4 w-4" : "h-5 w-5"} />
              {t("user.customerInformation")}
            </CardTitle>
          </CardHeader>
          <CardContent className={isMobile ? "space-y-2" : "space-y-4"}>
            <div className={isMobile ? "space-y-2" : "space-y-3"}>
              <div className="flex flex-col space-y-1">
                <span
                  className={`text-sm font-medium text-muted-foreground ${isMobile ? "text-xs" : ""}`}
                >
                  {t("order.customerName")}
                </span>
                <span className={`font-semibold ${isMobile ? "text-sm" : ""}`}>
                  {order.customer?.displayName || "--"}
                </span>
              </div>

              {order.customer?.phoneNumber && (
                <div className="flex flex-col space-y-1">
                  <span
                    className={`text-sm font-medium text-muted-foreground ${isMobile ? "text-xs" : ""}`}
                  >
                    {t("order.customerPhone")}
                  </span>
                  <span className={`font-medium ${isMobile ? "text-sm" : ""}`}>
                    {order.customer.phoneNumber}
                  </span>
                </div>
              )}

              {order.customer?.customerGroup && (
                <div className="flex flex-col space-y-2">
                  <span
                    className={`text-sm font-medium text-muted-foreground ${isMobile ? "text-xs" : ""}`}
                  >
                    {t(`user.customer.customerGroup.title`)}
                  </span>
                  <Badge
                    variant={
                      order.customer.customerGroup === "Loyal"
                        ? "default"
                        : "secondary"
                    }
                    className="w-fit"
                  >
                    {t(
                      `user.customer.customerGroup.${order.customer.customerGroup.toLowerCase()}`
                    )}
                  </Badge>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Order Information */}
        <Card className="hover:shadow-lg transition-shadow duration-300">
          <CardHeader className={isMobile ? "pb-2" : "pb-3"}>
            <CardTitle
              className={`flex items-center gap-2 text-primary ${isMobile ? "text-base" : "text-lg"}`}
            >
              <CalendarIcon className={isMobile ? "h-4 w-4" : "h-5 w-5"} />
              {t("cashier.orderInformation")}
            </CardTitle>
          </CardHeader>
          <CardContent className={isMobile ? "space-y-2" : "space-y-4"}>
            <div className={isMobile ? "space-y-2" : "space-y-3"}>
              {order.code && (
                <div className="flex flex-col space-y-1">
                  <span
                    className={`text-sm font-medium text-muted-foreground ${isMobile ? "text-xs" : ""}`}
                  >
                    {t("order.orderCode")}
                  </span>
                  <span
                    className={`font-mono font-semibold bg-muted px-2 py-1 rounded text-sm w-fit ${isMobile ? "text-xs" : ""}`}
                  >
                    {order.code}
                  </span>
                </div>
              )}

              {order.orderDate && (
                <div className="flex flex-col space-y-1">
                  <span
                    className={`text-sm font-medium text-muted-foreground ${isMobile ? "text-xs" : ""}`}
                  >
                    {t("order.orderDate")}
                  </span>
                  <span className={`font-medium ${isMobile ? "text-sm" : ""}`}>
                    {order.createdAt ? order.createdAt.toLocaleString() : "--"}
                  </span>
                </div>
              )}

              <div className="flex flex-col space-y-1">
                <span
                  className={`text-sm font-medium text-muted-foreground ${isMobile ? "text-xs" : ""}`}
                >
                  {t("order.discount")}
                </span>
                <span
                  className={`font-medium text-green-600 ${isMobile ? "text-sm" : ""}`}
                >
                  {order.discountFixed
                    ? formatPriceVN(order.discountValue)
                    : `${order.discountValue}%`}
                </span>
              </div>

              <Separator />

              <div className="flex flex-col space-y-1">
                <span
                  className={`text-sm font-medium text-muted-foreground ${isMobile ? "text-xs" : ""}`}
                >
                  {t("order.totalPaid")}
                </span>
                <span
                  className={`font-bold text-primary ${isMobile ? "text-lg" : "text-xl"}`}
                >
                  {formatPriceVN(order.total)}
                </span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Notes */}
        <Card className="hover:shadow-lg transition-shadow duration-300">
          <CardHeader className={isMobile ? "pb-2" : "pb-3"}>
            <CardTitle
              className={`flex items-center gap-2 text-primary ${isMobile ? "text-base" : "text-lg"}`}
            >
              <StickyNoteIcon className={isMobile ? "h-4 w-4" : "h-5 w-5"} />
              {t("common.note")}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div
              className={`min-h-[80px] p-3 bg-muted/50 rounded-lg border-l-4 border-primary/30 ${isMobile ? "min-h-[60px]" : ""}`}
            >
              <p
                className={`text-sm leading-relaxed ${isMobile ? "text-xs" : ""}`}
              >
                {order.note || (
                  <span className="text-muted-foreground italic">--</span>
                )}
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
