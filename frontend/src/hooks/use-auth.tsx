import { BranchAccount, UserProfile } from "@/types/user";
import { create } from "zustand";
import { persist } from "zustand/middleware";

export interface Credentials {
  refresh: string | null | undefined;
  token: string | null | undefined;
  accessTokenExpiredIn: number | null | undefined;
}

export interface AuthState {
  credentials: Credentials | null;
  user: UserProfile | null;
  isAuthenticated: boolean;
  branchActive: BranchAccount | null;
  // Actions
  login: (credentials: Credentials | null) => void;
  logout: () => void;
  updateUser: (user: any) => void;
  setBranchActive: (branch: BranchAccount | null) => void;
}

export const useAuth = create<AuthState>()(
  persist(
    (set) => ({
      credentials: null,
      user: null,
      isAuthenticated: false,
      branchActive: null,

      login: (credentials: Credentials | null) =>
        set(() => ({
          credentials,
          isAuthenticated: true,
        })),

      logout: () =>
        set(() => ({
          credentials: null,
          isAuthenticated: false,
          user: null,
          branchActiveId: null,
        })),

      updateUser: (newUserData: any) =>
        set((state) => ({
          ...state,
          user: state.user ? { ...state.user, ...newUserData } : newUserData,
        })),

      setBranchActive: (branch: BranchAccount | null) =>
        set(() => ({
          branchActive: branch,
        })),
    }),
    { name: "auth-storage" }
  )
);
