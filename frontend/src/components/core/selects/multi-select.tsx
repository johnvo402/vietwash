"use client";

import type React from "react";
import { useState, useRef, useEffect } from "react";
import { X, ChevronDown, Check, XCircle, ChevronUp } from "lucide-react";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { useTranslations } from "next-intl";

export interface Option {
  value: string;
  label: string;
}

interface MultiSelectProps {
  options: Option[];
  value: Option[];
  onChange: (value: Option[]) => void;
  placeholder?: string;
  label?: string;
  className?: string;
  disabled?: boolean;
}

export default function MultiSelect({
  options,
  value,
  onChange,
  placeholder = "Chọn...",
  label,
  className,
  disabled = false,
}: MultiSelectProps) {
  const t = useTranslations();
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const filteredOptions = options.filter(
    (option) =>
      !value.some((v) => v.value === option.value) &&
      option.label.toLowerCase().includes(searchTerm.toLowerCase()),
  );

  const handleSelect = (option: Option) => {
    onChange([...value, option]);
    setSearchTerm("");
    setIsOpen(false);
    inputRef.current?.focus();
  };

  const handleRemove = (option: Option) => {
    onChange(value.filter((v) => v.value !== option.value));
  };

  const handleSelectAll = () => {
    onChange([...options]);
    setSearchTerm("");
    setIsOpen(false);
  };

  const handleClearAll = () => {
    onChange([]);
    setSearchTerm("");
  };

  const handleInputClick = () => {
    setIsOpen(true);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
    setIsOpen(true);
  };

  const isAllSelected = value.length === options.length;

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  return (
    <div className={cn("w-full space-y-2", className)}>
      {label && <Label>{label}</Label>}
      <div ref={containerRef} className="relative">
        {/* Input field */}
        <div
          className={cn(
            "flex items-center w-full border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2",
            value.length > 0 ? "rounded-t-md" : "rounded-md",
          )}
        >
          <input
            ref={inputRef}
            type="text"
            placeholder={placeholder}
            value={searchTerm}
            onChange={handleInputChange}
            onClick={handleInputClick}
            disabled={disabled}
            className="flex-1 border-0 bg-transparent p-0 text-sm placeholder:text-muted-foreground focus:outline-none"
          />
          <div className="flex items-center gap-1">
            {value.length > 0 && (
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={handleClearAll}
                disabled={disabled}
                className="h-11 min-w-11 px-2 text-xs text-muted-foreground hover:text-foreground"
                aria-label={t("common.reset")}
              >
                <XCircle className="h-4 w-4" />
              </Button>
            )}
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={() => setIsOpen(!isOpen)}
              disabled={disabled}
              className="h-11 min-w-11 px-2 text-xs text-muted-foreground hover:text-foreground"
              aria-label={t("common.toggleDetails")}
              aria-expanded={isOpen}
            >
              {isOpen ? (
                <ChevronUp className="h-4 w-4" />
              ) : (
                <ChevronDown className="h-4 w-4" />
              )}
            </Button>
          </div>
        </div>

        {/* Tags container */}
        {value.length > 0 && (
          <div className="w-full border-l border-r border-b border-input rounded-b-md bg-background px-3 py-2 -mt-px">
            <div className="flex flex-wrap gap-1">
              {value.map((option) => (
                <div
                  key={option.value}
                  className="flex items-center gap-2 bg-gray-100 px-3 py-1 rounded-full"
                >
                  <span>{option.label}</span>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="p-0 h-auto"
                    disabled={disabled}
                    onClick={() => handleRemove(option)}
                    aria-label={t("common.removeItem", {
                      item: option.label,
                    })}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Dropdown */}
        {isOpen && (
          <div className="absolute top-full z-50 mt-1 w-full rounded-md border border-input bg-background shadow-lg">
            <div className="max-h-60 overflow-auto p-1">
              {!isAllSelected && filteredOptions.length > 0 && (
                <>
                  <button
                    type="button"
                    onClick={handleSelectAll}
                    className="w-full rounded px-3 py-2 text-left text-sm font-medium text-primary hover:bg-accent hover:text-accent-foreground flex items-center gap-2"
                  >
                    <Check className="h-4 w-4" />
                    {t("common.selectAll")}
                  </button>
                  <div className="border-t border-border my-1" />
                </>
              )}

              {filteredOptions.map((option) => (
                <button
                  key={option.value}
                  type="button"
                  onClick={() => handleSelect(option)}
                  className="w-full rounded px-3 py-2 text-left text-sm hover:bg-accent hover:text-accent-foreground"
                >
                  {option.label}
                </button>
              ))}

              {filteredOptions.length === 0 && searchTerm && (
                <div className="px-3 py-2 text-sm text-muted-foreground">
                  {t("common.noResultFor", { searchTerm })}
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
