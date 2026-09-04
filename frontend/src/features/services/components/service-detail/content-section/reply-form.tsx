"use client";

import type React from "react";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Send, X } from "lucide-react";
import dynamic from "next/dynamic";
import { useTranslations } from "next-intl";

const TextEditor = dynamic(() => import("@/components/ui/text-editor"), {
  ssr: false,
});

interface ReplyFormProps {
  reviewId: number;
  onSubmit: (reviewId: number, comment: string) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

export function ReplyForm({
  reviewId,
  onSubmit,
  onCancel,
  isSubmitting,
}: ReplyFormProps) {
  const [comment, setComment] = useState("");
  const t = useTranslations("user.review");
  const tCommon = useTranslations("common");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (comment.trim()) {
      onSubmit(reviewId, comment.trim());
      setComment("");
    }
  };

  return (
    <Card className="mt-4 border-blue-200 bg-blue-50/50">
      <CardContent className="p-4">
        <form onSubmit={handleSubmit} className="space-y-3">
          <div>
            <label
              htmlFor={`reply-${reviewId}`}
              className="block text-sm font-medium mb-2"
            >
              {t("reply")} {tCommon("staff")}
            </label>
            <TextEditor
              value={comment}
              onChange={(value) => setComment(value)}
              className="w-full p-2 border rounded-md text-sm"
              placeholder={t("replyPlaceholder")}
              disabled={isSubmitting}
            />
          </div>
          <div className="flex justify-end gap-2">
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={onCancel}
              disabled={isSubmitting}
            >
              <X className="h-4 w-4 mr-1" />
              {tCommon("cancel")}
            </Button>
            <Button
              type="submit"
              size="sm"
              disabled={!comment.trim() || isSubmitting}
            >
              <Send className="h-4 w-4 mr-1" />
              {isSubmitting ? tCommon("submitting") : tCommon("submit")}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
