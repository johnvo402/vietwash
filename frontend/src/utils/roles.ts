const roleMap: Record<string, string[]> = {
  ADMIN: ["STAFF", "MANAGER"],
  MANAGER: ["STAFF"],
};

export function getRoleOptionsByRole(
  role: string
): { value: string; label: string }[] {
  const roles = roleMap[role] ?? [];
  return roles.map((r) => ({
    value: r,
    label: r,
  }));
}

export const Role = {
  ADMIN: "ADMIN",
  MANAGER: "MANAGER",
  STAFF: "STAFF",
} as const;

export type Role = (typeof Role)[keyof typeof Role];
