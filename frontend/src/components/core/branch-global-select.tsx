import { useAuth } from "@/hooks/use-auth";
import { useEffect } from "react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { AlertCircle } from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { useQueryClient } from "@tanstack/react-query";

const BranchGlobalSelect = () => {
  const { user, branchActive, setBranchActive } = useAuth();
  const queryClient = useQueryClient();

  useEffect(() => {
    if (
      user?.branchAccounts &&
      user.branchAccounts.length > 0 &&
      !branchActive
    ) {
      setBranchActive(user.branchAccounts[0]);
    }
  }, [user, branchActive, setBranchActive, queryClient]);

  const handleBranchChange = async (value: string) => {
    const selectedBranch =
      user?.branchAccounts.find(
        (branch) => String(branch.branchId) === value
      ) || null;

    await setBranchActive(selectedBranch);
    queryClient.invalidateQueries();
  };

  const currentBranch = user?.branchAccounts?.find(
    (branch) => branch.branchId === branchActive?.branchId
  );

  if (!user?.branchAccounts || user.branchAccounts.length === 0) {
    return (
      <Alert className="max-w-sm">
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>
          No branches available for your account.
        </AlertDescription>
      </Alert>
    );
  }

  return (
    <div className="flex items-center gap-2 mr-2">
      <Select
        value={String(
          branchActive?.branchId ?? user.branchAccounts[0].branchId
        )}
        onValueChange={handleBranchChange}
      >
        <SelectTrigger className="w-[200px]">
          <SelectValue placeholder="Select a branch">
            {currentBranch?.branchName || user.branchAccounts[0]?.branchName}
          </SelectValue>
        </SelectTrigger>
        <SelectContent>
          {user.branchAccounts.map((branch) => (
            <SelectItem key={branch.branchId} value={String(branch.branchId)}>
              <div className="flex items-center gap-2">
                <span>{branch.branchName}</span>
              </div>
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
};

export default BranchGlobalSelect;
