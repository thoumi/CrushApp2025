# Monitoring & Observabilité
## Stratégie Complète pour Microservices

---

## 📋 Table des Matières

1. [Les 3 Piliers de l'Observabilité](#les-3-piliers-de-lobservabilité)
2. [Centralized Logging avec Seq](#centralized-logging-avec-seq)
3. [Health Checks & Monitoring](#health-checks--monitoring)
4. [Distributed Tracing (Optionnel)](#distributed-tracing-optionnel)
5. [Metrics & Dashboards](#metrics--dashboards)
6. [Alerting Strategy](#alerting-strategy)
7. [Debugging Distribué](#debugging-distribué)

---

## 🎯 Les 3 Piliers de l'Observabilité

### 1. **Logs** (ce qui s'est passé)
- Events applicatifs
- Erreurs & exceptions
- Audit trail
- **Outil** : Seq, ELK Stack

### 2. **Metrics** (quand et combien)
- CPU, RAM, requêtes/sec
- Latence P50, P95, P99
- Taux d'erreur
- **Outil** : Prometheus, Grafana

### 3. **Traces** (comment ça s'est propagé)
- Suivi requête à travers services
- Dépendances inter-services
- Bottlenecks identification
- **Outil** : OpenTelemetry, Jaeger

---

## 📊 Architecture Observabilité

```
┌──────────────────────────────────────────────────────────────┐
│                     CLIENT REQUEST                            │
└────────────────────┬─────────────────────────────────────────┘
                     │
            ┌────────▼────────┐
            │   API Gateway   │ ──┐
            └────────┬────────┘   │
                     │             │
        ┌────────────┼────────┐   │
        │            │        │   │
   ┌────▼───┐  ┌────▼───┐ ┌─▼───▼─┐
   │ Core   │  │Chatbot │ │ Media │
   │  API   │  │Service │ │Service│
   └────┬───┘  └────┬───┘ └───┬───┘
        │           │         │
        │           │         │
        └───────┬───┴─────┬───┘
                │         │
         ┌──────▼─────────▼──────┐
         │     Serilog Sinks      │
         └──────┬─────────┬───────┘
                │         │
        ┌───────▼──┐  ┌───▼──────────┐
        │   Seq    │  │ OpenTelemetry│
        │ (Logs)   │  │  (Traces)    │
        └──────────┘  └───┬──────────┘
                          │
                    ┌─────▼──────┐
                    │  Jaeger    │
                    │ (Tracing)  │
                    └────────────┘

        ┌────────────────────────────┐
        │  Prometheus + Grafana      │
        │      (Metrics)             │
        └────────────────────────────┘
```

---

## 🔍 Centralized Logging avec Seq

### Pourquoi Seq ?

✅ **Structured Logging** : JSON, clés-valeurs  
✅ **Query Language** : SQL-like pour filtres  
✅ **Gratuit** : Version self-hosted  
✅ **.NET Native** : Serilog integration parfaite  
✅ **Real-time** : Live tail logs  

### Installation Seq (Docker)

Déjà dans `docker-compose.yml` :

```yaml
  seq:
    image: datalust/seq:2024
    container_name: datingapp-seq
    environment:
      ACCEPT_EULA: "Y"
    ports:
      - "5341:80"
    volumes:
      - seq-data:/data
    networks:
      - datingapp-network
```

### Configuration Serilog (Tous Services)

#### 1. Packages NuGet

```powershell
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Enrichers.Thread
```

#### 2. `Program.cs` Configuration

```csharp
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configuration Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Service", "ChatbotService") // ⚠️ Adapter par service
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Service} {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.Seq(
        serverUrl: builder.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341",
        apiKey: builder.Configuration["Seq:ApiKey"] // Optionnel
    )
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting {Service}", "ChatbotService");
    
    // ... reste de la config
    
    var app = builder.Build();
    
    // Middleware request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("nameid")?.Value);
        };
    });
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "{Service} terminated unexpectedly", "ChatbotService");
}
finally
{
    Log.CloseAndFlush();
}
```

#### 3. `appsettings.json`

```json
{
  "Seq": {
    "ServerUrl": "http://seq:5341",
    "ApiKey": ""
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### Utilisation Logging dans Code

#### Injection ILogger

```csharp
public class ChatbotService : IChatbotService
{
    private readonly ILogger<ChatbotService> _logger;
    
    public ChatbotService(ILogger<ChatbotService> logger)
    {
        _logger = logger;
    }
    
    public async Task<string> SendMessageAsync(string message)
    {
        _logger.LogInformation(
            "Chatbot request received: {Message} from User {UserId}",
            message,
            _currentUserId // Context enrichment
        );
        
        try
        {
            var response = await _ollamaClient.GenerateAsync(message);
            
            _logger.LogInformation(
                "Chatbot response generated in {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds
            );
            
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Ollama API call failed for message: {Message}",
                message
            );
            throw;
        }
    }
}
```

#### Structured Logging Best Practices

```csharp
// ✅ DO: Structured avec propriétés
_logger.LogInformation(
    "User {UserId} uploaded photo {PhotoId} to {Service}",
    userId,
    photoId,
    "MediaService"
);

// ❌ DON'T: String interpolation (perte structure)
_logger.LogInformation($"User {userId} uploaded photo {photoId}");
```

### Queries Seq Utiles

#### 1. Erreurs dans les 24h

```sql
Level = 'Error' AND @Timestamp > Now() - 1d
ORDER BY @Timestamp DESC
```

#### 2. Requêtes lentes (>1s)

```sql
RequestPath IS NOT NULL 
AND Elapsed > 1000
ORDER BY Elapsed DESC
```

#### 3. Erreurs par service

```sql
Level = 'Error' 
GROUP BY Service
SELECT COUNT(*) AS ErrorCount
```

#### 4. Chatbot requests par utilisateur

```sql
Service = 'ChatbotService' 
AND MessageTemplate LIKE '%request received%'
GROUP BY UserId
SELECT COUNT(*)
```

### Seq Dashboards

#### Créer Dashboard

1. Accéder Seq UI : `http://localhost:5341`
2. Aller dans **Dashboards** → **New Dashboard**
3. Ajouter **Charts** :

**Chart 1: Requests par Service**
```sql
SELECT COUNT(*) AS Requests
FROM stream
WHERE RequestPath IS NOT NULL
GROUP BY Service
TIME 1h
```

**Chart 2: Taux d'Erreur**
```sql
SELECT 
  COUNT(CASE WHEN Level = 'Error' THEN 1 END) * 100.0 / COUNT(*) AS ErrorRate
FROM stream
TIME 5m
```

**Chart 3: Latence P95**
```sql
SELECT PERCENTILE(Elapsed, 95) AS P95Latency
FROM stream
WHERE Elapsed IS NOT NULL
TIME 5m
```

---

## ❤️ Health Checks & Monitoring

### Implémentation Health Checks

#### 1. Packages NuGet

```powershell
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
dotnet add package AspNetCore.HealthChecks.SqlServer
dotnet add package AspNetCore.HealthChecks.RabbitMQ
dotnet add package AspNetCore.HealthChecks.UI
dotnet add package AspNetCore.HealthChecks.UI.Client
dotnet add package AspNetCore.HealthChecks.UI.InMemory.Storage
```

#### 2. Configuration Health Checks (Core API exemple)

```csharp
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sql-server",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "db", "sql" },
        timeout: TimeSpan.FromSeconds(3)
    )
    .AddRabbitMQ(
        rabbitConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Host"]}",
        name: "rabbitmq",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "messaging" },
        timeout: TimeSpan.FromSeconds(3)
    )
    .AddUrlGroup(
        uri: new Uri("http://chatbot-service:8080/api/chat/health"),
        name: "chatbot-service",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "services" },
        timeout: TimeSpan.FromSeconds(5)
    )
    .AddCheck<CustomHealthCheck>("custom-logic");

var app = builder.Build();

// Endpoint health check JSON
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Endpoint simple (Kubernetes liveness)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Aucun check, juste "alive"
});

// Endpoint readiness (avec checks)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

#### 3. Custom Health Check Exemple

```csharp
public class CustomHealthCheck : IHealthCheck
{
    private readonly IUnitOfWork _unitOfWork;
    
    public CustomHealthCheck(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Vérifier connexion DB + logique métier
            var memberCount = await _unitOfWork.MemberRepository.GetMemberCountAsync();
            
            if (memberCount < 0)
            {
                return HealthCheckResult.Unhealthy(
                    "Database contains invalid data"
                );
            }
            
            return HealthCheckResult.Healthy(
                $"Database operational with {memberCount} members"
            );
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database check failed",
                exception: ex
            );
        }
    }
}
```

### Health Checks UI (Gateway)

#### Configuration Gateway

```csharp
var builder = WebApplication.CreateBuilder(args);

// Health Checks UI
builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(15); // Check chaque 15s
    setup.MaximumHistoryEntriesPerEndpoint(50);
    setup.AddHealthCheckEndpoint("Gateway", "http://localhost:8080/health");
    setup.AddHealthCheckEndpoint("Core API", "http://core-api:8080/health");
    setup.AddHealthCheckEndpoint("Chatbot", "http://chatbot-service:8080/health");
    setup.AddHealthCheckEndpoint("Media", "http://media-service:8080/health");
})
.AddInMemoryStorage();

var app = builder.Build();

// UI accessible sur /healthchecks-ui
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/healthchecks-ui";
    options.ApiPath = "/healthchecks-api";
});
```

#### Accès UI

```
http://localhost:5000/healthchecks-ui
```

### Réponse Health Check JSON

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "sql-server": {
      "status": "Healthy",
      "description": "SQL Server is responding",
      "duration": "00:00:00.0456789",
      "tags": ["db", "sql"]
    },
    "rabbitmq": {
      "status": "Healthy",
      "description": "RabbitMQ is responding",
      "duration": "00:00:00.0123456",
      "tags": ["messaging"]
    },
    "chatbot-service": {
      "status": "Degraded",
      "description": "HTTP 200 but high latency",
      "duration": "00:00:04.9876543",
      "tags": ["services"]
    }
  }
}
```

