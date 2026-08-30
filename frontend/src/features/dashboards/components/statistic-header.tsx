"use client";

import { useAuth } from "@/hooks/use-auth";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useTranslations } from "next-intl";

interface StatisticsHeaderProps {
  selectedBranch: string;
  setSelectedBranch: (branchId: string) => void;
}

export function StatisticsHeader({
  selectedBranch,
  setSelectedBranch,
}: StatisticsHeaderProps) {
  const { user } = useAuth();
  const t = useTranslations();
  return (
    <div className="flex items-center justify-between">
      <h1 className="text-3xl font-bold tracking-tight">
        {t("common.statistics").charAt(0).toUpperCase() +
          t("common.statistics").slice(1)}
      </h1>
      <Select value={selectedBranch} onValueChange={setSelectedBranch}>
        <SelectTrigger className="w-[200px]">
          <SelectValue placeholder="Select branch" />
        </SelectTrigger>
        <SelectContent>
          {user?.branchAccounts?.map((branch) => (
            <SelectItem key={branch.branchId} value={String(branch.branchId)}>
              {branch.branchName}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
