import { ReactNode } from "react";
import { ProfileField } from "./profile-field";

interface Option<T> {
  value: T;
  label: string;
}

interface ProfileSelectProps<T> {
  name: string;
  value: T;
  onChange: (e: React.ChangeEvent<HTMLSelectElement>) => void;
  label: string;
  icon: ReactNode;
  isEditing: boolean;
  options: Option<T>[];
  displayValue?: (value: T) => string;
}

export function ProfileSelect<T extends string | number>({
  name,
  value,
  onChange,
  label,
  icon,
  isEditing,
  options,
  displayValue,
}: ProfileSelectProps<T>) {
  const getDisplayValue = () => {
    if (displayValue) {
      return displayValue(value);
    }
    const option = options.find((opt) => opt.value === value);
    return option ? option.label : String(value);
  };

  return (
    <ProfileField label={label} icon={icon} isEditing={isEditing}>
      {isEditing ? (
        <select
          name={name}
          value={value}
          onChange={onChange}
          className="w-full border-b border-blue-300 focus:outline-none focus:border-blue-500"
        >
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      ) : (
        <p className="truncate">{getDisplayValue()}</p>
      )}
    </ProfileField>
  );
}
