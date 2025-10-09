# Guide de Migration vers Microservices
## Strangler Fig Pattern - Phase par Phase

---

## 📋 Table des Matières

1. [Vue d'ensemble de la Migration](#vue-densemble)
2. [Prérequis & Préparation](#prérequis--préparation)
3. [Phase 1: Infrastructure de Base](#phase-1-infrastructure-de-base)
4. [Phase 2: Extraction Chatbot Service](#phase-2-extraction-chatbot-service)
5. [Phase 3: Extraction Media Service](#phase-3-extraction-media-service)
6. [Phase 4: Communication Inter-Services](#phase-4-communication-inter-services)
7. [Phase 5: Monitoring & Observabilité](#phase-5-monitoring--observabilité)
8. [Rollback Strategy](#rollback-strategy)
9. [Checklist de Validation](#checklist-de-validation)

---

## 🎯 Vue d'ensemble

### Stratégie : Strangler Fig Pattern

Nous **n'allons PAS réécrire** l'application. Nous allons :
1. Construire les nouveaux services **à côté** du monolithe
2. Rediriger progressivement le trafic via API Gateway
3. Retirer les fonctionnalités du monolithe une fois migrées

```
Semaine 1-2  │  Semaine 3-4  │  Semaine 5-6  │  Semaine 7-8
─────────────┼───────────────┼───────────────┼──────────────
             │               │               │
 Monolithe   │  Monolithe    │  Core API     │  Core API
   100%      │    - AI       │  (reduced)    │  (optimized)
             │               │               │
             │  Chatbot Svc  │  Chatbot Svc  │  Chatbot Svc
             │   (new)       │               │
             │               │  Media Svc    │  Media Svc
             │               │   (new)       │
             │               │               │
             │  Gateway      │  Gateway      │  Gateway
             │  RabbitMQ     │  RabbitMQ     │  + Monitoring
```

### Timeline Estimée

| Phase | Durée | Effort (heures) | Livrable |
|-------|-------|-----------------|----------|
| **Préparation** | 1 semaine | 8-10h | Repo structure, Docker setup |
| **Infrastructure** | 1 semaine | 10-12h | Gateway, RabbitMQ, Docker Compose |
| **Chatbot Service** | 1-2 semaines | 16-20h | Service fonctionnel + tests |
| **Media Service** | 1-2 semaines | 20-24h | Service + events + migration data |
| **Observabilité** | 1 semaine | 8-12h | Logging, health checks |
| **TOTAL** | **6-8 semaines** | **62-78h** | Architecture complète |

---

## 🔧 Prérequis & Préparation

### Outils Requis

```powershell
# Vérifier les installations
node -v          # v22+ (pour Angular)
dotnet --version # 9.0+
docker --version # 24+
docker-compose --version # 2.20+

# Installer si manquant
winget install Docker.DockerDesktop
winget install Microsoft.DotNet.SDK.9
```

### Structure Repository

**Option A: Mono-repo (Recommandé pour apprentissage)**

```
datingapp-2025/
├── src/
│   ├── Gateway/                 # API Gateway (Ocelot)
│   ├── Services/
│   │   ├── CoreAPI/            # Monolith réduit
│   │   ├── ChatbotService/     # Microservice AI
│   │   └── MediaService/       # Microservice Photos
│   └── Client/                 # Angular SPA (inchangé)
├── docs/
│   └── architecture/           # Cette documentation
├── docker-compose.yml          # Orchestration
├── docker-compose.override.yml # Config dev
└── README.md
```

**Option B: Multi-repo (Production-like)**

```
datingapp-gateway/       (repo 1)
datingapp-core-api/      (repo 2)
datingapp-chatbot/       (repo 3)
datingapp-media/         (repo 4)
datingapp-client/        (repo 5)
```

### Préparation Initiale

#### Étape 1: Backup & Branches

```powershell
# Créer branche migration
cd d:\datingapp-2025-main\datingapp-2025-main
git checkout -b feature/microservices-migration

# Backup base de données
docker exec sql /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "REDACTED_DB_PASSWORD" `
  -Q "BACKUP DATABASE [DatingApp] TO DISK='/var/opt/mssql/backup/pre-migration.bak'"
```

#### Étape 2: Structure Directories

```powershell
# Créer structure mono-repo
mkdir src
mkdir src\Gateway
mkdir src\Services
mkdir src\Services\ChatbotService
mkdir src\Services\MediaService

# Déplacer projet existant
Move-Item -Path API -Destination src\Services\CoreAPI
Move-Item -Path client -Destination src\Client
```

#### Étape 3: Définir Contrats API (OpenAPI)

```powershell
# Installer Swashbuckle dans Core API (si pas déjà fait)
cd src\Services\CoreAPI
dotnet add package Swashbuckle.AspNetCore

# Générer swagger.json
dotnet run
curl http://localhost:5001/swagger/v1/swagger.json > ../../../docs/api-contracts/core-api.json
```

---

## 📦 Phase 1: Infrastructure de Base

**Durée** : 1-2 semaines  
**Objectif** : Gateway + RabbitMQ + Docker Compose fonctionnel

### Étape 1.1: Créer API Gateway avec Ocelot

#### Créer le projet

```powershell
cd src\Gateway
dotnet new web -n Gateway
cd Gateway
dotnet add package Ocelot
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

#### Configuration `Program.cs`

```csharp
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// JWT Authentication
var tokenKey = builder.Configuration["TokenKey"] ?? throw new Exception("TokenKey not found");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("AllowAngular");
await app.UseOcelot();

app.Run();
```

#### Configuration `ocelot.json`

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "core-api",
          "Port": 8080
        }
      ],
      "UpstreamPathTemplate": "/api/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE", "PATCH" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

#### `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "TokenKey": "super secret unguessable key for development only",
  "AllowedHosts": "*"
}
```

### Étape 1.2: Docker Compose Initial

#### `docker-compose.yml` (racine projet)

```yaml
version: '3.8'

services:
  # Base de données SQL Server
  sql:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: datingapp-sql
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "REDACTED_DB_PASSWORD"
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql
    networks:
      - datingapp-network

  # RabbitMQ Message Broker
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: datingapp-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    ports:
      - "5672:5672"   # AMQP
      - "15672:15672" # Management UI
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    networks:
      - datingapp-network
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 30s
      timeout: 10s
      retries: 5

  # Core API (Monolith réduit)
  core-api:
    build:
      context: ./src/Services/CoreAPI
      dockerfile: Dockerfile
    container_name: datingapp-core-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sql;Database=DatingApp;User Id=sa;Password=REDACTED_DB_PASSWORD;TrustServerCertificate=True;
      - TokenKey=super secret unguessable key for development only
      - RabbitMQ__Host=rabbitmq
    ports:
      - "5001:8080"
    depends_on:
      - sql
      - rabbitmq
    networks:
      - datingapp-network

  # API Gateway
  gateway:
    build:
      context: ./src/Gateway/Gateway
      dockerfile: Dockerfile
    container_name: datingapp-gateway
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - TokenKey=super secret unguessable key for development only
    ports:
      - "5000:8080"
    depends_on:
      - core-api
    networks:
      - datingapp-network

volumes:
  sql-data:
  rabbitmq-data:

networks:
  datingapp-network:
    driver: bridge
```

### Étape 1.3: Dockerfiles

#### `src/Services/CoreAPI/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["API.csproj", "./"]
RUN dotnet restore "API.csproj"
COPY . .
RUN dotnet build "API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
```

#### `src/Gateway/Gateway/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Gateway.csproj", "./"]
RUN dotnet restore "Gateway.csproj"
COPY . .
RUN dotnet build "Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.dll"]
```

### Étape 1.4: Tester l'Infrastructure

```powershell
# Build et démarrer
docker-compose build
docker-compose up -d

# Vérifier les containers
docker-compose ps

# Logs Gateway
docker-compose logs -f gateway

# Tester routing
curl http://localhost:5000/api/weatherforecast
# Devrait proxy vers core-api

# RabbitMQ Management UI
Start-Process http://localhost:15672
# Login: guest / guest
```

### Étape 1.5: Adapter Angular Frontend

#### `src/Client/src/environments/environment.development.ts`

```typescript
export const environment = {
  production: false,
  // Pointer vers Gateway au lieu de Core API direct
  apiUrl: 'http://localhost:5000/api',
  hubUrl: 'http://localhost:5000/hubs' // SignalR via Gateway
};
```

#### Tester end-to-end

```powershell
cd src\Client
npm start

# Ouvrir http://localhost:4200
# Vérifier que login/register fonctionnent via Gateway
```

---

## 🤖 Phase 2: Extraction Chatbot Service

**Durée** : 1-2 semaines  
**Objectif** : Microservice Chatbot indépendant + routing Gateway

### Étape 2.1: Créer Projet Chatbot Service

```powershell
cd src\Services
dotnet new webapi -n ChatbotService --use-minimal-apis
cd ChatbotService

# Packages nécessaires
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Serilog.AspNetCore
```

### Étape 2.2: Migrer Code Chatbot

#### Structure projet

```
ChatbotService/
├── Program.cs
├── Models/
│   ├── ChatRequest.cs
│   ├── ChatResponse.cs
│   └── OllamaOptions.cs
├── Services/
│   ├── IChatbotService.cs
│   └── OllamaChatbotService.cs
└── appsettings.json
```

#### `Models/ChatRequest.cs`

```csharp
namespace ChatbotService.Models;

public record ChatRequest(string Message, string? UserId = null);

public record ChatResponse(string Reply, DateTime Timestamp);
```

#### `Models/OllamaOptions.cs`

```csharp
namespace ChatbotService.Models;

public class OllamaOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3";
    public double Temperature { get; set; } = 0.7;
}
```

#### `Services/IChatbotService.cs`

```csharp
namespace ChatbotService.Services;

public interface IChatbotService
{
    Task<string> SendMessageAsync(string message, string? context = null);
}
```

#### `Services/OllamaChatbotService.cs`

```csharp
using ChatbotService.Models;
using System.Text;
using System.Text.Json;

namespace ChatbotService.Services;

public class OllamaChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaChatbotService> _logger;

    public OllamaChatbotService(
        HttpClient httpClient,
        OllamaOptions options,
        ILogger<OllamaChatbotService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<string> SendMessageAsync(string message, string? context = null)
    {
        try
        {
            var prompt = string.IsNullOrEmpty(context) 
                ? message 
                : $"{context}\n\nUser: {message}\nAssistant:";

            var request = new
            {
                model = _options.Model,
                prompt = prompt,
                stream = false,
                options = new { temperature = _options.Temperature }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"{_options.BaseUrl}/api/generate",
                content);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseBody);

            return ollamaResponse?.Response ?? "Je n'ai pas pu générer de réponse.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama API");
            return "Désolé, je rencontre un problème technique.";
        }
    }

    private class OllamaResponse
    {
        public string? Response { get; set; }
    }
}
```

#### `Program.cs`

```csharp
using ChatbotService.Models;
using ChatbotService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration Ollama
var ollamaOptions = builder.Configuration.GetSection("Ollama").Get<OllamaOptions>()
    ?? new OllamaOptions();

// Services
builder.Services.AddSingleton(ollamaOptions);
builder.Services.AddHttpClient<IChatbotService, OllamaChatbotService>();

// JWT Authentication
var tokenKey = builder.Configuration["TokenKey"] 
    ?? throw new Exception("TokenKey not found");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapPost("/api/chat/send", async (
    ChatRequest request,
    IChatbotService chatbot,
    HttpContext context) =>
{
    // Optionnel: récupérer userId du JWT
    var userId = context.User?.FindFirst("nameid")?.Value ?? request.UserId;

    var reply = await chatbot.SendMessageAsync(request.Message);

    return Results.Ok(new ChatResponse(reply, DateTime.UtcNow));
})
.RequireAuthorization();

app.MapGet("/api/chat/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
```

#### `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Ollama": {
    "BaseUrl": "http://host.docker.internal:11434",
    "Model": "llama3",
    "Temperature": 0.7
  },
  "TokenKey": "super secret unguessable key for development only",
  "AllowedHosts": "*"
}
```

### Étape 2.3: Dockerfile Chatbot

#### `src/Services/ChatbotService/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ChatbotService.csproj", "./"]
RUN dotnet restore "ChatbotService.csproj"
COPY . .
RUN dotnet build "ChatbotService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatbotService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatbotService.dll"]
```

### Étape 2.4: Ajouter au Docker Compose

#### Ajouter dans `docker-compose.yml`

```yaml
  # Chatbot Microservice
  chatbot-service:
    build:
      context: ./src/Services/ChatbotService
      dockerfile: Dockerfile
    container_name: datingapp-chatbot
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Ollama__BaseUrl=http://host.docker.internal:11434
      - TokenKey=super secret unguessable key for development only
    ports:
      - "5002:8080"
    networks:
      - datingapp-network
    extra_hosts:
      - "host.docker.internal:host-gateway"
