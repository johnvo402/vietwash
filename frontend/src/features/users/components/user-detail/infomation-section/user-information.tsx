import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton"; // Assuming a Skeleton component exists
import { Pencil, ArrowLeft } from "lucide-react";
import { format } from "date-fns";
import {
  ActivationStatus,
  GetAccountDetailResponse,
} from "@/api/generated/api";
import { useTranslations } from "next-intl";
import Image from "next/image";
import { useStringUtil } from "@/lib/stringUtil";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_USER_EDIT } from "@/types/router-type";

interface UserInformationProps {
  user: GetAccountDetailResponse | null; // Allow null for loading state
  isLoading?: boolean; // Optional prop to control loading state
}

export const UserInformation = ({
  user,
  isLoading = false,
}: UserInformationProps) => {
  const t = useTranslations();
  const route = usePushRouter();
  const { formatDistance } = useStringUtil();

  const statusTitle = t("common.status.title");
  const emailTitle = "Email";
  const phoneTitle = t("user.phoneNumber.title");
  const genderTitle = t("user.gender.title");
  const birthDayTitle = t("user.dateOfBirth");
  const addressTitle = t("user.address.title");

  const handleEdits = () => {
    if (!user?.publicId) return;
    const publicId = user.publicId;
    route.pushRouter({
      router: ROUTE_USER_EDIT,
      params: {
        publicId: publicId?.toString()!,
      },
      state: {
        [publicId?.toString()!]: user.id,
      },
    });
  };

  if (isLoading || !user) {
    return (
      <Card className="min-h-[calc(80vh_-_56px)] relative">
        <CardContent className="p-4 h-full flex flex-col">
          <div className="flex flex-col items-center mb-4">
            <Skeleton className="h-[200px] w-[200px] rounded mb-2" />
          </div>
          <div className="space-y-3 text-sm">
            {Array.from({ length: 10 }).map((_, index) => (
              <div key={index} className="flex justify-between">
                <Skeleton className="h-4 w-[100px]" />
                <Skeleton className="h-4 w-[150px]" />
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    );
  }

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
        onClick={handleEdits}
      >
        <Pencil className="h-3 w-3" />
        <span className="sr-only">{t("common.edit")}</span>
      </Button>
      <CardContent className="p-4 h-full flex flex-col">
        <div className="flex flex-col items-center mb-4">
          <Image
            src={user?.avtUrl || "/logo/favicon.svg"}
            alt={user?.displayName ?? ""}
            width={200}
            height={200}
            className="object-cover mb-2 rounded"
          />
        </div>

        <div className="space-y-3 text-sm">
          <div className="flex justify-between">
            <span className="text-muted-foreground">
              {t("user.displayName.title")}
            </span>
            <span>{user?.displayName || "--"}</span>
          </div>
          <div className="flex justify-between items-center">
            <span className="text-muted-foreground">{statusTitle}</span>
            <Badge
              variant={
                user?.status === ActivationStatus.Active
                  ? "default"
                  : "destructive"
              }
              className="capitalize text-xs"
            >
              {t(`common.status.${user?.status?.toLocaleLowerCase()}`)}
            </Badge>
          </div>

          <div className="flex justify-between">
            <span className="text-muted-foreground">{emailTitle}</span>
            <span>{user?.email || "--"}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">{phoneTitle}</span>
            <span>{user?.phoneNumber || "--"}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">{genderTitle}</span>
            <span>{t(`user.gender.${user?.gender}`) || "--"}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">{birthDayTitle}</span>
            <span>
              {user?.birthDay
                ? format(new Date(user.birthDay), "dd/MM/yyyy")
                : "--"}
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">{addressTitle}</span>
            <p className="truncate text-wrap">
              {user?.accountContact?.address || "--"}
            </p>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">{t("common.created")}</span>
            <div className="text-right">
              <div>
                {user?.createdAt
                  ? format(new Date(user.createdAt), "dd/MM/yyyy")
                  : "--"}
              </div>
              <div className="text-xs text-muted-foreground">
                {user?.createdAt
                  ? formatDistance(new Date(user.createdAt))
                  : "--"}
              </div>
            </div>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">{t("common.updated")}</span>
            <div className="text-right">
              <div>
                {user?.updatedAt
                  ? format(new Date(user.updatedAt), "dd/MM/yyyy")
                  : "--"}
              </div>
              <div className="text-xs text-muted-foreground">
                {user?.updatedAt
                  ? formatDistance(new Date(user.updatedAt))
                  : "--"}
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
};
