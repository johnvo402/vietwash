# syntax=docker/dockerfile:1
# Match the supported Node 22 LTS major used by frontend CI.
FROM node:22-alpine AS dependencies
WORKDIR /app
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

FROM dependencies AS builder
RUN apk add --no-cache openjdk17-jre-headless
COPY frontend/ ./
ARG NEXT_PUBLIC_MEDIA_URL=""
ARG CSP_CONNECT_SRC=""
ENV NEXT_TELEMETRY_DISABLED=1 \
    NEXT_PUBLIC_API_URL="" \
    NEXT_PUBLIC_CLIENT_ID=admin-laundry \
    NEXT_PUBLIC_PLATFORM=admin-web \
    NEXT_PUBLIC_MEDIA_URL=${NEXT_PUBLIC_MEDIA_URL} \
    CSP_CONNECT_SRC=${CSP_CONNECT_SRC}
RUN npm run generate && npm run build:test

FROM node:22-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production NEXT_TELEMETRY_DISABLED=1 PORT=3000 HOSTNAME=0.0.0.0
RUN addgroup --system --gid 1001 nodejs && adduser --system --uid 1001 nextjs
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static
COPY --from=builder --chown=nextjs:nodejs /app/public ./public
USER nextjs
EXPOSE 3000
CMD ["node", "server.js"]
