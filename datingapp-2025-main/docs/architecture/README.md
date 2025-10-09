# Documentation Architecture Microservices
## DatingApp 2025 - Migration Monolithe → Microservices

---

## 📚 Vue d'ensemble

Cette documentation complète guide la migration progressive d'une application monolithique ASP.NET Core vers une architecture microservices moderne.

**Objectif Principal** : Montée en compétences sur les architectures distribuées  
**Approche** : Migration progressive (Strangler Fig Pattern)  
**Stack Technique** : .NET 9, Angular 20, Docker, RabbitMQ, Seq, Ocelot

---

## 🗂️ Structure Documentation

### 1. Documents Principaux

| Document | Description | Audience | Temps Lecture |
|----------|-------------|----------|---------------|
| **[00-OVERVIEW.md](00-OVERVIEW.md)** | Vue d'ensemble architecture & stratégie | Tous | 20 min |
| **[01-ADR-INDEX.md](01-ADR-INDEX.md)** | Architecture Decision Records | Architectes, Devs | 30 min |
| **[02-MIGRATION-GUIDE.md](02-MIGRATION-GUIDE.md)** | Guide pas-à-pas migration | Devs | 45 min |

### 2. Guides Techniques

| Document | Description | Niveau | Temps |
|----------|-------------|--------|-------|
| **[03-DOCKER-COMPOSE-COMPLETE.md](03-DOCKER-COMPOSE-COMPLETE.md)** | Orchestration multi-services | Intermédiaire | 25 min |
| **[04-MONITORING-OBSERVABILITY.md](04-MONITORING-OBSERVABILITY.md)** | Logs, Metrics, Traces | Avancé | 35 min |
| **[05-CI-CD-PIPELINE.md](05-CI-CD-PIPELINE.md)** | Automatisation déploiements | DevOps | 30 min |

---

## 🚀 Quick Start

### Pour les Impatients

```powershell
# 1. Cloner le repo
git clone https://github.com/yourusername/datingapp-2025.git
cd datingapp-2025

# 2. Lire l'overview
start docs\architecture\00-OVERVIEW.md

# 3. Suivre le guide migration
start docs\architecture\02-MIGRATION-GUIDE.md

# 4. Démarrer l'infrastructure
docker-compose up -d

# 5. Vérifier health checks
curl http://localhost:5000/healthchecks-ui
```

### Pour les Méthodiques

1. **Semaine 1** : Lire toute la documentation (3h)
2. **Semaine 2-3** : Infrastructure de base (Gateway, Docker Compose)
3. **Semaine 4-5** : Extraction Chatbot Service
4. **Semaine 6-7** : Extraction Media Service
5. **Semaine 8** : Monitoring & Observabilité

---

## 🎯 Architecture Cible (Phase 2)

```
                    ┌─────────────┐
                    │   Angular   │
                    │     SPA     │
                    └──────┬──────┘
                           │
                    ┌──────▼──────┐
                    │ API Gateway │
                    │  (Ocelot)   │
                    └──┬────┬────┬┘
                       │    │    │
        ┌──────────────┘    │    └──────────────┐
        │                   │                   │
  ┌─────▼─────┐      ┌──────▼──────┐      ┌────▼─────┐
  │  Core API │      │   Chatbot   │      │  Media   │
  │           │      │   Service   │      │ Service  │
  │ • Auth    │      │             │      │          │
  │ • Members │      │ • Ollama AI │      │• Photos  │
  │ • Messages│      └─────────────┘      │• Cloudry │
  │ • Likes   │                           └──────────┘
  └─────┬─────┘
        │
  ┌─────▼──────┐      ┌──────────────┐
  │ SQL Server │      │   RabbitMQ   │
  └────────────┘      │ (Event Bus)  │
                      └──────────────┘
```

### Services Décomposés

| Service | Port | Responsabilités | Statut |
|---------|------|-----------------|--------|
| **Gateway** | 5000 | Routing, Auth, CORS | ✅ Requis |
| **Core API** | 5001 | Auth, Members, Messages, Likes | ✅ Requis |
| **Chatbot** | 5002 | AI Conversationnel (Ollama) | ⭐ Microservice 1 |
| **Media** | 5003 | Photos, Cloudinary, Modération | ⭐ Microservice 2 |
| **RabbitMQ** | 5672 | Message Broker asynchrone | ✅ Infrastructure |
| **Seq** | 5341 | Centralized Logging | ✅ Observabilité |

---

## 📖 Parcours d'Apprentissage

### Niveau 1 : Fondamentaux (Semaine 1-2)

**Objectifs** :
- ✅ Comprendre les principes microservices
- ✅ Lire les ADRs pour comprendre les décisions
- ✅ Setup environnement Docker

**Documents** :
1. [00-OVERVIEW.md](00-OVERVIEW.md) - Architecture générale
2. [01-ADR-INDEX.md](01-ADR-INDEX.md) - Décisions architecturales

**Pratique** :
```powershell
# Setup Docker Desktop
winget install Docker.DockerDesktop

# Tester Docker Compose basique
docker-compose -f docker-compose.yml up sql rabbitmq seq
```