---

## 🔗 Distributed Tracing (Optionnel)

### OpenTelemetry + Jaeger

#### 1. Packages NuGet (tous services)

```powershell
dotnet add package OpenTelemetry.Exporter.Jaeger
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.SqlClient
```

#### 2. Configuration `Program.cs`

```csharp
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("ChatbotService")
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: "ChatbotService", serviceVersion: "1.0.0")
            )
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
                options.RecordException = true;
            })
            .AddJaegerExporter(options =>
            {
                options.AgentHost = builder.Configuration["Jaeger:Host"] ?? "localhost";
                options.AgentPort = int.Parse(builder.Configuration["Jaeger:Port"] ?? "6831");
            });
    });
```

#### 3. Jaeger Docker (ajouter dans docker-compose.yml)

```yaml
  jaeger:
    image: jaegertracing/all-in-one:latest
    container_name: datingapp-jaeger
    environment:
      COLLECTOR_ZIPKIN_HOST_PORT: ":9411"
    ports:
      - "5775:5775/udp"
      - "6831:6831/udp"
      - "6832:6832/udp"
      - "5778:5778"
      - "16686:16686" # Jaeger UI
      - "14268:14268"
      - "14250:14250"
      - "9411:9411"
    networks:
      - datingapp-network
```

#### 4. Accès Jaeger UI

