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

    const listeners = Object.entries(item.handlers).flatMap(
      ([key, handler]) => {
        const link = container.querySelector(`[data-handler="${key}"]`);
        if (!link) return [];

        const listener = handleClick(handler);
        link.addEventListener("click", listener);
        return [{ link, listener }];
      },
    );

    return () => {
      listeners.forEach(({ link, listener }) => {
        link.removeEventListener("click", listener);
      });
    };
  }, [item.handlers, item.id, item.isRead, readOne]);

  return (
    <li className="group relative rounded-xl border-t-2 border-border p-1 transition-colors hover:bg-muted/50 md:p-2">
      {!item.isRead && (
        <span className="absolute top-0 left-0 -mt-1 -mr-1 flex size-3 z-50">
          <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-primary opacity-75"></span>
          <span className="relative inline-flex size-3 rounded-full bg-primary"></span>
        </span>
      )}
      <div className="flex justify-between items-start">
        <div className="text-sm font-normal leading-tight text-foreground md:text-base">
          {item.title}
        </div>
        <span className="ml-2 whitespace-nowrap text-xs leading-tight text-muted-foreground md:ml-4">
          {format(new Date(item.createdAt!), "dd/MM/yy HH:mm")}
        </span>
      </div>
      <div
        ref={contentRef}
        className={`mt-1 text-xs leading-relaxed text-muted-foreground md:mt-2 md:text-sm ${
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
    <Card className="flex min-h-[60vh] max-h-[100vh] w-full flex-col">
      <CardHeader>
        <CardTitle className="flex justify-between text-lg font-semibold md:text-xl">
          {t("dashboard.recentActivities")}
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="h-11 w-11 shrink-0 rounded-full"
            onClick={refetch}
            aria-label={t("dashboard.refreshRecentActivities")}
          >
            <RotateCcw className="h-4 w-4 md:h-5 md:w-5" aria-hidden="true" />
          </Button>
        </CardTitle>
      </CardHeader>
      <CardContent className="flex-1 flex flex-col overflow-hidden">
        <div className="flex-1 overflow-y-auto">
          <ul className="space-y-1 pt-1 md:space-y-2 md:pt-2">
            {error && (
              <li className="rounded-md bg-background text-xs text-red-500 md:text-sm">
                {error.message}
              </li>
            )}
            {isLoading ? (
              <li className="text-xs text-gray-500 md:text-sm">
                {t("common.loading")}
              </li>
            ) : processedNotifications.length === 0 ? (
              <li className="text-xs text-gray-500 md:text-sm">
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
          <div className="pt-2 md:pt-4">
            <Button
              onClick={fetchNextPage}
              disabled={isFetchingNextPage}
              className="w-full text-sm md:text-base"
            >
              {isFetchingNextPage ? t("common.loading") : t("common.more")}
            </Button>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
