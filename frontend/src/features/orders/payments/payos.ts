import axios from "axios";

import { apiClient } from "@/api/client";

type ProblemDetails = {
  title?: string;
  detail?: string;
};

export async function redirectToPayOsCheckout(orderId: number): Promise<void> {
  const response = await apiClient.createOrReuseOrderPaymentLink(orderId);
  const payment = response.data.results;
  const status = payment?.status?.trim().toUpperCase();

  if (
    (status === "PENDING" || status === "PROCESSING") &&
    payment?.checkoutUrl
  ) {
    window.location.assign(payment.checkoutUrl);
    return;
  }

  if (status === "PAID") {
    const query = new URLSearchParams({
      code: "00",
      cancel: "false",
      status: "PAID",
      orderCode: orderId.toString(),
    });
    window.location.assign(`/payment/payos-return?${query.toString()}`);
    return;
  }

  throw new Error("PayOS returned an unsupported payment state.");
}

export function getPaymentErrorMessage(
  error: unknown,
  fallback: string,
): string {
  if (axios.isAxiosError<ProblemDetails>(error)) {
    return (
      error.response?.data?.title ?? error.response?.data?.detail ?? fallback
    );
  }

  return error instanceof Error ? error.message : fallback;
}
