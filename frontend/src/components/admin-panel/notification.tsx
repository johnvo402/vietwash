import { useState, useEffect } from "react";
import { Bell, BellRing, CheckCheck } from "lucide-react";
import { useInView } from "react-intersection-observer";
import DOMPurify from "dompurify";
import {
  useNotification,
  useNotificationMutations,
} from "@/features/notification/hooks/use-notification";
import { useStringUtil } from "@/lib/stringUtil";
import { cn } from "@/lib/utils";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { useTranslations } from "next-intl";

export default function NotificationDropdown() {
  const [open, setOpen] = useState(false);
  const { ref, inView } = useInView();
  const { readAllNoti, isLoading: isMutating } = useNotificationMutations();
  const { textByLang } = useStringUtil();
  const t = useTranslations();
  // Pass the open state to useNotification to control data fetching
  const {
    notification: notifications,
    countNotification: numberNoty,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useNotification(
    {
      sort: "createdAt:desc",
      filter: {
        isRead: {
          $eq: false,
        },
      },
    },
    open
  );

  useEffect(() => {
    if (inView && hasNextPage && !isFetchingNextPage) {
      fetchNextPage();
    }
  }, [inView, hasNextPage, isFetchingNextPage, fetchNextPage]);

  const hasNotifications = numberNoty > 0;

  return (
    <TooltipProvider>
      <div className="relative mr-6 z-50">
        <Tooltip>
          <DropdownMenu
            open={open}
            onOpenChange={(isOpen) => {
              setOpen(isOpen);
            }}
          >
            <TooltipTrigger asChild>
              <DropdownMenuTrigger asChild>
                <button className="relative p-2 rounded-full hover:bg-primary-foreground">
                  {hasNotifications ? (
                    <BellRing className="w-6 h-6 text-primary animate-shake" />
                  ) : (
                    <Bell className="w-6 h-6 text-primary" />
                  )}
                  {hasNotifications && (
                    <span className="absolute -top-1 -right-1 bg-red-500 text-background text-xs font-bold px-1.5 py-0.5 rounded-full min-w-[1.25rem] text-center leading-none">
                      {numberNoty}
                    </span>
                  )}
                </button>
              </DropdownMenuTrigger>
            </TooltipTrigger>
            <TooltipContent>
              <p>{t("notification.title")}</p>
            </TooltipContent>
            <DropdownMenuContent className="w-72" align="end">
              <DropdownMenuLabel className="flex justify-between items-center">
                {t("notification.new_noti")}
                <button
                  onClick={() => readAllNoti()}
                  disabled={isMutating || numberNoty === 0}
                  className="text-sm text-primary hover:underline"
                >
                  <CheckCheck
                    className={cn(
                      "w-5 h-5",
                      numberNoty === 0 ? "text-gray-500" : ""
                    )}
                  />
                </button>
              </DropdownMenuLabel>
              <div className="max-h-60 overflow-y-auto">
                {error && (
                  <DropdownMenuItem className="text-sm text-red-500">
                    {error.message}
                  </DropdownMenuItem>
                )}
                {isLoading ? (
                  <DropdownMenuItem className="text-sm">
                    {t("common.loading")}
                  </DropdownMenuItem>
                ) : notifications.length === 0 ? (
                  <DropdownMenuItem className="text-sm">
                    Không có thông báo nào
                  </DropdownMenuItem>
                ) : (
                  notifications.map((item) => {
                    const title = textByLang(JSON.parse(item.title!));
                    const content = textByLang(JSON.parse(item.contentHtml!));
                    return (
                      <DropdownMenuItem
                        key={item.id}
                        className="flex-col items-start text-sm cursor-pointer"
                      >
                        <strong>{title}</strong>
                        <div
                          dangerouslySetInnerHTML={{
                            __html: DOMPurify.sanitize(content),
                          }}
                        />
                      </DropdownMenuItem>
                    );
                  })
                )}
                {hasNextPage && (
                  <DropdownMenuItem ref={ref} className="text-sm text-center">
                    {isFetchingNextPage
                      ? t("common.loading")
                      : t("common.more")}
                  </DropdownMenuItem>
                )}
              </div>
            </DropdownMenuContent>
          </DropdownMenu>
        </Tooltip>
      </div>
    </TooltipProvider>
  );
}