```

### Étape 2.5: Configurer Gateway Routing

#### Ajouter route dans `ocelot.json`

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/chat/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "chatbot-service",
          "Port": 8080
        }
      ],
      "UpstreamPathTemplate": "/api/chatbot/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "core-api",
          "Port": 8080
        }
      ],
      "UpstreamPathTemplate": "/api/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE", "PATCH" ]
    }
  ]
}
```

### Étape 2.6: Adapter Frontend

#### `src/Client/src/core/services/chatbot-service.ts`

```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ChatRequest {
  message: string;
  userId?: string;
}

export interface ChatResponse {
  reply: string;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class ChatbotService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  sendMessage(message: string): Observable<ChatResponse> {
    // Route via Gateway: /api/chatbot/send → chatbot-service
    return this.http.post<ChatResponse>(
      `${this.baseUrl}/chatbot/send`,
      { message }
    );
  }
}
```

### Étape 2.7: Supprimer Chatbot du Core API

```powershell
# Supprimer fichiers
cd src\Services\CoreAPI
Remove-Item Controllers\ChatbotController.cs
# Supprimer dans Program.cs les lignes Ollama
```

#### Nettoyer `Program.cs` Core API

```csharp
// SUPPRIMER ces lignes:
// using Chatbot.Core.Models;
// using Chatbot.Core.Services;

// var ollamaOptions = builder.Configuration.GetSection("Ollama").Get<OllamaOptions>();
// builder.Services.AddSingleton<IChatbotService>(new OllamaChatbotService(ollamaOptions));
```

