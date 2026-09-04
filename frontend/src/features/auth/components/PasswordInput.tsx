// src/features/auth/components/PasswordInput.tsx
import React, { useState } from "react";
import { Input } from "@/components/ui/input";
import { Eye, EyeOff } from "lucide-react"; // Or your icon library
import { Button } from "@/components/ui/button";
import { useTranslations } from "next-intl";

type PasswordInputProps = React.InputHTMLAttributes<HTMLInputElement>;

const PasswordInput = React.forwardRef<HTMLInputElement, PasswordInputProps>(
  ({ className, ...props }, ref) => {
    const [showPassword, setShowPassword] = useState(false);
    const t = useTranslations();

    return (
      <div className="relative">
        <Input
          type={showPassword ? "text" : "password"}
          ref={ref}
          className={`min-h-11 pr-12 ${className ?? ""}`}
          {...props}
        />
        <Button
          type="button"
          variant="ghost"
          size="sm"
          className="absolute right-0 top-0 h-full min-w-11 px-3"
          aria-label={t(
            showPassword ? "common.hidePassword" : "common.showPassword",
          )}
          aria-pressed={showPassword}
          onClick={() => setShowPassword((visible) => !visible)}
        >
          {showPassword ? (
            <EyeOff aria-hidden="true" />
          ) : (
            <Eye aria-hidden="true" />
          )}
        </Button>
      </div>
    );
  },
);

PasswordInput.displayName = "PasswordInput";

export default PasswordInput;
