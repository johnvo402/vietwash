"use client";

import { ScrollArea } from "@radix-ui/react-scroll-area";

interface ContentLayoutProps {
  scrollable: boolean;
  children: React.ReactNode;
}

export function ContentLayout({ scrollable, children }: ContentLayoutProps) {
  return scrollable ? (
    <ScrollArea className="h-[calc(100dvh-52px)]">
      <div className="flex flex-1 p-4 md:px-6">{children}</div>
    </ScrollArea>
  ) : (
    <div className="bg-background flex flex-1 p-4 m-4 md:px-6 md:mx-6">
      {children}
    </div>
  );
}
