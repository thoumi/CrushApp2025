# Docker Compose - Configuration Complète
## Orchestration Multi-Services pour DatingApp

---

## 📋 Vue d'ensemble

Ce document détaille la configuration Docker Compose complète pour l'architecture microservices.

### Services Orchestrés

| Service | Port(s) | Dépendances | Rôle |
|---------|---------|-------------|------|
| **gateway** | 5000 | core-api, chatbot-service, media-service | Point d'entrée unique |
| **core-api** | 5001 | sql, rabbitmq | Monolith réduit (Auth, Members, Messages) |
| **chatbot-service** | 5002 | - | Microservice AI |
| **media-service** | 5003 | sql, rabbitmq | Microservice Photos |
| **sql** | 1433 | - | SQL Server 2022 |
| **rabbitmq** | 5672, 15672 | - | Message Broker |
| **seq** | 5341 | - | Centralized Logging |
| **client** | 4200 | gateway | Angular SPA |

---

## 📄 Fichier docker-compose.yml Complet

```yaml
version: '3.8'

services:
  #############################################
  # INFRASTRUCTURE SERVICES
  #############################################

  # SQL Server Database
  sql:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: datingapp-sql
    hostname: sql
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "REDACTED_DB_PASSWORD"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init-db.sql:ro
    networks:
      - datingapp-network
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P REDACTED_DB_PASSWORD -Q 'SELECT 1' || exit 1"]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 60s
    restart: unless-stopped

  # RabbitMQ Message Broker
  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    container_name: datingapp-rabbitmq
    hostname: rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
      RABBITMQ_DEFAULT_VHOST: /
    ports:
      - "5672:5672"   # AMQP protocol
      - "15672:15672" # Management UI
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
      - ./rabbitmq/rabbitmq.conf:/etc/rabbitmq/rabbitmq.conf:ro
      - ./rabbitmq/definitions.json:/etc/rabbitmq/definitions.json:ro
    networks:
      - datingapp-network
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 30s
    restart: unless-stopped

  # Seq Centralized Logging
  seq:
    image: datalust/seq:2024
    container_name: datingapp-seq
    hostname: seq
    environment:
      ACCEPT_EULA: "Y"
      SEQ_FIRSTRUN_ADMINPASSWORDHASH: "${SEQ_ADMIN_PASSWORD_HASH:-}"
    ports:
      - "5341:80"   # Web UI
      - "5342:5341" # Ingestion
    volumes:
      - seq-data:/data
    networks:
      - datingapp-network
    restart: unless-stopped

  #############################################
  # APPLICATION SERVICES
  #############################################

  # API Gateway (Ocelot)
  gateway:
    build:
      context: ./src/Gateway/Gateway
      dockerfile: Dockerfile
      args:
        CONFIGURATION: ${BUILD_CONFIGURATION:-Release}
    image: datingapp-gateway:latest
    container_name: datingapp-gateway
    hostname: gateway
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development}
      - ASPNETCORE_URLS=http://+:8080
      - TokenKey=${JWT_TOKEN_KEY}
      - Seq__ServerUrl=http://seq:5341
    ports:
      - "5000:8080"
    depends_on:
      core-api:
        condition: service_healthy
      chatbot-service:
        condition: service_started
      media-service:
        condition: service_started
      seq:
        condition: service_started
    networks:
      - datingapp-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    restart: unless-stopped
    labels:
      - "com.datingapp.service=gateway"
      - "com.datingapp.description=API Gateway with Ocelot"

  # Core API (Monolith réduit)
  core-api:
    build:
      context: ./src/Services/CoreAPI
      dockerfile: Dockerfile
      args:
        CONFIGURATION: ${BUILD_CONFIGURATION:-Release}
    image: datingapp-core-api:latest
    container_name: datingapp-core-api
    hostname: core-api
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development}
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Server=sql;Database=DatingApp;User Id=sa;Password=REDACTED_DB_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true;
      - TokenKey=${JWT_TOKEN_KEY}
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__User=guest
      - RabbitMQ__Password=guest
      - Seq__ServerUrl=http://seq:5341
    ports:
      - "5001:8080"
    depends_on:
      sql:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      seq:
        condition: service_started
    networks:
      - datingapp-network
    volumes:
      - ./logs/core-api:/app/logs
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 60s
    restart: unless-stopped
    labels:
      - "com.datingapp.service=core-api"
      - "com.datingapp.description=Core API - Auth, Members, Messages"

  # Chatbot Microservice
  chatbot-service:
    build:
      context: ./src/Services/ChatbotService
      dockerfile: Dockerfile
      args:
        CONFIGURATION: ${BUILD_CONFIGURATION:-Release}
    image: datingapp-chatbot:latest
    container_name: datingapp-chatbot
    hostname: chatbot-service
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development}
      - ASPNETCORE_URLS=http://+:8080
      - Ollama__BaseUrl=${OLLAMA_BASE_URL:-http://host.docker.internal:11434}
      - Ollama__Model=${OLLAMA_MODEL:-llama3}
      - Ollama__Temperature=0.7
      - TokenKey=${JWT_TOKEN_KEY}
      - Seq__ServerUrl=http://seq:5341
      - Redis__ConnectionString=${REDIS_CONNECTION:-}
    ports:
      - "5002:8080"
    depends_on:
      seq:
        condition: service_started
    networks:
      - datingapp-network
    extra_hosts:
      - "host.docker.internal:host-gateway"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/api/chat/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 20s
    restart: unless-stopped
    labels:
      - "com.datingapp.service=chatbot"
      - "com.datingapp.description=AI Chatbot with Ollama"
    # Resource limits pour isoler CPU/RAM
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 2G
        reservations:
          cpus: '0.5'
          memory: 512M

  # Media Microservice
  media-service:
    build:
      context: ./src/Services/MediaService
      dockerfile: Dockerfile
      args:
        CONFIGURATION: ${BUILD_CONFIGURATION:-Release}
    image: datingapp-media:latest
    container_name: datingapp-media
    hostname: media-service
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development}
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Server=sql;Database=MediaService;User Id=sa;Password=REDACTED_DB_PASSWORD;TrustServerCertificate=True;
      - CloudinarySettings__CloudName=${CLOUDINARY_CLOUD_NAME}
      - CloudinarySettings__ApiKey=${CLOUDINARY_API_KEY}
      - CloudinarySettings__ApiSecret=${CLOUDINARY_API_SECRET}
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__User=guest
      - RabbitMQ__Password=guest
      - TokenKey=${JWT_TOKEN_KEY}
      - Seq__ServerUrl=http://seq:5341
    ports:
      - "5003:8080"
    depends_on:
      sql:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      seq:
        condition: service_started
    networks:
      - datingapp-network
    volumes:
      - media-temp:/app/temp-uploads
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    restart: unless-stopped
    labels:
      - "com.datingapp.service=media"
      - "com.datingapp.description=Media/Photo Management Service"

  #############################################
  # FRONTEND (Optionnel en Docker)
  #############################################

  # Angular Client (dev mode)
  client:
    build:
      context: ./src/Client
      dockerfile: Dockerfile.dev
      args:
        NODE_VERSION: 22
    image: datingapp-client:dev
    container_name: datingapp-client
    hostname: client
    environment:
      - NODE_ENV=development
      - API_URL=http://gateway:8080
    ports:
      - "4200:4200"
    depends_on:
      - gateway
    networks:
      - datingapp-network
    volumes:
      - ./src/Client:/app
      - /app/node_modules
    command: npm start
    restart: unless-stopped
    labels:
      - "com.datingapp.service=frontend"
      - "com.datingapp.description=Angular SPA"

#############################################
# VOLUMES
#############################################

volumes:
  sql-data:
    name: datingapp-sql-data
    driver: local
  
  rabbitmq-data:
    name: datingapp-rabbitmq-data
    driver: local
  
  seq-data:
    name: datingapp-seq-data
    driver: local
  
  media-temp:
    name: datingapp-media-temp
    driver: local

#############################################
# NETWORKS
#############################################

networks:
  datingapp-network:
    name: datingapp-network
    driver: bridge
    ipam:
      config:
        - subnet: 172.20.0.0/16
```

