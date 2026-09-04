import {
  FundStatus,
  FundType,
  GetFundDetailResponse,
  PaymentMethod,
} from "@/api/generated/api";
import { formatPriceVN } from "@/utils/format";
import { format } from "date-fns";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { usePushRouter } from "@/utils/router-utli";
import {
  ROUTE_INVENTORY_DOC_DETAIL,
  ROUTE_ORDERS_DETAIL,
} from "@/types/router-type";
import { useTranslations } from "next-intl";
import { useStringUtil } from "@/lib/stringUtil";
import { use, useEffect, useState } from "react";
import { SafeHtml } from "@/components/ui/safe-html";

// Mock data - In real app, this would come from API
const getPaymentMethodName = (t: any, method?: PaymentMethod) => {
  switch (method) {
    case PaymentMethod.Cash:
      return t("fund.paymentMethod.cash");
    case PaymentMethod.Card:
      return t("fund.paymentMethod.card");
    default:
      return "--";
  }
};

const getStatusBadge = (t: any, status?: FundStatus) => {
  switch (status) {
    case FundStatus.PendingConfirmation:
      return (
        <Badge
          variant="outline"
          className="bg-yellow-100 text-yellow-800 hover:bg-yellow-200"
        >
          {t("common.status.pendingConfirmation")};
        </Badge>
      );
    case FundStatus.Confirmed:
      return (
        <Badge
          variant="outline"
          className="bg-green-100 text-green-800 hover:bg-yellow-200"
        >
          {t("common.status.confirmed")};
        </Badge>
      );
    case FundStatus.Cancelled:
      return (
        <Badge
          variant="outline"
          className="bg-red-100 text-red-800 hover:bg-yellow-200"
        >
          {t("common.status.cancelled")};
        </Badge>
      );
    default:
      return <Badge className="bg-gray-100 text-gray-800">{"--"}</Badge>;
  }
};

const getTypeBadge = (t: any, type?: FundType) => {
  switch (type) {
    case FundType.Income:
      return (
        <span className="font-medium text-green-600">
          {t("fund.type.income")}
        </span>
      );
    case FundType.Spend:
      return (
        <span className="font-medium text-red-600">{t("fund.type.spend")}</span>
      );
    default:
      return <span className="font-medium text-gray-600">{"--"}</span>;
  }
};

