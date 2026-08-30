export interface PropsQuery {
  filter?: any | null;
  sort?: string;
  searchKeywords?: string | null;
  searchTarget?: string[] | null;
}

export type Option = {
  value: string;
  label: React.ReactNode;
};
