"use client";

import type React from "react";
import { useState } from "react";
import type {
  UserProfile,
  PasswordChangeData,
  AccountContact,
} from "@/types/user";
import { apiClient } from "@/api/client";
import { OtpType } from "@/api/generated";

interface UseProfileFormProps {
  initialUser: UserProfile;
}

export function useProfileForm({ initialUser }: UseProfileFormProps) {
  const normalizedInitialUser: UserProfile = {
    ...initialUser,
    accountContact: initialUser.accountContact ?? {
      address: "",
      commune: "",
      district: "",
      province: "",
      communeCode: "",
      districtCode: "",
      provinceCode: "",
      street: "",
    },
    otpEmail: initialUser.otpEmail || "",
    otpPhone: initialUser.otpPhone || "",
  };

  const [isEditing, setIsEditing] = useState(false);
  const [user, setUser] = useState<UserProfile>(normalizedInitialUser);
  const [showPasswordChange, setShowPasswordChange] = useState(false);
  const [passwords, setPasswords] = useState<PasswordChangeData>({
    current: "",
    new: "",
    confirm: "",
  });
  const [otpState, setOtpState] = useState({
    phone: {
      isOtpRequired: false,
      newValue: "",
      isOtpSent: false,
    },
    email: {
      isOtpRequired: false,
      newValue: "",
      isOtpSent: false,
    },
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setUser((prev) => ({ ...prev, [name]: value }));

    if (name === "phoneNumber" || name === "email") {
      const field = name === "phoneNumber" ? "phone" : "email";
      setOtpState((prev) => ({
        ...prev,
        [field]: {
          ...prev[field],
          isOtpRequired: value !== normalizedInitialUser[name],
          newValue: value,
          isOtpSent: false,
        },
      }));
      setUser((prev) => ({
        ...prev,
        [name === "phoneNumber" ? "otpPhone" : "otpEmail"]: "",
      }));
    }
  };

  const handleSelectChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const { name, value } = e.target;
    setUser((prev) => ({ ...prev, [name]: value }));
  };

  const handleContactChange = (field: keyof AccountContact, value: string) => {
    setUser((prev) => {
      const prevContact = prev.accountContact ?? {
        address: "",
        commune: "",
        district: "",
        province: "",
        communeCode: "",
        districtCode: "",
        provinceCode: "",
        street: "",
      };

      // Tạo contact mới đã cập nhật field
      const updatedContact: AccountContact = {
        ...prevContact,
        [field]: value,
      };

      const addressParts = [
        updatedContact.street,
        updatedContact.commune,
        updatedContact.district,
        updatedContact.province,
      ];

      const address = addressParts.filter(Boolean).join(", ");

      return {
        ...prev,
        accountContact: {
          ...updatedContact,
          address,
        },
      };
    });
  };

  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setPasswords((prev) => ({ ...prev, [name]: value }));
  };

  const handleSendOtp = async (field: "phone" | "email") => {
    try {
      await apiClient.authApiAccountsRequestOtpPost({
        to: otpState[field].newValue,
        type: field === "phone" ? OtpType.Phone : OtpType.Email,
      });
      setOtpState((prev) => ({
        ...prev,
        [field]: { ...prev[field], isOtpSent: true },
      }));
      return true;
    } catch (error) {
      console.error(`Failed to send OTP for ${field}:`, error);
      return false;
    }
  };

  const handleCancel = () => {
    setUser(normalizedInitialUser);
    setIsEditing(false);
    setShowPasswordChange(false);
    setPasswords({ current: "", new: "", confirm: "" });
    setOtpState({
      phone: {
        isOtpRequired: false,
        newValue: "",
        isOtpSent: false,
      },
      email: {
        isOtpRequired: false,
        newValue: "",
        isOtpSent: false,
      },
    });
  };

  const handleChangePassword = () => {
    console.log("Changing password:", passwords);
    setShowPasswordChange(false);
    setPasswords({ current: "", new: "", confirm: "" });
  };

  const isPasswordValid =
    passwords.current && passwords.new && passwords.new === passwords.confirm;

  return {
    user,
    setUser,
    isEditing,
    setIsEditing,
    showPasswordChange,
    setShowPasswordChange,
    passwords,
    isPasswordValid,
    handleInputChange,
    handleSelectChange,
    handleContactChange,
    handlePasswordChange,
    handleSendOtp,
    handleCancel,
    handleChangePassword,
    otpState,
    setOtpState,
  };
}
