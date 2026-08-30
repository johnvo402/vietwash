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

interface PaymentMethodSelectProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (method: PaymentMethod) => void;
}

export const PaymentMethodSelect = ({
  isOpen,
  onClose,
  onSubmit,
}: PaymentMethodSelectProps) => {
  const [selectedMethod, setSelectedMethod] = useState<PaymentMethod | null>(
    null
  );

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
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
          >
            Tiền mặt
          </Button>
          <Button
            variant={
              selectedMethod === PaymentMethod.Card ? "default" : "outline"
            }
            onClick={() => setSelectedMethod(PaymentMethod.Card)}
          >
            Thẻ tín dụng
          </Button>
        </div>
        <DialogFooter>
          <Button
            onClick={() => {
              if (selectedMethod !== null) {
                onSubmit(selectedMethod);
                onClose();
              }
            }}
            disabled={selectedMethod === null}
          >
            Xác nhận
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
