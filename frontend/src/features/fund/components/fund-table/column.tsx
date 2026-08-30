import { formatPriceVN } from "@/utils/format";
import type { ColumnDef } from "@tanstack/react-table";
import { format } from "date-fns";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSub,
  DropdownMenuSubContent,
  DropdownMenuSubTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { MoreHorizontal } from "lucide-react";
import { usePushRouter } from "@/utils/router-utli";
import {
  ROUTE_FUND,
  ROUTE_FUND_DETAIL,
  ROUTE_FUND_EDIT,
  ROUTE_INVENTORY_DOC_DETAIL,
  ROUTE_ORDERS_DETAIL,
} from "@/types/router-type";
import { FundStatus, FundType, ListFundResponse } from "@/api/generated/api";
import { useStringUtil } from "@/lib/stringUtil";
import { useTranslations } from "next-intl";
import { useAuth } from "@/hooks/use-auth";

// Định nghĩa enum PaymentMethod

export const useFundTable = ({
  onEdit,
  onStatusChange,
}: {
  onEdit?: (id: string) => void;
  onStatusChange?: (id: string, status: FundStatus) => void;
}) => {
  const pushRouter = usePushRouter();
  const t = useTranslations();
  const { textByLang } = useStringUtil();
  const { user } = useAuth();

  const getBranch = (branchId: number) => {
    return user?.branchAccounts.find((x) => x.branchId === branchId);
  };

  const getStatusBadge = (status?: FundStatus) => {
    switch (status) {
      case FundStatus.PendingConfirmation:
        return (
          <Badge variant="outline" className="bg-yellow-100 text-yellow-800">
            {t("common.status.pendingConfirmation")}
          </Badge>
        );
      case FundStatus.Confirmed:
        return (
          <Badge variant="outline" className="bg-green-100 text-green-800">
            {t("common.status.confirmed")}
          </Badge>
        );
      case FundStatus.Cancelled:
        return (
          <Badge variant="outline" className="bg-red-100 text-red-800">
            {t("common.status.cancelled")}
          </Badge>
        );
      default:
        return <Badge variant="outline">{t("common.status.unknown")}</Badge>;
    }
  };

  const getTypeBadge = (type?: FundType) => {
    switch (type) {
      case FundType.Income:
        return <span className="text-green-800">{t("fund.type.income")}</span>;
      case FundType.Spend:
        return <span className="text-red-800">{t("fund.type.spend")}</span>;
      default:
        return <span>{t("common.status.unknown")}</span>;
    }
  };

  const fundColumns: ColumnDef<ListFundResponse>[] = [
    {
      header: t("table.accessorKey.index"),
      cell: ({ row, table }) => {
        const pageIndex = table.getState().pagination.pageIndex;
        const pageSize = table.getState().pagination.pageSize;
        return pageIndex * pageSize + row.index + 1;
      },
    },
    {
      header: t("fund.type.title"),
      accessorKey: "type",
      cell: ({ row }) => {
        const type = row.original.type;
        return <div>{getTypeBadge(type)}</div>;
      },
    },
    {
      accessorKey: "code",
      header: t("fund.code"),
      cell: ({ row }) => <div>{row.original.code || "--"}</div>,
    },
    {
      header: t("fund.behavior"),
      accessorFn: (row) => row.fundBehavior?.name,
      cell: ({ row }) => {
        const nameObj = JSON.parse(row.original.fundBehavior?.name || "{}");
        return <div>{textByLang(nameObj) || "--"}</div>;
      },
    },
    {
      header: t("fund.payerReceiver"),
      accessorFn: (row) => row.user?.displayName,
      cell: ({ row }) => {
        let supplierName = "";
        try {
          const metadata = JSON.parse(row.original.metadata || "{}");
          supplierName = metadata.supplierName;
        } catch {
          supplierName = "";
        }
        return (
          <div>{row.original.user?.displayName || supplierName || "--"}</div>
        );
      },
    },
    {
      header: t("branch.title"),
      accessorFn: (row) => row.branchId,
      cell: ({ getValue }) => {
        const branchId = getValue() as number;
        return <div>{getBranch(branchId!)?.branchName || "--"}</div>;
      },
    },
    {
      accessorKey: "amount",
      header: t("table.accessorKey.amount"),
      cell: ({ row }) => {
        const amount = row.original.amount;
        return <div className="text-right">{formatPriceVN(amount ?? 0)}</div>;
      },
    },
    {
      accessorKey: "transactionDate",
      header: t("fund.transactionDate"),
      cell: ({ row }) => {
        const date = row.original.transactionDate;
        return (
          <div>{date ? format(new Date(date), "dd/MM/yy HH:mm:ss") : "--"}</div>
        );
      },
    },
    {
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ row }) => {
        const status = row.original.status;
        return <div>{getStatusBadge(status)}</div>;
      },
    },
    {
      header: t("fund.association"),
      accessorFn: (row) => {
        try {
          const metadata = JSON.parse(row.metadata || "{}");
          return metadata.code || "";
        } catch {
          return "";
        }
      },
      cell: ({ row }) => {
        let code = "";
        let publicId = "";
        let supplierId = "";
        let type = "";
        try {
          const metadata = JSON.parse(row.original.metadata || "{}");
          code = metadata.code;
          publicId = metadata.publicId;
          supplierId = metadata.supplierId;
          type = metadata.type || "import";
        } catch {
          code = "";
          supplierId = "";
        }
        if (!code) return <div className="text-gray-400">{"--"}</div>;

        return (
          <Button
            variant={"link"}
            onClick={() =>
              pushRouter.pushRouter({
                router: supplierId
                  ? ROUTE_INVENTORY_DOC_DETAIL
                  : ROUTE_ORDERS_DETAIL,
                params: {
                  publicId: publicId,
                  type: type.toLowerCase() || "import",
                },
                state: {
                  [publicId]: row.original.referenceId!,
                },
                redirect: "blank",
              })
            }
            className="text-blue-600 hover:underline"
          >
            {code}
          </Button>
        );
      },
    },
    {
      header: t("table.accessorKey.actions"),
      cell: ({ row }) => (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="default">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem
              onClick={() => {
                const publicId = row.original.publicId?.toString()!;
                return pushRouter.pushRouter({
                  router: ROUTE_FUND_DETAIL,
                  params: {
                    publicId: publicId,
                  },
                  state: {
                    [publicId]: row.original.id!,
                  },
                });
              }}
            >
              {t("common.details")}
            </DropdownMenuItem>

            <DropdownMenuItem
              onClick={() => {
                if (onEdit && row.original.id) {
                  onEdit(row.original.id.toString());
                }
              }}
            >
              {t("common.edit")}
            </DropdownMenuItem>

            {user?.role !== "STAFF" &&
              row.original.status !== FundStatus.Cancelled && (
                <DropdownMenuSub>
                  <DropdownMenuSubTrigger>
                    {t("common.updateStatus")} {/* "Cập nhật trạng thái" */}
                  </DropdownMenuSubTrigger>
                  <DropdownMenuSubContent>
                    {row.original.status === FundStatus.PendingConfirmation && (
                      <DropdownMenuItem
                        onClick={() => {
                          if (onStatusChange && row.original.id) {
                            onStatusChange(
                              row.original.id.toString(),
                              FundStatus.Confirmed
                            );
                          }
                        }}
                      >
                        {t("common.status.confirmed")} {/* "Đã xác nhận" */}
                      </DropdownMenuItem>
                    )}
                    {(row.original.status as FundStatus) !==
                      FundStatus.Cancelled &&
                      [5, 6].includes(row.original.fundBehavior?.id!) && (
                        <DropdownMenuItem
                          onClick={() => {
                            if (onStatusChange && row.original.id) {
                              onStatusChange(
                                row.original.id.toString(),
                                FundStatus.Cancelled
                              );
                            }
                          }}
                        >
                          {t("common.status.cancelled")} {/* "Đã hủy" */}
                        </DropdownMenuItem>
                      )}
                  </DropdownMenuSubContent>
                </DropdownMenuSub>
              )}
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    },
  ];

  return {
    columns: fundColumns,
  };
};