### Étape 2.8: Tests

```powershell
# Rebuild et restart
docker-compose down
docker-compose build
docker-compose up -d

# Test direct Chatbot Service
curl -X POST http://localhost:5002/api/chat/send `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer YOUR_JWT_TOKEN" `
  -d '{"message":"Hello"}'

# Test via Gateway
curl -X POST http://localhost:5000/api/chatbot/send `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer YOUR_JWT_TOKEN" `
  -d '{"message":"Hello via Gateway"}'

# Test Angular
cd src\Client
npm start
# Naviguer vers chatbot, tester conversation
```

---

## 📸 Phase 3: Extraction Media Service

**Durée** : 2-3 semaines (plus complexe que Chatbot)  
**Objectif** : Service Photos + Events RabbitMQ + Migration Data

### Étape 3.1: Créer Projet Media Service

```powershell
cd src\Services
dotnet new webapi -n MediaService
cd MediaService

# Packages
dotnet add package CloudinaryDotNet
dotnet add package RabbitMQ.Client
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### Étape 3.2: Modèles de Données

#### `Models/Photo.cs`

```csharp
namespace MediaService.Models;

public class Photo
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool IsMain { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
}
```

#### `Data/MediaDbContext.cs`

```csharp
using MediaService.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Data;

public class MediaDbContext : DbContext
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options) { }

    public DbSet<Photo> Photos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Photo>()
            .HasIndex(p => p.UserId);

        modelBuilder.Entity<Photo>()
            .HasIndex(p => p.IsApproved);
    }
}
```

