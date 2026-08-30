import { Metadata } from "next";
import { getLocale, getMessages } from "next-intl/server";

const DEFAULT_TITLE = "VietWash";

export function constructMetadata({
  title = "",
  description = "Laundry Management - VietWash",
  image = "/demos/demo.png",
  icons = {
    icon: [
      { url: "/logo/favicon-96x96.png", sizes: "96x96", type: "image/png" },
      { url: "/logo/favicon.svg", type: "image/svg+xml" },
      { url: "/logo/favicon.ico" },
    ],
    apple: [{ url: "/logo/apple-touch-icon.png", sizes: "180x180" }],
  },
  url = process.env.NODE_ENV === "development"
    ? "http://localhost:3000"
    : "https://vietwash.vercel.app",
  siteName = "Laundry Management System - VietWash",
  countryName = "Vietnam",
  manifest = "/manifest.json",
}: {
  title?: string;
  description?: string;
  image?: string;
  icons?: Metadata["icons"];
  url?: string;
  siteName?: string;
  countryName?: string;
  manifest?: string;
} = {}): Metadata {
  const setTitle = `${DEFAULT_TITLE} ${title ? `- ${title}` : ""}`;
  return {
    title: setTitle,
    description,
    openGraph: {
      title: setTitle,
      description,
      images: [{ url: image }],
      url,
      siteName,
      countryName,
      type: "website",
    },
    twitter: {
      card: "summary_large_image",
      title: setTitle,
      description,
      images: [image],
      creator: "@ththu0402",
    },
    icons,
    appleWebApp: {
      title: setTitle,
      statusBarStyle: "default",
      capable: true,
    },
    manifest,
    metadataBase: new URL(
      process.env.NODE_ENV === "development"
        ? "http://localhost:3000"
        : "https://vietwash.vercel.app"
    ),
  };
}

export async function generateTranslatedMetadata({
  pathname,
}: {
  pathname: string;
}) {
  const locale = await getLocale();
  const messages = await getMessages({ locale });
  const routeMessages = (messages as any)?.route ?? {};
  const titleKey = Object.keys(routeMessages).find((key) => {
    // Convert dynamic path [slug] to regex
    const regex = new RegExp(
      "^" + key.replace(/\[.*?\]/g, "[^/]+").replace(/\//g, "\\/") + "$"
    );
    return regex.test(pathname);
  });

  const translatedTitle = titleKey ? routeMessages[titleKey] : undefined;

  return constructMetadata({
    title: translatedTitle ?? "", // fallback title
  });
}
