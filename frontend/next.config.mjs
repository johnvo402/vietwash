import createNextIntlPlugin from "next-intl/plugin";
import withBundleAnalyzer from "@next/bundle-analyzer";
import withPWA from "next-pwa";

const withNextIntl = createNextIntlPlugin();
const withAnalyzer = withBundleAnalyzer({ enabled: false });
const withPwa = withPWA({
  dest: "public",
  register: true,
  skipWaiting: true,
  disable: process.env.NODE_ENV === "development",
});

const mediaUrl = process.env.NEXT_PUBLIC_MEDIA_URL;
const remotePatterns = [
  {
    protocol: "http",
    hostname: "127.0.0.1",
    port: "9000",
    pathname: "/**",
  },
];

if (mediaUrl) {
  const url = new URL(mediaUrl);
  remotePatterns.push({
    protocol: url.protocol.replace(":", ""),
    hostname: url.hostname,
    port: url.port,
    pathname: `${url.pathname.replace(/\/$/, "")}/**`,
  });
}

/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    remotePatterns,
  },
  async headers() {
    return [
      {
        source: "/:path*",
        headers: [
          {
            key: "Content-Security-Policy",
            value:
              "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; connect-src 'self' *; img-src blob: 'self' http://localhost:9000 http://127.0.0.1:9000 data: https:; font-src 'self'; frame-src 'self' blob:; object-src 'self' blob:",
          },
        ],
      },
    ];
  },
};

export default withPwa(withAnalyzer(withNextIntl(nextConfig)));
