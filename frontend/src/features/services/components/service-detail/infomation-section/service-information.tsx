import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Pencil, ArrowLeft } from "lucide-react";
import { format } from "date-fns";
import {
  ActivationStatus,
  GetServiceDetailResponse,
} from "@/api/generated/api";
import { useTranslations } from "next-intl";
import Image from "next/image";
import { useStringUtil } from "@/lib/stringUtil";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_SERVICE_EDIT } from "@/types/router-type";
import StarRating from "./start-rating";

interface ServiceInformationProps {
  service: GetServiceDetailResponse;
}

export const ServiceInformation = (props: ServiceInformationProps) => {
  const t = useTranslations();
  const route = usePushRouter();
  const { formatDistance } = useStringUtil();

  const statusTitle = t("common.status.title");
  const categoryTitle = t("common.category").replace(/^./, (c) =>
    c.toUpperCase()
  );
  const priceTitle = t("common.price"); // Giả sử bạn có key này trong translations
  const ratingTitle = t("service.rating");
  // Xử lý giá từ unitRelations
  const getPriceDisplay = () => {
    const unitRelations = props.service?.unitRelations || [];
    if (!unitRelations.length) return "--";

    if (unitRelations.length === 1) {
      return unitRelations[0].price?.toLocaleString() || "--";
    }

    const prices = unitRelations
      .map((unit) => unit.price)
      .filter((price) => price !== undefined && price !== null) as number[];
    if (!prices.length) return "--";

    const minPrice = Math.min(...prices);
    const maxPrice = Math.max(...prices);
    return `${minPrice.toLocaleString()} - ${maxPrice.toLocaleString()}`;
  };
  const handleEdit = () => {
    route.pushRouter({
      router: ROUTE_SERVICE_EDIT,
      params: {
        publicId: props.service?.publicId?.toString()!,
      },
      state: {
        [props.service?.publicId?.toString()!]: props.service?.id,
      },
    });
  };

  return (
    <Card className="min-h-[calc(80vh_-_56px)] relative">
      <Button
        size="icon"
        variant="ghost"
        className="absolute top-1 left-1 h-6 w-6"
        onClick={() => route.back()}
      >
        <ArrowLeft className="h-3 w-3" />
        <span className="sr-only">Back</span>
      </Button>
      <Button
        size="icon"
        variant="ghost"
        className="absolute top-1 right-1 h-6 w-6"
        onClick={handleEdit}
      >
        <Pencil className="h-3 w-3" />
        <span className="sr-only">{t("common.edit")}</span>
      </Button>
      <CardContent className="p-4 h-full flex flex-col">
        <div className="flex flex-col items-center mb-4">
          <Image
            src={props.service?.image || "/logo/favicon.svg"}
            alt={props.service?.name ?? ""}
            width={200}
            height={200}
            className="object-cover mb-2 rounded"
          />
          <h1 className="text-lg font-semibold">{props.service?.name}</h1>
          <div
            className="text-sm prose text-muted-foreground text-center"
            dangerouslySetInnerHTML={{
              __html: props.service?.description || "--",
            }}
          />
        </div>

        <div className="space-y-3 text-sm">
          <div className="flex justify-between items-center">
            <span className="text-muted-foreground">{statusTitle}</span>
            <Badge
              variant={
                props.service?.status === ActivationStatus.Active
                  ? "default"
                  : "destructive"
              }
              className="capitalize text-xs"
            >
              {t(`common.status.${props.service?.status?.toLocaleLowerCase()}`)}
            </Badge>
          </div>
          <div className="flex justify-between items-center">
            <span className="text-muted-foreground">{ratingTitle}</span>
            <StarRating averageRating={props.service?.averageRating ?? 0} />
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">{categoryTitle}</span>
            <span>{props.service?.category?.name}</span>
          </div>

          <div className="flex justify-between">
            <span className="text-muted-foreground">{priceTitle}</span>
            <span>{getPriceDisplay()}</span>
          </div>

          <div className="flex justify-between">
            <span className="text-muted-foreground">{t("common.created")}</span>
            <div className="text-right">
              <div>
                {props.service?.createdAt
                  ? format(new Date(props.service.createdAt), "dd/MM/yyyy")
                  : "--"}
              </div>
              <div className="text-xs text-muted-foreground">
                {props.service?.createdAt
                  ? formatDistance(new Date(props.service.createdAt))
                  : "--"}
              </div>
            </div>
          </div>

          <div className="flex justify-between">
            <span className="text-muted-foreground">{t("common.updated")}</span>
            <div className="text-right">
              <div>
                {props.service?.updatedAt
                  ? format(new Date(props.service.updatedAt), "dd/MM/yyyy")
                  : "--"}
              </div>
              <div className="text-xs text-muted-foreground">
                {props.service?.updatedAt
                  ? formatDistance(new Date(props.service.updatedAt))
                  : "--"}
              </div>
            </div>
          </div>

          {/* <div className="pt-2 border-t">
            <h3 className="font-medium text-sm mb-1">{createdBy}</h3>
            <div>{`${props.service?.createdByUser?.firstName || "--"} ${props.service?.createdByUser?.lastName || "--"}`}</div>
          </div>

          <div className="pt-2 border-t">
            <h3 className="font-medium text-sm mb-1">{updatedBy}</h3>
            <div>{`${props.service?.updatedByUser?.firstName || "--"} ${props.service?.updatedByUser?.lastName || "--"}`}</div>
          </div> */}
        </div>
      </CardContent>
    </Card>
  );
};
