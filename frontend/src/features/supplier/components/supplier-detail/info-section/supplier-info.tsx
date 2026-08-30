import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Pencil, ArrowLeft } from "lucide-react";
import { format } from "date-fns";
import { useTranslations } from "next-intl";
import { GetSupplierDetailResponse, ActivationStatus } from "@/api/generated";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_SUPPLIER_EDIT } from "@/types/router-type";
import { Avatar, AvatarImage } from "@/components/ui/avatar";
import { useStringUtil } from "@/lib/stringUtil";

interface SupplierInformationProps {
  supplier: GetSupplierDetailResponse;
}

export const SupplierInformation = (props: SupplierInformationProps) => {
  const t = useTranslations();
  const route = usePushRouter();
  const { formatDistance } = useStringUtil();

  const createdBy = t("table.accessorKey.createdBy");
  const updatedBy = t("table.accessorKey.updatedBy");
  const statusTitle = t("common.status.title");
  const codeTitle = t("table.accessorKey.code"); // Assumed translation key
  const emailTitle = t("user.email.title"); // Assumed translation key
  const addressTitle = t("user.address.title"); // Assumed translation key
  const phoneTitle = t("user.phoneNumber.title"); // Assumed translation key

  const handleEdits = () => {
    route.pushRouter({
      router: ROUTE_SUPPLIER_EDIT,
      params: {
        publicId: props.supplier?.publicId?.toString()!,
      },
      state: {
        [props.supplier?.publicId?.toString()!]: props.supplier?.id,
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
        <span className="sr-only">{t("common.back")}</span>
      </Button>
      <Button
        size="icon"
        variant="ghost"
        className="absolute top-1 right-1 h-6 w-6"
        onClick={handleEdits}
      >
        <Pencil className="h-3 w-3" />
        <span className="sr-only">{t("common.edit")}</span>
      </Button>
      <CardContent className="p-4 h-full flex flex-col">
        <div className="flex flex-col items-center mb-4">
          <Avatar className="w-24 h-24">
            <AvatarImage
              src="/img/company.png"
              alt={props.supplier?.name ?? ""}
            />
          </Avatar>
          <h1 className="text-lg font-semibold">{props.supplier?.name}</h1>
          <p className="text-sm text-muted-foreground text-center">
            {props.supplier?.description}
          </p>
        </div>

        <div className="space-y-3 text-sm">
          <div className="flex justify-between items-center">
            <span className="text-muted-foreground">{statusTitle}</span>
            <Badge
              variant={
                props.supplier?.status === ActivationStatus.Inactive
                  ? "destructive"
                  : "default"
              }
              className="capitalize text-xs"
            >
              {t(`common.status.${props.supplier?.status!.toLowerCase()}`)}
            </Badge>
          </div>

          <div className="flex justify-between">
            <span className="text-muted-foreground">{codeTitle}</span>
            <span>{props.supplier?.code || "--"}</span>
          </div>

          <div className="flex justify-between">
            <span className="text-muted-foreground">{emailTitle}</span>
            <span>{props.supplier?.email || "--"}</span>
          </div>

          <div className="flex justify-between">
            <span className="text-muted-foreground">{addressTitle}</span>
            <span>{props.supplier?.address || "--"}</span>
          </div>

          <div className="flex justify-between">
            <span className="text-muted-foreground">{phoneTitle}</span>
            <span>{props.supplier?.phone || "--"}</span>
          </div>

          <div className="flex justify-between">
            <span className="text-muted-foreground">
              {t("table.accessorKey.createdAt")}
            </span>
            <div className="text-right">
              <div>
                {props.supplier?.createdAt
                  ? format(new Date(props.supplier.createdAt), "dd/MM/yyyy")
                  : "--"}
              </div>
              <div className="text-xs text-muted-foreground">
                {props.supplier?.createdAt
                  ? formatDistance(new Date(props.supplier.createdAt))
                  : "--"}
              </div>
            </div>
          </div>

          <div className="flex justify-between">
            <span className="text-muted-foreground">
              {t("table.accessorKey.updatedAt")}
            </span>
            <div className="text-right">
              <div>
                {props.supplier?.updatedAt
                  ? format(new Date(props.supplier.updatedAt), "dd/MM/yyyy")
                  : "--"}
              </div>
              <div className="text-xs text-muted-foreground">
                {props.supplier?.updatedAt
                  ? formatDistance(new Date(props.supplier.updatedAt))
                  : "--"}
              </div>
            </div>
          </div>

          <div className="pt-2 border-t">
            <h3 className="font-medium text-sm mb-1">{createdBy}</h3>
            <div>{props.supplier?.createdBy || "--"}</div>
          </div>

          <div className="pt-2 border-t">
            <h3 className="font-medium text-sm mb-1">{updatedBy}</h3>
            <div>{props.supplier?.updatedBy || "--"}</div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
};
