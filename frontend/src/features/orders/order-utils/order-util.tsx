import { CustomerGroup, OrderStatus } from "@/api/generated/api";
import { Badge } from "@/components/ui/badge";
import { useTranslations } from "next-intl";

export const GetStatusBadge = (status: OrderStatus) => {
  const t = useTranslations();

  switch (status) {
    case OrderStatus.Pending:
      return (
        <Badge
          variant="outline"
          className="bg-yellow-100 text-yellow-900 border-yellow-300 hover:bg-yellow-100"
        >
          {t("common.status.pending")}
        </Badge>
      );
    case OrderStatus.InProgress:
      return (
        <Badge
          variant="outline"
          className="bg-blue-100 text-blue-900 border-blue-300 hover:bg-blue-100"
        >
          {t("common.status.handling")}
        </Badge>
      );
    case OrderStatus.Processed:
      return (
        <Badge
          variant="outline"
          className="bg-orange-100 text-orange-900 border-orange-300 hover:bg-orange-100"
        >
          {t("common.status.handled")}
        </Badge>
      );
    case OrderStatus.Completed:
      return (
        <Badge
          variant="outline"
          className="bg-green-100 text-green-900 border-green-300 hover:bg-green-100"
        >
          {t("common.status.completed")}
        </Badge>
      );
    case OrderStatus.Cancelled:
      return (
        <Badge
          variant="outline"
          className="bg-red-100 text-red-900 border-red-300 hover:bg-red-100"
        >
          {t("common.status.cancelled")}
        </Badge>
      );
    default:
      return (
        <Badge
          variant="outline"
          className="bg-gray-100 text-gray-800 border-gray-300"
        >
          {status}
        </Badge>
      );
  }
};

export const GetCustomerGroup = (t: any, group?: CustomerGroup) => {
  switch (group) {
    case CustomerGroup.Loyal:
      return (
        <Badge
          variant="outline"
          className="bg-primary-foreground text-primary hover:bg-primary-foreground"
        >
          {t("customer.loyal")}
        </Badge>
      );

    default:
      return <Badge variant="outline">{t("customer.normal")}</Badge>;
  }
};
