"use client";

import { useState, useEffect, useRef, useMemo, useCallback } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { format } from "date-fns";
import { Camera, Loader2, Undo2, X } from "lucide-react";
import Image from "next/image";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { useTranslations } from "next-intl";
import {
  AccountStatus,
  Gender,
  GetAccountDetailResponse,
  ActivationStatus,
} from "@/api/generated";
import { useParams } from "next/navigation";
import { useAuth } from "@/hooks/use-auth";
import MultiSelect from "@/components/core/selects/multi-select";
import { getRoleOptionsByRole } from "@/utils/roles";
import { useStringUtil } from "@/lib/stringUtil";

// Form schema with validation
const branchAccountSchema = (t: any) =>
  z.object({
    branchId: z.number().min(
      0,
      t("common.idRequired", {
        Entity: t("common.branch"),
      }),
    ),
    branchName: z.string().min(
      1,
      t("common.nameRequired", {
        Entity: t("common.branch"),
      }),
    ),
  });

const strongPasswordRegex = /^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])\S{8,}$/;

const formSchema = (t: any) =>
  z.object({
    displayName: z.string().min(
      1,
      t("common.entityRequired", {
        Entity: t("user.displayName.title").toLowerCase(),
      }),
    ),
    email: z.string().email(t("table.accessorKey.invalidEmail")),
    password: z
      .string()
      .regex(
        strongPasswordRegex,
        t("common.checkPasswordStrong"), // Ví dụ: "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường và số"
      )
      .optional(),
    phoneNumber: z.string().optional(),
    gender: z.nativeEnum(Gender).default(Gender.Other),
    birthday: z.date().optional(),
    avatar: z.instanceof(File).optional().nullable(),
    status: z.nativeEnum(AccountStatus).default(AccountStatus.Active),
    role: z
      .string()
      .min(1, t("common.idRequired", { Entity: t("role.title") })),
    branchAccounts: z.array(branchAccountSchema(t)).optional(),
  });

export type FormValues = z.infer<ReturnType<typeof formSchema>>;

interface PageProps {
  open: boolean;
  onClose: () => void;
  onCreateUser?: (data: { user: FormData; avt?: File }) => Promise<void>;
  onUpdateUser?: (
    data: FormValues & { id: number },
    formData: FormData,
    avt?: File,
  ) => Promise<void>;
  user?: GetAccountDetailResponse;
}

