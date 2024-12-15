# Build stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /src

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Copy project files and restore dependencies
COPY ["src/Micro.Shared/Micro.Shared.csproj", "Micro.Shared/"]
COPY ["src/AuthService/AuthService.API/AuthService.API.csproj", "AuthService/AuthService.API/"]
COPY ["src/AuthService/AuthService.Application/AuthService.Application.csproj", "AuthService/AuthService.Application/"]
COPY ["src/AuthService/AuthService.Domain/AuthService.Domain.csproj", "AuthService/AuthService.Domain/"]
COPY ["src/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService/AuthService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "./AuthService/AuthService.API/AuthService.API.csproj"
COPY . .
# Build the application
WORKDIR /src/AuthService/AuthService.API
RUN dotnet build "./AuthService.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "./AuthService.API.csproj" --no-restore -c Release -o /app/publish

FROM base AS final

WORKDIR /app

# Copy the published app from the build stage
COPY --from=publish /app/publish .

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
