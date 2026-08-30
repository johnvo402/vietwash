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

/** @type {import('next').NextConfig} */
const nextConfig = {
  // your existing config
  webpack: (config) => {
    config.snapshot = config.snapshot || {};
    config.snapshot.managedPaths = [
      /[\\/]node_modules[\\/](@next[\\/]swc.*|@parcel[\\/]watcher.*)/,
    ];
    return config;
  },
  images: {
    domains: ["cdn-kvweb.kiotviet.vn", "server.ttexe.id.vn", ""],
    remotePatterns: [
      {
        protocol: "https",
        hostname: "server.ttexe.id.vn",
        pathname: "image/the-template-project/Images/**",
      },
      {
        protocol: "http",
        hostname: "127.0.0.1",
        port: "9000",
        pathname: "/the-template-project/Images/**",
      },
    ],
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

// Kết hợp tất cả plugin
export default withPwa(withAnalyzer(withNextIntl(nextConfig)));
