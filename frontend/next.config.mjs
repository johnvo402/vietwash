import createNextIntlPlugin from "next-intl/plugin";
import withBundleAnalyzer from "@next/bundle-analyzer";
import withPWA from "next-pwa";

const withNextIntl = createNextIntlPlugin();
const withAnalyzer = withBundleAnalyzer({ enabled: false });
const withPwa = withPWA({
  dest: "public",
  register: true,
  skipWaiting: true,
  // App Router's build manifest is server-only, not a public precache asset.
  buildExcludes: [/app-build-manifest\.json$/],
  disable: process.env.NODE_ENV === "development",
});

const isDevelopment = process.env.NODE_ENV === "development";

const parseHttpUrl = (value, variableName) => {
  if (!value) return null;

  const url = new URL(value);
  if (url.protocol !== "http:" && url.protocol !== "https:") {
    throw new Error(`${variableName} must use http or https.`);
  }
  if (url.hostname.includes("*")) {
    throw new Error(`${variableName} must contain explicit origins.`);
  }

  const isLocal = ["localhost", "127.0.0.1", "::1"].includes(url.hostname);
  return !isDevelopment && isLocal ? null : url;
};

const apiUrl = parseHttpUrl(
  process.env.NEXT_PUBLIC_API_URL,
  "NEXT_PUBLIC_API_URL",
);
const mediaUrl = parseHttpUrl(
  process.env.NEXT_PUBLIC_MEDIA_URL,
  "NEXT_PUBLIC_MEDIA_URL",
);
const extraConnectUrls = (process.env.CSP_CONNECT_SRC ?? "")
  .split(/[\s,]+/)
  .filter(Boolean)
  .map((value) => parseHttpUrl(value, "CSP_CONNECT_SRC"))
  .filter(Boolean);

const remotePatterns = [];

if (isDevelopment) {
  remotePatterns.push({
    protocol: "http",
    hostname: "127.0.0.1",
    port: "9000",
    pathname: "/**",
  });
}

if (mediaUrl) {
  remotePatterns.push({
    protocol: mediaUrl.protocol.replace(":", ""),
    hostname: mediaUrl.hostname,
    port: mediaUrl.port,
    pathname: `${mediaUrl.pathname.replace(/\/$/, "")}/**`,
  });
}

const developmentConnectOrigins = isDevelopment
  ? [
      "http://localhost:5000",
      "http://127.0.0.1:5000",
      "http://localhost:9000",
      "http://127.0.0.1:9000",
      "ws://localhost:5000",
      "ws://127.0.0.1:5000",
    ]
  : [];
const configuredConnectOrigins = [apiUrl, mediaUrl, ...extraConnectUrls]
  .filter(Boolean)
  .flatMap((url) => {
    const websocketProtocol = url.protocol === "https:" ? "wss:" : "ws:";
    return [url.origin, `${websocketProtocol}//${url.host}`];
  });
const connectSources = [
  "'self'",
  ...developmentConnectOrigins,
  ...configuredConnectOrigins,
];
const imageSources = [
  "'self'",
  "blob:",
  "data:",
  ...(isDevelopment
    ? ["http://localhost:9000", "http://127.0.0.1:9000"]
    : []),
  ...(mediaUrl ? [mediaUrl.origin] : []),
];
const scriptSources = [
  "'self'",
  "'unsafe-inline'",
  ...(isDevelopment ? ["'unsafe-eval'"] : []),
];
const contentSecurityPolicy = [
  "default-src 'self'",
  `script-src ${scriptSources.join(" ")}`,
  "style-src 'self' 'unsafe-inline'",
  `connect-src ${[...new Set(connectSources)].join(" ")}`,
  `img-src ${[...new Set(imageSources)].join(" ")}`,
  "font-src 'self' data:",
  "frame-src 'self' blob:",
  "worker-src 'self' blob:",
  "object-src 'self' blob:",
  "base-uri 'self'",
  "form-action 'self'",
  "frame-ancestors 'self'",
].join("; ");

/** @type {import('next').NextConfig} */
const nextConfig = {
  output: "standalone",
  images: {
    // Signed same-origin media is served directly by the edge /image route.
    // Avoid the standalone server trying to optimize it through its own /image.
    unoptimized: !process.env.NEXT_PUBLIC_API_URL,
    remotePatterns,
  },
  async headers() {
    return [
      {
        source: "/:path*",
        headers: [
          {
            key: "Content-Security-Policy",
            value: contentSecurityPolicy,
          },
        ],
      },
    ];
  },
};

export default withPwa(withAnalyzer(withNextIntl(nextConfig)));