---

## 📄 Fichier .env (Variables d'Environnement)

Créer `.env` à la racine :

```bash
# Build Configuration
BUILD_CONFIGURATION=Release
ASPNETCORE_ENVIRONMENT=Development

# JWT Security
JWT_TOKEN_KEY=super_secret_unguessable_key_min_32_chars_for_production_use_vault

# Cloudinary (Photo Storage)
CLOUDINARY_CLOUD_NAME=your-cloud-name
CLOUDINARY_API_KEY=your-api-key
CLOUDINARY_API_SECRET=your-api-secret

# Ollama AI
OLLAMA_BASE_URL=http://host.docker.internal:11434
OLLAMA_MODEL=llama3

# Seq Logging (optionnel)
# Générer hash: echo -n "YourPassword" | docker run --rm -i datalust/seq config hash
SEQ_ADMIN_PASSWORD_HASH=

# Redis (optionnel pour Chatbot cache)
REDIS_CONNECTION=

# Timezone
TZ=Europe/Paris
```

**⚠️ Important** : Ajouter `.env` au `.gitignore` !

```bash
# .gitignore
.env
.env.local
.env.*.local
```

---

## 📄 Configuration RabbitMQ

### `rabbitmq/rabbitmq.conf`

```conf
# Persist messages
queue_master_locator = min-masters
disk_free_limit.absolute = 2GB

# Management plugin
management.load_definitions = /etc/rabbitmq/definitions.json

# Logging
log.console = true
log.console.level = info
```

