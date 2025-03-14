# Build stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /src

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Copy project files and restore dependencies
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