```
http://localhost:16686
```

### Exemple Trace Visualisée

```
User Request → Gateway → Core API → Media Service
                   │
                   └──→ Chatbot Service → Ollama API

Trace ID: 1a2b3c4d5e6f
├─ Gateway (20ms)
│  ├─ JWT Validation (5ms)
│  └─ Routing (2ms)
├─ Core API (150ms)
│  ├─ Auth Check (10ms)
│  ├─ DB Query Members (120ms) ⚠️ SLOW
│  └─ Response Serialize (20ms)
└─ Chatbot Service (300ms)
   └─ Ollama Generate (290ms)
```

---

## 📈 Metrics & Dashboards

### Prometheus + Grafana (Optionnel mais Pro)

#### 1. Packages .NET

```powershell
dotnet add package prometheus-net.AspNetCore
```

#### 2. Configuration `Program.cs`

```csharp
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Metrics endpoint /metrics
app.UseMetricServer(); // ou app.MapMetrics();

// HTTP metrics automatiques
app.UseHttpMetrics();

app.Run();
```

#### 3. Custom Metrics

```csharp
using Prometheus;

public class ChatbotService
{
    private static readonly Counter MessageCounter = Metrics
        .CreateCounter("chatbot_messages_total", "Total chatbot messages processed");
    
    private static readonly Histogram ResponseTime = Metrics
        .CreateHistogram("chatbot_response_duration_seconds", "Chatbot response time");
    
    public async Task<string> SendMessageAsync(string message)
    {
        using (ResponseTime.NewTimer())
        {
            MessageCounter.Inc();
            
            // ... logique
            
            return response;
        }
    }
}
```

#### 4. Prometheus Docker