### `rabbitmq/definitions.json`

```json
{
  "users": [
    {
      "name": "guest",
      "password": "guest",
      "tags": "administrator"
    }
  ],
  "vhosts": [
    {
      "name": "/"
    }
  ],
  "permissions": [
    {
      "user": "guest",
      "vhost": "/",
      "configure": ".*",
      "write": ".*",
      "read": ".*"
    }
  ],
  "exchanges": [
    {
      "name": "photos",
      "vhost": "/",
      "type": "topic",
      "durable": true,
      "auto_delete": false
    }
  ],
  "queues": [
    {
      "name": "core-api-photos",
      "vhost": "/",
      "durable": true,
      "auto_delete": false
    }
  ],
  "bindings": [
    {
      "source": "photos",
      "vhost": "/",
      "destination": "core-api-photos",
      "destination_type": "queue",
      "routing_key": "photo.*"
    }
  ]
}
```

---

## 📄 docker-compose.override.yml (Dev Local)

Pour configurations spécifiques dev (optionnel) :

```yaml
version: '3.8'

services:
  gateway:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - ./src/Gateway/Gateway:/app
      - /app/bin
      - /app/obj

  core-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - ./src/Services/CoreAPI:/app
      - /app/bin
      - /app/obj

  chatbot-service:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - ./src/Services/ChatbotService:/app
      - /app/bin
      - /app/obj

  media-service:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - ./src/Services/MediaService:/app
      - /app/bin
      - /app/obj
```

---

## 🚀 Commandes Utiles

### Démarrage & Arrêt

```powershell
# Build tous les services
docker-compose build

# Démarrer en détaché
docker-compose up -d

# Démarrer avec logs
docker-compose up

# Démarrer services spécifiques
docker-compose up -d gateway core-api

# Arrêter tous les services
docker-compose down

# Arrêter et supprimer volumes (⚠️ perte données)
docker-compose down -v
```

### Monitoring

```powershell
# Voir statut tous services
docker-compose ps

# Logs d'un service spécifique
docker-compose logs -f gateway

# Logs de tous les services
docker-compose logs -f

# Logs des 100 dernières lignes
docker-compose logs --tail=100 core-api

# Stream logs temps réel
docker-compose logs -f --tail=0 chatbot-service
```

### Gestion Services

```powershell
# Restart un service
docker-compose restart core-api

# Rebuild et restart un service
docker-compose up -d --build core-api

# Scaler un service (horizontalement)
docker-compose up -d --scale chatbot-service=3

# Exec dans un container
docker-compose exec core-api /bin/bash

# Voir les ressources utilisées
docker stats
```

### Base de Données

```powershell
# Se connecter à SQL Server
docker-compose exec sql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "REDACTED_DB_PASSWORD"

# Backup database
docker-compose exec sql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "REDACTED_DB_PASSWORD" -Q "BACKUP DATABASE [DatingApp] TO DISK='/var/opt/mssql/backup/datingapp.bak'"

# Copier backup vers host
docker cp datingapp-sql:/var/opt/mssql/backup/datingapp.bak ./backups/
```

### RabbitMQ

```powershell
# Voir queues
docker-compose exec rabbitmq rabbitmqctl list_queues

# Voir exchanges
docker-compose exec rabbitmq rabbitmqctl list_exchanges

# Voir bindings
docker-compose exec rabbitmq rabbitmqctl list_bindings

# Purger une queue
docker-compose exec rabbitmq rabbitmqctl purge_queue core-api-photos
```

### Nettoyage

```powershell
# Supprimer containers arrêtés
docker container prune

# Supprimer images non utilisées
docker image prune

# Supprimer volumes non utilisés
docker volume prune

# Nettoyage complet (⚠️ dangereux)
docker system prune -a --volumes
```

---

## 🔧 Troubleshooting

### Problème: Service ne démarre pas

