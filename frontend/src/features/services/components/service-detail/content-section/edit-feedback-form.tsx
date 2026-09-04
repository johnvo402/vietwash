import { useState } from "react";
import { useTranslations } from "next-intl";
import { Button } from "@/components/ui/button";
import { Loader2 } from "lucide-react";
import dynamic from "next/dynamic";

const TextEditor = dynamic(() => import("@/components/ui/text-editor"), {
  ssr: false,
});

interface EditReplyFormProps {
  initialComment: string;
  onSubmit: (comment: string) => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

export const EditReplyForm = ({
  initialComment,
  onSubmit,
  onCancel,
  isSubmitting,
}: EditReplyFormProps) => {
  const t = useTranslations();
  const [comment, setComment] = useState(initialComment);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (comment.trim()) {
      onSubmit(comment);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-2">
      <TextEditor
        value={comment}
        onChange={(value) => setComment(value)}
        className="w-full p-2 border rounded-md text-sm"
        placeholder={t("user.review.replyPlaceholder")}
      />
      <div className="flex gap-2">
        <Button
          type="submit"
          size="sm"
          disabled={isSubmitting || !comment.trim()}
        >
          {isSubmitting ? (
            <>
              <Loader2 className="h-4 w-4 animate-spin mr-2" />
            </>
          ) : (
            t("common.save")
          )}
        </Button>
        <Button type="button" variant="outline" size="sm" onClick={onCancel}>
          {t("common.cancel")}
        </Button>
      </div>
    </form>
  );
};
