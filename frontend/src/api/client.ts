import axios, {
  AxiosError,
  AxiosInstance,
  AxiosResponse,
  InternalAxiosRequestConfig,
} from "axios";
import { ClientApi, Configuration } from "./generated";
import { ROUTE_LOGIN } from "@/types/router-type";
import { setBearerAuthToObject } from "./generated/common";
import {
  Credentials,
  CredentialsInput,
  isValidCredentials,
  useAuth,
} from "@/hooks/use-auth";

// An explicit empty base prevents the generated client's localhost fallback.
const BASE_URL = process.env.NEXT_PUBLIC_API_URL?.replace(/\/+$/, "") ?? "";
const publicClientId = process.env.NEXT_PUBLIC_CLIENT_ID;
const platform = process.env.NEXT_PUBLIC_PLATFORM;

const defaultHeaders: Record<string, string> = {};

// This browser-visible value identifies the client; it is not an authorization secret.
if (publicClientId) defaultHeaders["X-Api-Key"] = publicClientId;
if (platform) defaultHeaders.Platform = platform;

interface RefreshResponse {
  results?: CredentialsInput | null;
}

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

let refreshPromise: Promise<Credentials> | null = null;
let sessionExpirationHandled = false;

const configuration = new Configuration({
  basePath: BASE_URL,
  accessToken: async () => useAuth.getState().credentials?.token ?? "",
  baseOptions: {
    headers: defaultHeaders,
    timeout: 60000,
    timeoutErrorMessage: "Request timeout",
    withCredentials: true,
  },
});

const apiInstance: AxiosInstance = axios.create({
  baseURL: BASE_URL,
  headers: { ...defaultHeaders, "Content-Type": "application/json" },
});

const refreshInstance = axios.create({
  baseURL: BASE_URL,
  headers: { ...defaultHeaders, "Content-Type": "application/json" },
  withCredentials: true,
  timeout: 60000,
  timeoutErrorMessage: "Refresh token request timeout",
});

const redirectTo = (path: string) => {
  if (typeof window !== "undefined" && window.location.pathname !== path) {
    window.location.assign(path);
  }
};

const expireSession = () => {
  if (sessionExpirationHandled) return;

  sessionExpirationHandled = true;
  useAuth.getState().logout();
  redirectTo(ROUTE_LOGIN);
};

const refreshTokenRequest = async (
  refreshToken: string,
): Promise<Credentials> => {
  const response = await refreshInstance.post<RefreshResponse>(
    "/Auth/api/Accounts/RefreshToken",
    { refreshToken },
  );
  const credentials = response.data.results;

  if (!isValidCredentials(credentials)) {
    throw new Error("Refresh response did not contain valid credentials.");
  }

  return credentials;
};

const refreshSession = (refreshToken: string): Promise<Credentials> => {
  if (!refreshPromise) {
    refreshPromise = refreshTokenRequest(refreshToken)
      .then((credentials) => {
        useAuth.getState().login(credentials);
        sessionExpirationHandled = false;
        return credentials;
      })
      .catch((error: unknown) => {
        expireSession();
        throw error instanceof Error
          ? error
          : new Error("Unable to refresh the current session.");
      })
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
};

apiInstance.interceptors.request.use(
  async (config) => {
    const { credentials } = useAuth.getState();
    if (credentials?.token) {
      // A successfully authenticated request starts a fresh session lifecycle.
      sessionExpirationHandled = false;
      config.headers.Authorization = `Bearer ${credentials.token}`;
    } else {
      await setBearerAuthToObject(config.headers, configuration);
    }

    return config;
  },
  (error: AxiosError) => Promise.reject(error),
);

apiInstance.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error: AxiosError) => {
    const status = error.response?.status;
    const originalRequest = error.config as RetryableRequestConfig | undefined;

    if (status === 401 && originalRequest) {
      const isRefreshRequest = originalRequest.url
        ?.toLowerCase()
        .includes("/refreshtoken");

      if (originalRequest._retry || isRefreshRequest) {
        expireSession();
        return Promise.reject(error);
      }

      const refreshToken = useAuth.getState().credentials?.refresh;
      if (!refreshToken) {
        expireSession();
        return Promise.reject(
          new Error("No refresh token is available for this session."),
        );
      }

      originalRequest._retry = true;

      try {
        await refreshSession(refreshToken);
        return apiInstance(originalRequest);
      } catch (refreshError) {
        return Promise.reject(refreshError);
      }
    }

    if (status === 403) redirectTo("/403");

    return Promise.reject(error);
  },
);

export const apiClient = new ClientApi(configuration, BASE_URL, apiInstance);
