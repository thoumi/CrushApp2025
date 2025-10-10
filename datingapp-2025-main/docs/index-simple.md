# CrushApp - Documentation Technique

## Application de Rencontres - Architecture Microservices

---

## Présentation du Projet

**CrushApp** est une application web de rencontres développée avec **ASP.NET Core 9** et **Angular 20**. Ce projet démontre une expertise technique complète en développement full-stack, architecture microservices, et migration de technologies.

**Contexte de développement** : Projet personnel initié en août 2024 avec Angular 12 et .NET 8, migré progressivement vers les dernières versions pour démontrer la maîtrise des technologies modernes.

### Spécifications Techniques

| Composant | Technologie | Version | Migration |
|-----------|-------------|---------|-----------|
| **Frontend** | Angular | 20.3.2 | 12 → 20 (8 versions) |
| **Backend** | ASP.NET Core | 9.0 | 8 → 9 |
| **Base de données** | SQL Server | 2022 | - |
| **ORM** | Entity Framework Core | 9.0 | - |
| **Temps réel** | SignalR | 9.0 | - |
| **IA** | Ollama + Phi-3 | Latest | - |
| **Architecture** | Microservices | - | Monolithe → Microservices |

---

## Architecture et Fonctionnalités

### Vue d'Ensemble Technique

**Fonctionnalités Implémentées** :
- **Authentification JWT** avec refresh tokens
- **Système de matching** avec algorithmes de compatibilité
- **Messagerie temps réel** via SignalR WebSockets
- **Chatbot IA** intégré avec Ollama + Phi-3
- **Gestion des médias** avec Cloudinary
- **Panel d'administration** avec modération
- **Architecture microservices** avec API Gateway

### Documentation Technique

1. [Fonctionnalités](guide/01-FONCTIONNALITES.md) - Architecture fonctionnelle et flux métier
2. [Guide Utilisateur](guide/02-GUIDE-UTILISATEUR.md) - Interface et expérience utilisateur

---

## Expertise Technique Développée

### Stack Frontend

**Angular 20** - Migration complète depuis Angular 12
- **Standalone Components** : Architecture moderne sans NgModules
- **Signals** : Gestion d'état réactive fine-grained
- **Control Flow** : Nouveau syntaxe `@if`, `@for`, `@switch`
- **ESBuild** : Build 75% plus rapide que Webpack
- **TailwindCSS + DaisyUI** : Styling utility-first

### Stack Backend

**ASP.NET Core 9** - Migration depuis .NET 8
- **Minimal APIs** : Endpoints optimisés
- **Entity Framework Core** : ORM avec migrations
- **SignalR** : Communication temps réel WebSockets
- **JWT Authentication** : Sécurité avec refresh tokens
- **Repository Pattern** : Architecture en couches

### Intégrations Externes

- **Cloudinary** : Gestion et optimisation d'images
- **Ollama + Phi-3** : IA conversationnelle locale
- **RabbitMQ** : Message broker pour événements
- **Seq** : Centralisation des logs

## Migration Angular 12 → 20

### Stratégie de Migration

**Approche incrémentale** : Migration version par version pour minimiser les risques
- **Standalone Components** : Suppression progressive des NgModules
- **Signals** : Remplacement de RxJS pour la gestion d'état
- **Control Flow** : Migration des directives structurelles
- **ESBuild** : Optimisation du processus de build

### Résultats de Performance

| Métrique | Avant (Angular 12) | Après (Angular 20) | Amélioration |
|----------|-------------------|-------------------|--------------|
| **Build Time** | 120s | 32s | -73% |
| **Bundle Size** | 485kb | 312kb | -36% |
| **Time to Interactive** | 2.8s | 1.4s | -50% |
| **Fichiers** | 100% | 65% | -35% |

**Documentation** : [Migration Angular 12 → 20](guide/03-MIGRATION-ANGULAR.md)

## Architecture Microservices

### Architecture Déployée