### Étape 3.3: Service Cloudinary

#### `Services/IPhotoService.cs`

```csharp
using CloudinaryDotNet.Actions;

namespace MediaService.Services;

public interface IPhotoService
{
    Task<ImageUploadResult> UploadPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}
```

#### `Services/PhotoService.cs`

```csharp
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace MediaService.Services;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResult> UploadPhotoAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();

        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill")
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }
}

public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}
```

### Étape 3.4: Event Publishing (RabbitMQ)

#### `Events/IEventBus.cs`

```csharp
namespace MediaService.Events;

public interface IEventBus
{
    void Publish<T>(T @event) where T : class;
}
```

#### `Events/PhotoUploadedEvent.cs`

```csharp
namespace MediaService.Events;

public record PhotoUploadedEvent(
    Guid PhotoId,
    string UserId,
    string Url,
    string PublicId,
    DateTime UploadedAt
);

public record PhotoApprovedEvent(
    Guid PhotoId,
    string UserId,
    string Url,
    string ApprovedBy,
    DateTime ApprovedAt
);

public record PhotoRejectedEvent(
    Guid PhotoId,
    string UserId,
    string Reason,
    DateTime RejectedAt
);
```

#### `Events/RabbitMQEventBus.cs`

```csharp
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MediaService.Events;

public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQEventBus> _logger;

    public RabbitMQEventBus(IConfiguration config, ILogger<RabbitMQEventBus> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"] ?? "localhost",
            UserName = config["RabbitMQ:User"] ?? "guest",
            Password = config["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Déclarer exchanges
        _channel.ExchangeDeclare("photos", ExchangeType.Topic, durable: true);
    }

    public void Publish<T>(T @event) where T : class
    {
        var eventName = @event.GetType().Name;
        var routingKey = $"photo.{eventName.Replace("Event", "").ToLower()}";

        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: "photos",
            routingKey: routingKey,
            basicProperties: null,
            body: body
        );

        _logger.LogInformation("Published event {EventName} with routing key {RoutingKey}", 
            eventName, routingKey);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
```

