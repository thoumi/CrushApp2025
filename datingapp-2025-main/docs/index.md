# CrushApp - Documentation Technique Complète

## Application Moderne de Rencontres avec Architecture Microservices

---

## 🎯 Bienvenue

**CrushApp** est une application web moderne de rencontres développée avec **ASP.NET Core 9** et **Angular 20**. Cette documentation complète couvre à la fois les aspects fonctionnels de l'application et sa migration progressive vers une architecture microservices.

### À Propos du Projet

- **Type d'application** : Application web de rencontres (Dating App)
- **Stack Frontend** : Angular 20, TailwindCSS, DaisyUI
- **Stack Backend** : ASP.NET Core 9, Entity Framework, SignalR
- **Architecture** : Hybride (Monolithe + Microservices)
- **Statut** : En développement actif

### Thématiques Couvertes

Cette documentation est organisée en plusieurs sections pour répondre aux besoins de différents publics :

1. **📖 Guide Utilisateur** : Pour les utilisateurs finaux et modérateurs
2. **💻 Guide Technique** : Pour les développeurs et architectes
3. **🏗️ Architecture Microservices** : Migration et patterns distribués
4. **🚀 Migration & Déploiement** : Guides pratiques
5. **📊 Opérations** : Monitoring et CI/CD

---

## 🚀 Démarrage Rapide

### Pour les Utilisateurs

Vous souhaitez comprendre **comment utiliser l'application** ?

👉 **Commencez par** :
1. [Présentation des Fonctionnalités](guide/01-FONCTIONNALITES.md) - Découvrez ce que fait l'application (10 min)
2. [Guide Utilisateur Complet](guide/02-GUIDE-UTILISATEUR.md) - Mode d'emploi détaillé (20 min)

**Fonctionnalités Principales** :
- ✅ Profils utilisateurs avec photos
- ✅ Système de likes et matching
- ✅ Messagerie en temps réel (SignalR)
- ✅ Chatbot IA conversationnel (Ollama)
- ✅ Panel d'administration et modération
- ✅ Interface multilingue (FR/EN)

---

### Pour les Développeurs

Vous souhaitez **comprendre l'architecture technique** ou **contribuer au projet** ?

👉 **Parcours Développeur** :

#### 1️⃣ Comprendre la Stack Technique

**Frontend** :
- **Angular 20** : Framework SPA moderne avec Standalone Components
- **Signals** : Nouvelle gestion d'état réactive
- **TailwindCSS + DaisyUI** : Styling utility-first
- **SignalR Client** : Communication temps réel

**Backend** :
- **ASP.NET Core 9** : APIs REST + SignalR Hubs
- **Entity Framework Core** : ORM avec SQL Server
- **ASP.NET Identity** : Authentification JWT
- **Cloudinary** : Stockage et optimisation d'images
- **Ollama** : Modèle LLM local pour chatbot

📖 **Lire** : [Présentation des Fonctionnalités](guide/01-FONCTIONNALITES.md)

#### 2️⃣ Découvrir la Migration Angular 12 → 20

Un **retour d'expérience complet** sur la migration progressive d'Angular 12 à 20 :

**Thèmes Abordés** :
- ✅ Stratégie de migration incrémentale (version par version)
- ✅ Passage aux Standalone Components
- ✅ Adoption des Signals pour state management
- ✅ Nouveau Control Flow (`@if`, `@for`, `@switch`)
- ✅ ESBuild : Build 75% plus rapide
- ✅ Défis rencontrés et solutions apportées
- ✅ Bonnes pratiques et leçons apprises

📖 **Lire** : [Migration Angular 12 → 20](guide/03-MIGRATION-ANGULAR.md) (45 min)

**Résultats Mesurés** :
- 🚀 Build **73% plus rapide** (120s → 32s)
- 📦 Bundle size **-36%** (485kb → 312kb)
- ⚡ Time to Interactive **-50%** (2.8s → 1.4s)
- 🧹 **-35%** de fichiers (suppression des NgModules)

#### 3️⃣ Explorer l'Architecture Microservices

Si vous êtes intéressé par la **migration vers une architecture distribuée** :

**✅ Migration 100% Terminée** :
- Migration **complète et opérationnelle** (Pattern Strangler Fig)
- **3 microservices déployés** : Chatbot Service, Media Service, API Gateway
- **Event-Driven Architecture** avec RabbitMQ
- **Observabilité totale** : Serilog + Seq sur tous services
- **Production Ready** : Dockerisé, testé, documenté

📖 **Lire** : [Introduction aux Microservices](MICROSERVICES-MIGRATION.md) (10 min)

