/// <reference lib="webworker" />
import { Serwist, type PrecacheEntry, type SerwistGlobalConfig } from "serwist";

declare global {
  interface WorkerGlobalScope extends SerwistGlobalConfig {
    __SW_MANIFEST: (PrecacheEntry | string)[] | undefined;
  }
}
declare const self: ServiceWorkerGlobalScope;

// Authenticated data remains network-only. An offline POS must not display
// another user's cached account/order data or pretend a mutation succeeded.
const serwist = new Serwist({
  precacheEntries: self.__SW_MANIFEST,
  skipWaiting: true,
  clientsClaim: true,
  runtimeCaching: [],
});

// Remove only cache names owned by the retired next-pwa Workbox defaults.
// Other applications sharing this origin must retain their own caches.
const legacyCacheNames = new Set([
  "google-fonts-webfonts",
  "google-fonts-stylesheets",
  "static-font-assets",
  "static-image-assets",
  "next-image",
  "static-audio-assets",
  "static-video-assets",
  "static-js-assets",
  "static-style-assets",
  "next-data",
  "static-data-assets",
  "apis",
  "others",
  "cross-origin",
]);
self.addEventListener("activate", (event) => {
  event.waitUntil(
    caches
      .keys()
      .then((names) =>
        Promise.all(
          names
            .filter(
              (name) =>
                legacyCacheNames.has(name) ||
                name.startsWith("workbox-precache-v2-"),
            )
            .map((name) => caches.delete(name)),
        ),
      ),
  );
});
serwist.addEventListeners();
