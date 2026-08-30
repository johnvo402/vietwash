"use client";

import type React from "react";

import { ProfileField } from "./profile-field";
import type { ReactNode } from "react";

interface ProfileInputProps {
  name: string;
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  label: string;
  icon: ReactNode;
  isEditing: boolean;
  type?: string;
  placeholder?: string;
}

export function ProfileInput({
  name,
  value,
  onChange,
  label,
  icon,
  isEditing,
  type = "text",
  placeholder,
}: ProfileInputProps) {
  return (
    <ProfileField label={label} icon={icon} isEditing={isEditing}>
      {isEditing ? (
        <input
          type={type}
          name={name}
          value={value || ""}
          onChange={onChange}
          placeholder={placeholder}
          className="w-full border-b border-blue-300 focus:outline-none focus:border-blue-500"
        />
      ) : (
        <p className="truncate">{value || "__"}</p>
      )}
    </ProfileField>
  );
}
