# Build stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /src

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Copy project files and restore dependencies
COPY ["src/Micro.Shared/Micro.Shared.csproj", "Micro.Shared/"]
COPY ["src/ApiGateway/ApiGateway.csproj", "ApiGateway/"]

# Restore dependencies
RUN dotnet restore "./ApiGateway/ApiGateway.csproj"
COPY . .
# Build the application
WORKDIR /src/ApiGateway
RUN dotnet build "./ApiGateway.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "./ApiGateway.csproj" --no-restore -c Release -o /app/publish

FROM base AS final

WORKDIR /app

# Copy the published app from the build stage
COPY --from=publish /app/publish .

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
