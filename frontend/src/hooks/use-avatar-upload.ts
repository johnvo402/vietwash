"use client";

import type React from "react";

import { useState, useRef } from "react";
import type { UserProfile } from "@/types/user";
import { useTranslations } from "next-intl";

interface UseAvatarUploadProps {
  setUser: React.Dispatch<React.SetStateAction<UserProfile>>;
  isEditing: boolean;
  onFileChange?: (file: File) => void; // 👈 THÊM DÒNG NÀY
}

export function useAvatarUpload({
  setUser,
  isEditing,
  onFileChange,
}: UseAvatarUploadProps) {
  const [isUploading, setIsUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const t = useTranslations();
  const handleAvatarClick = () => {
    if (isEditing && fileInputRef.current) {
      fileInputRef.current.click();
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const validTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];
    if (!validTypes.includes(file.type)) {
      alert(t("user.avatar.invalidType"));
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      alert(t("user.avatar.maxSize"));
      return;
    }

    const previewUrl = URL.createObjectURL(file);
    setIsUploading(true);

    // 👇 Gọi callback truyền file ra ngoài
    onFileChange?.(file);

    setTimeout(() => {
      setUser((prev) => ({ ...prev, avtUrl: previewUrl }));
      setIsUploading(false);
    }, 1500);
  };

  return {
    isUploading,
    fileInputRef,
    handleAvatarClick,
    handleFileChange,
  };
}
