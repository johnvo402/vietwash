import { useRef, useState } from "react";
import { useTranslations } from "next-intl";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Loader2 } from "lucide-react";

interface PaymentMethodSelectProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: () => void | Promise<void>;
}

export const PaymentMethodSelect = ({
  isOpen,
  onClose,
  onSubmit,
}: PaymentMethodSelectProps) => {
  const t = useTranslations();
  const submitting = useRef(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async () => {
    if (submitting.current) return;

    submitting.current = true;
    setError("");
    setIsSubmitting(true);
    try {
      await onSubmit();
      onClose();
    } catch (submitError) {
      setError(
        submitError instanceof Error ? submitError.message : t("common.error"),
      );
    } finally {
      setIsSubmitting(false);
      submitting.current = false;
    }
  };

  return (
    <Dialog
      open={isOpen}
      onOpenChange={(open) => !open && !isSubmitting && onClose()}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("order.confirmCashPayment")}</DialogTitle>
          <DialogDescription>
            {t("order.cashPaymentDescription")}
          </DialogDescription>
        </DialogHeader>
        {error && (
          <p className="text-sm text-destructive" role="alert">
            {error}
          </p>
        )}
        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={isSubmitting}>
            {t("common.cancel")}
          </Button>
          <Button onClick={handleSubmit} disabled={isSubmitting}>
            {isSubmitting && (
              <Loader2
                className="mr-2 h-4 w-4 animate-spin"
                aria-hidden="true"
              />
            )}
            {isSubmitting ? t("order.processingPayment") : t("order.cash")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
