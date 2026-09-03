import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { PaymentMethod } from "@/api/generated/api";
import { Loader2 } from "lucide-react";

interface PaymentMethodSelectProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (method: PaymentMethod) => void | Promise<void>;
}

export const PaymentMethodSelect = ({
  isOpen,
  onClose,
  onSubmit,
}: PaymentMethodSelectProps) => {
  const [selectedMethod, setSelectedMethod] = useState<PaymentMethod | null>(
    null,
  );
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async () => {
    if (selectedMethod === null || isSubmitting) return;

    setError("");
    setIsSubmitting(true);
    try {
      await onSubmit(selectedMethod);
      onClose();
    } catch (submitError) {
      setError(
        submitError instanceof Error
          ? submitError.message
          : "Không thể bắt đầu thanh toán. Vui lòng thử lại.",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog
      open={isOpen}
      onOpenChange={(open) => !open && !isSubmitting && onClose()}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Chọn phương thức thanh toán</DialogTitle>
        </DialogHeader>
        <div className="flex flex-col gap-4 mt-4">
          <Button
            variant={
              selectedMethod === PaymentMethod.Cash ? "default" : "outline"
            }
            onClick={() => setSelectedMethod(PaymentMethod.Cash)}
            disabled={isSubmitting}
          >
            Tiền mặt
          </Button>
          <Button
            variant={
              selectedMethod === PaymentMethod.Card ? "default" : "outline"
            }
            onClick={() => setSelectedMethod(PaymentMethod.Card)}
            disabled={isSubmitting}
          >
            Thẻ tín dụng
          </Button>
        </div>
        {error && (
          <p className="text-sm text-destructive" role="alert">
            {error}
          </p>
        )}
        <DialogFooter>
          <Button
            onClick={handleSubmit}
            disabled={selectedMethod === null || isSubmitting}
          >
            {isSubmitting && (
              <Loader2
                className="mr-2 h-4 w-4 animate-spin"
                aria-hidden="true"
              />
            )}
            {isSubmitting ? "Đang xử lý..." : "Xác nhận"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
