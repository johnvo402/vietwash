import axios, { AxiosInstance, AxiosResponse } from "axios";
import { ClientApi } from "./generated";
import { ROUTE_LOGIN } from "@/types/router-type";
import { setBearerAuthToObject } from "./generated/common";
import { RequestError } from "@/types/error";
import { Credentials, useAuth } from "@/hooks/use-auth";
import { Configuration } from "./generated";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL;
let isRefreshing = false;
let refreshSubscribers: (() => void)[] = [];

const apiKey = process.env.NEXT_PUBLIC_API_KEY || "";
const platform = process.env.NEXT_PUBLIC_PLATFORM || "";

const defaultHeaders = {
  "X-Api-Key": apiKey,
  Platform: platform,
};

// ✅ Cấu hình API client
const configuration = new Configuration({
  accessToken: async () => {
    const { credentials } = useAuth.getState();
    return credentials?.token || "";
  },
  baseOptions: {
    headers: defaultHeaders,
    timeout: 60000,
    timeoutErrorMessage: "Request timeout",
    withCredentials: true,
  },
});

const apiInstance: AxiosInstance = axios.create({
  baseURL: BASE_URL,
  headers: { "Content-Type": "application/json" },
});

const refreshInstance = axios.create({
  baseURL: BASE_URL,
  headers: { ...defaultHeaders, "Content-Type": "application/json" },
  withCredentials: true,
  timeout: 60000,
  timeoutErrorMessage: "Refresh token request timeout",
});

apiInstance.interceptors.request.use(
  async (config) => {
    const { credentials } = useAuth.getState();
    if (credentials?.token) {
      config.headers["Authorization"] = `Bearer ${credentials.token}`;
    } else {
      await setBearerAuthToObject(config.headers, configuration);
    }

    return config;
  },
  (error) => Promise.reject(error)
);

// ✅ 4. Hàm gọi refresh token bằng instance không interceptor
const refreshTokenRequest = async (refreshToken: string) => {
  const res = await refreshInstance.post("/Auth/api/Accounts/RefreshToken", {
    refreshToken,
  });
  return res.data;
};

// ✅ 5. Response interceptor để bắt lỗi và xử lý 401
apiInstance.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error) => {
    const originalRequest = error.config;
    const { credentials, login, logout } = useAuth.getState();

    // ❗ Nếu chính request refresh bị lỗi => logout
    if (
      originalRequest._retry ||
      originalRequest.url?.includes("/RefreshToken")
    ) {
      logout();
      window.location.href = ROUTE_LOGIN;
      return Promise.reject(error);
    }

    // 🔁 Nếu bị 401 thì thử refresh token
    if (error.response?.status === 401) {
      if (!credentials?.refresh) {
        logout();
        window.location.href = ROUTE_LOGIN;
        return Promise.reject("No refresh token available.");
      }

      if (!isRefreshing) {
        isRefreshing = true;
        originalRequest._retry = true;

        try {
          const res = await refreshTokenRequest(credentials.refresh);

          const newCredentials: Credentials = {
            token: res.results?.token,
            refresh: res.results?.refresh,
            accessTokenExpiredIn: res.results?.accessTokenExpiredIn,
          };

          login(newCredentials);

          refreshSubscribers.forEach((cb) => cb());
          refreshSubscribers = [];
          isRefreshing = false;

          return apiInstance(originalRequest);
        } catch (refreshError) {
          isRefreshing = false;
          refreshSubscribers = [];

          logout();
          window.location.href = ROUTE_LOGIN;
          return Promise.reject("Session expired, please login again.");
        }
      }

      // ⏳ Nếu đang refresh, thì đợi
      return new Promise((resolve) => {
        refreshSubscribers.push(() => {
          resolve(apiInstance(originalRequest));
        });
      });
    }

    if (error.response?.status === 403) {
      window.location.href = "/403";
      return Promise.reject("Forbidden");
    }

    return Promise.reject(
      (error.response?.data as RequestError) ?? error.message
    );
  }
);

// ✅ 6. Tạo API client chính (sử dụng instance đã gắn interceptor)
export const apiClient = new ClientApi(configuration, BASE_URL, apiInstance);
