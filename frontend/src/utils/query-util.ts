import { useRouter, useSearchParams } from "next/navigation";

export const usePushQuery = () => {
  const router = useRouter();
  const searchParams = useSearchParams();

  return (newParams: Record<string, string | number | null | undefined>) => {
    if (typeof window === "undefined") return;

    // Lấy query hiện tại
    const currentParams = new URLSearchParams(searchParams.toString());

    // Cập nhật query mới
    Object.entries(newParams).forEach(([key, value]) => {
      if (value === null || value === undefined) {
        currentParams.delete(key); // Xóa query nếu giá trị null/undefined
      } else {
        currentParams.set(key, String(value)); // Cập nhật query mới
      }
    });

    router.push(`?${currentParams.toString()}`, { scroll: false });
  };
};
export interface XPaginationProps {
  pageSizeOptions?: number[];
  totalRows?: number;
}
export const DEFAULT_PAGE_SIZE = 10;
export const DEFAULT_PAGE_SIZE_NUMBER = 7;
export const DEFAULT_PAGE_SIZE_OPTIONS = [5, 7, 10, 20, 50, 100];
export const getInitialPagination = (
  pageSizeOptions: number[] = DEFAULT_PAGE_SIZE_OPTIONS
) => {
  const initialPageSize = pageSizeOptions[1] || DEFAULT_PAGE_SIZE;
  return {
    limit: initialPageSize,
    offset: 0,
  };
};
interface PaginationParams {
  order_by?: string;
  pageOption?: number[];
  totalRows?: number;
}
export const useGetPaginationParams = (PaginationParams: PaginationParams) => {
  const XPaginationProps: XPaginationProps = {
    pageSizeOptions: PaginationParams.pageOption || DEFAULT_PAGE_SIZE_OPTIONS,
    totalRows: PaginationParams.totalRows || 0,
  };
  const searchParams = useSearchParams();
  const limit =
    Number(searchParams.get("limit")) || getInitialPagination().limit;
  const offset =
    Number(searchParams.get("offset")) || getInitialPagination().offset;
  const where = searchParams.get("where") || undefined;
  const orderBy = PaginationParams.order_by || "id";
  return { where, orderBy, offset, limit, XPaginationProps };
};
