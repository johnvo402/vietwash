import { Gender, GetAccountProfileResponse } from "@/api/generated";

export interface UserProfile {
  id: number;
  displayName: string;
  email: string;
  phoneNumber: string;
  birthDay: string;
  gender: Gender | undefined;
  avtUrl: string | null;
  role: string;
  accountContact?: AccountContact | undefined;
  branchAccounts: BranchAccount[];
  otpEmail?: string;
  otpPhone?: string;
}

export interface AccountContact {
  address: string;
  commune: string;
  district: string;
  province: string;
  communeCode: string;
  districtCode: string;
  provinceCode: string;
  street: string;
}

export interface BranchAccount {
  branchId: number;
  branchName: string;
}

export interface PasswordChangeData {
  current: string;
  new: string;
  confirm: string;
}

export const toUserProfile = (
  profile: GetAccountProfileResponse | null | undefined,
): UserProfile | null => {
  if (
    typeof profile?.id !== "number" ||
    typeof profile.displayName !== "string" ||
    typeof profile.email !== "string" ||
    typeof profile.phoneNumber !== "string" ||
    typeof profile.birthDay !== "string" ||
    typeof profile.role !== "string"
  ) {
    return null;
  }

  const branchAccounts = (profile.branchAccounts ?? []).flatMap((branch) =>
    typeof branch.branchId === "number" && typeof branch.branchName === "string"
      ? [{ branchId: branch.branchId, branchName: branch.branchName }]
      : [],
  );

  const contact = profile.accountContact;
  const accountContact =
    contact &&
    typeof contact.address === "string" &&
    typeof contact.commune === "string" &&
    typeof contact.district === "string" &&
    typeof contact.province === "string" &&
    typeof contact.communeCode === "string" &&
    typeof contact.districtCode === "string" &&
    typeof contact.provinceCode === "string" &&
    typeof contact.street === "string"
      ? {
          address: contact.address,
          commune: contact.commune,
          district: contact.district,
          province: contact.province,
          communeCode: contact.communeCode,
          districtCode: contact.districtCode,
          provinceCode: contact.provinceCode,
          street: contact.street,
        }
      : undefined;

  return {
    id: profile.id,
    displayName: profile.displayName,
    email: profile.email,
    phoneNumber: profile.phoneNumber,
    birthDay: profile.birthDay,
    gender: profile.gender,
    avtUrl: profile.avtUrl ?? null,
    role: profile.role,
    accountContact,
    branchAccounts,
  };
};
