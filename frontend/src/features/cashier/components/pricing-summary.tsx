import type { PreviewOrderResponse } from "@/api/generated";
import { formatOrderMoney } from "../../../utils/format";

export function PricingSummary({
  preview,
  calculating,
  error,
  labels,
}: {
  preview?: PreviewOrderResponse;
  calculating: boolean;
  error: boolean;
  labels: {
    amount: string;
    discount: string;
    total: string;
    calculating: string;
    error: string;
  };
}) {
  if (calculating || error)
    return (
      <p role="status" className="text-sm text-muted-foreground">
        {calculating ? labels.calculating : labels.error}
      </p>
    );
  return (
    <dl className="space-y-2 w-full tabular-nums" aria-live="polite">
      {[
        [labels.amount, preview?.amount ?? 0],
        [labels.discount, preview?.discountAmount ?? 0],
        [`VAT ${preview?.vatPercent ?? 0}%`, preview?.vatAmount ?? 0],
        [labels.total, preview?.total ?? 0],
      ].map(([label, value]) => (
        <div key={label} className="flex justify-between gap-2">
          <dt>{label}</dt>
          <dd className="font-bold">{formatOrderMoney(Number(value))}</dd>
        </div>
      ))}
    </dl>
  );
}