### Étape 3.5: Controllers

#### `Controllers/PhotosController.cs`

```csharp
using MediaService.Data;
using MediaService.Events;
using MediaService.Models;
using MediaService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PhotosController : ControllerBase
{
    private readonly MediaDbContext _context;
    private readonly IPhotoService _photoService;
    private readonly IEventBus _eventBus;

    public PhotosController(MediaDbContext context, IPhotoService photoService, IEventBus eventBus)
    {
        _context = context;
        _photoService = photoService;
        _eventBus = eventBus;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var userId = User.FindFirst("nameid")?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _photoService.UploadPhotoAsync(file);

        if (result.Error != null) 
            return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            Url = result.SecureUrl.ToString(),
            PublicId = result.PublicId,
            UserId = userId,
            IsApproved = false
        };

        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();

        // Publish event
        _eventBus.Publish(new PhotoUploadedEvent(
            photo.Id,
            photo.UserId,
            photo.Url,
            photo.PublicId,
            photo.UploadedAt
        ));

        return Ok(photo);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> GetPending()
    {
        var photos = await _context.Photos
            .Where(p => !p.IsApproved)
            .OrderBy(p => p.UploadedAt)
            .ToListAsync();

        return Ok(photos);
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var photo = await _context.Photos.FindAsync(id);
        if (photo == null) return NotFound();

        photo.IsApproved = true;
        photo.ApprovedAt = DateTime.UtcNow;
        photo.ApprovedBy = User.FindFirst("nameid")?.Value;

        await _context.SaveChangesAsync();

        // Publish event
        _eventBus.Publish(new PhotoApprovedEvent(
            photo.Id,
            photo.UserId,
            photo.Url,
            photo.ApprovedBy!,
            photo.ApprovedAt.Value
        ));

        return Ok(photo);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var photo = await _context.Photos.FindAsync(id);
        if (photo == null) return NotFound();

        var userId = User.FindFirst("nameid")?.Value;
        if (photo.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        await _photoService.DeletePhotoAsync(photo.PublicId);
        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
```

### Étape 3.6: Configuration & Program.cs

#### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql;Database=MediaService;User Id=sa;Password=REDACTED_DB_PASSWORD;TrustServerCertificate=True;"
  },
  "CloudinarySettings": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "User": "guest",
    "Password": "guest"
  },
  "TokenKey": "super secret unguessable key for development only"
}
```

#### `Program.cs`

```csharp
using MediaService.Data;
using MediaService.Events;
using MediaService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<MediaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();

// JWT
var tokenKey = builder.Configuration["TokenKey"] ?? throw new Exception("TokenKey not found");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Étape 3.7: Ajouter au Docker Compose

