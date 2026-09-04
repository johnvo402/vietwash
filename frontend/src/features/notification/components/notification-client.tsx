"use client";

import { useEffect, useRef } from "react";
import { startSignalRConnection, stopSignalRConnection } from "@/lib/signalr";
import { useAuth } from "@/hooks/use-auth";
import {
  requestNotificationPermission,
  showBrowserNotification,
} from "@/utils/notification-util";
import { toast } from "react-toastify";
import { useStringUtil } from "@/lib/stringUtil";
import NotificationDropdown from "@/components/admin-panel/notification";
import { useQueryClient } from "@tanstack/react-query";
import { SafeHtml } from "@/components/ui/safe-html";

// Define interfaces for better type safety
interface NotificationMessage {
  id: string;
  title: string;
  content: string;
  contentHtml: string;
}

interface CustomToastProps {
  title: string;
  content?: string;
}

// CustomToast component
export const CustomToast = ({ title, content }: CustomToastProps) => (
  <div>
    <strong>{title}</strong>
    {content && <SafeHtml html={content} />}
  </div>
);

const NotificationClient = () => {
  const { credentials } = useAuth();
  const { textByLang } = useStringUtil();
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const isConnectingRef = useRef(false);
  const retryTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const queryClient = useQueryClient();
  // Hàm thử kết nối lại với exponential backoff
  const attemptConnection = async (
    retryDelay = 1000,
    maxRetries = Infinity,
  ) => {
    if (isConnectingRef.current || !credentials?.token) return;

    isConnectingRef.current = true;
    let retries = 0;

    const connect = async () => {
      try {
        connectionRef.current = await startSignalRConnection({
          accessToken: credentials.token!,
          onConnected: () => {
            // Recover notifications persisted while this browser was disconnected.
            queryClient.invalidateQueries({ queryKey: ["notification"] });
            queryClient.invalidateQueries({ queryKey: ["countNotify"] });
          },
          onReceiveMessage: (message: NotificationMessage) => {
            const title = textByLang(JSON.parse(message.title));
            const content = textByLang(JSON.parse(message.content));

            queryClient.invalidateQueries({ queryKey: ["notification"] });
            queryClient.invalidateQueries({ queryKey: ["countNotify"] });
            toast.info(content, {
              toastId: message.id,
              position: "bottom-right",
              style: {
                backgroundColor: "hsl(var(--primary-foreground))",
                color: "hsl(var(--foreground))",
              },
            });

            showBrowserNotification(title, {
              body: content,
              icon: "/logo/favicon.svg",
            });
          },
        });
        console.log("✅ Connection established successfully");
        isConnectingRef.current = false;
        // Xóa timeout nếu kết nối thành công
        if (retryTimeoutRef.current) {
          clearTimeout(retryTimeoutRef.current);
          retryTimeoutRef.current = null;
        }
      } catch (err) {
        console.error("🚫 Connection attempt failed:", err);
        if (retries < maxRetries && navigator.onLine) {
          const delay = Math.min(retryDelay * Math.pow(2, retries), 30000); // Tối đa 30 giây
          console.log(`🔄 Retrying in ${delay}ms... (Attempt ${retries + 1})`);
          retryTimeoutRef.current = setTimeout(() => {
            retries++;
            connect();
          }, delay);
        } else {
          isConnectingRef.current = false;
          console.warn(
            "❌ Max retries reached or offline, stopping retry attempts",
          );
        }
      }
    };

    await connect();
  };

  useEffect(() => {
    if (!credentials?.token) return;

    // Yêu cầu quyền thông báo
    requestNotificationPermission();

    // Thử kết nối ban đầu
    attemptConnection();

    // Lắng nghe sự kiện online
    const handleOnline = () => {
      console.log("🌐 Network online, attempting to reconnect...");
      if (
        !connectionRef.current ||
        connectionRef.current.state !== "Connected"
      ) {
        attemptConnection(); // Thử kết nối lại khi online
      }
    };

    // Lắng nghe sự kiện offline
    const handleOffline = () => {
      console.warn("🌐 Network offline, stopping connection attempts...");
      if (retryTimeoutRef.current) {
        clearTimeout(retryTimeoutRef.current);
        retryTimeoutRef.current = null;
      }
      if (connectionRef.current) {
        stopSignalRConnection(connectionRef.current);
      }
    };

    window.addEventListener("online", handleOnline);
    window.addEventListener("offline", handleOffline);

    return () => {
      // Dọn dẹp khi component unmount
      window.removeEventListener("online", handleOnline);
      window.removeEventListener("offline", handleOffline);
      if (retryTimeoutRef.current) {
        clearTimeout(retryTimeoutRef.current);
      }
      if (connectionRef.current) {
        stopSignalRConnection(connectionRef.current);
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [credentials?.token]);

  return <NotificationDropdown />;
};

export default NotificationClient;
