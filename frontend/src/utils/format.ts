export const formatPriceVN = (price: number = 0) => {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(price);
};

// Preserve fractional monetary values returned by the server, without recalculating totals.
export const formatOrderMoney = (price: number = 0) =>
  new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    minimumFractionDigits: 0,
    maximumFractionDigits: 20,
  }).format(price);
export const formatNumberVN = (num: number) => {
  return new Intl.NumberFormat("vi-VN", {
    maximumFractionDigits: 0,
  }).format(num);
};
export const parseNumberVN = (str: string): number => {
  return Number(str.replace(/\./g, "").replace(/,/g, "."));
};
export const getInitials = (name: string): string => {
  return name
    .split(" ")
    .map((part) => part[0])
    .join("")
    .toUpperCase()
    .substring(0, 2);
};
