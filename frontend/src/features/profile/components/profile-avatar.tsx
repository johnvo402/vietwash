"use client";

import type React from "react";

import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Camera, Upload } from "lucide-react";
import { getInitials } from "@/utils/format";
import { useAvatarUpload } from "@/hooks/use-avatar-upload";
import type { UserProfile } from "@/types/user";
import Image from "next/image";

interface ProfileAvatarProps {
  user: {
    avtUrl: string | null;
    displayName: string;
  };
  setUser: React.Dispatch<React.SetStateAction<UserProfile>>;
  isEditing: boolean;
  onFileChange?: (file: File) => void;
}

export function ProfileAvatar({
  user,
  setUser,
  isEditing,
  onFileChange,
}: ProfileAvatarProps) {
  const { isUploading, fileInputRef, handleAvatarClick, handleFileChange } =
    useAvatarUpload({ setUser, isEditing, onFileChange });

  return (
    <div className="relative">
      <Avatar
        className={`h-20 w-20 ${isEditing ? "cursor-pointer ring-2 ring-blue-400" : ""}`}
        onClick={handleAvatarClick}
      >
        {user?.avtUrl ? (
          <Image
            src={user.avtUrl}
            alt="Avatar"
            className="h-8 w-8 rounded-full object-cover"
            fill
            style={{ objectFit: "contain" }}
          />
        ) : (
          <AvatarFallback className="text-lg bg-blue-100 text-blue-600">
            {getInitials(user.displayName)}
          </AvatarFallback>
        )}

        {isEditing && (
          <div className="absolute inset-0 bg-blue-500/40 rounded-full flex items-center justify-center text-white">
            {isUploading ? (
              <div className="h-10 w-10 rounded-full border-2 border-white border-t-transparent animate-spin"></div>
            ) : (
              <Camera className="h-6 w-6" />
            )}
          </div>
        )}
      </Avatar>
      <input
        type="file"
        ref={fileInputRef}
        className="hidden"
        accept="image/*"
        onChange={handleFileChange}
      />
      {isEditing && (
        <div className="absolute -bottom-1 -right-1 bg-blue-600 text-white p-1 rounded-full">
          <Upload className="h-4 w-4" />
        </div>
      )}
    </div>
  );
}
