# Use the official .NET runtime image as a base for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0.12 AS base
WORKDIR /app
EXPOSE 8080

# Cài các thư viện cần thiết để chạy Chrome headless
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    libnss3 \
    libatk-bridge2.0-0 \
    libgtk-3-0 \
    libxss1 \
    libasound2 \
    libgbm-dev \
    libglib2.0-0 \
    libdrm2 \
    libx11-xcb1 \
    libxcomposite1 \
    libxcursor1 \
    libxdamage1 \
    libxrandr2 \
    libatk1.0-0 \
    libpangocairo-1.0-0 \
    libpango-1.0-0 \
    fonts-liberation \
    libappindicator3-1 \
    libxext6 \
    --no-install-recommends && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy the published application from the workflow
FROM base AS final
WORKDIR /app
COPY src/EcommeceService/app/publish .
COPY src/EcommeceService/Presentation/wwwroot wwwroot

ENTRYPOINT ["dotnet", "Presentation.dll"]
