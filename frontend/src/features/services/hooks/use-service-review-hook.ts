// File: hooks/useFormReviews.ts
import { apiClient } from "@/api/client";
import { ListFeedbackResponse, ReplyFeedbackModel } from "@/api/generated";
import { useQueryFilter } from "@/lib/filter";
import {
  useInfiniteQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

interface UseFormReviewsResult {
  reviews: ListFeedbackResponse[];
  isLoading: boolean;
  error: unknown;
  fetchNextPage: () => void;
  hasNextPage: boolean | undefined;
}

export const useFormReviews = (
  ratingFilter?: number,
  serviceId?: number,
  sortBy: "date" | "rating" = "date",
  sortOrder: "asc" | "desc" = "desc"
): UseFormReviewsResult => {
  const { flattenQueryObject, prepareApiParams } = useQueryFilter();

  const params = {
    page: 1,
    pageSize: 10, // Mặc định lấy 10 đánh giá mỗi trang
    sort: sortBy === "date" ? `createdAt:${sortOrder}` : `rating:${sortOrder}`,
    filter: flattenQueryObject({
      ...(ratingFilter !== undefined
        ? {
            rating: {
              $eq: ratingFilter,
            },
          }
        : {}),
      serviceId: {
        $eq: serviceId,
      },
    }),
  };

  const searchApiParamsKeys = [
    "page",
    "pageSize",
    "before",
    "after",
    "searchKeyword",
    "searchTargets",
    "sort",
    "filter",
  ] as const;

  const { data, isLoading, error, fetchNextPage, hasNextPage } =
    useInfiniteQuery({
      queryKey: ["reviews", { ratingFilter, sortBy, sortOrder }],
      queryFn: async ({ pageParam = 1 }) => {
        const args = prepareApiParams(searchApiParamsKeys, {
          ...params,
          page: pageParam,
        });
        const response = await apiClient.ecommerceApiFeedbacksGet(...args);
        return {
          results: response.data.results?.data || [],
          paging: response.data.results?.paging,
        };
      },
      getNextPageParam: (lastPage) => {
        const paging = lastPage.paging;
        const current_page = paging?.currentPage ?? 1;
        const total_pages = paging?.totalPage ?? 1;
        return current_page < total_pages ? current_page + 1 : undefined;
      },
      initialPageParam: 1,
    });

  const reviews = data?.pages.flatMap((page) => page.results) || [];

  return {
    reviews,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
  };
};
interface ReplyMutationProps {
  reviewId: number;
  comment: string;
}

interface EditReplyMutationProps {
  replyId: number;
  comment: string;
}

interface DeleteReplyMutationProps {
  replyId: number;
}

export const useReplyMutations = () => {
  const queryClient = useQueryClient();

  // Create reply mutation
  const createReply = useMutation({
    mutationFn: async ({ reviewId, comment }: ReplyMutationProps) => {
      const reviewComment: ReplyFeedbackModel = { comment };
      return await apiClient.ecommerceApiFeedbacksIdFeedbackRepliesPost(
        reviewId,
        reviewComment
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reviews"] });
    },
  });

  // Edit reply mutation
  const editReply = useMutation({
    mutationFn: async ({ replyId, comment }: EditReplyMutationProps) => {
      const reviewComment: ReplyFeedbackModel = { comment };
      return await apiClient.ecommerceApiFeedbacksIdPut(replyId, reviewComment);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reviews"] });
    },
  });

  // Delete reply mutation
  const deleteReply = useMutation({
    mutationFn: async ({ replyId }: DeleteReplyMutationProps) => {
      return await apiClient.ecommerceApiFeedbacksIdDelete(replyId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reviews"] });
    },
  });

  return {
    createReply,
    editReply,
    deleteReply,
  };
};
