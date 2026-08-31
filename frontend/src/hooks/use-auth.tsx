"use client";

import { BranchAccount, UserProfile } from "@/types/user";
import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

export interface Credentials {
  refresh: string;
  token: string;
  accessTokenExpiredIn: number;
}

export interface CredentialsInput {
  refresh?: string | null;
  token?: string | null;
  accessTokenExpiredIn?: number | null;
}

export const isValidCredentials = (
  credentials: CredentialsInput | null | undefined,
): credentials is Credentials =>
  Boolean(
    credentials?.token &&
      credentials.refresh &&
      typeof credentials.accessTokenExpiredIn === "number" &&
      Number.isFinite(credentials.accessTokenExpiredIn),
  );

export interface AuthState {
  credentials: Credentials | null;
  user: UserProfile | null;
  isAuthenticated: boolean;
  branchActive: BranchAccount | null;
  login: (credentials: CredentialsInput | null) => void;
  logout: () => void;
  updateUser: (user: UserProfile) => void;
  setBranchActive: (branch: BranchAccount | null) => void;
}

const loggedOutState = {
  credentials: null,
  user: null,
  isAuthenticated: false,
  branchActive: null,
} satisfies Pick<
  AuthState,
  "credentials" | "user" | "isAuthenticated" | "branchActive"
>;

if (typeof window !== "undefined") {
  // Remove credentials written by the previous localStorage-based implementation.
  window.localStorage.removeItem("auth-storage");
}

export const useAuth = create<AuthState>()(
  persist(
    (set) => ({
      ...loggedOutState,

      login: (credentials) =>
        set(() =>
          isValidCredentials(credentials)
            ? { credentials, isAuthenticated: true }
            : loggedOutState,
        ),

      logout: () => set(() => loggedOutState),

      updateUser: (newUserData) =>
        set((state) => ({
          user: state.user ? { ...state.user, ...newUserData } : newUserData,
        })),

      setBranchActive: (branch) => set(() => ({ branchActive: branch })),
    }),
    {
      name: "vietwash-auth-session",
      storage: createJSONStorage(() => sessionStorage),
      partialize: ({ credentials, user, isAuthenticated, branchActive }) => ({
        credentials,
        user,
        isAuthenticated,
        branchActive,
      }),
    },
  ),
);
