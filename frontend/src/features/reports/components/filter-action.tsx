"use client";

import { Button } from "@/components/ui/button";
import { RotateCcw } from "lucide-react";
import { useTranslations } from "next-intl";

interface FilterActionsProps {
  onSubmit: () => void;
  onReset: () => void;
  isLoading: boolean;
  hasChanges: boolean;
}

export default function FilterActions({
  onSubmit,
  onReset,
  isLoading,
}: FilterActionsProps) {
  const t = useTranslations();
  return (
    <div className="flex items-center gap-2 mt-2">
      <Button
        type="button"
        variant={"ghost"}
        onClick={onReset}
        disabled={isLoading}
        className="flex items-center py-2 px-3 justify-center bg-primary-foreground rounded-md focus:outline-none focus:ring-2  focus:ring-offset-2 transition-colors text-sm disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <RotateCcw size={16} />
      </Button>
      <Button
        type="button"
        variant={"default"}
        onClick={onSubmit}
        disabled={isLoading}
        className={`flex-1 py-2 px-3 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary-foreground text-background focus:ring-offset-2 transition-colors font-medium ${isLoading ? "opacity-50 cursor-not-allowed" : "bg-primary hover:bg-border"}`}
      >
        {isLoading ? (
          <div className="flex items-center justify-center">
            <div className="animate-spin rounded-full h-3 w-3 border-b-2 border-background mr-2"></div>
          </div>
        ) : (
          t("common.apply")
        )}
      </Button>
    </div>
  );
}
