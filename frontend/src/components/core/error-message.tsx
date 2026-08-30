"use client"

// Error message component
import { AlertCircle, X } from "lucide-react"
import { Button } from "@/components/ui/button"
import { useTranslations } from "next-intl"

interface ErrorMessageProps {
  message: string
  onDismiss?: () => void
  onRetry?: () => void
}

export function ErrorMessage({ message, onDismiss, onRetry }: ErrorMessageProps) {
  const t = useTranslations();
  return (
    
    <div className="bg-red-50 border border-red-200 rounded-lg p-4">
      <div className="flex items-start">
        <AlertCircle className="w-5 h-5 text-red-600 mt-0.5 mr-3" />
        <div className="flex-1">
          <p className="text-sm text-red-800">{message}</p>
          {onRetry && (
            <Button variant="outline" size="sm" onClick={onRetry} className="mt-2">
              {t("common.retry")}
            </Button>
          )}
        </div>
        {onDismiss && (
          <Button variant="ghost" size="sm" onClick={onDismiss} className="p-1">
            <X className="w-4 h-4" />
          </Button>
        )}
      </div>
    </div>
  )
}
