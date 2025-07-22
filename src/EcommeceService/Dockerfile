# Use the official .NET runtime image as a base for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0.12 AS base
WORKDIR /app
EXPOSE 8080


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
    libappindicator3-1 \
    libxext6 \
    --no-install-recommends && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy the published application
FROM base AS final
WORKDIR /app
COPY src/EcommeceService/app/publish .
COPY src/EcommeceService/Presentation/wwwroot wwwroot

# 🔽 Copy font Times New Roman từ thư mục bạn chỉ định
COPY src/EcommeceService/Presentation/Data/Font /usr/share/fonts/truetype/custom/

# Làm mới cache font
RUN fc-cache -f -v

ENTRYPOINT ["dotnet", "Presentation.dll"]
