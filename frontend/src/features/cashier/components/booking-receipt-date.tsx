"use client";

import type React from "react";
import { useState, useEffect } from "react";
import { Calendar } from "@/components/ui/calendar";
import { Button } from "@/components/ui/button";
import { CalendarIcon, ChevronLeft, ChevronRight, Clock } from "lucide-react";
import {
  format,
  getYear,
  getMonth,
  setMonth,
  setYear,
  parse,
  isValid,
  isBefore,
  startOfDay,
} from "date-fns";
import { vi } from "date-fns/locale";
import { cn } from "@/lib/utils";
import { Label } from "@/components/ui/label";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useTranslations } from "next-intl";

interface CustomDateTimeProps {
  className?: string;
  onChange?: (dateTime: Date | undefined) => void;
  required?: boolean;
  label?: string;
  description?: string;
  placeholder?: string;
  showSeconds?: boolean;
  date?: Date | null;
  disabled?: boolean;
}

export default function CustomDateTime({
  className,
  onChange,
  required = false,
  label,
  description,
  placeholder,
  showSeconds = false,
  date = null,
  disabled = false,
}: CustomDateTimeProps) {
  const t = useTranslations();
  const now = new Date();
  const [dateTime, setDateTime] = useState<Date | null>(date ?? null);
  const [open, setOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [inputValue, setInputValue] = useState("");
  const [calendarDate, setCalendarDate] = useState<Date>(date ?? now);
  const [hours, setHours] = useState<string>(date ? format(date, "HH") : "00");
  const [minutes, setMinutes] = useState<string>(
    date ? format(date, "mm") : "00",
  );
  const [seconds, setSeconds] = useState<string>(
    date ? format(date, "ss") : "00",
  );

  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 21 }, (_, i) => currentYear - 10 + i);
  const months = Array.from({ length: 12 }, (_, i) => ({
    value: i.toString(),
    label: t(`dateAndTime.months.${i + 1}`),
  }));

  const isToday = (date: Date) =>
    startOfDay(date).getTime() === startOfDay(now).getTime();

  const hoursOptions = Array.from({ length: 24 }, (_, i) =>
    i.toString().padStart(2, "0"),
  ).filter((h) =>
    dateTime && isToday(dateTime) ? parseInt(h) >= now.getHours() : true,
  );

  const minutesOptions = Array.from({ length: 60 }, (_, i) =>
    i.toString().padStart(2, "0"),
  ).filter((m) =>
    dateTime && isToday(dateTime) && parseInt(hours) === now.getHours()
      ? parseInt(m) >= now.getMinutes()
      : true,
  );

  const secondsOptions = Array.from({ length: 60 }, (_, i) =>
    i.toString().padStart(2, "0"),
  ).filter((s) =>
    dateTime &&
    isToday(dateTime) &&
    parseInt(hours) === now.getHours() &&
    parseInt(minutes) === now.getMinutes()
      ? parseInt(s) >= now.getSeconds()
      : true,
  );

  useEffect(() => {
    if (date) {
      setDateTime(date);
      setCalendarDate(date);
      setInputValue(
        format(date, showSeconds ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy HH:mm"),
      );
      setHours(format(date, "HH"));
      setMinutes(format(date, "mm"));
      setSeconds(format(date, "ss"));
    } else {
      setDateTime(null);
      setInputValue("");
      setHours("00");
      setMinutes("00");
      setSeconds("00");
    }
  }, [date, showSeconds]);

  const updateDateTime = (selectedDate?: Date) => {
    if (!selectedDate) {
      setDateTime(null);
      setInputValue("");
      setHours("00");
      setMinutes("00");
      setSeconds("00");
      if (onChange) onChange(undefined);
      return;
    }

    const newDateTime = new Date(selectedDate);
    newDateTime.setHours(Number.parseInt(hours, 10));
    newDateTime.setMinutes(Number.parseInt(minutes, 10));
    newDateTime.setSeconds(Number.parseInt(seconds, 10));

    if (isBefore(newDateTime, now)) {
      setDateTime(now);
      setInputValue(
        format(now, showSeconds ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy HH:mm"),
      );
      setHours(format(now, "HH"));
      setMinutes(format(now, "mm"));
      setSeconds(format(now, "ss"));
      setError(t("dateAndTime.invalidPastTime"));
      if (onChange) onChange(now);
      return;
    }

    setDateTime(newDateTime);
    setInputValue(
      format(
        newDateTime,
        showSeconds ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy HH:mm",
      ),
    );
    if (onChange) onChange(newDateTime);
  };

  const handleSelect = (selectedDate: Date | undefined) => {
    if (selectedDate) {
      updateDateTime(selectedDate);
      setError(null);
    }
  };

  const handleTimeChange = (
    type: "hours" | "minutes" | "seconds",
    value: string,
  ) => {
    if (type === "hours") setHours(value);
    if (type === "minutes") setMinutes(value);
    if (type === "seconds") setSeconds(value);

    if (!dateTime) {
      const newDateTime = new Date();
      newDateTime.setHours(type === "hours" ? parseInt(value) : 0);
      newDateTime.setMinutes(type === "minutes" ? parseInt(value) : 0);
      newDateTime.setSeconds(type === "seconds" ? parseInt(value) : 0);
      setDateTime(newDateTime);
      setInputValue(
        format(
          newDateTime,
          showSeconds ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy HH:mm",
        ),
      );
      if (onChange) onChange(newDateTime);
      return;
    }

    const newDateTime = new Date(dateTime);
    if (type === "hours") newDateTime.setHours(Number.parseInt(value, 10));
    if (type === "minutes") newDateTime.setMinutes(Number.parseInt(value, 10));
    if (type === "seconds") newDateTime.setSeconds(Number.parseInt(value, 10));

    if (isBefore(newDateTime, now)) {
      setDateTime(now);
      setInputValue(
        format(now, showSeconds ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy HH:mm"),
      );
      setHours(format(now, "HH"));
      setMinutes(format(now, "mm"));
      setSeconds(format(now, "ss"));
      setError(t("dateAndTime.invalidPastTime"));
      if (onChange) onChange(now);
    } else {
      setDateTime(newDateTime);
      setInputValue(
        format(
          newDateTime,
          showSeconds ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy HH:mm",
        ),
      );
      if (onChange) onChange(newDateTime);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setInputValue(value);

    if (value === "") {
      setDateTime(null);
      setHours("00");
      setMinutes("00");
      setSeconds("00");
      setError(null);
      if (onChange) onChange(undefined);
      return;
    }

    try {
      const timeFormat = showSeconds
        ? "dd/MM/yyyy HH:mm:ss"
        : "dd/MM/yyyy HH:mm";
      const parsedDate = parse(value, timeFormat, new Date());
      if (isValid(parsedDate)) {
        if (isBefore(parsedDate, now)) {
          setDateTime(now);
          setInputValue(
            format(
              now,
              showSeconds ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy HH:mm",
            ),
          );
          setHours(format(now, "HH"));
          setMinutes(format(now, "mm"));
          setSeconds(format(now, "ss"));
          setError(t("dateAndTime.invalidPastTime"));
          if (onChange) onChange(now);
        } else {
          setDateTime(parsedDate);
          setCalendarDate(parsedDate);
          setHours(format(parsedDate, "HH"));
          setMinutes(format(parsedDate, "mm"));
          setSeconds(format(parsedDate, "ss"));
          setError(null);
          if (onChange) onChange(parsedDate);
        }
      } else {
        setError(t("dateAndTime.invalidDateTimeFormat"));
      }
    } catch {
      setError(t("dateAndTime.invalidDateTimeFormat"));
    }
  };

  const handleMonthChange = (value: string) => {
    const monthIndex = Number.parseInt(value, 10);
    setCalendarDate(setMonth(calendarDate, monthIndex));
  };

  const handleYearChange = (value: string) => {
    const year = Number.parseInt(value, 10);
    setCalendarDate(setYear(calendarDate, year));
  };

  const handlePrevMonth = () => {
    const prevMonth = new Date(calendarDate);
    prevMonth.setMonth(prevMonth.getMonth() - 1);
    setCalendarDate(prevMonth);
  };

  const handleNextMonth = () => {
    const nextMonth = new Date(calendarDate);
    nextMonth.setMonth(nextMonth.getMonth() + 1);
    setCalendarDate(nextMonth);
  };

  const handleSetCurrentTime = () => {
    const now = new Date();
    setDateTime(now);
    setCalendarDate(now);
    setHours(format(now, "HH"));
    setMinutes(format(now, "mm"));
    setSeconds(format(now, "ss"));
    setInputValue(
      format(now, showSeconds ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy HH:mm"),
    );
    if (onChange) onChange(now);
  };

  return (
    <div className={cn("space-y-2", className)}>
      <div className="space-y-1">
        {label && (
          <Label htmlFor="booking-datetime">
            {label}
            {required && <span className="text-destructive ml-1">*</span>}
          </Label>
        )}
        {description && (
          <p className="text-sm text-muted-foreground">{description}</p>
        )}
      </div>

      <div className="relative flex-1">
        <Input
          id="booking-datetime"
          value={inputValue}
          onChange={handleInputChange}
          placeholder={placeholder}
          disabled={disabled}
          className={cn("pr-10", error && "border-destructive")}
        />
        <Popover open={open} onOpenChange={setOpen}>
          <PopoverTrigger asChild>
            <Button
              variant="ghost"
              size="icon"
              type="button"
              className="absolute right-0 top-0 h-full px-3 py-2"
            >
              <CalendarIcon className="h-4 w-4 opacity-50" />
              <span className="sr-only">{t("dateAndTime.openCalendar")}</span>
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-auto p-3 flex space-x-4" align="end">
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <Button
                  variant="outline"
                  size="icon"
                  className="h-7 w-7"
                  onClick={handlePrevMonth}
                  disabled={
                    getMonth(calendarDate) <= getMonth(now) &&
                    getYear(calendarDate) <= getYear(now)
                  }
                >
                  <ChevronLeft className="h-4 w-4" />
                </Button>
                <div className="flex space-x-2">
                  <Select
                    value={getMonth(calendarDate).toString()}
                    onValueChange={handleMonthChange}
                  >
                    <SelectTrigger className="w-[120px]">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {months.map((month) => (
                        <SelectItem key={month.value} value={month.value}>
                          {month.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <Select
                    value={getYear(calendarDate).toString()}
                    onValueChange={handleYearChange}
                  >
                    <SelectTrigger className="w-[80px]">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {years.map((year) => (
                        <SelectItem key={year} value={year.toString()}>
                          {year}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <Button
                  variant="outline"
                  size="icon"
                  className="h-7 w-7"
                  onClick={handleNextMonth}
                >
                  <ChevronRight className="h-4 w-4" />
                </Button>
              </div>
              <Calendar
                mode="single"
                selected={dateTime || undefined}
                onSelect={handleSelect}
                locale={vi}
                month={calendarDate}
                onMonthChange={setCalendarDate}
                initialFocus
                disabled={(date) => isBefore(date, startOfDay(now))}
              />
            </div>

            <div className="space-y-3 w-[200px]">
              <div className="flex items-center justify-between">
                <h3 className="text-sm font-medium">
                  {t("dateAndTime.pickTime")}
                </h3>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleSetCurrentTime}
                  className="h-8 text-xs"
                >
                  <Clock className="mr-1 h-3 w-3" />
                  {t("dateAndTime.now")}
                </Button>
              </div>
              <div className="flex space-x-2">
                <Select
                  value={hours}
                  onValueChange={(value) => handleTimeChange("hours", value)}
                >
                  <SelectTrigger className="w-[60px]">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {hoursOptions.map((hour) => (
                      <SelectItem key={hour} value={hour}>
                        {hour}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <Select
                  value={minutes}
                  onValueChange={(value) => handleTimeChange("minutes", value)}
                >
                  <SelectTrigger className="w-[60px]">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {minutesOptions.map((minute) => (
                      <SelectItem key={minute} value={minute}>
                        {minute}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {showSeconds && (
                  <Select
                    value={seconds}
                    onValueChange={(value) =>
                      handleTimeChange("seconds", value)
                    }
                  >
                    <SelectTrigger className="w-[60px]">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {secondsOptions.map((second) => (
                        <SelectItem key={second} value={second}>
                          {second}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              </div>
              <div className="text-center text-sm font-medium">
                {dateTime ? (
                  <span>
                    {format(dateTime, "EEEE, dd MMMM yyyy", { locale: vi })}
                    <br />
                    {format(dateTime, showSeconds ? "HH:mm:ss" : "HH:mm")}
                  </span>
                ) : (
                  <span className="text-muted-foreground">
                    {t("common.noResult")}
                  </span>
                )}
              </div>
              <Button
                className="w-full"
                onClick={() => setOpen(false)}
                disabled={!dateTime}
              >
                {t("common.status.confirm")}
              </Button>
            </div>
          </PopoverContent>
        </Popover>
      </div>

      {error && <p className="text-sm text-destructive">{error}</p>}
    </div>
  );
}
