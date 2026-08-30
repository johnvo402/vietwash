"use client";

import * as React from "react";
import { useTheme } from "next-themes";
import { MoonIcon, SunIcon } from "@radix-ui/react-icons";

import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
  TooltipProvider,
} from "@/components/ui/tooltip";
import { SunMoon } from "lucide-react";

export function ModeToggle() {
  const { setTheme, theme } = useTheme();

  const toggleTheme = () => {
    setTheme(
      theme === "system" ? "light" : theme === "light" ? "dark" : "system"
    );
  };

  return (
    <TooltipProvider disableHoverableContent>
      <Tooltip delayDuration={100}>
        <TooltipTrigger asChild>
          <Button
            className="rounded-full w-8 h-8 bg-background mr-2"
            variant="outline"
            size="icon"
            onClick={toggleTheme}
          >
            {/* Show SunIcon in light mode */}
            <SunIcon
              className={`w-[1.2rem] h-[1.2rem] absolute transition-all duration-500 
                ${theme === "light" ? "rotate-0 scale-100" : "rotate-90 scale-0"}`}
            />
            {/* Show MoonIcon in dark mode */}
            <MoonIcon
              className={`w-[1.2rem] h-[1.2rem] absolute transition-all duration-500 
                ${theme === "dark" ? "rotate-0 scale-100" : "-rotate-90 scale-0"}`}
            />
            {/* Show SunMoon in system mode */}
            <SunMoon
              className={`w-[1.2rem] h-[1.2rem] absolute transition-all duration-500 
                ${theme === "system" ? "rotate-0 scale-100" : "rotate-90 scale-0"}`}
            />
            <span className="sr-only">Switch Theme</span>
          </Button>
        </TooltipTrigger>
        <TooltipContent side="bottom">Switch Theme</TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
}
