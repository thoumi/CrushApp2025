# ============================================
# MediaService - Dockerfile
# Multi-stage build pour optimisation
# ============================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["MediaService.csproj", "./"]
RUN dotnet restore "MediaService.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "MediaService.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "MediaService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "MediaService.dll"]


