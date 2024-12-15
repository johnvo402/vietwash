# Build stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /src
EXPOSE 80
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Copy project files and restore dependencies
COPY ["src/Micro.Shared/Micro.Shared.csproj", "Micro.Shared/"]
COPY ["src/ProductService/ProductService.API/ProductService.API.csproj", "ProductService/ProductService.API/"]
COPY ["src/ProductService/ProductService.Application/ProductService.Application.csproj", "ProductService/ProductService.Application/"]
COPY ["src/ProductService/ProductService.Domain/ProductService.Domain.csproj", "ProductService/ProductService.Domain/"]
COPY ["src/ProductService/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "ProductService/ProductService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "./ProductService/ProductService.API/ProductService.API.csproj"
COPY . .
# Build the application
WORKDIR /src/ProductService/ProductService.API
RUN dotnet build "./ProductService.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "./ProductService.API.csproj" --no-restore -c Release -o /app/publish

FROM base AS final

WORKDIR /app

# Copy the published app from the build stage
COPY --from=publish /app/publish .

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "ProductService.API.dll"]
