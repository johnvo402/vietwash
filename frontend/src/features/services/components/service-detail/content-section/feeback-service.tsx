"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  MessageCircle,
  Star,
  ThumbsDown,
  ThumbsUp,
  Reply,
  Filter,
  Loader2,
  ArrowUpDown,
  Calendar,
  Edit,
  Trash2,
} from "lucide-react";
import {
  useFormReviews,
  useReplyMutations,
} from "@/features/services/hooks/use-service-review-hook";
import Image from "next/image";
import { useStringUtil } from "@/lib/stringUtil";
import { getInitials } from "@/utils/format";
import { GetCustomerGroup } from "@/features/orders/order-utils/order-util";
import { useAuth } from "@/hooks/use-auth";
import { ReplyForm } from "./reply-form";
import { EditReplyForm } from "./edit-feedback-form";

const StarRating = ({ rating }: { rating: number }) => {
  return (
    <div className="flex items-center gap-1">
      {[1, 2, 3, 4, 5].map((star) => (
        <Star
          key={star}
          className={`h-4 w-4 ${star <= rating ? "fill-yellow-400 text-yellow-400" : "text-gray-300"}`}
        />
      ))}
      <span className="ml-2 text-sm text-muted-foreground">({rating}/5)</span>
    </div>
  );
};

const StarFilter = ({
  rating,
  isSelected,
  onClick,
}: {
  rating: number;
  isSelected: boolean;
  onClick: () => void;
}) => (
  <button
    onClick={onClick}
    className={`flex items-center gap-1 px-3 py-2 rounded-md border transition-colors ${
      isSelected
        ? "bg-primary/10 border-primary text-yellow-800"
        : "bg-white border-gray-200 hover:bg-gray-50"
    }`}
  >
    {[1, 2, 3, 4, 5].map((star) => (
      <Star
        key={star}
        className={`h-4 w-4 ${star <= rating ? "fill-yellow-400 text-yellow-400" : "text-gray-300"}`}
      />
    ))}
  </button>
);