```yaml
  prometheus:
    image: prom/prometheus:latest
    container_name: datingapp-prometheus
    volumes:
      - ./prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus-data:/prometheus
    ports:
      - "9090:9090"
    networks:
      - datingapp-network
```

#### `prometheus/prometheus.yml`

```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'gateway'
    static_configs:
      - targets: ['gateway:8080']
  
  - job_name: 'core-api'
    static_configs:
      - targets: ['core-api:8080']
  
  - job_name: 'chatbot-service'
    static_configs:
      - targets: ['chatbot-service:8080']
  
  - job_name: 'media-service'
    static_configs:
      - targets: ['media-service:8080']
```

#### 5. Grafana Docker

```yaml
  grafana:
    image: grafana/grafana:latest
    container_name: datingapp-grafana
    environment:
      GF_SECURITY_ADMIN_PASSWORD: admin
    ports:
      - "3000:3000"
    volumes:
      - grafana-data:/var/lib/grafana
    networks:
      - datingapp-network
```

#### Dashboard Grafana Exemple

1. Accéder `http://localhost:3000` (admin/admin)
2. Add Data Source → Prometheus → `http://prometheus:9090`
3. Import Dashboard ID `1860` (Node Exporter)
4. Créer dashboard custom :

**Panel: Requests per Service**
```promql
rate(http_requests_total[5m])
```

**Panel: P95 Latency**
```promql
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))
```

**Panel: Error Rate**
```promql
rate(http_requests_total{status=~"5.."}[5m])
```

---

## 🚨 Alerting Strategy

### Seq Alerts

#### 1. Créer Alert dans Seq UI

**Alert: High Error Rate**
```sql
SELECT COUNT(*) AS ErrorCount
FROM stream
WHERE Level = 'Error'
  AND @Timestamp > Now() - 5m
GROUP BY Service
HAVING COUNT(*) > 10
```

**Action** : Send to Webhook (Slack, Teams, Email)

#### 2. Webhook Slack Exemple

```json
{
  "text": "⚠️ High error rate detected in {Service}: {ErrorCount} errors in 5 minutes"
}
```

### Grafana Alerts

**Alert: High Memory Usage**
```yaml
Condition: 
  WHEN avg() OF query(A, 5m, now) IS ABOVE 80

Notification:
  - Slack channel #ops-alerts
  - Email: team@example.com
```

---

## 🐛 Debugging Distribué

### Correlation IDs

#### 1. Middleware Gateway

```csharp
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString();
    
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers.Add("X-Correlation-ID", correlationId);
    
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});
```

#### 2. Propagation vers Services

```csharp
public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = _httpContextAccessor.HttpContext?
            .Items["CorrelationId"]?.ToString();
        
        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.Add("X-Correlation-ID", correlationId);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}

// Enregistrer
builder.Services.AddHttpClient<IChatbotClient, ChatbotClient>()
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
```

#### 3. Query dans Seq

```sql
CorrelationId = 'abc-123-def'
ORDER BY @Timestamp
```

### Distributed Debugging Workflow

```
1. User signale erreur
2. Récupérer Correlation ID depuis response header
3. Query Seq avec Correlation ID
4. Visualiser flow complet:
   - Gateway received request
   - Core API processed
   - Media Service uploaded
   - ERROR: Cloudinary timeout ← ROOT CAUSE
```

---

## 📚 Récapitulatif Outils

| Pilier | Outil | Port | Usage |
|--------|-------|------|-------|
| **Logs** | Seq | 5341 | Centralized structured logging |
| **Metrics** | Prometheus | 9090 | Time-series metrics |
| **Metrics** | Grafana | 3000 | Dashboards & alerting |
| **Traces** | Jaeger | 16686 | Distributed tracing |
| **Health** | HealthChecks UI | 5000/healthchecks-ui | Service health monitoring |

---

## ✅ Checklist Production-Ready

- [ ] Tous services logs vers Seq
- [ ] Correlation IDs propagés
- [ ] Health checks implémentés
- [ ] Metrics exposées `/metrics`
- [ ] Grafana dashboards créés
- [ ] Alerts configurés (erreurs, latence, disponibilité)
- [ ] Distributed tracing activé (optionnel)
- [ ] Runbooks documentation (comment debug chaque alert)

---

**Prochaines Étapes** : [05-CI-CD.md](05-CI-CD.md) pour pipeline déploiement.

---

**Date** : 2025-01-09  
**Version** : 1.0

