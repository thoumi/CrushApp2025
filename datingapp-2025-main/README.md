# CrushApp - Application de Rencontres Moderne

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-20-DD0031?logo=angular)](https://angular.io/)
[![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Application web moderne de rencontres développée avec **ASP.NET Core 9** et **Angular 20**, migrant progressivement vers une architecture microservices.

## Documentation Complète

**[Documentation en ligne](https://thoumi.github.io/CrushApp2025/)**

La documentation complète est disponible en ligne et couvre :
- Guide utilisateur complet
- Migration Angular 12 → 20 (Retour d'expérience)
- Architecture microservices
- Guides de déploiement Docker
- Monitoring et observabilité
- **Résolution des problèmes WebSocket** (Nouveau)

### Guide de Résolution des Problèmes

Si vous rencontrez des problèmes avec les connexions WebSocket, les messages ou les notifications :

**[Guide de Résolution WebSocket](docs/TROUBLESHOOTING-WEBSOCKET.md)**

Ce guide couvre :
- Erreurs "Cannot send data if the connection is not in the 'Connected' State"
- Erreurs "Handshake was canceled"
- Solutions complètes avec exemples de code
- Commandes de diagnostic et résolution

## Quick Start

### Prérequis

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optionnel)
- [SQL Server](https://www.microsoft.com/sql-server) ou LocalDB
- [Ollama](https://ollama.ai/) (pour le chatbot IA)

### Option 1 : Lancement Standard (Sans Docker)

#### Backend

```bash
# 1. Aller dans le dossier API
cd API

# 2. Restaurer les dépendances
dotnet restore

# 3. Configurer la base de données
# Éditer appsettings.Development.json avec votre connection string

# 4. Appliquer les migrations
dotnet ef database update

# 5. Lancer l'API
dotnet run

# L'API démarre sur https://localhost:5001
```

#### Frontend

```bash
# 1. Aller dans le dossier client
cd client

# 2. Installer les dépendances
npm install

# 3. Lancer l'application Angular
npm start

# L'app démarre sur http://localhost:4200
```

### Option 2 : Lancement avec Docker (Architecture Microservices)

```bash
# 1. Créer fichier .env depuis l'exemple
cp .env.example .env

# 2. Éditer .env et remplir vos credentials Cloudinary

# 3. Lancer tous les services
docker-compose up -d

# 4. Vérifier l'état des services
docker-compose ps

# 5. Voir les logs
docker-compose logs -f

# Services disponibles :
# - Frontend : http://localhost:4200
# - API Gateway : http://localhost:5000
# - Core API : http://localhost:5001
# - Chatbot Service : http://localhost:5002
# - Media Service : http://localhost:5003
# - RabbitMQ Management : http://localhost:15672 (admin/REDACTED_RABBITMQ_PASSWORD)
# - Seq Logs : http://localhost:5341 (admin/admin)
```

## ✨ Fonctionnalités

### Pour les Utilisateurs

- 👤 **Profils riches** : Multi-photos, description personnalisée
- 🔍 **Recherche avancée** : Filtres par âge, genre, localisation
- ⭐ **Système de likes** : Détection automatique des matches mutuels
- 💬 **Messagerie temps réel** : SignalR avec indicateurs de présence
- 🤖 **Chatbot IA** : Assistant conversationnel avec Ollama
- 🌍 **Multilingue** : Français et Anglais
- 🎨 **Thèmes** : Mode clair et sombre

### Pour les Administrateurs

- 🛡️ **Gestion des rôles** : User, Moderator, Admin
- 📸 **Modération photos** : File d'attente d'approbation
- 📊 **Statistiques** : Dashboard en temps réel
- 🔧 **Panel admin** : Gestion complète des utilisateurs

## 🏗️ Architecture

### Monolithe Actuel

```
Angular SPA ──> ASP.NET Core API ──> SQL Server
                      │
                      ├──> Cloudinary (Photos)
                      └──> Ollama (Chatbot)
```

### Architecture Microservices Cible

```
Angular SPA
    │
    ↓
API Gateway (Ocelot)
    │
    ├──> Core API (Membres, Messages, Likes)
    ├──> Chatbot Service (IA)
    └──> Media Service (Photos)
         │
         ├──> RabbitMQ (Events)
         ├──> SQL Server
         ├──> Seq (Logs)
         └──> Cloudinary
```

## 📦 Stack Technique

### Frontend

- **Framework** : Angular 20.3 (Standalone Components)
- **State Management** : Signals
- **UI** : TailwindCSS 4 + DaisyUI 5
- **Temps Réel** : SignalR Client
- **Build** : ESBuild (75% plus rapide)

### Backend

- **Framework** : ASP.NET Core 9
- **ORM** : Entity Framework Core 9
- **Auth** : ASP.NET Identity + JWT
- **Temps Réel** : SignalR
- **Database** : SQL Server 2022

### Infrastructure

- **Conteneurisation** : Docker + Docker Compose
- **Message Broker** : RabbitMQ 3.13
- **Logs** : Seq (centralisé)
- **API Gateway** : Ocelot
- **Stockage** : Cloudinary
- **IA** : Ollama (LLM local)

## 📊 Performances

### Build Times

| Métrique | Angular 12 | Angular 20 | Amélioration |
|----------|-----------|-----------|--------------|
| Cold Build | 45s | 12s | **-73%** |
| Hot Reload | 3s | 0.5s | **-83%** |
| Prod Build | 120s | 32s | **-73%** |

### Bundle Size

- **Initial** : 312kb (vs 485kb avant) → **-36%**
- **Main (gzip)** : 124kb (vs 187kb) → **-34%**

### Runtime

- **Time to Interactive** : 1.4s (vs 2.8s) → **-50%**
- **Largest Contentful Paint** : 1.1s (vs 2.3s) → **-52%**

## 🧪 Tests

```bash
# Tests Backend
cd API
dotnet test

# Tests Frontend
cd client
npm test

# Tests E2E
npm run e2e
```

## 📖 Documentation

### Guides Principaux

1. **[Guide Utilisateur](https://thoumi.github.io/CrushApp2025/guide/02-GUIDE-UTILISATEUR/)** - Comment utiliser l'application
2. **[Migration Angular](https://thoumi.github.io/CrushApp2025/guide/03-MIGRATION-ANGULAR/)** - Retour d'expérience Angular 12 → 20
3. **[Architecture Microservices](https://thoumi.github.io/CrushApp2025/architecture/00-OVERVIEW/)** - Vue d'ensemble technique
4. **[Guide de Migration](https://thoumi.github.io/CrushApp2025/architecture/02-MIGRATION-GUIDE/)** - Étapes de migration

### Documentation Locale

```bash
# Installer MkDocs
pip install -r requirements.txt

# Lancer le serveur de documentation
mkdocs serve

# Accéder à http://localhost:8000
```

## 🤝 Contribution

Les contributions sont les bienvenues ! Voir [CONTRIBUTING.md](CONTRIBUTING.md) pour les détails.

### Workflow

1. Fork le projet
2. Créer une branche : `git checkout -b feature/ma-fonctionnalite`
3. Commit : `git commit -m "feat: ajout de X"`
4. Push : `git push origin feature/ma-fonctionnalite`
5. Ouvrir une Pull Request

## 🐛 Signaler un Bug

Ouvrir une [issue](https://github.com/thoumi/CrushApp2025/issues) avec :
- Description du bug
- Étapes pour reproduire
- Comportement attendu vs actuel
- Screenshots si pertinent
- Environnement (OS, browser, version)

## 📝 Changelog

Voir [CHANGELOG.md](CHANGELOG.md) pour l'historique des versions.

## 📜 Licence

Ce projet est sous licence MIT. Voir [LICENSE](LICENSE) pour plus de détails.

## 👏 Remerciements

- Communauté [Angular](https://angular.io/)
- Communauté [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- [Ollama](https://ollama.ai/) pour le LLM local
- [Cloudinary](https://cloudinary.com/) pour le stockage d'images

## 📞 Support

- **Documentation** : https://thoumi.github.io/CrushApp2025/
- **Issues** : https://github.com/thoumi/CrushApp2025/issues
- **Discussions** : https://github.com/thoumi/CrushApp2025/discussions

---

**Développé avec ❤️ par l'équipe CrushApp**

⭐ N'oubliez pas de star le projet si vous le trouvez utile !

