import DOMPurify from "dompurify";
import { PushRouterInput } from "./router-utli";
import { ListNotificationResponse } from "@/api/generated";

export const requestNotificationPermission = async () => {
  if (!("Notification" in window)) return;

  if (Notification.permission === "default") {
    await Notification.requestPermission();
  }
};
export const showBrowserNotification = (
  title: string,
  options?: NotificationOptions
) => {
  if (!("Notification" in window)) return;

  if (Notification.permission === "granted") {
    new Notification(title, options);
  } else if (Notification.permission !== "denied") {
    Notification.requestPermission().then((permission) => {
      if (permission === "granted") {
        new Notification(title, options);
      }
    });
  }
};

interface AutoMapLinkParams {
  routeMap: Record<
    string,
    {
      route: string;
      paramCustoms: Record<string, string | number | boolean>;
    }
  >;
  dictionary: ListNotificationResponse;
  contentHtml: string;
}

export const autoMapLink = ({
  routeMap,
  dictionary,
  contentHtml,
}: AutoMapLinkParams): {
  html: string;
  handlers: Record<string, (e: MouseEvent) => void>;
} => {
  const handlers: Record<string, (e: MouseEvent) => void> = {};
  let sanitizedContent = DOMPurify.sanitize(contentHtml);

  const idRegex = /<strong id="([^"]+)">([^<]+)<\/strong>/g;

  sanitizedContent = sanitizedContent.replace(idRegex, (match, id, text) => {
    const paramValue = dictionary.data?.publicId;
    const idValue = dictionary.data?.[id];
    const routeConfig = routeMap[id];

    if (!paramValue || !idValue || !routeConfig) {
      return match;
    }

    const pushRouterInput: PushRouterInput = {
      router: routeConfig.route,
      params: {
        publicId: paramValue,
        ...routeConfig.paramCustoms,
      },
      state: { [paramValue]: idValue },
      replace: false,
    };

    const handlerKey = `link_${paramValue}_${id}`;
    let path = pushRouterInput.router;
    if (pushRouterInput.params) {
      Object.entries(pushRouterInput.params).forEach(([key, value]) => {
        path = path.replace(`[${key}]`, String(value));
      });
    }

    handlers[handlerKey] = (e: MouseEvent) => {
      e.preventDefault();
      if (pushRouterInput.state) {
        Object.entries(pushRouterInput.state).forEach(([key, value]) => {
          sessionStorage.setItem(
            key,
            typeof value === "string" ? value : JSON.stringify(value)
          );
        });
      }
      window.open(path, "_blank");
    };

    return `<a href="${path}" class="text-blue-600 hover:underline" data-handler="${handlerKey}">${text}</a>`;
  });

  return { html: sanitizedContent, handlers };
};