```powershell
# Voir logs détaillés
docker-compose logs service-name

# Vérifier health check
docker-compose ps

# Rebuild from scratch
docker-compose down
docker-compose build --no-cache service-name
docker-compose up -d
```

### Problème: SQL Server connexion refusée

```powershell
# Vérifier que SQL est healthy
docker-compose ps sql

# Tester connexion manuelle
docker-compose exec core-api ping sql

# Vérifier network
docker network inspect datingapp-network
```

### Problème: RabbitMQ messages non consommés

```powershell
# Accéder Management UI
Start-Process http://localhost:15672

# Vérifier consumers
docker-compose exec rabbitmq rabbitmqctl list_consumers

# Vérifier messages dans queue
docker-compose exec rabbitmq rabbitmqctl list_queues name messages
```

### Problème: Gateway ne route pas

```powershell
# Tester direct service (bypass Gateway)
curl http://localhost:5001/api/weatherforecast

# Tester via Gateway
curl http://localhost:5000/api/weatherforecast

# Vérifier config Ocelot chargée
docker-compose logs gateway | Select-String "ocelot"
```

---

## 📊 Architecture Network Docker

```
┌────────────────────────────────────────────────────────────┐
│            datingapp-network (172.20.0.0/16)               │
│                                                            │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐ │
│  │ gateway  │  │ core-api │  │ chatbot  │  │  media   │ │
│  │ :8080    │  │ :8080    │  │ :8080    │  │  :8080   │ │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘ │
│       │             │             │             │        │
│       └─────────────┼─────────────┴─────────────┘        │
│                     │                                     │
│  ┌──────────┐  ┌───┴──────┐  ┌──────────┐  ┌──────────┐ │
│  │   seq    │  │   sql    │  │ rabbitmq │  │  client  │ │
│  │  :80     │  │  :1433   │  │ :5672    │  │  :4200   │ │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘ │
│                                                            │
└────────────────────────────────────────────────────────────┘
                         │
                         │ Port Mapping
                         ▼
┌────────────────────────────────────────────────────────────┐
│                    Host Machine (Windows)                   │
│                                                            │
│  localhost:5000  → gateway                                 │
│  localhost:5001  → core-api                                │
│  localhost:5002  → chatbot-service                         │
│  localhost:5003  → media-service                           │
│  localhost:1433  → sql                                     │
│  localhost:5672  → rabbitmq (AMQP)                         │
│  localhost:15672 → rabbitmq (Management)                   │
│  localhost:5341  → seq                                     │
│  localhost:4200  → client                                  │
└────────────────────────────────────────────────────────────┘
```

---

## 📚 Bonnes Pratiques

### 1. Variables d'Environnement

✅ **DO** : Utiliser `.env` pour secrets  
❌ **DON'T** : Hard-coder dans docker-compose.yml

### 2. Health Checks

✅ **DO** : Implémenter pour tous services  
❌ **DON'T** : Utiliser `depends_on` sans `condition: service_healthy`

### 3. Volumes

✅ **DO** : Named volumes pour persistance  
❌ **DON'T** : Bind mounts en production

### 4. Networks

✅ **DO** : Custom network bridge  
❌ **DON'T** : Default network

### 5. Resource Limits

✅ **DO** : Limiter CPU/RAM pour services gourmands  
❌ **DON'T** : Laisser consommer sans limite

---

## 🎯 Production Considerations

### Différences Dev vs Prod

| Aspect | Dev (docker-compose) | Prod (Kubernetes) |
|--------|---------------------|-------------------|
| **Orchestration** | Docker Compose | Kubernetes/AKS |
| **Secrets** | `.env` file | Azure Key Vault |
| **Scaling** | Manual (`--scale`) | Auto-scaling (HPA) |
| **Load Balancing** | Docker | Ingress Controller |
| **Monitoring** | Seq local | Azure Monitor |
| **SSL/TLS** | Non (dev) | Let's Encrypt / Cert Manager |

### Migration vers Production

```powershell
# 1. Exporter images vers registry
docker tag datingapp-gateway:latest myregistry.azurecr.io/gateway:v1.0
docker push myregistry.azurecr.io/gateway:v1.0

# 2. Adapter docker-compose pour swarm (alternative K8s)
docker stack deploy -c docker-compose.prod.yml datingapp

# 3. Ou générer manifests Kubernetes
kompose convert -f docker-compose.yml
```

---

**Prochaines Étapes** : [08-MONITORING.md](08-MONITORING.md) pour observabilité avancée.

---

**Date** : 2025-01-09  
**Version** : 1.0