export function CreateUserDialog({
  open,
  onClose,
  onCreateUser,
  onUpdateUser,
  user: propUser,
}: PageProps) {
  const t = useTranslations();
  const { textByLang } = useStringUtil();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const { id } = useParams();
  const userId = id ? Number(id) : undefined;
  const { user } = useAuth();
  const isEditMode = !!userId || !!propUser;
  const ROLES = useMemo(() => getRoleOptionsByRole(user?.role!), [user?.role]);

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema(t)),
    defaultValues: {
      displayName: "",
      phoneNumber: "",
      birthday: undefined,
      avatar: null,
      email: "",
      password: "",
      gender: Gender.Other,
      status: AccountStatus.Active,
      role: "",
      branchAccounts: [],
    },
  });

  // Populate form with fetched user data or prop user data
  useEffect(() => {
    if (isEditMode && propUser) {
      const newValues = {
        displayName: propUser.displayName ?? "",
        phoneNumber: propUser.phoneNumber ?? "",
        birthday: propUser.birthDay ? new Date(propUser.birthDay) : undefined,
        avatar: null,
        email: propUser.email ?? "",
        password: undefined,
        gender: propUser.gender ?? Gender.Other,
        status: propUser.status ?? ActivationStatus.Active,
        role: ROLES.some((role: any) => role.value === propUser.role)
          ? (propUser.role ?? "")
          : "",
        branchAccounts: (propUser.branchAccounts as any) ?? [],
      };

      form.reset(newValues);
      setPreviewUrl(propUser.avtUrl || null);
    } else {
      form.reset({
        displayName: "",
        phoneNumber: "",
        birthday: undefined,
        avatar: null,
        email: "",
        password: "",
        gender: Gender.Other,
        status: AccountStatus.Active,
        role: "",
        branchAccounts: [],
      });
      setPreviewUrl(null);
    }
  }, [propUser, isEditMode, ROLES, form]);

  // Clean up preview URL to avoid memory leaks
  useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [previewUrl]);

  const getInitials = useCallback((name: string) => {
    if (!name) return "";
    const names = name.trim().split(" ");
    return names
      .map((n) => n.charAt(0))
      .join("")
      .substring(0, 2)
      .toUpperCase();
  }, []);

  const handleAvatarClick = useCallback(() => {
    if (fileInputRef.current) {
      fileInputRef.current.click();
    }
  }, []);

  const handleFileChange = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0] || null;
      if (file) {
        const validTypes = ["image/jpeg", "image/png", "image/webp"];
        if (!validTypes.includes(file.type)) {
          form.setError("avatar", { message: t("user.avatar.invalidType") });
          return;
        }
        if (file.size > 5 * 1024 * 1024) {
          form.setError("avatar", { message: t("user.avatar.maxSize") });
          return;
        }

        setIsUploading(true);
        try {
          form.setValue("avatar", file);
          const newPreviewUrl = URL.createObjectURL(file);
          setPreviewUrl(newPreviewUrl);
        } catch (error) {
          form.setError("avatar", { message: t("common.error") });
        } finally {
          setIsUploading(false);
        }
      } else {
        form.setValue("avatar", null);
        setPreviewUrl(null);
      }
    },
    [form, t],
  );

  const handleClearAvatar = useCallback(() => {
    form.setValue("avatar", null);
    setPreviewUrl(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  }, [form]);

  const onSubmit = useCallback(
    async (data: FormValues) => {
      setIsSubmitting(true);
      try {
        const formData = new FormData();
        formData.append("displayName", data.displayName);
        formData.append("email", data.email);
        if (data.password) formData.append("password", data.password);
        if (data.phoneNumber) formData.append("phoneNumber", data.phoneNumber);
        formData.append("gender", data.gender);
        if (data.birthday)
          formData.append("birthday", format(data.birthday, "yyyy-MM-dd"));
        formData.append("status", String(data.status));
        formData.append("role", data.role);
        if (data.branchAccounts) {
          formData.append(
            "branchAccounts",
            JSON.stringify(data.branchAccounts),
          );
        }

        if (isEditMode) {
          const id = userId || propUser?.id!;
          if (!data.avatar && propUser?.avtUrl) {
            formData.append("removeAvatar", "true");
          }
          await onUpdateUser?.(
            { ...data, id },
            formData,
            data.avatar || undefined,
          );
        } else {
          await onCreateUser?.({
            user: formData,
            avt: data.avatar || undefined,
          });
        }

        // Only close the dialog and clear the form on successful submission
      } catch (err: any) {
        console.error("Error creating user:", err);

        // Nếu API trả về dạng ValidationError
        if (err.response?.data?.invalidParams) {
          const invalidParams = err.response.data.invalidParams;
          invalidParams.forEach((param: any) => {
            // Đảm bảo propertyName match với field name trong form schema (case-insensitive)
            const fieldName =
              param.propertyName.charAt(0).toLowerCase() +
              param.propertyName.slice(1);

            // Lấy message tiếng Việt nếu có, fallback sang tiếng Anh
            const message =
              textByLang(JSON.parse(param.reasons)) || "Invalid value";

            form.setError(fieldName as keyof FormValues, {
              type: "server",
              message,
            });
          });
        } else {
          // Lỗi khác (server error, network...)
          form.setError("root", { type: "server", message: t("common.error") });
        }
      } finally {
        setIsSubmitting(false);
        handleClose();
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [form, t, isEditMode, userId, propUser, onUpdateUser, onCreateUser],
  );

  const handleClose = useCallback(() => {
    form.reset({
      displayName: "",
      phoneNumber: "",
      birthday: undefined,
      avatar: null,
      email: "",
      password: "",
      gender: Gender.Other,
      status: AccountStatus.Active,
      role: "",
      branchAccounts: [],
    });
    setPreviewUrl(null);
    onClose();
  }, [form, onClose]);

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[80vw] max-h-[90vh] overflow-y-auto p-0">
        <DialogHeader className="sticky top-0 z-10 bg-primary p-6 text-background">
          <DialogTitle>
            {isEditMode
              ? t("dialog.edit.title", { entity: t("common.user") })
              : t("dialog.create.title", { entity: t("common.user") })}
          </DialogTitle>
          <DialogDescription className="text-background">
            {isEditMode
              ? t("dialog.edit.description", { entity: t("common.user") })
              : t("dialog.create.description", { entity: t("common.user") })}
          </DialogDescription>
          <Button
            variant="ghost"
            size="icon"
            className="absolute right-4 top-4"
            onClick={handleClose}
          >
            <Undo2 className="h-4 w-4" />
            <span className="sr-only">{t("common.close")}</span>
          </Button>
        </DialogHeader>
        <div className="p-6">
          <Form {...form}>
            <form
              id="formUser"
              onSubmit={form.handleSubmit(onSubmit)}
              className="space-y-6"
            >
              <FormField
                control={form.control}
                name="avatar"
                render={({ field }) => (
                  <FormItem className="flex flex-col items-center">
                    <FormControl>
                      <div className="relative">
                        <Avatar
                          className="h-20 w-20 cursor-pointer ring-2 ring-blue-400"
                          onClick={handleAvatarClick}
                        >
                          {previewUrl || propUser?.avtUrl ? (
                            <Image
                              src={previewUrl || propUser?.avtUrl || ""}
                              alt={
                                form.getValues("displayName") ||
                                t("user.avatar.title")
                              }
                              width={80}
                              height={80}
                              className="rounded-full object-cover"
                            />
                          ) : (
                            <AvatarFallback className="text-lg bg-blue-100 text-blue-600">
                              {getInitials(form.getValues("displayName"))}
                            </AvatarFallback>
                          )}
                          <div className="absolute inset-0 bg-blue-500/40 rounded-full flex items-center justify-center text-white">
                            {isUploading ? (
                              <div className="h-10 w-10 rounded-full border-2 border-white border-t-transparent animate-spin"></div>
                            ) : (
                              <Camera className="h-6 w-6" />
                            )}
                          </div>
                        </Avatar>
                        <Input
                          type="file"
                          ref={fileInputRef}
                          className="hidden"
                          accept="image/jpeg,image/png,image/webp"
                          onChange={handleFileChange}
                        />
                      </div>
                    </FormControl>
                    {(form.getValues("avatar") || propUser?.avtUrl) && (
                      <Button
                        type="button"
                        variant="destructive"
                        size="sm"
                        onClick={handleClearAvatar}
                        className="mt-2 h-11 w-11"
                        aria-label={t("common.removeItem", {
                          item: t("common.image"),
                        })}
                      >
                        <X className="h-4 w-4" />
                      </Button>
                    )}
                    <FormMessage />
                  </FormItem>
                )}
              />
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="displayName"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t("user.displayName.title")}</FormLabel>
                      <FormControl>
                        <Input
                          placeholder={t("user.placeholder", {
                            entity: t("user.displayName.title").toLowerCase(),
                          })}
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="email"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t("user.email.title")}</FormLabel>
                      <FormControl>
                        <Input
                          type="email"
                          placeholder={t("user.placeholder", {
                            entity: t("user.email.title").toLowerCase(),
                          })}
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                {!isEditMode && (
                  <FormField
                    control={form.control}
                    name="password"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>{t("user.password.title")}</FormLabel>
                        <FormControl>
                          <Input
                            type="password"
                            placeholder={t("user.placeholder", {
                              entity: t("user.password.title").toLowerCase(),
                            })}
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                )}
                <FormField
                  control={form.control}
                  name="phoneNumber"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t("user.phoneNumber.title")}</FormLabel>
                      <FormControl>
                        <Input
                          placeholder={t("user.phoneNumberPlaceholder")}
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="gender"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t("user.gender.title")}</FormLabel>
                      <Select
                        onValueChange={(value) => field.onChange(value)}
                        defaultValue={field.value}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue
                              placeholder={t("common.placeholderSelect", {
                                entity: t("user.gender.title").toLowerCase(),
                              })}
                            />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {Object.entries(Gender).map(([key, value]) => (
                            <SelectItem key={value} value={key}>
                              {t(`user.gender.${key}`)}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="birthday"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t("user.dateOfBirth")}</FormLabel>
                      <FormControl>
                        <Input
                          type="date"
                          value={
                            field.value ? format(field.value, "yyyy-MM-dd") : ""
                          }
                          onChange={(e) => {
                            const date = e.target.value
                              ? new Date(e.target.value)
                              : undefined;
                            if (date && !isNaN(date.getTime())) {
                              field.onChange(date);
                            } else {
                              field.onChange(undefined);
                            }
                          }}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="status"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t("common.status.title")}</FormLabel>
                      <Select
                        onValueChange={(value) => field.onChange(value)}
                        defaultValue={String(field.value)}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue
                              placeholder={t("common.placeholderSelect", {
                                entity: t("common.status.title").toLowerCase(),
                              })}
                            />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {Object.entries(ActivationStatus).map(
                            ([key, value]) => (
                              <SelectItem key={value} value={String(value)}>
                                {t(`common.status.${key.toLowerCase()}`, {
                                  defaultValue: key,
                                })}
                              </SelectItem>
                            ),
                          )}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="role"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t("role.title")}</FormLabel>
                      <Select
                        onValueChange={field.onChange}
                        defaultValue={field.value}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue
                              placeholder={t("common.placeholderSelect", {
                                entity: t("role.title").toLowerCase(),
                              })}
                            />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {ROLES.map((role) => (
                            <SelectItem key={role.value} value={role.value}>
                              {t(`role.${role.label}`)}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="branchAccounts"
                  render={({ field }) => (
                    <FormItem>
                      <FormControl>
                        <MultiSelect
                          label={t("common.branch")}
                          placeholder={t("common.placeholderSelect", {
                            entity: t("common.branch").toLowerCase(),
                          })}
                          options={
                            user?.branchAccounts.map((branch) => ({
                              value: String(branch.branchId),
                              label: branch.branchName,
                            })) || []
                          }
                          value={
                            field.value?.map((branch) => ({
                              value: String(branch.branchId),
                              label: branch.branchName,
                            })) || []
                          }
                          onChange={(selectedOptions) => {
                            const updatedBranches = selectedOptions.map(
                              (option) => ({
                                branchId: Number(option.value),
                                branchName: option.label,
                              }),
                            );
                            field.onChange(updatedBranches);
                          }}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            </form>
          </Form>
        </div>
        <DialogFooter className="sticky bottom-0 z-10 p-6 bg-background border-t border-secondary">
          <Button type="button" variant="outline" onClick={handleClose}>
            {t("common.cancel")}
          </Button>
          <Button form="formUser" type="submit" disabled={isSubmitting}>
            {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isEditMode ? t("common.update") : t("common.create")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
