"use client";

import DOMPurify from "dompurify";
import { useMemo } from "react";

interface SafeHtmlProps {
  html?: string | null;
  className?: string;
  fallback?: string;
}

export function SafeHtml({ html, className, fallback = "" }: SafeHtmlProps) {
  const sanitizedHtml = useMemo(
    () =>
      DOMPurify.sanitize(html || fallback, {
        ALLOWED_TAGS: [
          "p",
          "br",
          "strong",
          "b",
          "em",
          "i",
          "u",
          "s",
          "h1",
          "h2",
          "h3",
          "ul",
          "ol",
          "li",
          "blockquote",
          "pre",
          "code",
          "a",
        ],
        ALLOWED_ATTR: ["href", "title", "target", "rel"],
      }),
    [fallback, html],
  );

  return (
    <div
      className={className}
      dangerouslySetInnerHTML={{ __html: sanitizedHtml }}
    />
  );
}