---

### Niveau 2 : Infrastructure (Semaine 3-4)

**Objectifs** :
- ✅ Déployer Gateway + Services existants
- ✅ Comprendre networking Docker
- ✅ Configurer Ocelot routing

**Documents** :
1. [02-MIGRATION-GUIDE.md](02-MIGRATION-GUIDE.md) - Phase 1: Infrastructure
2. [03-DOCKER-COMPOSE-COMPLETE.md](03-DOCKER-COMPOSE-COMPLETE.md)

**Pratique** :
```powershell
# Créer API Gateway
cd src/Gateway
dotnet new web -n Gateway
# Suivre guide migration section "Phase 1"
```

**Validation** :
- [ ] Gateway route vers Core API
- [ ] RabbitMQ Management UI accessible
- [ ] Seq reçoit logs de tous services

---

### Niveau 3 : Premier Microservice (Semaine 5-6)

**Objectifs** :
- ✅ Extraire Chatbot Service
- ✅ Configurer routing Gateway
- ✅ Tester end-to-end

**Documents** :
1. [02-MIGRATION-GUIDE.md](02-MIGRATION-GUIDE.md) - Phase 2: Chatbot

**Pratique** :
```powershell
# Créer Chatbot Service
cd src/Services
dotnet new webapi -n ChatbotService --use-minimal-apis
# Suivre guide section "Phase 2"
```

