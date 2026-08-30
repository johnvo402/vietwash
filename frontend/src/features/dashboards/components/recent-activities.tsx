"use client";

import { useEffect, useRef } from "react";
import { ListNotificationResponse } from "@/api/generated";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { useStringUtil } from "@/lib/stringUtil";
import { useTranslations } from "next-intl";
import { RotateCcw } from "lucide-react";
import { ROUTE_INVENTORY_DOC_DETAIL } from "@/types/router-type";
import { autoMapLink } from "@/utils/notification-util";
import { format } from "date-fns";
import { useIsMobile } from "@/hooks/use-mobile";
import { useNotificationMutations } from "@/features/notification/hooks/use-notification";

interface RecentActivitiesCardProps {
  notifications: ListNotificationResponse[];
  isLoading: boolean;
  error: any;
  fetchNextPage: () => void;
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
  refetch: () => void;
}

interface NotificationItemProps {
  item: ListNotificationResponse & {
    title: string;
    html: string;
    handlers: Record<string, (e: MouseEvent) => void>;
  };
}

function NotificationItem({ item }: NotificationItemProps) {
  const { readOne } = useNotificationMutations();

  const contentRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const container = contentRef.current;
    if (!container) return;
    const handleClick = (handler: (e: MouseEvent) => void) => (e: Event) => {
      handler(e as MouseEvent);
      if (!item.isRead) {
        readOne({ id: item.id! });
      }
    };

    Object.entries(item.handlers).forEach(([key, handler]) => {
      const link = container.querySelector(`[data-handler="${key}"]`);
      if (link) {
        link.addEventListener("click", handleClick(handler));
      }
    });

    return () => {
      Object.entries(item.handlers).forEach(([key, handler]) => {
        const link = container.querySelector(`[data-handler="${key}"]`);
        if (link) {
          link.removeEventListener("click", handleClick(handler));
        }
      });
    };
  }, [item.handlers, item.id, item.isRead, readOne]);

  return (
    <li
      className={`group p-${useIsMobile() ? "1" : "2"} rounded-xl border-t-2 border-border transition-colors relative hover:bg-muted/50`}
    >
      {!item.isRead && (
        <span className="absolute top-0 left-0 -mt-1 -mr-1 flex size-3 z-50">
          <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-primary opacity-75"></span>
          <span className="relative inline-flex size-3 rounded-full bg-primary"></span>
        </span>
      )}
      <div className="flex justify-between items-start">
        <div
          className={`text-${useIsMobile() ? "sm" : "base"} leading-tight font-normal text-foreground`}
        >
          {item.title}
        </div>
        <span
          className={`text-${useIsMobile() ? "xs" : "xs"} text-muted-foreground whitespace-nowrap ml-${useIsMobile() ? "2" : "4"} leading-tight`}
        >
          {format(new Date(item.createdAt!), "dd/MM/yy HH:mm")}
        </span>
      </div>
      <div
        ref={contentRef}
        className={`text-${useIsMobile() ? "xs" : "sm"} text-muted-foreground mt-${useIsMobile() ? "1" : "2"} leading-relaxed ${
          item.isRead ? "" : "font-medium"
        }`}
        dangerouslySetInnerHTML={{ __html: item.html }}
      />
    </li>
  );
}

export function RecentActivitiesCard({
  notifications,
  isLoading,
  error,
  fetchNextPage,
  hasNextPage,
  isFetchingNextPage,
  refetch,
}: RecentActivitiesCardProps) {
  const t = useTranslations();
  const { textByLang } = useStringUtil();
  const isMobile = useIsMobile();

  const routeMap = {
    import_id: {
      route: ROUTE_INVENTORY_DOC_DETAIL,
      paramCustoms: {
        type: "import",
      },
    },
    order_id: {
      route: "/orders/[publicId]",
      paramCustoms: {
        type: "order",
      },
    },
  };

  const processedNotifications = notifications.map((item) => {
    const title = item.title ? textByLang(JSON.parse(item.title)) : "";
    const contentHtml = item.contentHtml
      ? textByLang(JSON.parse(item.contentHtml))
      : "";
    const { html, handlers } = autoMapLink({
      routeMap,
      dictionary: item,
      contentHtml,
    });

    return { ...item, title, html, handlers };
  });

  return (
    <Card className={`w-full min-h-[60vh] max-h-[100vh] flex flex-col`}>
      <CardHeader>
        <CardTitle
          className={`text-${isMobile ? "lg" : "xl"} font-semibold flex justify-between`}
        >
          {t("dashboard.recentActivities")}
          <button
            className="p-1 rounded-full hover:bg-primary-foreground"
            onClick={refetch}
          >
            <RotateCcw
              className={`w-${isMobile ? "4" : "5"} h-${isMobile ? "4" : "5"}`}
            />
          </button>
        </CardTitle>
      </CardHeader>
      <CardContent className="flex-1 flex flex-col overflow-hidden">
        <div className="flex-1 overflow-y-auto">
          <ul
            className={`space-y-${isMobile ? "1" : "2"} pt-${isMobile ? "1" : "2"}`}
          >
            {error && (
              <li
                className={`text-${isMobile ? "xs" : "sm"} text-red-500 bg-background rounded-md`}
              >
                {error.message}
              </li>
            )}
            {isLoading ? (
              <li className={`text-${isMobile ? "xs" : "sm"} text-gray-500`}>
                {t("common.loading")}
              </li>
            ) : processedNotifications.length === 0 ? (
              <li className={`text-${isMobile ? "xs" : "sm"} text-gray-500`}>
                {t("dashboard.noneRecentActivities")}
              </li>
            ) : (
              processedNotifications.map((item) => (
                <NotificationItem key={item.id} item={item} />
              ))
            )}
          </ul>
        </div>
        {hasNextPage && (
          <div className={`pt-${isMobile ? "2" : "4"}`}>
            <Button
              onClick={fetchNextPage}
              disabled={isFetchingNextPage}
              className={`w-full text-${isMobile ? "sm" : "base"}`}
            >
              {isFetchingNextPage ? t("common.loading") : t("common.more")}
            </Button>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