```
┌─────────────────┐
│ Angular SPA     │ Port 4200
│ (Frontend)      │
└─────────┬───────┘
          │
┌─────────▼───────┐
│ API Gateway     │ Port 5000
│ (Ocelot)        │
└─────────┬───────┘
          │
    ┌─────┴─────┬─────────┬─────────┐
    │           │         │         │
┌───▼───┐ ┌────▼───┐ ┌───▼───┐ ┌───▼───┐
│ Core  │ │Chatbot │ │ Media │ │RabbitMQ│
│ API   │ │Service │ │Service│ │Events │
│5001   │ │ 5002   │ │ 5003  │ │ 5672  │
└───┬───┘ └────┬───┘ └───┬───┘ └───┬───┘
    │          │         │         │
┌───▼───┐ ┌────▼───┐ ┌───▼───┐ ┌───▼───┐
│ SQL   │ │ Ollama │ │Cloud- │ │  Seq  │
│Server │ │ Phi-3  │ │inary  │ │ Logs  │
│ 1433  │ │ 11434  │ │       │ │ 5341  │
└───────┘ └────────┘ └───────┘ └───────┘
```

### Services Implémentés

- **API Gateway** : Routage et authentification centralisée
- **Core API** : Services métier principaux
- **Chatbot Service** : IA conversationnelle
- **Media Service** : Gestion des fichiers
- **RabbitMQ** : Communication asynchrone
- **Seq** : Observabilité et logs

**Documentation** : [Architecture](architecture/00-OVERVIEW.md)

---

## Déploiement et Configuration

### Environnement de Développement

**Prérequis techniques** :
- **Node.js** 20+ avec npm
- **.NET 9 SDK** 
- **SQL Server 2022** (ou LocalDB)
- **Docker Desktop** (pour architecture microservices)
- **Ollama** (pour IA conversationnelle)

### Services Externes

- **Cloudinary** : Stockage et optimisation d'images
- **Ollama + Phi-3** : Modèle IA local pour chatbot

---

### Installation et Démarrage

**Développement local** :
```bash
# 1. Cloner le repository
git clone https://github.com/thoumi/CrushApp2025.git
cd CrushApp2025

# 2. Backend - Configuration
cd API
dotnet restore
dotnet ef database update
dotnet run  # Port 5001

# 3. Frontend - Configuration
cd ../client
npm install
npm start  # Port 4200

# 4. IA - Ollama + Phi-3
ollama run phi3  # Port 11434
```

**Déploiement microservices** :
```bash
# Architecture complète avec Docker
docker-compose up -d
```

### Accès aux Services

| Service | URL | Description |
|---------|-----|-------------|
| **Frontend** | http://localhost:4200 | Interface utilisateur |
| **API Gateway** | http://localhost:5000 | Point d'entrée API |
| **Core API** | http://localhost:5001 | Services métier |
| **Chatbot** | http://localhost:5002 | Service IA |
| **Media** | http://localhost:5003 | Gestion fichiers |
| **RabbitMQ** | http://localhost:15672 | Message broker |
| **Seq** | http://localhost:5341 | Logs centralisés |

**Documentation** : [Configuration Docker Compose](architecture/03-DOCKER-COMPOSE-COMPLETE.md)

---

## Résolution des Problèmes

### Problèmes WebSocket et SignalR

**Documentation** : [Guide de Résolution WebSocket](TROUBLESHOOTING-WEBSOCKET.md)

**Problèmes résolus** :
- Erreurs de connexion WebSocket
- Handshake SignalR échoué
- Gestion d'état des connexions
- Routage API Gateway pour WebSockets

---

## Compétences Techniques Développées

### Développement Full-Stack

- **Frontend** : Angular 20, TypeScript, TailwindCSS, SignalR Client
- **Backend** : ASP.NET Core 9, C#, Entity Framework, SignalR Hubs
- **Base de données** : SQL Server, migrations EF Core
- **Architecture** : Microservices, API Gateway, Event-Driven

### Technologies Avancées

- **IA/ML** : Intégration Ollama + Phi-3 pour chatbot conversationnel
- **Temps réel** : SignalR WebSockets pour messagerie instantanée
- **DevOps** : Docker, Docker Compose, CI/CD
- **Observabilité** : Logging centralisé avec Seq, monitoring

### Migration et Modernisation

- **Migration Angular** : 12 → 20 (8 versions) avec amélioration des performances
- **Migration .NET** : 8 → 9 avec adoption des nouvelles fonctionnalités
- **Architecture** : Monolithe → Microservices avec pattern Strangler Fig

---


---

## Contact et Portfolio

**Documentation technique** : https://thoumi.github.io/CrushApp2025/

*Projet développé pour démontrer l'expertise technique en développement full-stack et architecture moderne*

*Documentation mise à jour : Octobre 2025*
