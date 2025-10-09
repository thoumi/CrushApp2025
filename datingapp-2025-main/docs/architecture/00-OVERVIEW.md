# Architecture Microservices - DatingApp 2025
## Vue d'ensemble de la migration

---

## 📋 Table des Matières

1. [Introduction](#introduction)
2. [État Actuel (Monolithe)](#état-actuel-monolithe)
3. [Architecture Cible (Phase 2)](#architecture-cible-phase-2)
4. [Bounded Contexts & Domain Mapping](#bounded-contexts--domain-mapping)
5. [Stratégie de Migration](#stratégie-de-migration)
6. [Technologies & Stack Technique](#technologies--stack-technique)
7. [Documents Associés](#documents-associés)

---

## 🎯 Introduction

### Objectifs de la Migration

**Objectif Principal** : Montée en compétences sur l'architecture distribuée

**Objectifs Secondaires** :
- ✅ Apprendre les patterns microservices (API Gateway, Event-Driven)
- ✅ Maîtriser Docker Compose multi-services
- ✅ Comprendre les trade-offs réels des systèmes distribués
- ✅ Construire un portfolio technique moderne
- ✅ Expérimenter avec orchestration et monitoring distribué

**Non-Objectifs** :
- ❌ Optimisation prématurée de performance
- ❌ Scalabilité massive (ce n'est pas Netflix)
- ❌ Réarchitecture complète immédiate

---

## 🏛️ État Actuel (Monolithe)

### Architecture Actuelle

```
┌─────────────────────────────────────────────────────────────┐
│                     Angular SPA (Client)                     │
│                      localhost:4200                          │
│  ┌──────────┬──────────┬──────────┬──────────┬───────────┐ │
│  │  Members │ Messages │  Likes   │  Admin   │  Chatbot  │ │
│  └──────────┴──────────┴──────────┴──────────┴───────────┘ │
└────────────────────────────┬────────────────────────────────┘
                             │ HTTPS
                             │
┌────────────────────────────▼────────────────────────────────┐
│                    ASP.NET Core API                          │
│                      localhost:5000                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Controllers Layer                        │  │
│  │  Account │ Members │ Messages │ Likes │ Admin │      │  │
│  │          │         │          │       │       │Chat  │  │
│  └────┬─────┴────┬────┴────┬─────┴───┬───┴───┬───┴──────┘  │
│       │          │         │         │       │              │
│  ┌────▼──────────▼─────────▼─────────▼───────▼──────────┐  │
│  │              Services Layer                           │  │
│  │  TokenService │ PhotoService │ ChatbotService        │  │
│  └────┬──────────┴──────────┬────────┴───────────────────┘  │
│       │                     │                               │
│  ┌────▼─────────────────────▼───────────────────────────┐  │
│  │              Data Layer (Unit of Work)               │  │
│  │  Member │ Message │ Likes │ Photo │ Admin Repository│  │
│  └────┬─────────────────────────────────────────────────┘  │
│       │                                                     │
│  ┌────▼─────────────────────────────────────────────────┐  │
│  │          SignalR Hubs (Real-time)                    │  │
│  │  PresenceHub │ MessageHub │ PresenceTracker         │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────┘
                             │
            ┌────────────────┼────────────────┐
            │                │                │
     ┌──────▼──────┐  ┌──────▼──────┐  ┌─────▼──────┐
     │ SQL Server  │  │ Cloudinary  │  │   Ollama   │
     │ (Identity,  │  │   (Photos)  │  │ (Chatbot)  │
     │  Members,   │  │             │  │            │
     │  Messages,  │  └─────────────┘  └────────────┘
     │  Likes)     │
     └─────────────┘
```

### Composants Principaux

| Composant | Responsabilités | Technologies |
|-----------|----------------|--------------|
| **Frontend** | SPA, UI/UX, Routing | Angular 20, TailwindCSS, DaisyUI |
| **API Backend** | Business Logic, Data Access | ASP.NET Core 9, Entity Framework |
| **Authentication** | JWT, Identity Management | ASP.NET Identity, JWT Bearer |
| **Real-time** | Messaging temps réel, Présence | SignalR |
| **Database** | Stockage relationnel | SQL Server 2022 |
| **Media Storage** | Photos, Images | Cloudinary API |
| **AI Chatbot** | Conversational AI | Ollama (local LLM) |

### Points de Douleur Identifiés

1. **Couplage Fort**
   - Tous les modules partagent le même DbContext
   - Modification d'une entité impacte plusieurs controllers
   - Difficile de scaler indépendamment

2. **Single Point of Failure**
   - Si l'API tombe, tout l'app est down
   - Impossible de déployer partiellement

3. **Ressources Partagées**
   - Chatbot Ollama consomme beaucoup de CPU/RAM
   - Impacte les performances des autres features

4. **Testing & Déploiement**
   - Tests d'intégration lourds
   - Déploiement "big bang" risqué
   - Rollback complexe

---

## 🎯 Architecture Cible (Phase 2)

### Vue d'Ensemble - Microservices Progressifs

```
                    ┌─────────────────────────────────┐
                    │      Angular SPA (Client)       │
                    │       localhost:4200            │
                    └────────────┬────────────────────┘
                                 │ HTTPS
                                 │
                    ┌────────────▼────────────────────┐
                    │      API Gateway (Ocelot)       │
                    │       localhost:5000            │
                    │  ┌──────────────────────────┐   │
                    │  │ Routing, Authentication, │   │
                    │  │ Rate Limiting, CORS      │   │
                    │  └──────────────────────────┘   │
                    └────┬────────┬──────────┬────────┘
                         │        │          │
        ┌────────────────┘        │          └──────────────────┐
        │                         │                             │
┌───────▼────────┐     ┌──────────▼──────────┐     ┌───────────▼────────┐
│  Core API      │     │  Chatbot Service    │     │  Media Service     │
│  (Monolith)    │     │  (Microservice)     │     │  (Microservice)    │
│  Port: 5001    │     │  Port: 5002         │     │  Port: 5003        │
├────────────────┤     ├─────────────────────┤     ├────────────────────┤
│ • Auth/Account │     │ • Ollama Integration│     │ • Photo Upload     │
│ • Members      │     │ • Conversation Mgmt │     │ • Cloudinary       │
│ • Messages     │     │ • Context Memory    │     │ • Moderation Queue │
│ • Likes        │     │ • AI Prompts        │     │ • Image Processing │
│ • Admin        │     │                     │     │ • Approval System  │
│ • SignalR Hubs │     │                     │     │                    │
└────┬───────────┘     └──────┬──────────────┘     └─────┬──────────────┘
     │                        │                           │
     │                        │                           │
┌────▼───────────┐     ┌──────▼──────────┐     ┌─────────▼──────────┐
│  SQL Server    │     │  Ollama Server  │     │  Cloudinary API    │
│  (Main DB)     │     │  (Local LLM)    │     │  (External)        │
│                │     │                 │     │                    │
│ • Users        │     │ • Llama3/Mistral│     │ • CDN Storage      │
│ • Members      │     │                 │     │                    │
│ • Messages     │     └─────────────────┘     └────────────────────┘
│ • Likes        │
│ • Photos       │     ┌──────────────────────────────────────────┐
│ • Roles        │     │      Message Broker (RabbitMQ)           │
│                │     │      Port: 5672, Management: 15672       │
└────────────────┘     │  ┌────────────────────────────────────┐  │
                       │  │  Events:                           │  │
                       │  │  • user.registered                 │  │
                       │  │  • photo.uploaded                  │  │
                       │  │  • photo.approved                  │  │
                       │  │  • chatbot.message.sent            │  │
                       │  └────────────────────────────────────┘  │
                       └──────────────────────────────────────────┘
```

### Services Décomposés

#### 1️⃣ **Core API Service** (Monolithe Partiel)
**Port** : 5001  
**Responsabilités** :
- Authentification & Authorization (JWT, Identity)
- Gestion des membres (CRUD, profils)
- Système de messaging (SignalR)
- Likes & Matching
- Administration utilisateurs

**Technologies** :
- ASP.NET Core 9
- Entity Framework Core
- SignalR
- SQL Server

**Raison de rester monolithique** :
- Couplage fort entre Members, Messages, Likes
- Transactions ACID nécessaires
- SignalR state management partagé

---

#### 2️⃣ **Chatbot Service** (Premier Microservice) ⭐
**Port** : 5002  
**Responsabilités** :
- Interface avec Ollama LLM
- Gestion des conversations
- Historique de contexte par utilisateur
- Prompts & personnalisation

**Technologies** :
- ASP.NET Core 9 (Minimal API)
- Ollama SDK
- Redis (cache conversation - optionnel)
- MongoDB (historique - optionnel)

**Raison d'extraction** :
- ✅ Indépendant du domaine métier principal
- ✅ Consommation ressources élevée (isolation CPU/RAM)
- ✅ Évolution technologique rapide (changement de LLM)
- ✅ Pas de transactions cross-service
- ✅ Facile à tester en isolation

**API Endpoints** :
```http
POST   /api/chat/send           # Envoyer un message
GET    /api/chat/history/{userId} # Récupérer historique
DELETE /api/chat/clear/{userId}   # Effacer contexte
```

---

#### 3️⃣ **Media Service** (Deuxième Microservice) ⭐
**Port** : 5003  
**Responsabilités** :
- Upload photos vers Cloudinary
- Queue de modération photos
- Gestion des photos approuvées/rejetées
- Transformation d'images

**Technologies** :
- ASP.NET Core 9
- Cloudinary .NET SDK
- File Storage (temporaire)
- SQL Server (propre DB) ou PostgreSQL

**Raison d'extraction** :
- ✅ Bounded context clair (Media Management)
- ✅ Service externe (Cloudinary) isolable
- ✅ Scalabilité indépendante (upload intensif)
- ✅ Modération asynchrone (event-driven)
- ✅ Peut être réutilisé par d'autres apps

**API Endpoints** :
```http
POST   /api/media/upload         # Upload photo
GET    /api/media/pending        # Photos en attente modération
PUT    /api/media/approve/{id}   # Approuver photo
DELETE /api/media/{id}           # Supprimer photo
```

---

#### 4️⃣ **API Gateway (Ocelot/YARP)**
**Port** : 5000 (point d'entrée unique)  
**Responsabilités** :
- Routing vers les microservices
- Authentification centralisée (JWT validation)
- Rate limiting
- CORS global
- Load balancing (futur)
- Request/Response transformation

**Configuration Example** :
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/chat/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{"Host": "chatbot-service", "Port": 5002}],
      "UpstreamPathTemplate": "/api/chatbot/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "DELETE"],
      "AuthenticationOptions": {"AuthenticationProviderKey": "Bearer"}
    },
    {
      "DownstreamPathTemplate": "/api/media/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{"Host": "media-service", "Port": 5003}],
      "UpstreamPathTemplate": "/api/photos/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
      "AuthenticationOptions": {"AuthenticationProviderKey": "Bearer"}
    }
  ]
}
```

---

#### 5️⃣ **Message Broker (RabbitMQ)**
**Ports** : 5672 (AMQP), 15672 (Management UI)  
**Responsabilités** :
- Communication asynchrone entre services
- Event publishing/subscribing
- Retry & Dead Letter Queue
- Message persistence

**Events Définis** :

| Event | Publisher | Subscribers | Payload |
|-------|----------|-------------|---------|
| `user.registered` | Core API | Media Service | `{userId, email, displayName}` |
| `photo.uploaded` | Media Service | Core API | `{photoId, userId, url}` |
| `photo.approved` | Media Service | Core API | `{photoId, approvedBy}` |
| `photo.rejected` | Media Service | Core API | `{photoId, reason}` |
| `chatbot.message.sent` | Chatbot Service | Core API (analytics) | `{userId, message, timestamp}` |

---

## 🗺️ Bounded Contexts & Domain Mapping

### Domain-Driven Design Analysis

```
┌────────────────────────────────────────────────────────────────┐
│                    DATINGAPP DOMAIN                             │
└────────────────────────────────────────────────────────────────┘

┌──────────────────────┐  ┌──────────────────────┐  ┌──────────────────────┐
│  IDENTITY CONTEXT    │  │  MATCHING CONTEXT    │  │  MEDIA CONTEXT       │
│                      │  │                      │  │                      │
│  • User              │  │  • Member            │  │  • Photo             │
│  • Role              │  │  • Like              │  │  • Moderation        │
│  • Authentication    │  │  • Match             │  │  • Upload            │
│  • Authorization     │  │  • Preference        │  │  • Storage           │
└──────────────────────┘  └──────────────────────┘  └──────────────────────┘
         │                         │                          │
         │                         │                          │
         └─────────┬───────────────┴──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────────────┐
│              COMMUNICATION CONTEXT                               │
│                                                                  │
│  • Message                                                       │
│  • Conversation                                                  │
│  • Real-time Presence (SignalR)                                 │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│              AI/CHATBOT CONTEXT (Isolated)                       │
│                                                                  │
│  • ChatMessage                                                   │
│  • Conversation History                                          │
│  • AI Model                                                      │
└──────────────────────────────────────────────────────────────────┘
```

### Mapping Context → Services

| Bounded Context | Service Cible | Raison |
|----------------|---------------|--------|
| **Identity** | Core API | Couplage fort avec Members, Messages |
| **Matching** | Core API | Transactions avec Members, Likes |
| **Communication** | Core API | SignalR state partagé, temps réel |
| **Media** | **Media Service** ✅ | Isolation claire, service externe |
| **AI/Chatbot** | **Chatbot Service** ✅ | Indépendant, ressources isolées |

---

## 📋 Stratégie de Migration

### Approche : **Strangler Fig Pattern**

Au lieu de réécrire tout d'un coup, on **étouffe progressivement** le monolithe :

```
Phase 0 (Actuel)          Phase 1                Phase 2 (Cible)
┌─────────────┐          ┌─────────────┐         ┌─────────────┐
│             │          │             │         │             │
│  Monolith   │    →     │  Monolith   │   →     │  Core API   │
│             │          │    -AI      │         │  (reduced)  │
│             │          │    -Media   │         │             │
└─────────────┘          └─────────────┘         └─────────────┘
                         ┌─────────────┐         ┌─────────────┐
                         │ Chatbot Svc │         │ Chatbot Svc │
                         └─────────────┘         └─────────────┘
                                                 ┌─────────────┐
                                                 │ Media Svc   │
                                                 └─────────────┘
```

### Étapes Détaillées

#### **Étape 1 : Préparation (Semaine 1-2)**
- [ ] Créer documentation architecture
- [ ] Setup repository Git (multi-repo ou mono-repo)
- [ ] Préparer environnement Docker
- [ ] Définir contrats API (OpenAPI/Swagger)
- [ ] Configurer CI/CD basique

#### **Étape 2 : Infrastructure (Semaine 2-3)**
- [ ] Setup Docker Compose multi-services
- [ ] Installer RabbitMQ container
- [ ] Configurer API Gateway (Ocelot)
- [ ] Setup monitoring basique (Seq logs)
- [ ] Tester routing Gateway → Monolith

#### **Étape 3 : Extraction Chatbot Service (Semaine 3-4)**
- [ ] Créer nouveau projet `ChatbotService`
- [ ] Migrer `ChatbotController` + `OllamaChatbotService`
- [ ] Implémenter API REST indépendante
- [ ] Configurer routing Gateway → Chatbot
- [ ] Tests d'intégration
- [ ] Déployer en parallèle (feature flag)
- [ ] Migration complète + suppression du monolith

#### **Étape 4 : Extraction Media Service (Semaine 5-6)**
- [ ] Créer projet `MediaService`
- [ ] Migrer `PhotoService` + Cloudinary
- [ ] Créer DB séparée pour photos metadata
- [ ] Implémenter event publishing (RabbitMQ)
- [ ] Core API subscribe aux events photos
- [ ] Tests end-to-end
- [ ] Migration progressive + cleanup

#### **Étape 5 : Communication Inter-Services (Semaine 6-7)**
- [ ] Implémenter tous les events RabbitMQ
- [ ] Gestion erreurs & retry logic
- [ ] Dead Letter Queue configuration
- [ ] Tests de résilience (chaos engineering)

#### **Étape 6 : Monitoring & Observabilité (Semaine 7-8)**
- [ ] Centralized logging (Seq/ELK)
- [ ] Distributed tracing (OpenTelemetry - optionnel)
- [ ] Health checks tous services
- [ ] Dashboards monitoring (Grafana - optionnel)

---

## 🛠️ Technologies & Stack Technique

### Backend Services

| Composant | Technologie | Version | Raison |
|-----------|-------------|---------|--------|
| **Core API** | ASP.NET Core | 9.0 | Existant, performant |
| **Chatbot Service** | ASP.NET Core Minimal API | 9.0 | Léger, rapide |
| **Media Service** | ASP.NET Core | 9.0 | Consistance stack |
| **API Gateway** | Ocelot | 23.x | .NET native, simple |
| **Alt. Gateway** | YARP | 2.x | Microsoft officiel, moderne |

### Data Layer

| Composant | Technologie | Usage |
|-----------|-------------|-------|
| **Core DB** | SQL Server 2022 | Users, Members, Messages, Likes |
| **Media DB** | SQL Server / PostgreSQL | Photos metadata, moderation queue |
| **Chatbot Cache** | Redis (optionnel) | Conversation context |
| **ORM** | Entity Framework Core 9 | Tous les services .NET |

### Communication

| Composant | Technologie | Usage |
|-----------|-------------|-------|
| **Sync REST** | HTTP/JSON | Client → Gateway, Gateway → Services |
| **Async Events** | RabbitMQ | Inter-services communication |
| **Real-time** | SignalR | Messages, Présence (reste dans Core API) |

### Infrastructure & DevOps

| Composant | Technologie | Usage |
|-----------|-------------|-------|
| **Containerization** | Docker | Tous les services |
| **Orchestration** | Docker Compose | Local dev + simple production |
| **Logging** | Serilog + Seq | Centralized structured logging |
| **Monitoring** | Health Checks UI | Service health monitoring |
| **Tracing** | OpenTelemetry (optionnel) | Distributed tracing |
| **CI/CD** | GitHub Actions | Build, test, deploy |

### Frontend (Inchangé)

| Composant | Technologie | Version |
|-----------|-------------|---------|
| **Framework** | Angular | 20.x |
| **UI** | TailwindCSS + DaisyUI | Latest |
| **HTTP Client** | HttpClient | Angular built-in |

---

## 📂 Documents Associés

Cette overview fait partie d'une suite de documents :

1. **[00-OVERVIEW.md](00-OVERVIEW.md)** ← Vous êtes ici
2. **[01-ADR-INDEX.md](01-ADR-INDEX.md)** - Architecture Decision Records
3. **[02-MIGRATION-GUIDE.md](02-MIGRATION-GUIDE.md)** - Guide pas-à-pas migration
4. **[03-IMPLEMENTATION-CHATBOT.md](03-IMPLEMENTATION-CHATBOT.md)** - Implémentation Chatbot Service
5. **[04-IMPLEMENTATION-MEDIA.md](04-IMPLEMENTATION-MEDIA.md)** - Implémentation Media Service
6. **[05-API-GATEWAY-SETUP.md](05-API-GATEWAY-SETUP.md)** - Configuration Ocelot/YARP
7. **[06-DOCKER-COMPOSE.md](06-DOCKER-COMPOSE.md)** - Orchestration multi-services
8. **[07-EVENT-DRIVEN.md](07-EVENT-DRIVEN.md)** - RabbitMQ & Event Bus
9. **[08-MONITORING.md](08-MONITORING.md)** - Observabilité & Logging
10. **[09-CI-CD.md](09-CI-CD.md)** - Pipeline déploiement

---

## 📊 Métriques de Succès

### Objectifs Mesurables

| Métrique | Avant (Monolithe) | Après (Microservices) |
|----------|-------------------|----------------------|
| **Services déployables** | 1 | 3+ |
| **Temps déploiement** | ~5 min | ~2 min/service |
| **Isolation CPU Chatbot** | Partagée | Isolée (container) |
| **Time to Market (AI features)** | Couplé au monolithe | Indépendant |
| **Tests unitaires temps** | 30s | <10s/service |
| **Nombre de technologies maîtrisées** | 4 | 10+ |

### KPIs d'Apprentissage

- [ ] Comprendre les trade-offs microservices vs monolithe
- [ ] Maîtriser Docker Compose multi-services
- [ ] Implémenter un API Gateway de production
- [ ] Gérer communication async (RabbitMQ)
- [ ] Mettre en place monitoring distribué
- [ ] Documenter architecture (ADR, C4)
- [ ] Présenter le projet en entretien technique

---

## 🚀 Prochaines Étapes

1. **Lire les ADRs** → [01-ADR-INDEX.md](01-ADR-INDEX.md)
2. **Suivre le Migration Guide** → [02-MIGRATION-GUIDE.md](02-MIGRATION-GUIDE.md)
3. **Implémenter Chatbot Service** → [03-IMPLEMENTATION-CHATBOT.md](03-IMPLEMENTATION-CHATBOT.md)

---

**Date de création** : {{ DATE }}  
**Auteur** : DatingApp Team  
**Version** : 1.0  
**Statut** : ✅ Validé

