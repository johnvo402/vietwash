export const getBranch = (id: number, user: any) => {
  return user?.branchAccounts.find((branch: any) => branch.branchId === id);
};
