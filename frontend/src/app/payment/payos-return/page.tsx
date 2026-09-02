"use client";

import Link from "next/link";
import { Suspense, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import { useTranslations } from "next-intl";
import {
  CheckCircle2,
  CircleAlert,
  Clock3,
  Loader2,
  XCircle,
} from "lucide-react";

import { apiClient } from "@/api/client";
import { OrderStatus } from "@/api/generated/api";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader } from "@/components/ui/card";

const MAX_POLL_ATTEMPTS = 15;
const POLL_INTERVAL_MS = 2_000;

type ReturnState =
  | "checking"
  | "confirmed"
  | "cancelled"
  | "timeout"
  | "error"
  | "invalid";

function PayOsReturnContent() {
  const t = useTranslations("order");
  const searchParams = useSearchParams();
  const [state, setState] = useState<ReturnState>("checking");
  const [retryKey, setRetryKey] = useState(0);

  const paymentReturn = useMemo(() => {
    const rawOrderCode = searchParams.get("orderCode");
    const orderCode = rawOrderCode ? Number(rawOrderCode) : Number.NaN;
    return {
      code: searchParams.get("code"),
      paymentLinkId: searchParams.get("id"),
      cancelled:
        searchParams.get("cancel")?.toLowerCase() === "true" ||
        searchParams.get("status")?.toUpperCase() === "CANCELLED",
      providerStatus: searchParams.get("status")?.toUpperCase() ?? "",
      orderCode,
      validOrderCode: Number.isSafeInteger(orderCode) && orderCode > 0,
    };
  }, [searchParams]);

  useEffect(() => {
    if (!paymentReturn.validOrderCode || paymentReturn.code !== "00") {
      setState("invalid");
      return;
    }

    if (paymentReturn.cancelled) {
      setState("cancelled");
      return;
    }

    if (
      !["PAID", "PENDING", "PROCESSING"].includes(paymentReturn.providerStatus)
    ) {
      setState("invalid");
      return;
    }

    let disposed = false;
    let timer: ReturnType<typeof setTimeout> | undefined;
    let attempts = 0;
    setState("checking");

    const checkLocalOrder = async () => {
      attempts += 1;
      try {
        const response = await apiClient.ecommerceApiOrdersId(
          paymentReturn.orderCode,
        );
        if (disposed) return;

        const localStatus = response.data.results?.status;
        if (localStatus === OrderStatus.Completed) {
          setState("confirmed");
          return;
        }
      } catch {
        if (disposed) return;
        if (attempts >= MAX_POLL_ATTEMPTS) {
          setState("error");
          return;
        }
      }

      if (attempts >= MAX_POLL_ATTEMPTS) {
        setState("timeout");
        return;
      }
      timer = setTimeout(checkLocalOrder, POLL_INTERVAL_MS);
    };

    void checkLocalOrder();
    return () => {
      disposed = true;
      if (timer) clearTimeout(timer);
    };
  }, [paymentReturn, retryKey]);

  const isPaidHint = paymentReturn.providerStatus === "PAID";
  const content = {
    checking: {
      icon: <Loader2 className="h-8 w-8 animate-spin" aria-hidden="true" />,
      title: isPaidHint ? t("payOsReceived") : t("payOsWaiting"),
      description: t("payOsConfirming"),
      className: "text-blue-700 dark:text-blue-300",
    },
    confirmed: {
      icon: <CheckCircle2 className="h-8 w-8" aria-hidden="true" />,
      title: t("payOsConfirmed"),
      description: t("payOsConfirmedDescription"),
      className: "text-green-700 dark:text-green-300",
    },
    cancelled: {
      icon: <XCircle className="h-8 w-8" aria-hidden="true" />,
      title: t("payOsCancelled"),
      description: t("payOsCancelledDescription"),
      className: "text-amber-700 dark:text-amber-300",
    },
    timeout: {
      icon: <Clock3 className="h-8 w-8" aria-hidden="true" />,
      title: isPaidHint ? t("payOsReceived") : t("payOsWaiting"),
      description: t("payOsTimeout"),
      className: "text-amber-700 dark:text-amber-300",
    },
    error: {
      icon: <CircleAlert className="h-8 w-8" aria-hidden="true" />,
      title: t("payOsCheckFailed"),
      description: t("payOsCheckFailedDescription"),
      className: "text-destructive",
    },
    invalid: {
      icon: <CircleAlert className="h-8 w-8" aria-hidden="true" />,
      title: t("payOsInvalidReturn"),
      description: t("payOsInvalidReturnDescription"),
      className: "text-destructive",
    },
  }[state];

  return (
    <main className="flex min-h-dvh items-center justify-center bg-muted/30 px-4 py-10">
      <Card className="w-full max-w-xl shadow-sm">
        <CardHeader className="space-y-4 text-center">
          <div className={`mx-auto ${content.className}`}>{content.icon}</div>
          <h1 className="text-balance text-2xl font-semibold leading-tight tracking-tight">
            {content.title}
          </h1>
        </CardHeader>
        <CardContent className="space-y-6">
          <Alert
            role="status"
            aria-live="polite"
            aria-atomic="true"
            aria-busy={state === "checking"}
          >
            <AlertTitle>
              {t("payOsOrder", { orderCode: paymentReturn.orderCode })}
            </AlertTitle>
            <AlertDescription className="mt-2 leading-relaxed">
              {content.description}
            </AlertDescription>
          </Alert>

          {(state === "timeout" || state === "error") && (
            <Button
              className="w-full"
              onClick={() => setRetryKey((value) => value + 1)}
            >
              {t("payOsRetry")}
            </Button>
          )}

          <Button asChild variant="outline" className="w-full">
            <Link href="/manage/orders">{t("payOsBackToOrders")}</Link>
          </Button>

          {paymentReturn.paymentLinkId && (
            <p className="[overflow-wrap:anywhere] text-center text-xs text-muted-foreground">
              {t("payOsReference")}: {paymentReturn.paymentLinkId}
            </p>
          )}
        </CardContent>
      </Card>
    </main>
  );
}

export default function PayOsReturnPage() {
  return (
    <Suspense
      fallback={<div className="min-h-dvh bg-muted/30" aria-busy="true" />}
    >
      <PayOsReturnContent />
    </Suspense>
  );
}
