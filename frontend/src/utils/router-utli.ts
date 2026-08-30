"use client";

import { useRouter } from "nextjs-toploader/app";

export type PushRouterInput = {
  router: string;
  params?: Record<string, string>;
  query?: Record<string, string | number | boolean>;
  state?: Record<string, any>;
  replace?: boolean;
  redirect?: "current" | "blank" | "custom";
};

export function usePushRouter() {
  const router = useRouter();

  function pushRouter({
    router: pathTemplate,
    params,
    query,
    state,
    replace = false,
    redirect = "current",
  }: PushRouterInput) {
    let path = pathTemplate;

    // Thay thế các tham số động trong path
    if (params && Object.keys(params).length > 0) {
      Object.entries(params).forEach(([key, value]) => {
        path = path.replace(`[${key}]`, String(value));
      });
    }

    // Thêm query string nếu có
    if (query && Object.keys(query).length > 0) {
      const queryString = Object.entries(query)
        .map(
          ([key, value]) =>
            `${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`
        )
        .join("&");
      path += `?${queryString}`;
    }

    // Lưu state vào sessionStorage nếu có
    if (state && typeof state === "object") {
      Object.entries(state).forEach(([key, value]) => {
        sessionStorage.setItem(
          key,
          typeof value === "string" ? value : JSON.stringify(value)
        );
      });
    }

    // Xử lý redirect
    if (redirect === "blank") {
      window.open(path, "_blank");
    } else if (redirect === "current") {
      if (replace) {
        router.replace(path);
      } else {
        router.push(path);
      }
    } else {
      console.warn("[usePushRouter] Unsupported redirect type:", redirect);
    }
  }

  return {
    pushRouter,
    back: () => router.back(),
  };
}
