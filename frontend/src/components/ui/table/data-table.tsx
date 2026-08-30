"use client";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  TableBody,
  TableCell,
  TableHead,
  TableRow,
} from "@/components/ui/table";
import {
  DoubleArrowLeftIcon,
  DoubleArrowRightIcon,
} from "@radix-ui/react-icons";
import {
  ColumnDef,
  flexRender,
  getCoreRowModel,
  getPaginationRowModel,
  PaginationState,
  useReactTable,
} from "@tanstack/react-table";
import { ChevronLeftIcon, ChevronRightIcon } from "lucide-react";
import { parseAsInteger, useQueryState } from "nuqs";
import { Skeleton } from "../skeleton";
import { useVirtualizer } from "@tanstack/react-virtual";
import { useRef } from "react";
import { useTranslations } from "next-intl";

interface DataTableProps<TData, TValue> {
  columns: ColumnDef<TData, TValue>[];
  data: TData[];
  paging?: Paging;
  loading?: boolean;
  pageSizeOptions?: number[];
  error?: any;
}
export interface Paging {
  currentPage?: number | null;
  pageSize?: number;
  totalPage?: number;
  hasNextPage?: boolean | null;
  hasPreviousPage?: boolean | null;
}

export function DataTable<TData, TValue>({
  columns,
  data,
  paging,
  loading,
  error,
  pageSizeOptions = [10, 20, 30, 40, 50],
}: DataTableProps<TData, TValue>) {
  const [currentPage, setCurrentPage] = useQueryState(
    "page",
    parseAsInteger
      .withOptions({ shallow: false, history: "push" })
      .withDefault(paging?.currentPage || 1)
  );
  const selectedPageSize = pageSizeOptions.includes(paging?.pageSize ?? -1)
    ? paging?.pageSize
    : pageSizeOptions[0];
  const [pageSize, setPageSize] = useQueryState(
    "pageSize",
    parseAsInteger
      .withOptions({ shallow: false, history: "push" })
      .withDefault(selectedPageSize || pageSizeOptions[0])
  );
  const paginationState = {
    pageIndex: currentPage - 1,
    pageSize: pageSize,
  };

  const handlePaginationChange = (
    updaterOrValue:
      | PaginationState
      | ((old: PaginationState) => PaginationState)
  ) => {
    const pagination =
      typeof updaterOrValue === "function"
        ? updaterOrValue(paginationState)
        : updaterOrValue;
    setCurrentPage(pagination.pageIndex + 1);
    setPageSize(pagination.pageSize);
  };

  const table = useReactTable({
    data,
    columns,
    pageCount: paging?.totalPage || 1,
    state: {
      pagination: paginationState,
    },
    onPaginationChange: handlePaginationChange,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    manualPagination: true,
    manualFiltering: true,
  });

  const parentRef = useRef<HTMLDivElement>(null);

  const rowVirtualizer = useVirtualizer({
    count: table.getRowModel().rows.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 32,
    overscan: 50,
  });
  const t = useTranslations();
  return (
    <div>
      <div
        ref={parentRef}
        className="max-h-[60vh] rounded-md border overflow-auto"
      >
        <table className=" caption-bottom text-sm table-auto w-full">
          <thead className="bg-secondary sticky top-0 z-20">
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header: any) => {
                  const className =
                    header.column.columnDef.meta?.header?.className ?? "";
                  return (
                    <TableHead
                      key={header.id}
                      className={`text-primary whitespace-nowrap h-12 ${className}`}
                    >
                      {header.isPlaceholder
                        ? null
                        : flexRender(
                            header.column.columnDef.header,
                            header.getContext()
                          )}
                    </TableHead>
                  );
                })}
              </TableRow>
            ))}
          </thead>

          <TableBody
            style={{
              height: `${rowVirtualizer.getTotalSize()}px`,
              position: "relative",
              width: "100%",
            }}
          >
            {loading ? (
              Array.from({ length: pageSize }).map((_, rowIndex) => (
                <TableRow key={rowIndex} className="hover:bg-transparent">
                  {Array.from({ length: table.getAllColumns().length }).map(
                    (_, colIndex) => (
                      <TableCell key={colIndex}>
                        <Skeleton className="h-8 w-full" />
                      </TableCell>
                    )
                  )}
                </TableRow>
              ))
            ) : error ? (
              <TableRow>
                <TableCell
                  colSpan={columns.length}
                  className="h-24 text-start text-red-500"
                >
                  {(error as Error).message}
                </TableCell>
              </TableRow>
            ) : table.getRowModel().rows?.length ? (
              rowVirtualizer.getVirtualItems().map((virtualRow) => {
                const row = table.getRowModel().rows[virtualRow.index];
                if (!row) return null;
                return (
                  <TableRow
                    key={virtualRow.key}
                    data-state={row.getIsSelected() && "selected"}
                    ref={rowVirtualizer.measureElement}
                  >
                    {row.getVisibleCells().map((cell: any) => {
                      const className =
                        cell.column.columnDef.meta?.body?.className ?? "";
                      return (
                        <TableCell
                          key={cell.id}
                          className={`whitespace-nowrap ${className}`}
                        >
                          {flexRender(
                            cell.column.columnDef.cell,
                            cell.getContext()
                          )}
                        </TableCell>
                      );
                    })}
                  </TableRow>
                );
              })
            ) : (
              <TableRow>
                <TableCell
                  colSpan={columns.length}
                  className="h-24 text-center"
                >
                  {t("common.noData")}
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </table>
      </div>

      <div className="flex flex-col items-center justify-end gap-2 space-x-2 py-2 sm:flex-row">
        <div className="flex w-full items-center justify-between gap-2 sm:justify-end">
          <div className="flex flex-col items-center gap-4 sm:flex-row sm:gap-6 lg:gap-8">
            <div className="flex items-center space-x-2">
              <Select
                value={`${paginationState.pageSize}`}
                onValueChange={(value) => {
                  table.setPageSize(Number(value));
                }}
              >
                <SelectTrigger className="h-8 w-[70px]">
                  <SelectValue placeholder={paginationState.pageSize} />
                </SelectTrigger>
                <SelectContent side="top">
                  {pageSizeOptions.map((pageSize) => (
                    <SelectItem key={pageSize} value={`${pageSize}`}>
                      {pageSize}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="flex items-center space-x-2 mr-2">
            <Button
              aria-label="Go to first page"
              variant="outline"
              className="hidden h-8 w-8 p-0 lg:flex"
              onClick={() => table.setPageIndex(0)}
              disabled={!table.getCanPreviousPage()}
            >
              <DoubleArrowLeftIcon className="h-4 w-4" aria-hidden="true" />
            </Button>
            <Button
              aria-label="Go to previous page"
              variant="outline"
              className="h-8 w-8 p-0"
              onClick={() => table.previousPage()}
              disabled={!table.getCanPreviousPage()}
            >
              <ChevronLeftIcon className="h-4 w-4" aria-hidden="true" />
            </Button>

            <div className="flex items-center space-x-1">
              {Array.from({ length: table.getPageCount() }, (_, i) => {
                const currentPage = paginationState.pageIndex + 1;
                const totalPages = table.getPageCount();

                const showPage =
                  i === 0 ||
                  i === totalPages - 1 ||
                  totalPages <= 7 ||
                  (i >= currentPage - 1 && i <= currentPage + 1) ||
                  i <= 2 ||
                  i >= totalPages - 3;

                if (!showPage) {
                  if (i === 3 || i === totalPages - 3) {
                    return (
                      <span
                        key={i}
                        className="px-2 text-sm text-muted-foreground"
                      >
                        ...
                      </span>
                    );
                  }
                  return null;
                }

                return (
                  <Button
                    key={i}
                    variant={currentPage === i + 1 ? "default" : "outline"}
                    className="size-8"
                    onClick={() => table.setPageIndex(i)}
                  >
                    {i + 1}
                  </Button>
                );
              })}
            </div>
            <Button
              aria-label="Go to next page"
              variant="outline"
              className="h-8 w-8 p-0"
              onClick={() => table.nextPage()}
              disabled={!table.getCanNextPage()}
            >
              <ChevronRightIcon className="h-4 w-4" aria-hidden="true" />
            </Button>
            <Button
              aria-label="Go to last page"
              variant="outline"
              className="hidden h-8 w-8 p-0 lg:flex"
              onClick={() => table.setPageIndex(table.getPageCount() - 1)}
              disabled={!table.getCanNextPage()}
            >
              <DoubleArrowRightIcon className="h-4 w-4" aria-hidden="true" />
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