export default function StaffReviewsComponent({
  serviceId,
}: {
  serviceId: number;
}) {
  const t = useTranslations();
  const { user } = useAuth();
  const [ratingFilter, setRatingFilter] = useState<number | undefined>(
    undefined
  );
  const [sortBy, setSortBy] = useState<"date" | "rating">("date");
  const [sortOrder, setSortOrder] = useState<"asc" | "desc">("desc");
  const [replyingTo, setReplyingTo] = useState<number | null>(null);
  const [editingReplyId, setEditingReplyId] = useState<number | null>(null);
  const { formatDate } = useStringUtil();

  const { reviews, isLoading, error, fetchNextPage, hasNextPage } =
    useFormReviews(ratingFilter, serviceId, sortBy, sortOrder);
  const { createReply, editReply, deleteReply } = useReplyMutations();

  const handleReply = (reviewId: number, comment: string) => {
    createReply.mutate(
      { reviewId, comment },
      {
        onSuccess: () => setReplyingTo(null),
      }
    );
  };

  const handleEditReply = (replyId: number, comment: string) => {
    editReply.mutate(
      { replyId, comment },
      {
        onSuccess: () => setEditingReplyId(null),
      }
    );
  };

  const handleDeleteReply = (replyId: number) => {
    deleteReply.mutate({ replyId });
  };

  const canEditReply = (createdAt: string) => {
    const createdDate = new Date(createdAt);
    const now = new Date();
    const hoursDiff =
      (now.getTime() - createdDate.getTime()) / (1000 * 60 * 60);
    return hoursDiff <= 24;
  };

  return (
    <div>
      <div className="mb-6 p-4 bg-gray-50 rounded-lg space-y-4">
        <div className="flex items-center gap-4">
          <Filter className="h-5 w-5 text-gray-600" />
          <div className="grid grid-cols-3 md:grid-cols-6 items-center gap-2">
            <Button
              onClick={() => setRatingFilter(undefined)}
              className={`px-3 py-2 rounded-md border text-sm transition-colors ${
                ratingFilter === undefined
                  ? "bg-primary/10 border-primary text-primary hover:bg-primary/30"
                  : "bg-primary-foreground text-primary border-border hover:text-background"
              }`}
            >
              {t("user.review.allReviews")}
            </Button>
            {[1, 2, 3, 4, 5].map((rating) => (
              <StarFilter
                key={rating}
                rating={rating}
                isSelected={ratingFilter === rating}
                onClick={() => setRatingFilter(rating)}
              />
            ))}
          </div>
        </div>

        <div className="flex items-center gap-4">
          <ArrowUpDown className="h-5 w-5 text-gray-600" />
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <label className="text-sm font-medium">
                {t("user.review.sortLabel")}
              </label>
              <Select
                value={sortBy}
                onValueChange={(value: "date" | "rating") => setSortBy(value)}
              >
                <SelectTrigger className="w-32">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="date">
                    <div className="flex items-center gap-2">
                      <Calendar className="h-4 w-4" />
                      {t("user.review.sortDate")}
                    </div>
                  </SelectItem>
                  <SelectItem value="rating">
                    <div className="flex items-center gap-2">
                      <Star className="h-4 w-4" />
                      {t("user.review.sortRating")}
                    </div>
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="flex items-center gap-2">
              <label className="text-sm font-medium">
                {t("user.review.orderLabel")}
              </label>
              <Select
                value={sortOrder}
                onValueChange={(value: "asc" | "desc") => setSortOrder(value)}
              >
                <SelectTrigger className="w-32">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="desc">
                    {t(
                      sortBy === "date"
                        ? "user.review.newestFirst"
                        : "user.review.highestFirst"
                    )}
                  </SelectItem>
                  <SelectItem value="asc">
                    {t(
                      sortBy === "date"
                        ? "user.review.oldestFirst"
                        : "user.review.lowestFirst"
                    )}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </div>
      </div>

      <ScrollArea className="h-[400px] pr-4">
        <div className="space-y-6">
          {error ? (
            <div className="text-center py-8 text-red-600">
              {t("user.review.error")}
            </div>
          ) : isLoading ? (
            <div className="flex items-center justify-center h-64">
              <Loader2 className="h-8 w-8 animate-spin" />
              <span className="ml-2">{t("user.review.loading")}</span>
            </div>
          ) : (
            reviews.map((review) => (
              <Card key={review.id}>
                <CardHeader className="pb-4">
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-3">
                      <Avatar className="h-10 w-10">
                        {review.createdUser?.avatar ? (
                          <Image
                            src={
                              review.createdUser?.avatar || "/placeholder.svg"
                            }
                            alt={review.createdUser?.displayName ?? "Avatar"}
                            className="h-8 w-8 rounded-full object-contain"
                            fill
                          />
                        ) : (
                          <AvatarFallback className="bg-secondary text-primary text-xs">
                            {review.createdUser?.displayName
                              ? getInitials(review.createdUser.displayName)
                              : "?"}
                          </AvatarFallback>
                        )}
                      </Avatar>
                      <div className="space-y-1">
                        <div className="flex items-center gap-2">
                          <h3 className="font-semibold">
                            {review.createdUser?.displayName}
                          </h3>
                          {GetCustomerGroup(
                            t,
                            review.createdUser?.customerGroup
                          )}
                        </div>
                        <p className="text-sm text-muted-foreground">
                          {formatDate(review.createdAt!)}
                        </p>
                      </div>
                    </div>
                    <StarRating rating={review.rating!} />
                  </div>
                </CardHeader>

                <CardContent className="space-y-4">
                  <div
                    className="prose"
                    dangerouslySetInnerHTML={{ __html: review.comment ?? "" }}
                  />
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-4">
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <ThumbsUp className="h-4 w-4" />
                        <span className="text-sm">
                          {review.likes} {t("user.review.likes")}
                        </span>
                      </div>
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <ThumbsDown className="h-4 w-4" />
                        <span className="text-sm">
                          {review.dislikes} {t("user.review.dislikes")}
                        </span>
                      </div>
                      {review.replies!.length > 0 && (
                        <div className="flex items-center gap-2 text-muted-foreground">
                          <MessageCircle className="h-4 w-4" />
                          <span className="text-sm">
                            {t("user.review.replyCount", {
                              count: review.replies!.length,
                            })}
                          </span>
                        </div>
                      )}
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() =>
                        setReplyingTo(
                          replyingTo === review.id ? null : (review.id ?? null)
                        )
                      }
                      className="gap-2"
                    >
                      <Reply className="h-4 w-4" />
                      {t("user.review.reply")}
                    </Button>
                  </div>

                  {replyingTo === review.id && (
                    <ReplyForm
                      reviewId={review.id}
                      onSubmit={handleReply}
                      onCancel={() => setReplyingTo(null)}
                      isSubmitting={createReply.isPending}
                    />
                  )}

                  {review.replies!.length > 0 && (
                    <div className="space-y-3">
                      <Separator />
                      {review.replies!.map((reply) => (
                        <div
                          key={reply.id}
                          className="bg-primary-foreground rounded-lg p-4 ml-6 border-l-4 border-primary"
                        >
                          <div className="flex items-center justify-between mb-2">
                            <div className="flex items-center gap-3">
                              <Avatar className="h-8 w-8">
                                {reply.createdUser?.avatar ? (
                                  <Image
                                    src={
                                      reply.createdUser?.avatar ||
                                      "/placeholder.svg"
                                    }
                                    alt={
                                      reply.createdUser?.displayName ?? "Avatar"
                                    }
                                    className="h-8 w-8 rounded-full object-contain"
                                    fill
                                  />
                                ) : (
                                  <AvatarFallback className="bg-secondary text-primary text-xs">
                                    {reply.createdUser?.displayName
                                      ? getInitials(
                                          reply.createdUser.displayName
                                        )
                                      : "?"}
                                  </AvatarFallback>
                                )}
                              </Avatar>
                              <div>
                                <div className="flex items-center gap-2">
                                  <h4 className="font-medium text-sm">
                                    {reply.createdUser?.displayName}
                                  </h4>
                                </div>
                                <p className="text-xs text-muted-foreground">
                                  {formatDate(reply.createdAt!)}
                                </p>
                              </div>
                            </div>
                            {reply.staffId === user?.id && (
                              <div className="flex gap-2">
                                {canEditReply(reply.createdAt!) && (
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={() =>
                                      setEditingReplyId(
                                        editingReplyId === reply.id
                                          ? null
                                          : (reply.id ?? null)
                                      )
                                    }
                                    className="gap-2"
                                  >
                                    <Edit className="h-4 w-4" />
                                  </Button>
                                )}
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  onClick={() => handleDeleteReply(reply.id!)}
                                  className="gap-2"
                                  disabled={deleteReply.isPending}
                                >
                                  <Trash2 className="h-4 w-4 text-destructive" />
                                </Button>
                              </div>
                            )}
                          </div>
                          {editingReplyId === reply.id ? (
                            <EditReplyForm
                              initialComment={reply.comment!}
                              onSubmit={(comment) =>
                                handleEditReply(reply.id!, comment)
                              }
                              onCancel={() => setEditingReplyId(null)}
                              isSubmitting={editReply.isPending}
                            />
                          ) : (
                            <div
                              className="text-sm prose"
                              dangerouslySetInnerHTML={{
                                __html: reply.comment ?? "",
                              }}
                            />
                          )}
                        </div>
                      ))}
                    </div>
                  )}
                </CardContent>
              </Card>
            ))
          )}

          {hasNextPage && (
            <div className="flex justify-center py-4">
              <Button
                onClick={() => fetchNextPage()}
                disabled={isLoading}
                variant="outline"
                className="gap-2"
              >
                {isLoading ? (
                  <>
                    <Loader2 className="h-4 w-4 animate-spin" />
                    {t("user.review.loadingMore")}
                  </>
                ) : (
                  t("user.review.loadMore")
                )}
              </Button>
            </div>
          )}

          {!hasNextPage && reviews.length > 0 && (
            <div className="text-center py-4 text-muted-foreground">
              {t("user.review.noMoreReviews")}
            </div>
          )}
        </div>
      </ScrollArea>
    </div>
  );
}
