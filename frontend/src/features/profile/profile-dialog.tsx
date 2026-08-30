"use client";

import type React from "react";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import {
  Calendar,
  Phone,
  Mail,
  User,
  Lock,
  ArrowLeft,
  Pencil,
  UserCircle,
  Contact,
  Plus,
} from "lucide-react";
import { useProfileForm } from "@/features/profile/hooks/use-profile-form";
import { ProfileAvatar } from "@/features/profile/components/profile-avatar";
import { ProfileInput } from "@/features/profile/components/profile-input";
import { ProfileSelect } from "@/features/profile/components/profile-select";
import { PasswordChange } from "@/features/profile/components/password-change";
import { DialogDescription } from "@radix-ui/react-dialog";
import type { UserProfile } from "@/types/user";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ContactForm } from "./components/contact-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Gender } from "@/api/generated/api";
import { useTranslations } from "next-intl";

interface ProfileDialogProps {
  user: UserProfile;
  visible: boolean;
  onClose?: () => void;
  updateProfileMutation: any;
  changePasswordMutation: any;
}

export function ProfileDialog({
  user: initialUser,
  visible,
  onClose,
  updateProfileMutation,
  changePasswordMutation,
}: ProfileDialogProps) {
  const [open, setOpen] = useState(false);
  const [activeTab, setActiveTab] = useState("profile");
  const [avtFile, setAvtFile] = useState<File | null>(null);

  useEffect(() => {
    setOpen(visible || false);
  }, [visible]);

  const {
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
    handlePasswordChange,
    handleCancel,
    handleChangePassword,
    handleContactChange,
    otpState,
    handleSendOtp,
  } = useProfileForm({ initialUser });

  const handleSave = () => {
    updateProfileMutation.mutate({ user, avt: avtFile });
    setIsEditing(false);
    setOpen(false);
    onClose?.();
  };

  const handlePasswordUpdate = () => {
    try {
      changePasswordMutation.mutate({
        oldPassword: passwords.current,
        newPassword: passwords.new,
      });
    } finally {
      handleChangePassword();
    }
  };

  const handleOpenChange = (newOpen: boolean) => {
    setOpen(newOpen);
    if (!newOpen) {
      handleCancel();
      setActiveTab("profile");
      onClose?.();
    }
  };

  const genderOptions = [
    { value: Gender.Male, label: "Male" },
    { value: Gender.Female, label: "Female" },
    { value: Gender.Other, label: "Other" },
  ];
  const t = useTranslations();

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogDescription></DialogDescription>
        <DialogHeader>
          <DialogTitle className="text-blue-600">
            {showPasswordChange ? "Change Password" : "User Profile"}
          </DialogTitle>
        </DialogHeader>

        {!showPasswordChange && (
          <div className="pt-4 pb-6 border-b border-blue-100">
            <div className="flex flex-col md:flex-row md:items-center gap-4">
              <div className="flex items-center gap-2 columns-6">
                <ProfileAvatar
                  user={{
                    avtUrl: user.avtUrl,
                    displayName: user.displayName,
                  }}
                  setUser={setUser}
                  isEditing={isEditing}
                  onFileChange={(file) => setAvtFile(file)}
                />
                <div className="min-w-0 flex-shrink">
                  {isEditing ? (
                    <input
                      type="text"
                      name="displayName"
                      value={user.displayName}
                      onChange={handleInputChange}
                      className="text-xl font-bold border-b border-blue-300 focus:outline-none focus:border-blue-500 w-full"
                    />
                  ) : (
                    <h2 className="text-xl font-bold truncate">
                      {user.displayName}
                    </h2>
                  )}
                  <Badge
                    variant="secondary"
                    className="mt-1 bg-blue-100 text-blue-700"
                  >
                    {user.role}
                  </Badge>
                </div>
              </div>

              <div className="flex items-center gap-2 md:ml-auto">
                <Mail className="h-5 w-5 text-blue-500 flex-shrink-0" />
                <div className="min-w-0 flex-1">
                  <p className="text-sm text-blue-600">
                    {t("user.email.title")}
                  </p>
                  {isEditing ? (
                    <>
                      <Input
                        type="email"
                        name="email"
                        value={user.email}
                        onChange={handleInputChange}
                        className="border-b border-blue-300 focus:outline-none focus:border-blue-500 w-full"
                      />
                      {otpState.email.isOtpRequired && (
                        <div className="mt-2">
                          <Label>
                            {t("user.otpFor", {
                              entity: t("user.email.title").replace(/^./, (c) =>
                                c.toLowerCase()
                              ),
                            })}
                          </Label>
                          <div className="flex gap-2">
                            <Input
                              type="text"
                              placeholder={t("user.enterOTP")}
                              value={user.otpEmail || ""}
                              onChange={(e) =>
                                setUser((prev) => ({
                                  ...prev,
                                  otpEmail: e.target.value,
                                }))
                              }
                            />
                            <Button
                              onClick={() => handleSendOtp("email")}
                              disabled={otpState.email.isOtpSent}
                              className="bg-primary"
                            >
                              {otpState.email.isOtpSent
                                ? t("user.sendOTP")
                                : t("user.SendOTP")}
                            </Button>
                          </div>
                        </div>
                      )}
                    </>
                  ) : (
                    <p className="truncate">{user.email}</p>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}

        <div className="space-y-6 w-full py-6">
          {!showPasswordChange ? (
            <Tabs
              value={activeTab}
              onValueChange={setActiveTab}
              className="w-full"
            >
              <TabsList className="grid w-full grid-cols-2">
                <TabsTrigger
                  value="profile"
                  className="flex items-center gap-2"
                >
                  <UserCircle className="h-4 w-4" />
                  {t("user.profile").replace(/^./, (c) => c.toUpperCase())}
                </TabsTrigger>
                <TabsTrigger
                  value="contact"
                  className="flex items-center gap-2"
                >
                  <Contact className="h-4 w-4" />
                  {t("user.contact").replace(/^./, (c) => c.toUpperCase())}
                </TabsTrigger>
              </TabsList>
              <TabsContent value="profile" className="space-y-6 mt-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <ProfileInput
                      name="phoneNumber"
                      value={user.phoneNumber}
                      onChange={handleInputChange}
                      label={t("user.phoneNumber.title")}
                      icon={<Phone className="h-5 w-5" />}
                      isEditing={isEditing}
                      type="tel"
                    />
                    {isEditing && otpState.phone.isOtpRequired && (
                      <div className="mt-2">
                        <Label>
                          {t("user.otpFor", {
                            entity: t("user.phoneNumber.title").toLowerCase(),
                          })}
                        </Label>
                        <div className="flex gap-2">
                          <Input
                            type="text"
                            placeholder={t("user.enterOTP")}
                            value={user.otpPhone || ""}
                            onChange={(e) =>
                              setUser((prev) => ({
                                ...prev,
                                otpPhone: e.target.value,
                              }))
                            }
                          />
                          <Button
                            onClick={() => handleSendOtp("phone")}
                            disabled={otpState.phone.isOtpSent}
                            className="bg-blue-600 hover:bg-blue-700"
                          >
                            {otpState.phone.isOtpSent ? "OTP Sent" : "Send OTP"}
                          </Button>
                        </div>
                      </div>
                    )}
                  </div>

                  <ProfileInput
                    name="birthDay"
                    value={user.birthDay}
                    onChange={handleInputChange}
                    label={t("user.dateOfBirth")}
                    icon={<Calendar className="h-5 w-5" />}
                    isEditing={isEditing}
                    type="date"
                  />

                  <ProfileSelect
                    name="gender"
                    value={user.gender ?? Gender.Other}
                    onChange={handleSelectChange}
                    label={t("user.gender.title")}
                    icon={<User className="h-5 w-5" />}
                    isEditing={isEditing}
                    options={genderOptions}
                    displayValue={(value) =>
                      value === Gender.Male
                        ? t("user.gender.Male")
                        : value === Gender.Female
                          ? t("user.gender.Female")
                          : t("user.gender.Other")
                    }
                  />
                </div>
              </TabsContent>
              <TabsContent value="contact" className="space-y-6 mt-6">
                <div className="flex items-center justify-between">
                  <h3 className="text-lg font-medium text-blue-700">
                    {t("user.contactInfo")}
                  </h3>
                </div>
                <div className="mt-4">
                  <ContactForm
                    contact={user.accountContact}
                    isEditing={isEditing}
                    onChange={handleContactChange}
                  />
                </div>
              </TabsContent>
            </Tabs>
          ) : (
            <PasswordChange
              passwords={passwords}
              onChange={handlePasswordChange}
            />
          )}
        </div>

        <div className="flex flex-wrap gap-2 justify-end mt-6 pt-6 border-t border-blue-100">
          {isEditing ? (
            <>
              <Button
                variant="outline"
                className="border-blue-300 text-blue-700 hover:bg-blue-50"
                onClick={handleCancel}
              >
                {t("common.cancel")}
              </Button>
              <Button
                className="bg-blue-600 hover:bg-blue-700"
                onClick={handleSave}
              >
                {t("common.saveChanges")}
              </Button>
            </>
          ) : showPasswordChange ? (
            <>
              <Button
                variant="outline"
                className="border-blue-300 text-blue-700 hover:bg-blue-50"
                onClick={() => {
                  setShowPasswordChange(false);
                }}
              >
                <ArrowLeft className="h-4 w-4 mr-2" />
                {t("common.backTo", {
                  entity: t("user.profile").replace(/^./, (c) =>
                    c.toUpperCase()
                  ),
                })}
              </Button>
              <Button
                className="bg-green-600 hover:bg-green-700"
                onClick={() => handlePasswordUpdate()}
                disabled={!isPasswordValid}
              >
                {t("user.updatePassword")}
              </Button>
            </>
          ) : (
            <>
              <Button
                variant="outline"
                className="border-blue-300 text-blue-700 hover:bg-blue-50"
                onClick={() => setIsEditing(true)}
              >
                <Pencil className="h-4 w-4 mr-2" />
                {t("common.edit")} {t("user.profile")}
              </Button>
              <Button
                className="bg-green-600 hover:bg-green-700"
                onClick={() => setShowPasswordChange(true)}
              >
                <Lock className="h-4 w-4 mr-2" />
                {t("user.changePassword")}
              </Button>
            </>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
