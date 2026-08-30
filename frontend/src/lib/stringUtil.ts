import { formatDistanceToNow } from "date-fns";
import { enUS, vi } from "date-fns/locale";
import { useLocale } from "next-intl";

export const useStringUtil = () => {
  const local = useLocale();

  function removeDiacritics(str: string): string {
    return str.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
  }

  function processText(str: string): string {
    return local === "en" ? removeDiacritics(str) : str;
  }
  type LocalizedObject = {
    fullName: string;
    fullNameEn: string;
  };
  type ObjectLangType = {
    vi: string;
    en: string;
  };
  function textByLang(str: ObjectLangType): string {
    return local === "en" ? str.en : str.vi;
  }

  const getLocalizedName = (obj: LocalizedObject) => {
    return local === "en" ? obj?.fullNameEn : obj?.fullName || undefined;
  };

  function formatDistance(date?: Date | string | null): string {
    if (!date) return "--";

    try {
      return formatDistanceToNow(new Date(date), {
        addSuffix: true,
        locale: local === "en" ? enUS : vi,
      });
    } catch {
      return "--";
    }
  }
  const formatDate = (dateString: string) =>
    new Date(dateString).toLocaleDateString(
      local === "en" ? "en-US" : "vi-VN",
      {
        year: "numeric",
        month: "short",
        day: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      }
    );
  function capitalizeWords(str: string) {
    return str
      .toLowerCase()
      .split(" ")
      .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
      .join(" ");
  }

  return {
    processText,
    removeDiacritics,
    getLocalizedName,
    textByLang,
    formatDistance,
    formatDate,
    capitalizeWords,
  };
};
