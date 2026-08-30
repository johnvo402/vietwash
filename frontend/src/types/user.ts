import { Gender } from "@/api/generated";

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
