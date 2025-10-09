# CI/CD Pipeline
## Automatisation Build, Test & Deploy

---

## 📋 Table des Matières

1. [Vue d'ensemble Pipeline](#vue-densemble-pipeline)
2. [GitHub Actions Workflows](#github-actions-workflows)
3. [Build & Test Strategy](#build--test-strategy)
4. [Docker Registry & Images](#docker-registry--images)
5. [Deployment Strategies](#deployment-strategies)
6. [Secrets Management](#secrets-management)
7. [Rollback Procedures](#rollback-procedures)

---

## 🎯 Vue d'ensemble Pipeline

### Architecture CI/CD

```
┌─────────────────────────────────────────────────────────────┐
│                  DEVELOPER WORKFLOW                          │
└─────────────────────────────────────────────────────────────┘
                           │
                    git push origin main
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                  GITHUB ACTIONS                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Stage 1: Build & Test                               │  │
│  │  - Restore dependencies                              │  │
│  │  - Build all services                                │  │
│  │  - Run unit tests                                    │  │
│  │  - Code coverage                                     │  │
│  └──────────────────────────────────────────────────────┘  │
│                           │                                  │
│                           ▼                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Stage 2: Docker Build & Push                        │  │
│  │  - Build Docker images                               │  │
│  │  - Tag with git SHA                                  │  │
│  │  - Push to Container Registry                        │  │
│  └──────────────────────────────────────────────────────┘  │
│                           │                                  │
│                           ▼                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Stage 3: Deploy (Auto/Manual)                       │  │
│  │  - Deploy to staging (auto)                          │  │
│  │  - Smoke tests                                       │  │
│  │  - Deploy to production (manual approval)            │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Environnements

| Environnement | Trigger | Approbation | URL |
|---------------|---------|-------------|-----|
| **Development** | Chaque commit dev/* | Non | http://localhost:5000 |
| **Staging** | Merge to main | Non | https://staging.datingapp.com |
| **Production** | Tag release | Oui (manual) | https://datingapp.com |

---

## 🔄 GitHub Actions Workflows

### Structure Repository

```
.github/
└── workflows/
    ├── ci.yml                    # Build & Test
    ├── cd-staging.yml            # Deploy Staging
    ├── cd-production.yml         # Deploy Production
    └── docker-build.yml          # Build Docker images
```

---

### 1. CI Workflow (Build & Test)

**`.github/workflows/ci.yml`**

```yaml
name: CI - Build & Test

on:
  push:
    branches: [ main, develop, feature/** ]
  pull_request:
    branches: [ main ]

env:
  DOTNET_VERSION: '9.0.x'
  NODE_VERSION: '22.x'

jobs:
  #############################################
  # Backend Build & Test
  #############################################
  build-backend:
    name: Build & Test .NET Services
    runs-on: ubuntu-latest
    
    strategy:
      matrix:
        service: 
          - CoreAPI
          - ChatbotService
          - MediaService
          - Gateway
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-
      
      - name: Restore dependencies
        run: dotnet restore src/Services/${{ matrix.service }}/${{ matrix.service }}.csproj
      
      - name: Build
        run: dotnet build src/Services/${{ matrix.service }}/${{ matrix.service }}.csproj --configuration Release --no-restore
      
      - name: Run Unit Tests
        run: dotnet test tests/${{ matrix.service }}.Tests/${{ matrix.service }}.Tests.csproj --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
      - name: Upload coverage reports
        uses: codecov/codecov-action@v4
        with:
          files: ./coverage.cobertura.xml
          flags: ${{ matrix.service }}
          name: ${{ matrix.service }}-coverage

  #############################################
  # Frontend Build & Test
  #############################################
  build-frontend:
    name: Build & Test Angular
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: src/Client/package-lock.json
      
      - name: Install dependencies
        working-directory: src/Client
        run: npm ci --no-audit --no-fund
      
      - name: Lint
        working-directory: src/Client
        run: npm run lint || true
      
      - name: Build
        working-directory: src/Client
        run: npm run build -- --configuration production
      
      - name: Run tests
        working-directory: src/Client
        run: npm run test -- --watch=false --browsers=ChromeHeadless --code-coverage
      
      - name: Upload build artifacts
        uses: actions/upload-artifact@v4
        with:
          name: angular-build
          path: src/Client/dist/
          retention-days: 7

  #############################################
  # Security Scanning
  #############################################
  security-scan:
    name: Security Scan
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Run Trivy vulnerability scanner
        uses: aquasecurity/trivy-action@master
        with:
          scan-type: 'fs'
          scan-ref: '.'
          format: 'sarif'
          output: 'trivy-results.sarif'
      
      - name: Upload Trivy results to GitHub Security
        uses: github/codeql-action/upload-sarif@v3
        with:
          sarif_file: 'trivy-results.sarif'
```

---

### 2. Docker Build Workflow

**`.github/workflows/docker-build.yml`**

```yaml
name: Docker Build & Push

on:
  push:
    branches: [ main ]
    tags: [ 'v*.*.*' ]
  workflow_dispatch:

env:
  REGISTRY: ghcr.io
  IMAGE_PREFIX: ${{ github.repository_owner }}/datingapp

jobs:
  build-and-push:
    name: Build & Push Docker Images
    runs-on: ubuntu-latest
    
    permissions:
      contents: read
      packages: write
    
    strategy:
      matrix:
        service:
          - name: gateway
            context: ./src/Gateway/Gateway
          - name: core-api
            context: ./src/Services/CoreAPI
          - name: chatbot-service
            context: ./src/Services/ChatbotService
          - name: media-service
            context: ./src/Services/MediaService
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3
      
      - name: Log in to GitHub Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      
      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.service.name }}
          tags: |
            type=ref,event=branch
            type=ref,event=pr
            type=semver,pattern={{version}}
            type=semver,pattern={{major}}.{{minor}}
            type=sha,prefix={{branch}}-
      
      - name: Build and push
        uses: docker/build-push-action@v5
        with:
          context: ${{ matrix.service.context }}
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
          build-args: |
            CONFIGURATION=Release
```

---

### 3. Deploy Staging Workflow

**`.github/workflows/cd-staging.yml`**

```yaml
name: CD - Deploy to Staging

on:
  push:
    branches: [ main ]
  workflow_dispatch:

env:
  ENVIRONMENT: staging

jobs:
  deploy:
    name: Deploy to Staging
    runs-on: ubuntu-latest
    environment:
      name: staging
      url: https://staging.datingapp.com
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Setup SSH
        uses: webfactory/ssh-agent@v0.9.0
        with:
          ssh-private-key: ${{ secrets.STAGING_SSH_KEY }}
      
      - name: Copy docker-compose to server
        run: |
          scp -o StrictHostKeyChecking=no \
            docker-compose.yml \
            docker-compose.staging.yml \
            ${{ secrets.STAGING_USER }}@${{ secrets.STAGING_HOST }}:/app/
      
      - name: Deploy via SSH
        uses: appleboy/ssh-action@v1.0.3
        with:
          host: ${{ secrets.STAGING_HOST }}
          username: ${{ secrets.STAGING_USER }}
          key: ${{ secrets.STAGING_SSH_KEY }}
          script: |
            cd /app
            
            # Pull latest images
            docker compose -f docker-compose.yml -f docker-compose.staging.yml pull
            
            # Deploy
            docker compose -f docker-compose.yml -f docker-compose.staging.yml up -d --remove-orphans
            
            # Health check
            sleep 30
            curl -f http://localhost:5000/health || exit 1
      
      - name: Run smoke tests
        run: |
          curl -f https://staging.datingapp.com/health
          curl -f https://staging.datingapp.com/api/weatherforecast
      
      - name: Notify Slack on success
        if: success()
        uses: slackapi/slack-github-action@v1.25.0
        with:
          webhook-url: ${{ secrets.SLACK_WEBHOOK }}
          payload: |
            {
              "text": "✅ Staging deployment successful - ${{ github.sha }}"
            }
      
      - name: Notify Slack on failure
        if: failure()
        uses: slackapi/slack-github-action@v1.25.0
        with:
          webhook-url: ${{ secrets.SLACK_WEBHOOK }}
          payload: |
            {
              "text": "❌ Staging deployment FAILED - ${{ github.sha }}"
            }
```

---

### 4. Deploy Production Workflow

**`.github/workflows/cd-production.yml`**

```yaml
name: CD - Deploy to Production

on:
  release:
    types: [ published ]
  workflow_dispatch:
    inputs:
      version:
        description: 'Version to deploy (e.g., v1.2.3)'
        required: true

env:
  ENVIRONMENT: production

jobs:
  deploy:
    name: Deploy to Production
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://datingapp.com
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
        with:
          ref: ${{ github.event.inputs.version || github.ref }}
      
      - name: Create deployment backup
        uses: appleboy/ssh-action@v1.0.3
        with:
          host: ${{ secrets.PROD_HOST }}
          username: ${{ secrets.PROD_USER }}
          key: ${{ secrets.PROD_SSH_KEY }}
          script: |
            cd /app
            docker compose exec sql /opt/mssql-tools/bin/sqlcmd \
              -S localhost -U sa -P "${{ secrets.SQL_SA_PASSWORD }}" \
              -Q "BACKUP DATABASE [DatingApp] TO DISK='/var/opt/mssql/backup/pre-deploy-$(date +%Y%m%d-%H%M%S).bak'"
      
      - name: Deploy to Production
        uses: appleboy/ssh-action@v1.0.3
        with:
          host: ${{ secrets.PROD_HOST }}
          username: ${{ secrets.PROD_USER }}
          key: ${{ secrets.PROD_SSH_KEY }}
          script: |
            cd /app
            
            # Pull specific version
            export VERSION=${{ github.event.inputs.version || github.ref_name }}
            
            docker compose pull
            docker compose up -d --remove-orphans
            
            # Wait for services
            sleep 60
            
            # Health check
            curl -f https://datingapp.com/health || exit 1
      
      - name: Post-deployment tests
        run: |
          curl -f https://datingapp.com/health
          # Ajouter tests critiques
      
      - name: Notify team
        uses: slackapi/slack-github-action@v1.25.0
        with:
          webhook-url: ${{ secrets.SLACK_WEBHOOK }}
          payload: |
            {
              "text": "🚀 Production deployment ${{ github.event.inputs.version || github.ref_name }} completed!",
              "blocks": [
                {
                  "type": "section",
                  "text": {
                    "type": "mrkdwn",
                    "text": "*Production Deployment*\n✅ Version: ${{ github.event.inputs.version || github.ref_name }}\n🔗 <https://datingapp.com|Open App>"
                  }
                }
              ]
            }
```

---

## 🧪 Build & Test Strategy

### Tests Structure

```
tests/
├── CoreAPI.Tests/
│   ├── Unit/
│   │   ├── Services/
│   │   └── Controllers/
│   ├── Integration/
│   │   └── Repositories/
│   └── E2E/
│       └── Scenarios/
├── ChatbotService.Tests/
├── MediaService.Tests/
└── Client.Tests/ (Angular)
```

### Exemple Unit Test (.NET)

**`tests/ChatbotService.Tests/Unit/Services/ChatbotServiceTests.cs`**

```csharp
using Xunit;
using Moq;
using FluentAssertions;

namespace ChatbotService.Tests.Unit.Services;

public class ChatbotServiceTests
{
    [Fact]
    public async Task SendMessageAsync_ValidMessage_ReturnsResponse()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var service = new OllamaChatbotService(mockHttpClient.Object, options, logger);
        
        // Act
        var result = await service.SendMessageAsync("Hello");
        
        // Assert
        result.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task SendMessageAsync_OllamaDown_ThrowsException()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        mockHttpClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ThrowsAsync(new HttpRequestException());
        
        var service = new OllamaChatbotService(mockHttpClient.Object, options, logger);
        
        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.SendMessageAsync("Hello")
        );
    }
}
```

### Code Coverage Configuration

**`.coverlet.runsettings`**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>cobertura,opencover</Format>
          <Exclude>[*.Tests]*,[*]Program</Exclude>
          <IncludeTestAssembly>false</IncludeTestAssembly>
          <Threshold>80</Threshold>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

---

## 🐳 Docker Registry & Images

### GitHub Container Registry

**Tags Strategy** :

```
ghcr.io/yourusername/datingapp-gateway:
  - latest              (dernière version main)
  - v1.2.3             (release tag)
  - main-abc123       (branch + SHA)
  - staging           (staging env)
  - production        (prod env)
```

### Pull Images

```powershell
# Login
echo ${{ secrets.GITHUB_TOKEN }} | docker login ghcr.io -u ${{ github.actor }} --password-stdin

# Pull
docker pull ghcr.io/yourusername/datingapp-gateway:latest

# Tag & Run
docker tag ghcr.io/yourusername/datingapp-gateway:latest datingapp-gateway:local
docker run -p 5000:8080 datingapp-gateway:local
```

---

## 🚀 Deployment Strategies

### Blue-Green Deployment

```yaml
# docker-compose.blue.yml
services:
  gateway-blue:
    image: datingapp-gateway:v1.0.0
    container_name: gateway-blue
    ports:
      - "5000:8080"

# docker-compose.green.yml
services:
  gateway-green:
    image: datingapp-gateway:v1.1.0
    container_name: gateway-green
    ports:
      - "5001:8080"
```

**Déploiement** :

```bash
# 1. Deploy green (nouvelle version)
docker compose -f docker-compose.green.yml up -d

# 2. Test green
curl http://localhost:5001/health

# 3. Switch traffic (nginx/load balancer)
# Update upstream from :5000 to :5001

# 4. Supprimer blue
docker compose -f docker-compose.blue.yml down
```

### Canary Deployment

```nginx
# nginx.conf
upstream gateway {
    server gateway-stable:8080 weight=9;
    server gateway-canary:8080 weight=1;
}
```

10% trafic vers canary, 90% vers stable.

---

## 🔐 Secrets Management

### GitHub Secrets

**Repository Settings → Secrets → Actions**

```
STAGING_SSH_KEY          (Private SSH key)
STAGING_HOST             (staging.datingapp.com)
STAGING_USER             (deploy)

PROD_SSH_KEY
PROD_HOST
PROD_USER

SQL_SA_PASSWORD
JWT_TOKEN_KEY
CLOUDINARY_API_KEY
CLOUDINARY_API_SECRET

SLACK_WEBHOOK
```

### Utilisation dans Workflow

```yaml
steps:
  - name: Deploy
    env:
      DB_PASSWORD: ${{ secrets.SQL_SA_PASSWORD }}
      JWT_KEY: ${{ secrets.JWT_TOKEN_KEY }}
    run: |
      docker compose --env-file <(echo "SQL_PASSWORD=$DB_PASSWORD") up -d
```

### Azure Key Vault (Production)

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()
);

// Récupérer secrets
var jwtKey = builder.Configuration["JwtTokenKey"];
```

---

## 🔄 Rollback Procedures

### Rollback Docker Compose

```bash
# 1. Identifier version précédente
docker images | grep datingapp-gateway

# 2. Tag version stable
docker tag ghcr.io/user/datingapp-gateway:v1.0.0 datingapp-gateway:stable

# 3. Update docker-compose.yml
services:
  gateway:
    image: datingapp-gateway:stable  # Au lieu de :latest

# 4. Redeploy
docker compose up -d gateway

# 5. Vérifier
docker compose logs -f gateway
curl http://localhost:5000/health
```

### Rollback Database

```bash
# Restore backup
docker compose exec sql /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "REDACTED_DB_PASSWORD" \
  -Q "RESTORE DATABASE [DatingApp] FROM DISK='/var/opt/mssql/backup/pre-deploy-20250109.bak' WITH REPLACE"
```

### Rollback Automatique

```yaml
# deploy-with-rollback.yml
- name: Deploy new version
  id: deploy
  run: docker compose up -d
  
- name: Health check
  id: health
  run: |
    sleep 30
    curl -f http://localhost:5000/health || exit 1
  continue-on-error: true

- name: Rollback on failure
  if: steps.health.outcome == 'failure'
  run: |
    echo "Health check failed, rolling back..."
    docker compose down
    docker tag datingapp-gateway:previous datingapp-gateway:latest
    docker compose up -d
```

---

## 📊 Pipeline Metrics

### GitHub Actions Dashboard

```
Build Success Rate:     95%
Average Build Time:     8min 32s
Deployment Frequency:   3x/week
Lead Time for Changes:  2 hours
Mean Time to Recovery:  15 minutes
```

### Monitoring CI/CD

- **GitHub Actions Insights** : Built-in metrics
- **Datadog CI Visibility** : Advanced monitoring
- **Grafana + Prometheus** : Custom dashboards

---

## ✅ Checklist Production CI/CD

- [ ] CI pipeline tests tous services
- [ ] Code coverage > 80%
- [ ] Security scan (Trivy, Snyk)
- [ ] Docker images optimisées (<500MB)
- [ ] Secrets dans GitHub Secrets / Azure Key Vault
- [ ] Blue-Green ou Canary deployment
- [ ] Database backup automatique pre-deploy
- [ ] Rollback procédure testée
- [ ] Monitoring pipeline (temps build, success rate)
- [ ] Notifications Slack/Teams sur deploy
- [ ] Post-deployment smoke tests

---

**Prochaines Étapes** : Implémenter infrastructure Terraform (optionnel) ou commencer migration Phase 2.

---

**Date** : 2025-01-09  
**Version** : 1.0

