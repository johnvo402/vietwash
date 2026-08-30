"use client";

import type React from "react";

import type { PasswordChangeData } from "@/types/user";
import { Lock } from "lucide-react";
import { useTransition } from "react";
import { useTranslations } from "next-intl";

interface PasswordChangeProps {
  passwords: PasswordChangeData;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

export function PasswordChange({ passwords, onChange }: PasswordChangeProps) {
  const t = useTranslations();
  return (
    <div className="w-full max-w-md mx-auto">
      <div className="flex justify-center mb-6">
        <div className="h-16 w-16 rounded-full bg-green-100 flex items-center justify-center">
          <Lock className="h-8 w-8 text-green-600" />
        </div>
      </div>

      <p className="text-center text-gray-600 mb-8">
        {t("user.descriptionChangePassword")}
      </p>

      <div className="space-y-4">
        <div>
          <label className="text-sm text-blue-600 block mb-1">
            {t("user.currentPassword")}
          </label>
          <input
            type="password"
            name="current"
            value={passwords.current}
            onChange={onChange}
            className="w-full border-b border-blue-300 focus:outline-none focus:border-blue-500 py-2"
            placeholder={t("user.placeholder",{entity:t("user.currentPassword").toLowerCase()})}
          />
        </div>
        <div>
          <label className="text-sm text-blue-600 block mb-1">
            {t("user.newPassword")}
          </label>
          <input
            type="password"
            name="new"
            value={passwords.new}
            onChange={onChange}
            className="w-full border-b border-blue-300 focus:outline-none focus:border-blue-500 py-2"
            placeholder={t("user.placeholder",{entity:t("user.newPassword").toLowerCase()})}
          />
        </div>
        <div>
          <label className="text-sm text-blue-600 block mb-1">
            {t("user.confirmNewPassword.title")}
          </label>
          <input
            type="password"
            name="confirm"
            value={passwords.confirm}
            onChange={onChange}
            className="w-full border-b border-blue-300 focus:outline-none focus:border-blue-500 py-2"
            placeholder={t("user.confirmNewPassword.placeholder")}
          />
        </div>
      </div>
    </div>
  );
}