```yaml
  # Media Microservice
  media-service:
    build:
      context: ./src/Services/MediaService
      dockerfile: Dockerfile
    container_name: datingapp-media
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sql;Database=MediaService;User Id=sa;Password=REDACTED_DB_PASSWORD;TrustServerCertificate=True;
      - CloudinarySettings__CloudName=${CLOUDINARY_CLOUD_NAME}
      - CloudinarySettings__ApiKey=${CLOUDINARY_API_KEY}
      - CloudinarySettings__ApiSecret=${CLOUDINARY_API_SECRET}
      - RabbitMQ__Host=rabbitmq
      - TokenKey=super secret unguessable key for development only
    ports:
      - "5003:8080"
    depends_on:
      - sql
      - rabbitmq
    networks:
      - datingapp-network
```

### Étape 3.8: Core API - Event Subscriber

#### Créer `Events/IEventSubscriber.cs` dans Core API

```csharp
namespace API.Events;

public interface IEventSubscriber
{
    void Subscribe();
}
```

#### Créer `Events/RabbitMQSubscriber.cs`

```csharp
using API.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace API.Events;

public class PhotoApprovedEvent
{
    public Guid PhotoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public DateTime ApprovedAt { get; set; }
}

public class RabbitMQSubscriber : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQSubscriber> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQSubscriber(
        IServiceProvider serviceProvider,
        IConfiguration config,
        ILogger<RabbitMQSubscriber> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"] ?? "localhost",
            UserName = config["RabbitMQ:User"] ?? "guest",
            Password = config["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare("photos", ExchangeType.Topic, durable: true);
        _channel.QueueDeclare("core-api-photos", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("core-api-photos", "photos", "photo.*");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;

            _logger.LogInformation("Received message with routing key: {RoutingKey}", routingKey);

            try
            {
                if (routingKey == "photo.photoapproved")
                {
                    var @event = JsonSerializer.Deserialize<PhotoApprovedEvent>(message);
                    if (@event != null)
                    {
                        await HandlePhotoApproved(@event);
                    }
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume("core-api-photos", false, consumer);

        return Task.CompletedTask;
    }

    private async Task HandlePhotoApproved(PhotoApprovedEvent @event)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ajouter photo à la table Photos (ou mettre à jour Member)
        var photo = new Entities.Photo
        {
            Id = (int)@event.PhotoId.GetHashCode(), // Adapter selon votre mapping
            Url = @event.Url,
            IsApproved = true
        };

        // Logique métier pour associer au Member
        _logger.LogInformation("Photo {PhotoId} approved for user {UserId}", @event.PhotoId, @event.UserId);

        await context.SaveChangesAsync();
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
```

#### Enregistrer dans `Program.cs` Core API

```csharp
// Ajouter RabbitMQ
builder.Services.AddHostedService<RabbitMQSubscriber>();
```

---

## 🔗 Phase 4: Communication Inter-Services

Déjà couverte dans Phase 3 avec RabbitMQ. Points supplémentaires :

### Circuit Breaker (Polly)

```powershell
dotnet add package Polly.Extensions.Http
```

```csharp
// Dans un service qui appelle autre service HTTP
builder.Services.AddHttpClient<IChatbotClient, ChatbotClient>()
    .AddTransientHttpErrorPolicy(policy => 
        policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddTransientHttpErrorPolicy(policy => 
        policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
```

---

## 📊 Phase 5: Monitoring & Observabilité

### Étape 5.1: Centralized Logging avec Seq

#### Ajouter au `docker-compose.yml`

```yaml
  seq:
    image: datalust/seq:latest
    container_name: datingapp-seq
    environment:
      ACCEPT_EULA: "Y"
    ports:
      - "5341:80"
    volumes:
      - seq-data:/data
    networks:
      - datingapp-network

volumes:
  seq-data:
```

#### Ajouter Serilog à tous les services

```powershell
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
```

#### `Program.cs` (tous services)

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .Enrich.WithProperty("Service", "ChatbotService") // Nom du service
    .CreateLogger();

builder.Host.UseSerilog();
```

#### `appsettings.json`

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    }
  }
}
```

### Étape 5.2: Health Checks

#### Tous services ajoutent