export default function FundDetails({ fund }: { fund: GetFundDetailResponse }) {
  const routePush = usePushRouter();
  const t = useTranslations();
  const { textByLang } = useStringUtil();
  const [displayName, setDisplayName] = useState(
    fund.user?.displayName || "--",
  );

  useEffect(() => {
    if (fund.user?.displayName) {
      setDisplayName(fund.user.displayName);
    } else {
      let supplierName = "";
      try {
        const metadata = JSON.parse(fund.metadata || "{}");
        supplierName = metadata.supplierName;
        if (supplierName) {
          setDisplayName(supplierName);
        } else {
          setDisplayName("--");
        }
      } catch {
        supplierName = "";
      }
    }
  }, [fund.metadata, fund.user?.displayName]);

  const getMetadata = () => {
    let code = "";
    let publicId = "";
    let supplierId = "";
    let type = "";
    try {
      const metadata = JSON.parse(fund.metadata || "{}");
      code = metadata.code;
      publicId = metadata.publicId;
      supplierId = metadata.supplierId;
      type = metadata.type || "import";
    } catch {
      code = "";
      supplierId = "";
    }
    if (!code) return <div className="text-gray-400">{t("--")}</div>;

    return (
      <Button
        variant={"link"}
        onClick={() =>
          routePush.pushRouter({
            router: supplierId
              ? ROUTE_INVENTORY_DOC_DETAIL
              : ROUTE_ORDERS_DETAIL,
            params: {
              publicId: publicId,
              type: type.toLowerCase() || "import",
            },
            state: {
              [publicId]: fund.referenceId!,
            },
            redirect: "blank",
          })
        }
        className="text-blue-600 hover:underline"
      >
        {code}
      </Button>
    );
  };
  return (
    <div className="w-full py-8 px-4 sm:px-6 lg:px-8">
      <Card className="shadow-lg border-0 rounded-2xl overflow-hidden">
        <CardHeader className="bg-gradient-to-r from-blue-50 to-indigo-50">
          <CardTitle className="text-2xl font-bold text-gray-800">
            {t("fund.transactionDetails")}
          </CardTitle>
        </CardHeader>
        <CardContent className="p-6">
          <div className="space-y-8">
            {/* Transaction Info Section */}
            <section className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              <div className="space-y-2">
                <p className="text-sm font-semibold text-gray-600">
                  {t("fund.code")}
                </p>
                <p className="text-gray-900">{fund.code || "--"}</p>
              </div>

              <div className="space-y-2">
                <p className="text-sm font-semibold text-gray-600">
                  {t("fund.type.title")}
                </p>
                <p>{getTypeBadge(t, fund.type)}</p>
              </div>
              <div className="space-y-2">
                <p className="text-sm font-semibold text-gray-600">
                  {t("common.status.title")}
                </p>
                <p>{getStatusBadge(t, fund.status)}</p>
              </div>
              <div className="space-y-2">
                <p className="text-sm font-semibold text-gray-600">
                  {t("table.accessorKey.amount")}
                </p>
                <p className="text-gray-900 font-medium">
                  {formatPriceVN(fund.amount ?? 0)}
                </p>
              </div>
              <div className="space-y-2">
                <p className="text-sm font-semibold text-gray-600">
                  {t("fund.paymentMethod.title")}
                </p>
                <p className="text-gray-900">
                  {getPaymentMethodName(t, fund.paymentMethod)}
                </p>
              </div>
              <div className="space-y-2">
                <p className="text-sm font-semibold text-gray-600">
                  {t("fund.transactionDate")}
                </p>
                <p className="text-gray-900">
                  {fund.transactionDate
                    ? format(
                        new Date(fund.transactionDate),
                        "dd/MM/yy HH:mm:ss",
                      )
                    : "--"}
                </p>
              </div>
            </section>

            {/* Date and Audit Section */}
            <section className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              <div className="space-y-2">
                <p className="text-sm font-semibold text-gray-600">
                  {t("fund.behavior")}
                </p>
                <p className="text-gray-900">
                  {textByLang(JSON.parse(fund.fundBehavior?.name)) || "--"}
                </p>
              </div>
              <div className="space-y-2">
                <p className="text-sm font-semibold text-gray-600">
                  {t("table.accessorKey.createdAt")}
                </p>
                <p className="text-gray-900">
                  {fund.createdAt
                    ? format(new Date(fund.createdAt), "dd/MM/yy HH:mm:ss")
                    : "--"}
                </p>
              </div>
              <div className="space-y-2">
                <p className="text-sm font-semibold text-gray-600">
                  {t("table.accessorKey.updatedAt")}
                </p>
                <p className="text-gray-900">
                  {fund.updatedAt
                    ? format(new Date(fund.updatedAt), "dd/MM/yy HH:mm:ss")
                    : "--"}
                </p>
              </div>
            </section>

            {/* User Info Section */}
            <section className="border-t pt-6">
              <h3 className="text-lg font-semibold text-gray-800 mb-4">
                {t("user.userInformation")}
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-2">
                  <p className="text-sm font-semibold text-gray-600">
                    {t("user.displayName.title")}
                  </p>
                  <p className="text-gray-900">{displayName || "--"}</p>
                </div>
                <div className="space-y-2">
                  <p className="text-sm font-semibold text-gray-600">
                    {t("user.email.title")}
                  </p>
                  <p className="text-gray-900">{fund.user?.email || "--"}</p>
                </div>
                <div className="space-y-2">
                  <p className="text-sm font-semibold text-gray-600">
                    {t("user.phoneNumber.title")}
                  </p>
                  <p className="text-gray-900">
                    {fund.user?.phoneNumber || "--"}
                  </p>
                </div>
                <div className="space-y-2">
                  <p className="text-sm font-semibold text-gray-600">
                    {t("user.customerGroup.title")}
                  </p>
                  <p className="text-gray-900">
                    {fund.user?.customerGroup
                      ? `${t("user.customerGroup.group")} ${fund.user.customerGroup}`
                      : "--"}
                  </p>
                </div>
              </div>
            </section>

            {/* Notes and Metadata Section */}
            <section className="border-t pt-6 space-y-6">
              <div>
                <p className="text-sm font-semibold text-gray-600">
                  {t("user.note")}
                </p>
                <SafeHtml
                  className="text-gray-900 mt-2 bg-gray-50 p-4 rounded-lg"
                  html={fund.note}
                  fallback="--"
                />
              </div>
              <div>
                <p className="text-sm font-semibold text-gray-600">
                  {t("fund.association")}
                </p>
                <p className="text-gray-900 mt-2 bg-gray-50 p-4 rounded-lg">
                  {getMetadata()}
                </p>
              </div>
            </section>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
