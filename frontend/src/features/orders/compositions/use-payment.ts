import { useState, useCallback, useEffect } from "react";
import { useTranslations } from "next-intl";
import { apiClient } from "@/api/client";
import { GetOrderDetailByCodeResponse, OrderStatus } from "@/api/generated/api";
import { usePushRouter } from "@/utils/router-utli";
interface UsePaymentProps {
  onPaymentSuccess?: (method: "cash" | "card") => void;
  onClose?: () => void;
}

export function usePayment({ onPaymentSuccess, onClose }: UsePaymentProps) {
  const t = useTranslations();
  const router = usePushRouter(); // dùng để redirect
  const [paymentStep, setPaymentStep] = useState<"barcode" | "paymentMethod">(
    "barcode"
  );
  const [paymentMethod, setPaymentMethod] = useState<"cash" | "card" | null>(
    null
  );
  const [barcode, setBarcode] = useState<string | null>(null);
  const [order, setOrder] = useState<GetOrderDetailByCodeResponse | null>(null);
  const [isCreatingLink, setIsCreatingLink] = useState(false);
  const [message, setMessage] = useState<string>("");

  const handleBarcodeScan = useCallback(
    async (scannedCode: string) => {
      try {
        const response = await apiClient.ecommerceApiOrdersGetByCode({
          code: scannedCode,
        });
        const result = response.data.results;
        if (!result) {
          throw new Error(t("order.invalidBarcode"));
        }

        setBarcode(scannedCode);
        setOrder(result);
        setPaymentStep("paymentMethod");
      } catch (error) {
        console.error("Lỗi khi lấy đơn dịch vụ:", error);
        setMessage(error instanceof Error ? error.message : t("common.error"));
      }
    },
    [t]
  );

  const handlePaymentMethod = useCallback(
    async (method: "cash" | "card") => {
      setPaymentMethod(method);
      if (method === "cash") {
        await apiClient.ecommerceApiOrdersUpdateStatusidPut(
          order?.id?.toString()!,
          { status: OrderStatus.Completed, paymentMethod: "Cash" }
        );
        setMessage(t("order.cashConfirmed"));
        onPaymentSuccess?.("cash");
        onClose?.();
      }
    },
    [order?.id, t, onPaymentSuccess, onClose]
  );

  const handleGetPaymentLink = useCallback(async () => {
    if (!order) return;

    setIsCreatingLink(true);
    try {
      const result = await apiClient.ecommerceApiOrdersGetLinkPaymentid(
        order.id!,
        window.location.href
      );

      if (result.data.status != 200) {
        throw new Error(t("common.error"));
      }
      const data = result.data.results;
      router.pushRouter({
        router: data!.checkoutUrl!,
        redirect: "current",
      });
      setIsCreatingLink(false);
    } catch (error) {
      console.error("Lỗi khi tạo link thanh toán:", error);
      setMessage(t("common.error"));
      setIsCreatingLink(false);
    }
  }, [order, router, t]);

  const handleClosePayOS = useCallback(() => {
    setPaymentStep("barcode");
    onClose?.();
  }, [onClose]);

  const resetState = useCallback(() => {
    setMessage("");
    setPaymentStep("barcode");
    setBarcode(null);
    setPaymentMethod(null);
    setOrder(null);
  }, []);

  return {
    paymentStep,
    barcode,
    order,
    paymentMethod,
    isCreatingLink,
    message,
    handleBarcodeScan,
    handlePaymentMethod,
    handleGetPaymentLink,
    handleClosePayOS,
    resetState,
    setPaymentStep,
    setOrder,
  };
}