```csharp
builder.Services.AddHealthChecks()
    .AddDbContext<AppDbContext>() // Si DB
    .AddRabbitMQ(builder.Configuration["RabbitMQ:Host"]); // Si RabbitMQ

app.MapHealthChecks("/health");
```

#### Gateway agrège health checks

```powershell
dotnet add package AspNetCore.HealthChecks.UI
dotnet add package AspNetCore.HealthChecks.UI.Client
dotnet add package AspNetCore.HealthChecks.UI.InMemory.Storage
```

```csharp
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

app.MapHealthChecksUI(options => options.UIPath = "/healthchecks-ui");
```

#### Configuration `appsettings.json` Gateway

```json
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "Core API",
        "Uri": "http://core-api:8080/health"
      },
      {
        "Name": "Chatbot Service",
        "Uri": "http://chatbot-service:8080/health"
      },
      {
        "Name": "Media Service",
        "Uri": "http://media-service:8080/health"
      }
    ],
    "EvaluationTimeInSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}
```

---

## 🔄 Rollback Strategy

### Stratégie de Rollback par Phase

| Phase | Rollback Action | Temps Estimé |
|-------|----------------|--------------|
| **Gateway** | Pointer Angular vers Core API direct | 5 min |
| **Chatbot** | Restaurer ChatbotController dans Core API | 15 min |
| **Media** | Restaurer PhotoService + controllers | 30 min |
| **Full Rollback** | `git checkout main && docker-compose up` | 10 min |

### Feature Flags

#### Core API conserve endpoints Chatbot pendant transition

```csharp
// appsettings.json
"FeatureFlags": {
  "UseChatbotMicroservice": true
}

// Controller
if (!_config.GetValue<bool>("FeatureFlags:UseChatbotMicroservice"))
{
    // Fallback vers ancien code
}
```

---

## ✅ Checklist de Validation

### Infrastructure
- [ ] Docker Compose démarre tous les services
- [ ] RabbitMQ Management UI accessible (localhost:15672)
- [ ] SQL Server accessible
- [ ] Gateway route vers Core API
- [ ] Seq logs visible (localhost:5341)
- [ ] Health checks tous verts

### Chatbot Service
- [ ] Build Docker réussie
- [ ] Service démarre sans erreur
- [ ] Endpoint `/api/chat/send` répond
- [ ] JWT authentication fonctionne
- [ ] Gateway route `/api/chatbot/*` correctement
- [ ] Frontend peut envoyer messages chatbot
- [ ] Logs apparaissent dans Seq

### Media Service
- [ ] Build Docker réussie
- [ ] Migration base de données OK
- [ ] Upload photo fonctionne
- [ ] Event `PhotoUploaded` publié dans RabbitMQ
- [ ] Core API reçoit event `PhotoApproved`
- [ ] Cloudinary intégration fonctionnelle
- [ ] Moderation workflow complet

### End-to-End
- [ ] Login via Gateway fonctionne
- [ ] Membres visibles
- [ ] Messages temps réel (SignalR)
- [ ] Upload photo → modération → apparition profil
- [ ] Chatbot répond aux messages
- [ ] Performance acceptable (<500ms API calls)

---

## 📚 Ressources & Prochaines Étapes

### Documents à Lire Ensuite

1. **[03-IMPLEMENTATION-CHATBOT.md](03-IMPLEMENTATION-CHATBOT.md)** - Détails Chatbot
2. **[05-API-GATEWAY-SETUP.md](05-API-GATEWAY-SETUP.md)** - Configuration avancée Gateway
3. **[07-EVENT-DRIVEN.md](07-EVENT-DRIVEN.md)** - Patterns RabbitMQ

### Améliorations Futures

- [ ] Distributed tracing (OpenTelemetry)
- [ ] Cache Redis pour Chatbot conversations
- [ ] API Versioning
- [ ] GraphQL Gateway (Hot Chocolate)
- [ ] CQRS + Event Sourcing (si pertinent)
- [ ] Kubernetes migration (optionnel)

---

**Bon courage pour la migration ! 🚀**

**Rappel** : C'est un **projet d'apprentissage**, pas de pression pour tout faire parfait du premier coup. L'important est de comprendre les concepts.

---

**Date** : 2025-01-09  
**Version** : 1.0  
**Auteur** : DatingApp Team

