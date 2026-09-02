"use client";

import { useEffect, useId, useRef, useState } from "react";
import { Loader2, XCircle } from "lucide-react";
import { useTranslations } from "next-intl";

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";

const MIN_REASON_LENGTH = 3;
const MAX_REASON_LENGTH = 500;

interface CancelOrderDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  orderCode?: string | null;
  onConfirm: (reason: string) => Promise<void>;
  isPending?: boolean;
}

export function CancelOrderDialog({
  open,
  onOpenChange,
  orderCode,
  onConfirm,
  isPending = false,
}: CancelOrderDialogProps) {
  const t = useTranslations();
  const reasonId = useId();
  const descriptionId = `${reasonId}-description`;
  const errorId = `${reasonId}-error`;
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const [reason, setReason] = useState("");
  const [touched, setTouched] = useState(false);

  const normalizedReason = reason.trim();
  const isValid =
    normalizedReason.length >= MIN_REASON_LENGTH &&
    normalizedReason.length <= MAX_REASON_LENGTH;
  const validationError =
    normalizedReason.length === 0
      ? t("order.cancellationReasonRequired")
      : t("order.cancellationReasonLength", {
          min: MIN_REASON_LENGTH,
          max: MAX_REASON_LENGTH,
        });

  useEffect(() => {
    if (!open) {
      setReason("");
      setTouched(false);
    }
  }, [open]);

  const handleOpenChange = (nextOpen: boolean) => {
    if (!isPending) onOpenChange(nextOpen);
  };

  const handleConfirm = async (event: React.MouseEvent<HTMLButtonElement>) => {
    event.preventDefault();
    setTouched(true);
    if (!isValid || isPending) return;

    try {
      await onConfirm(normalizedReason);
      handleOpenChange(false);
    } catch {
      // The caller owns API error feedback; keep the dialog open for recovery.
    }
  };

  return (
    <AlertDialog open={open} onOpenChange={handleOpenChange}>
      <AlertDialogContent
        className="sm:max-w-lg"
        onOpenAutoFocus={(event) => {
          event.preventDefault();
          textareaRef.current?.focus();
        }}
      >
        <AlertDialogHeader>
          <AlertDialogTitle className="flex items-center gap-2">
            <XCircle className="h-5 w-5 text-destructive" aria-hidden="true" />
            {t("order.cancelOrderTitle")}
          </AlertDialogTitle>
          <AlertDialogDescription id={descriptionId}>
            {t("order.cancelOrderDescription", {
              code: orderCode ? `#${orderCode}` : "",
            })}
          </AlertDialogDescription>
        </AlertDialogHeader>

        <div className="space-y-2 py-2">
          <Label htmlFor={reasonId}>
            {t("order.cancellationReason")}{" "}
            <span className="text-destructive" aria-hidden="true">
              *
            </span>
          </Label>
          <Textarea
            ref={textareaRef}
            id={reasonId}
            value={reason}
            maxLength={MAX_REASON_LENGTH}
            rows={4}
            disabled={isPending}
            placeholder={t("order.cancellationReasonPlaceholder")}
            aria-required="true"
            aria-invalid={touched && !isValid}
            aria-describedby={`${descriptionId} ${touched && !isValid ? errorId : ""}`}
            onBlur={() => setTouched(true)}
            onChange={(event) => setReason(event.target.value)}
          />
          <div className="flex min-h-5 justify-between gap-4 text-xs">
            <span
              id={errorId}
              className={
                touched && !isValid
                  ? "text-destructive"
                  : "text-muted-foreground"
              }
              role={touched && !isValid ? "alert" : undefined}
            >
              {touched && !isValid
                ? validationError
                : t("order.cancellationReasonLength", {
                    min: MIN_REASON_LENGTH,
                    max: MAX_REASON_LENGTH,
                  })}
            </span>
            <span className="shrink-0 text-muted-foreground">
              {reason.length}/{MAX_REASON_LENGTH}
            </span>
          </div>
        </div>

        <AlertDialogFooter>
          <AlertDialogCancel disabled={isPending}>
            {t("common.cancel")}
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={!isValid || isPending}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            {isPending && (
              <Loader2
                className="mr-2 h-4 w-4 animate-spin"
                aria-hidden="true"
              />
            )}
            {isPending
              ? t("common.status.cancelling")
              : t("order.cancelOrderConfirm")}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
