"use client";

import type React from "react";
import { useEffect, useRef } from "react";
import { useTranslations } from "next-intl"; // Assuming next-intl for translations
import { useAddressApi } from "@/hooks/use-address-data";
import type { AccountContact } from "@/types/user";

interface AddressSelectorProps {
  contact?: AccountContact;
  isEditing: boolean;
  onChange: (field: keyof AccountContact, value: string) => void;
}

export function AddressSelector({
  contact,
  isEditing,
  onChange,
}: AddressSelectorProps) {
  const t = useTranslations("AddressSelector"); // Translation hook
  const {
    provinces,
    districts,
    communes,
    loading,
    loadDistricts,
    loadCommunes,
  } = useAddressApi();

  const prevProvinceCode = useRef<string>("");
  const prevDistrictCode = useRef<string>("");

  useEffect(() => {
    if (
      contact?.provinceCode &&
      contact.provinceCode !== prevProvinceCode.current
    ) {
      prevProvinceCode.current = contact.provinceCode;
      loadDistricts(contact.provinceCode);
    }
  }, [contact?.provinceCode, loadDistricts]);

  useEffect(() => {
    if (
      contact?.districtCode &&
      contact.districtCode !== prevDistrictCode.current
    ) {
      prevDistrictCode.current = contact.districtCode;
      loadCommunes(contact.districtCode);
    }
  }, [contact?.districtCode, loadCommunes]);

  const handleProvinceChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedId = e.target.value;
    const selectedProvince = provinces.find((p) => p.id === selectedId);

    onChange("provinceCode", selectedId || "");
    onChange("province", selectedProvince?.full_name || "");
    onChange("districtCode", "");
    onChange("district", "");
    onChange("communeCode", "");
    onChange("commune", "");
    onChange("address", selectedProvince?.full_name || "");
  };

  const handleDistrictChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedId = e.target.value;
    const selectedDistrict = districts.find((d) => d.id === selectedId);

    onChange("districtCode", selectedId || "");
    onChange("district", selectedDistrict?.full_name || "");
    onChange("communeCode", "");
    onChange("commune", "");
    onChange(
      "address",
      [contact?.street, selectedDistrict?.full_name, contact?.province]
        .filter(Boolean)
        .join(", ")
    );
  };

  const handleCommuneChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedId = e.target.value;
    const selectedCommune = communes.find((c) => c.id === selectedId);

    onChange("communeCode", selectedId || "");
    onChange("commune", selectedCommune?.full_name || "");
    onChange(
      "address",
      [
        contact?.street,
        selectedCommune?.full_name,
        contact?.district,
        contact?.province,
      ]
        .filter(Boolean)
        .join(", ")
    );
  };

  if (!isEditing) {
    return (
      <div className="grid grid-cols-1 gap-2">
        <AddressDisplay label={t("province")} value={contact?.province} t={t} />
        <AddressDisplay label={t("district")} value={contact?.district} t={t} />
        <AddressDisplay label={t("commune")} value={contact?.commune} t={t} />
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-2">
      <AddressSelect
        label={t("province")}
        value={contact?.provinceCode || ""}
        options={provinces}
        loading={loading.provinces}
        onChange={handleProvinceChange}
        disabled={false}
      />
      <AddressSelect
        label={t("district")}
        value={contact?.districtCode || ""}
        options={districts}
        loading={loading.districts}
        onChange={handleDistrictChange}
        disabled={!contact?.provinceCode}
      />
      <AddressSelect
        label={t("commune")}
        value={contact?.communeCode || ""}
        options={communes}
        loading={loading.communes}
        onChange={handleCommuneChange}
        disabled={!contact?.districtCode}
      />
    </div>
  );
}

function AddressDisplay({
  label,
  value,
  t,
}: {
  label: string;
  value?: string;
  t: any;
}) {
  return (
    <div className="flex items-center gap-2">
      <div className="flex-1 min-w-0">
        <p className="text-xs text-blue-600">{label}</p>
        <p className="truncate text-sm">{value || t("not_specified")}</p>
      </div>
    </div>
  );
}

function AddressSelect({
  label,
  value,
  options,
  loading,
  onChange,
  disabled,
}: {
  label: string;
  value: string;
  options: { id: string; full_name: string }[];
  loading: boolean;
  onChange: (e: React.ChangeEvent<HTMLSelectElement>) => void;
  disabled: boolean;
}) {
  const t = useTranslations("AddressSelector"); // Translation hook

  return (
    <div className="flex items-center gap-2">
      <div className="flex-1 min-w-0">
        <p className="text-xs ">{label}</p>
        <div className="relative">
          <select
            value={value}
            onChange={onChange}
            className="w-full border-b border-border focus:outline-none text-sm pr-6"
            disabled={disabled || loading}
          >
            <option value="">{t("select", { label })}</option>
            {options.map((option) => (
              <option key={option.id} value={option.id}>
                {option.full_name}
              </option>
            ))}
          </select>
          {loading && (
            <span className="absolute right-1 top-1/2 transform -translate-y-1/2 h-3 w-3 animate-spin">
              {t("loading")}
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
