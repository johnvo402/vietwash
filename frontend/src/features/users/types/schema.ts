import { z } from "zod";
import { AccountStatus, Gender } from "@/api/generated";

export const getUserFormSchema = (t: (key: string, params?: any) => string) => {
    const branchAccountSchema = z.object({
        branchId: z
            .number()
            .min(0, t("common.idRequired", { Entity: t("common.branch").replace(/^./, (c) => c.toUpperCase()) })),
        branchName: z
            .string()
            .min(1, t("common.nameRequired", { Entity: t("common.branch").replace(/^./, (c) => c.toUpperCase()) })),
    });

    return z.object({
        displayName: z
            .string()
            .min(1, t("common.entityRequired", { Entity: t("user.displayName") })),
        email: z.string().email(t("table.accessorKey.invalidEmail")),
        password: z.string().min(8, t("common.checkPassword")).optional(),
        phoneNumber: z.string().optional(),
        gender: z.nativeEnum(Gender).default(Gender.Other),
        birthday: z.date().optional(),
        avatar: z.instanceof(File).optional().nullable(),
        status: z.nativeEnum(AccountStatus).default(AccountStatus.Active),
        role: z
            .string()
            .min(1, t("common.entityRequired", { Entity: t("role.title") })),
        branchAccounts: z.array(branchAccountSchema).optional(),
    });
};

export type FormValues = z.infer<ReturnType<typeof getUserFormSchema>>;
