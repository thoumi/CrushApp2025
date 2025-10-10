# ============================================
# CrushApp - Core API Dockerfile
# Multi-stage build pour optimisation
# ============================================

# Stage 1 : Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copier le fichier .csproj et restaurer les dépendances
COPY ["API.csproj", "./"]
RUN dotnet restore "API.csproj"

# Copier tout le code source
COPY . .

# Build de l'application
RUN dotnet build "API.csproj" -c Release -o /app/build

# Publish de l'application
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2 : Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Installer wget pour healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Copier les fichiers publiés depuis le stage de build
COPY --from=publish /app/publish .

# Exposer le port 80
EXPOSE 80

# Point d'entrée
ENTRYPOINT ["dotnet", "API.dll"]

