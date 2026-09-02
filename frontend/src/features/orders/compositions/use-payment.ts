import { useState, useCallback } from "react";
import { useTranslations } from "next-intl";
import { apiClient } from "@/api/client";
import { GetOrderDetailByCodeResponse, OrderStatus } from "@/api/generated/api";
import {
  getPaymentErrorMessage,
  redirectToPayOsCheckout,
} from "../payments/payos";
interface UsePaymentProps {
  onPaymentSuccess?: (method: "cash" | "card") => void;
  onClose?: () => void;
}

export function usePayment({ onPaymentSuccess, onClose }: UsePaymentProps) {
  const t = useTranslations();
  const [paymentStep, setPaymentStep] = useState<"barcode" | "paymentMethod">(
    "barcode",
  );
  const [barcode, setBarcode] = useState<string | null>(null);
  const [order, setOrder] = useState<GetOrderDetailByCodeResponse | null>(null);
  const [isCreatingLink, setIsCreatingLink] = useState(false);
  const [isCompletingCash, setIsCompletingCash] = useState(false);
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
        if (result.status !== OrderStatus.Processed) {
          setMessage("");
          return;
        }
        setMessage("");
        setPaymentStep("paymentMethod");
      } catch (error) {
        console.error("Lỗi khi lấy đơn dịch vụ:", error);
        setMessage(error instanceof Error ? error.message : t("common.error"));
      }
    },
    [t],
  );

  const handleCashPayment = useCallback(async () => {
    if (order?.status !== OrderStatus.Processed) {
      setMessage(t("order.notProcessed"));
      return;
    }

    setIsCompletingCash(true);
    try {
      await apiClient.ecommerceApiOrdersUpdateStatusidPut(
        order.id!.toString(),
        {
          status: OrderStatus.Completed,
          paymentMethod: "Cash",
        },
      );
      setMessage(t("order.cashConfirmed"));
      onPaymentSuccess?.("cash");
      onClose?.();
    } catch (error) {
      setMessage(getPaymentErrorMessage(error, t("common.error")));
    } finally {
      setIsCompletingCash(false);
    }
  }, [order?.id, order?.status, t, onPaymentSuccess, onClose]);

  const handleGetPaymentLink = useCallback(async () => {
    if (!order || order.status !== OrderStatus.Processed) {
      setMessage(t("order.notProcessed"));
      return;
    }

    setIsCreatingLink(true);
    try {
      await redirectToPayOsCheckout(order.id!);
    } catch (error) {
      console.error("Lỗi khi tạo link thanh toán:", error);
      setMessage(getPaymentErrorMessage(error, t("common.error")));
    } finally {
      setIsCreatingLink(false);
    }
  }, [order, t]);

  const handleClosePayOS = useCallback(() => {
    setPaymentStep("barcode");
    onClose?.();
  }, [onClose]);

  const resetState = useCallback(() => {
    setMessage("");
    setPaymentStep("barcode");
    setBarcode(null);
    setOrder(null);
  }, []);

  return {
    paymentStep,
    barcode,
    order,
    isCreatingLink,
    isCompletingCash,
    message,
    handleBarcodeScan,
    handleCashPayment,
    handleGetPaymentLink,
    handleClosePayOS,
    resetState,
    setPaymentStep,
    setOrder,
  };
}