**Validation** :
- [ ] Chatbot Service répond sur port 5002
- [ ] Gateway route /api/chatbot/* correctement
- [ ] Frontend peut discuter avec chatbot
- [ ] Logs dans Seq avec "Service=ChatbotService"

---

### Niveau 4 : Second Microservice (Semaine 7-8)

**Objectifs** :
- ✅ Extraire Media Service
- ✅ Implémenter communication async (RabbitMQ)
- ✅ Events PhotoUploaded, PhotoApproved

**Documents** :
1. [02-MIGRATION-GUIDE.md](02-MIGRATION-GUIDE.md) - Phase 3: Media Service

**Pratique** :
```powershell
# Créer Media Service
cd src/Services
dotnet new webapi -n MediaService
# Suivre guide Phase 3
```

**Validation** :
- [ ] Upload photo via Media Service
- [ ] Event `PhotoUploaded` publié dans RabbitMQ
- [ ] Core API consomme event
- [ ] Photo apparaît dans profil utilisateur

---

### Niveau 5 : Observabilité (Semaine 9-10)

**Objectifs** :
- ✅ Centralized logging opérationnel
- ✅ Health checks tous services
- ✅ Dashboards Seq configurés
- ✅ Distributed tracing (optionnel)

**Documents** :
1. [04-MONITORING-OBSERVABILITY.md](04-MONITORING-OBSERVABILITY.md)

**Pratique** :
```powershell
# Ajouter Serilog à tous services
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq

# Configurer health checks
dotnet add package AspNetCore.HealthChecks.UI
```

**Validation** :
- [ ] Seq UI affiche logs de 4+ services
- [ ] Health Checks UI montre statut de tous services
- [ ] Correlation IDs tracent requêtes cross-services
- [ ] Dashboard Seq avec requêtes/sec, erreurs, latence

---

### Niveau 6 : CI/CD (Semaine 11-12)

**Objectifs** :
- ✅ Pipeline GitHub Actions
- ✅ Build & Test automatisés
- ✅ Docker images publiées
- ✅ Déploiement staging

**Documents** :
1. [05-CI-CD-PIPELINE.md](05-CI-CD-PIPELINE.md)

**Pratique** :
```yaml
# Créer .github/workflows/ci.yml
# Suivre guide CI/CD
```

**Validation** :
- [ ] Push → CI build & test automatique
- [ ] Merge main → Deploy staging
- [ ] Tag release → Deploy production (manual approval)
- [ ] Notifications Slack sur deploy

---

## 🛠️ Outils Requis

### Développement

| Outil | Version | Installation |
|-------|---------|--------------|
| **.NET SDK** | 9.0+ | [Download](https://dotnet.microsoft.com/download) |
| **Node.js** | 22+ | [Download](https://nodejs.org/) |
| **Docker Desktop** | 24+ | [Download](https://www.docker.com/products/docker-desktop) |
| **Git** | Latest | [Download](https://git-scm.com/) |
| **VS Code** | Latest | [Download](https://code.visualstudio.com/) |

### Optionnel mais Recommandé

| Outil | Usage |
|-------|-------|
| **Postman** | Tester APIs |
| **Azure Data Studio** | SQL Server management |
| **Lens** | Kubernetes (si migration K8s) |
| **Terraform** | Infrastructure as Code |

---

## 📊 Métriques de Succès

### Objectifs Techniques

- [ ] **3+ microservices** déployés indépendamment
- [ ] **Temps déploiement** < 5 min/service
- [ ] **Code coverage** > 80%
- [ ] **Health checks** tous verts
- [ ] **Logs centralisés** opérationnels
- [ ] **Zero downtime** deployment

### Objectifs Apprentissage

- [ ] Comprendre **trade-offs microservices vs monolithe**
- [ ] Maîtriser **Docker Compose** multi-services
- [ ] Implémenter **API Gateway** production-ready
- [ ] Gérer **communication async** (RabbitMQ)
- [ ] Mettre en place **observabilité** complète
- [ ] Documenter **architecture** (ADR, C4)

---

## ❓ FAQ

### Pourquoi migrer vers microservices ?

**Réponse courte** : Pour apprendre, pas pour la production.

Ce projet est trop petit pour justifier microservices en production. Mais c'est un **excellent terrain d'apprentissage** pour :
- Comprendre les patterns modernes
- Acquérir compétences CV-boosting
- Expérimenter sans risque

### Faut-il tout découper ?

**Non !** Phase 2 recommande :
- ✅ Extraire Chatbot (indépendant, facile)
- ✅ Extraire Media (bounded context clair)
- ❌ Garder Core API monolithique (Members, Messages, Likes couplés)

### Combien de temps ça prend ?

**Estimation réaliste** :
- **Lecture docs** : 3-4 heures
- **Phase 1 (Infrastructure)** : 10-12 heures
- **Phase 2 (Chatbot)** : 16-20 heures
- **Phase 3 (Media)** : 20-24 heures
- **Phase 4 (Observabilité)** : 8-12 heures
- **Total** : **60-75 heures** sur 2-3 mois (weekend projects)

### Quelle valeur pour mon CV ?

**Compétences démontrables** :
- Architecture microservices
- Docker & containers
- API Gateway (Ocelot/YARP)
- Message Broker (RabbitMQ)
- Observabilité (Seq, Prometheus, Jaeger)
- CI/CD (GitHub Actions)
- .NET 9 + Angular 20

**Impact entretiens** : ⭐⭐⭐⭐⭐ (très prisé en 2025)

### Puis-je adapter pour mon projet ?

**Absolument !** Cette doc est un **template générique** :
- Adapte les ADRs à ton contexte
- Change les services selon tes bounded contexts
- Remplace RabbitMQ par Azure Service Bus si cloud
- Utilise Kubernetes au lieu de Docker Compose si tu veux

---

## 🤝 Contribution

### Améliorer la Documentation

```bash
git checkout -b docs/improve-migration-guide
# Éditer docs/architecture/*.md
git commit -m "docs: clarify Docker networking section"
git push origin docs/improve-migration-guide
# Créer Pull Request
```

### Rapporter un Problème

**Issues GitHub** : [Créer issue](https://github.com/yourusername/datingapp-2025/issues/new)

Template :
```markdown
**Document** : 02-MIGRATION-GUIDE.md
**Section** : Phase 2, Étape 2.3
**Problème** : Commande Docker build échoue
**Erreur** : [Copier erreur]
**Environnement** : Windows 11, Docker Desktop 4.26
```

---

## 📚 Ressources Externes

### Livres Recommandés

- **Building Microservices** - Sam Newman (O'Reilly)
- **.NET Microservices Architecture** - Microsoft eBook (gratuit)
- **Microservices Patterns** - Chris Richardson

### Cours en Ligne

- **Microservices with .NET** - Pluralsight
- **Docker for .NET Developers** - Udemy
- **Building Event-Driven Microservices** - O'Reilly

### Articles & Blogs

- [Microsoft .NET Microservices Guide](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/)
- [Microservices.io Patterns](https://microservices.io/patterns/)
- [Martin Fowler - Microservices](https://martinfowler.com/articles/microservices.html)

---

## 🗓️ Changelog Documentation

### Version 1.0 (2025-01-09)

- ✅ Architecture overview complète
- ✅ 8 Architecture Decision Records
- ✅ Guide migration Phase 1-5
- ✅ Docker Compose complet
- ✅ Monitoring & Observabilité
- ✅ CI/CD pipelines GitHub Actions

### Prochaines Versions

- [ ] Terraform IaC pour Azure
- [ ] Kubernetes manifests & Helm charts
- [ ] GraphQL Gateway (Hot Chocolate)
- [ ] CQRS + Event Sourcing patterns
- [ ] Performance benchmarks & tuning

---

## 📞 Support

**Questions** : Créer [GitHub Discussion](https://github.com/yourusername/datingapp-2025/discussions)  
**Bugs** : Créer [GitHub Issue](https://github.com/yourusername/datingapp-2025/issues)  
**Slack Community** : [Rejoindre](https://join.slack.com/t/datingapp-dev) (fictif)

---

## 📝 License

Ce projet et sa documentation sont sous licence **MIT**.

---

**Bon courage pour ta migration ! 🚀**

N'oublie pas : **l'objectif est d'apprendre**, pas de faire du perfect engineering. Fais des erreurs, expérimente, documente ce que tu apprends.

**Happy coding!** 💻✨

---

**Dernière mise à jour** : 2025-01-09  
**Version** : 1.0.0  
**Auteurs** : DatingApp Dev Team