**Architecture Finale Déployée** :

```
Angular SPA (Port 4200)
        ↓
API Gateway Ocelot (Port 5000) ✅
        ↓
┌───────┴────────┬──────────────┬─────────────┐
│                │              │             │
Core API      Chatbot       Media       RabbitMQ
(Port 5001)   Service ✅    Service ✅   (Events) ✅
              (Port 5002)   (Port 5003)
                ↓              ↓              ↓
            Ollama         Cloudinary       Seq
                                          (Logs) ✅
```

📖 **Lire** : [Vue d'Ensemble Architecture](architecture/00-OVERVIEW.md) (20 min)

---

### Pour les Architectes

Vous souhaitez comprendre les **décisions architecturales** et les **patterns utilisés** ?

👉 **Parcours Architecte** :

1. **Architecture d'Ensemble**
   - [Vue d'Ensemble](architecture/00-OVERVIEW.md) - Architecture hybride monolithe + microservices
   - [Décisions Architecturales (ADR)](architecture/01-ADR-INDEX.md) - 8 ADRs documentant les choix techniques

2. **Migration Technique** ✅ **100% Complète**
   - [Guide de Migration](architecture/02-MIGRATION-GUIDE.md) - Migration progressive en 5 phases
   - [Migration Complète](../MIGRATION-COMPLETE.md) - Récapitulatif complet 🆕
   - [Status Final](../STATUS-FINAL.md) - État détaillé de la migration 🆕
   - [Docker Compose](architecture/03-DOCKER-COMPOSE-COMPLETE.md) - Orchestration multi-services

3. **Opérations**
   - [Monitoring et Observabilité](architecture/04-MONITORING-OBSERVABILITY.md) - Seq, Health Checks, Métriques
   - [Pipeline CI/CD](architecture/05-CI-CD-PIPELINE.md) - GitHub Actions, déploiement automatisé
   - [Tests End-to-End](../TESTS-E2E.md) - Guide de tests complet 🆕

**Patterns et Concepts Clés** :
- **Strangler Fig Pattern** : Extraction progressive de services ✅ Implémenté
- **Event-Driven Architecture** : RabbitMQ pour communication asynchrone ✅ Implémenté
- **API Gateway** : Ocelot pour routing centralisé ✅ Implémenté
- **Domain-Driven Design** : Bounded Contexts et agrégats
- **CQRS** (partiel) : Séparation lecture/écriture pour performance
- **Observabilité** : Serilog + Seq pour logs centralisés ✅ Implémenté

---

## 📚 Organisation de la Documentation

| Section | Public Cible | Temps Lecture | Description |
|---------|-------------|---------------|-------------|
| **📖 Guide Utilisateur** | Utilisateurs, Modérateurs | 30 min | Fonctionnalités et mode d'emploi |
| **💻 Guide Technique** | Développeurs | 60 min | Stack technique et migration Angular |
| **🏗️ Architecture** | Architectes, Tech Leads | 60 min | Architecture microservices et ADRs |
| **🚀 Migration** | DevOps, Développeurs | 90 min | Guides de migration et Docker |
| **📊 Opérations** | SRE, DevOps | 60 min | Monitoring, logs, CI/CD |

**Temps total de lecture** : ~5 heures pour une compréhension complète

---

## 💡 Points Forts de CrushApp

### 1. Stack Technique Moderne

**Frontend de Pointe** :
- ✅ Angular 20 avec Standalone Components
- ✅ Signals pour gestion d'état (fine-grained reactivity)
- ✅ New Control Flow (`@if`, `@for`)
- ✅ TailwindCSS 4.x + DaisyUI 5.x
- ✅ ESBuild pour builds ultra-rapides

**Backend Robuste** :
- ✅ ASP.NET Core 9 (dernière version LTS)
- ✅ Entity Framework Core avec migrations
- ✅ SignalR pour temps réel (WebSockets)
- ✅ JWT Authentication avec refresh tokens
- ✅ Repository Pattern + Unit of Work

### 2. Fonctionnalités Complètes

**Expérience Utilisateur** :
- 🔐 Authentification sécurisée (JWT + refresh tokens)
- 👤 Profils riches avec multi-photos
- 🔍 Recherche avancée avec filtres (âge, genre, localisation)
- ⭐ Système de likes avec détection de matches mutuels
- 💬 Messagerie temps réel avec indicateurs de présence
- 🤖 Chatbot IA (Ollama) pour conseils et assistance
- 🌍 Interface multilingue (français / anglais)
- 🎨 Thème clair / sombre

**Administration** :
- 🛡️ Gestion des rôles (User / Moderator / Admin)
- 📸 Modération de photos avec file d'attente
- 📊 Statistiques utilisateurs en temps réel
- 🔧 Panel d'administration complet

### 3. Performance et Qualité

**Optimisations** :
- ⚡ Build production en 32 secondes (vs 120s avant)
- 📦 Bundle initial : 312kb gzip (vs 485kb avant)
- 🚀 Time to Interactive : 1.4s (vs 2.8s avant)
- 🎯 Lighthouse Score : 90+ (Performance, Accessibility, Best Practices)

**Qualité du Code** :
- ✅ TypeScript strict mode activé
- ✅ ESLint + Prettier configurés
- ✅ Tests unitaires (couverture > 70%)
- ✅ CI/CD avec GitHub Actions

### 4. Architecture Évolutive

**✅ Migration Microservices Complète (100%)** :
- 🏗️ Pattern Strangler Fig (extraction sans réécriture) - ✅ Implémenté
- 🐰 RabbitMQ pour événements asynchrones - ✅ Opérationnel
- 🐳 Docker Compose pour orchestration - ✅ 8 services déployés
- 📊 Seq pour centralisation des logs - ✅ Tous services connectés
- 🔍 Health Checks pour monitoring - ✅ Sur tous services
- 🚪 API Gateway Ocelot - ✅ Routing configuré

**Résultats Obtenus** :
- ✅ Chatbot Service isolé et scalable (CPU-intensive)
- ✅ Media Service avec Event-Driven Architecture
- ✅ Scalabilité indépendante par service
- ✅ Déploiement par service (zero downtime)
- ✅ Observabilité 100% (Seq + Health Checks)
- ✅ ~60 fichiers créés, ~10,000 lignes de code

---

## 🎓 Ce que Vous Apprendrez

### Pour les Développeurs Frontend

**Angular Moderne** :
- Migration d'une application Angular 12 vers 20
- Adoption des Standalone Components (suppression NgModules)
- Utilisation des Signals pour state management
- New Control Flow (`@if`, `@for`, `@switch`)
- Best practices Angular 2025

**Performance** :
- ESBuild vs Webpack
- Bundle optimization
- Lazy loading avec `@defer`
- Change detection optimization

### Pour les Développeurs Backend

**.NET Moderne** :
- ASP.NET Core 9 avec minimal APIs
- Entity Framework Core migrations
- SignalR pour communication temps réel
- JWT Authentication + refresh tokens
- Repository Pattern et Unit of Work

**Patterns** :
- CQRS (lecture/écriture séparées)
- Event-Driven Architecture
- API Gateway (Ocelot)
- Middleware personnalisés

### Pour les Architectes

**Architecture Distribuée** :
- Migration monolithe → microservices (Strangler Fig)
- Bounded Contexts (Domain-Driven Design)
- Event-Driven Architecture avec RabbitMQ
- API Gateway pattern
- Service mesh (observabilité, résilience)

**Décisions Architecturales** :
- 8 ADRs documentant les choix techniques
- Trade-offs entre microservices et monolithe
- Stratégies de déploiement
- Gestion des données dans un système distribué

### Pour les DevOps / SRE

**Infrastructure** :
- Docker Compose multi-services
- Health Checks et monitoring
- Centralisation des logs (Seq)
- CI/CD avec GitHub Actions
- Déploiement progressif (Blue/Green, Canary)

**Observabilité** :
- Les 3 piliers : Logs, Métriques, Traces
- Distributed tracing
- Alerting et dashboards
- Debugging distribué

---

## 🗺️ Parcours de Lecture Recommandés

### Parcours "Utilisateur Final" (30 min)

1. [Présentation de l'Application](guide/01-FONCTIONNALITES.md) ⏱️ 10 min
2. [Guide Utilisateur](guide/02-GUIDE-UTILISATEUR.md) ⏱️ 20 min

**Résultat** : Vous saurez utiliser toutes les fonctionnalités de CrushApp

---

### Parcours "Développeur Frontend" (90 min)

1. [Présentation des Fonctionnalités](guide/01-FONCTIONNALITES.md) ⏱️ 10 min
2. [Migration Angular 12 → 20](guide/03-MIGRATION-ANGULAR.md) ⏱️ 45 min
3. [Vue d'Ensemble Architecture](architecture/00-OVERVIEW.md) ⏱️ 20 min
4. [Guide de Migration](architecture/02-MIGRATION-GUIDE.md) ⏱️ 15 min (Phase 1 uniquement)

**Résultat** : Vous maîtriserez Angular moderne et pourrez contribuer au frontend

---

### Parcours "Développeur Backend" (60 min)

1. [Présentation des Fonctionnalités](guide/01-FONCTIONNALITES.md) ⏱️ 10 min
2. [Vue d'Ensemble Architecture](architecture/00-OVERVIEW.md) ⏱️ 20 min
3. [Décisions Architecturales](architecture/01-ADR-INDEX.md) ⏱️ 30 min

**Résultat** : Vous comprendrez l'architecture backend et pourrez contribuer

---

### Parcours "Architecte / Tech Lead" (2h30)

1. [Introduction Microservices](MICROSERVICES-MIGRATION.md) ⏱️ 10 min
2. [Migration Angular 12 → 20](guide/03-MIGRATION-ANGULAR.md) ⏱️ 45 min
3. [Vue d'Ensemble Architecture](architecture/00-OVERVIEW.md) ⏱️ 20 min
4. [Décisions Architecturales](architecture/01-ADR-INDEX.md) ⏱️ 30 min
5. [Guide de Migration](architecture/02-MIGRATION-GUIDE.md) ⏱️ 45 min

**Résultat** : Compréhension complète des choix architecturaux et de la stratégie

---

### Parcours "DevOps / SRE" (2h)

1. [Vue d'Ensemble Architecture](architecture/00-OVERVIEW.md) ⏱️ 20 min
2. [Configuration Docker Compose](architecture/03-DOCKER-COMPOSE-COMPLETE.md) ⏱️ 25 min
3. [Monitoring et Observabilité](architecture/04-MONITORING-OBSERVABILITY.md) ⏱️ 35 min
4. [Pipeline CI/CD](architecture/05-CI-CD-PIPELINE.md) ⏱️ 30 min
5. [Guide de Migration](architecture/02-MIGRATION-GUIDE.md) ⏱️ 10 min (Phase 5 uniquement)

**Résultat** : Vous pourrez déployer et monitorer l'application complète

---

### Parcours "Complet" (5h)

📖 Lire toute la documentation dans l'ordre de navigation

**Résultat** : Expertise complète sur CrushApp (fonctionnel + technique + architectural)

---

## 🛠️ Technologies et Outils

### Frontend

| Technologie | Version | Usage |
|------------|---------|-------|
| **Angular** | 20.3.2 | Framework SPA |
| **TypeScript** | 5.8.2 | Langage principal |
| **TailwindCSS** | 4.1.7 | Styling utility-first |
| **DaisyUI** | 5.0.37 | Composants UI |
| **SignalR Client** | 8.0.7 | WebSockets temps réel |
| **RxJS** | 7.8.0 | Programmation réactive |

### Backend

| Technologie | Version | Usage |
|------------|---------|-------|
| **ASP.NET Core** | 9.0 | Framework API |
| **C#** | 13 | Langage principal |
| **Entity Framework Core** | 9.0 | ORM |
| **SignalR** | 9.0 | Communication temps réel |
| **SQL Server** | 2022 | Base de données |
| **Cloudinary** | - | Stockage images |
| **Ollama** | - | LLM local (chatbot) |

### Infrastructure

| Technologie | Version | Usage |
|------------|---------|-------|
| **Docker** | 24+ | Conteneurisation |
| **Docker Compose** | 2.x | Orchestration |
| **RabbitMQ** | 3.13 | Message broker |
| **Seq** | 2024.x | Centralisation logs |
| **Ocelot** | 23.x | API Gateway |

### DevOps

| Technologie | Usage |
|------------|-------|
| **GitHub Actions** | CI/CD |
| **Git** | Contrôle de version |
| **ESLint / Prettier** | Linting et formatting |
| **Lighthouse CI** | Monitoring performance |

---

## 📊 Métriques du Projet

### Statistiques Générales

- **Lignes de code** : ~15 000 (Frontend + Backend)
- **Composants Angular** : 25
- **Services Angular** : 15
- **Controllers ASP.NET** : 8
- **Entités** : 7
- **Migrations EF** : 12
- **Tests unitaires** : 80+ (couverture 70%)

### Métriques de Qualité

- **Lighthouse Score** : 90+ (Performance, Accessibility, Best Practices, SEO)
- **Bundle Size** : 312kb (gzip, initial)
- **Time to Interactive** : 1.4s
- **First Contentful Paint** : 0.8s
- **Largest Contentful Paint** : 1.1s

### Performance Build

- **Cold Build** : 12s (vs 45s Angular 12)
- **Incremental Build** : 2s (vs 8s)
- **Production Build** : 32s (vs 120s)
- **Hot Reload** : 0.5s (vs 3s)

---

## 🚀 Lancer le Projet

### Prérequis

**Logiciels Requis** :
- Node.js 20+ et npm
- .NET 9 SDK
- SQL Server 2022 (ou LocalDB)
- Docker Desktop (optionnel, pour microservices)

**Comptes Externes** :
- Cloudinary (pour stockage photos) - gratuit
- Ollama installé localement (pour chatbot) - gratuit

### Installation Rapide

```bash
# 1. Cloner le repository
git clone https://github.com/thoumi/CrushApp2025.git
cd CrushApp2025

# 2. Backend - Installer dépendances
cd API
dotnet restore

# 3. Backend - Configurer la DB (appsettings.Development.json)
# Puis appliquer les migrations
dotnet ef database update

# 4. Backend - Lancer l'API
dotnet run
# L'API démarre sur https://localhost:5001

# 5. Frontend - Installer dépendances (nouveau terminal)
cd ../client
npm install

# 6. Frontend - Lancer l'app
npm start
# L'app démarre sur http://localhost:4200
```

### Lancement avec Docker (Architecture Microservices)

```bash
# Depuis la racine du projet
docker-compose up -d

# Vérifier que tous les services sont up
docker-compose ps

# Accéder à l'application
# Frontend: http://localhost:4200
# API Gateway: http://localhost:5000
# RabbitMQ Management: http://localhost:15672
# Seq Logs: http://localhost:5341
```

📖 **Guide Complet** : [Configuration Docker Compose](architecture/03-DOCKER-COMPOSE-COMPLETE.md)

---

## 🤝 Contribuer au Projet

CrushApp est un projet open-source, les contributions sont les bienvenues !

### Comment Contribuer ?

1. **Fork** le repository
2. **Créez une branche** : `git checkout -b feature/ma-nouvelle-fonctionnalite`
3. **Commitez** vos changements : `git commit -m "feat: ajout de X"`
4. **Pushez** la branche : `git push origin feature/ma-nouvelle-fonctionnalite`
5. **Ouvrez une Pull Request** avec description détaillée

### Types de Contributions

- 🐛 **Bug fixes** : Corrections de bugs
- ✨ **Features** : Nouvelles fonctionnalités
- 📝 **Documentation** : Améliorations de la doc
- 🎨 **UI/UX** : Améliorations d'interface
- ⚡ **Performance** : Optimisations
- ✅ **Tests** : Ajout/amélioration de tests

### Conventions

- **Commits** : Convention [Conventional Commits](https://www.conventionalcommits.org/)
- **Code Style** : ESLint + Prettier (automatique)
- **Branches** : `feature/`, `fix/`, `docs/`, `refactor/`

---

## 📞 Support et Ressources

### Documentation

- **Documentation en ligne** : [thoumi.github.io/CrushApp2025](https://thoumi.github.io/CrushApp2025)
- **Repository GitHub** : [github.com/thoumi/CrushApp2025](https://github.com/thoumi/CrushApp2025)
- **Utilisation de la doc** : [Mode d'emploi](architecture/README.md)

### Communauté

- **GitHub Issues** : Signaler bugs et proposer features
- **GitHub Discussions** : Poser des questions générales
- **Pull Requests** : Contribuer au code

### Liens Utiles

**Angular** :
- [Documentation Officielle Angular](https://angular.dev/)
- [Angular Update Guide](https://update.angular.io/)
- [Angular Blog](https://blog.angular.dev/)

**ASP.NET Core** :
- [Documentation .NET](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/)

**Architecture** :
- [Microservices.io Patterns](https://microservices.io/patterns/)
- [Microsoft Architecture Guides](https://learn.microsoft.com/en-us/dotnet/architecture/)

---

## 🎯 Prochaines Étapes

Choisissez votre parcours et commencez l'exploration !

### Je suis un Utilisateur 👤

👉 [Découvrir les Fonctionnalités](guide/01-FONCTIONNALITES.md)

### Je suis un Développeur 💻

👉 [Migration Angular 12 → 20](guide/03-MIGRATION-ANGULAR.md)

### Je suis un Architecte 🏗️

👉 [Vue d'Ensemble Architecture](architecture/00-OVERVIEW.md)

### Je veux Déployer l'Application 🚀

👉 [Configuration Docker Compose](architecture/03-DOCKER-COMPOSE-COMPLETE.md)

---

## 📜 Licence

Ce projet est sous licence MIT. Voir le fichier `LICENSE` pour plus de détails.

---

## 👏 Remerciements

Merci aux communautés **Angular**, **ASP.NET Core**, et à tous les contributeurs qui rendent ce projet possible !

---

**Bonne exploration de la documentation ! 🚀**

*Documentation mise à jour : Octobre 2025*  
*Version : 1.0.0*
