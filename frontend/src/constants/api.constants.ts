export const API_ENDPOINTS = {
  AUTH: {
    BASE: "/auth/api/v1/auth",
    LOGIN: "/auth/api/v1/auth/login",
    REGISTER: "/auth/api/v1/auth/register",
    ME: "/auth/api/v1/auth/me",
    REFRESH_TOKEN: "/auth/api/v1/auth/refresh-token",
  },
  PRODUCTS: {
    BASE: "/product/api/v1/products",
    DETAIL: (id: string) => `/product/api/v1/products/${id}`,
  },
  USERS: {
    BASE: "/users",
    DETAIL: (id: string) => `/users/${id}`,
  },
} as const;
