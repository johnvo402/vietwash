"use client";

import type React from "react";
import { ProfileInput } from "./profile-input";
import { AddressSelector } from "./address-selector";
import type { AccountContact } from "@/types/user";
import { ProfileField } from "./profile-field";
import { useTranslations } from "next-intl";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

interface ContactFormProps {
  contact?: AccountContact;
  isEditing: boolean;
  onChange: (field: keyof AccountContact, value: string) => void;
}

export function ContactForm({
  contact,
  isEditing,
  onChange,
}: ContactFormProps) {
  const t = useTranslations("AddressSelector"); // Use AddressSelector namespace for address-related translations
  const tCommon = useTranslations("user"); // Use common namespace for general translations

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    onChange(name as keyof AccountContact, value);
  };

  return (
    <div className="border border-blue-200 rounded-lg p-4">
      {isEditing ? (
        <div>
          <AddressSelector
            contact={contact}
            isEditing={isEditing}
            onChange={onChange}
          />
          <p className="text-xs font-normal">{t("street")}</p>
          <input
            name="street"
            value={contact?.street ?? ""}
            onChange={handleInputChange}
            className="w-full border-b border-border focus:outline-none text-sm pr-6"
            placeholder={t("placeholder", {
              entity: t("street").toLowerCase(),
            })}
          />
        </div>
      ) : (
        <div>
          <ProfileField
            label={tCommon("address.title")} // Use translation for "Address"
            icon={<p></p>}
            isEditing={isEditing}
          >
            <p className="truncate text-wrap">
              {contact?.address || t("not_specified")}{" "}
              {/* Use translation for "Not specified" */}
            </p>
          </ProfileField>
        </div>
      )}
    </div>
  );
}
