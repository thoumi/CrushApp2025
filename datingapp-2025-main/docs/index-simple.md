# CrushApp - Documentation Essentielle

## Application Moderne de Rencontres

---

## Bienvenue

**CrushApp** est une application web moderne de rencontres développée avec **ASP.NET Core 9** et **Angular 20**.

### À Propos du Projet

- **Type d'application** : Application web de rencontres (Dating App)
- **Stack Frontend** : Angular 20, TailwindCSS, DaisyUI
- **Stack Backend** : ASP.NET Core 9, Entity Framework, SignalR
- **Architecture** : Hybride (Monolithe + Microservices)
- **Statut** : En développement actif

---

## Démarrage Rapide

### Pour les Utilisateurs

Vous souhaitez comprendre **comment utiliser l'application** ?

👉 **Commencez par** :
1. [Fonctionnalités](guide/01-FONCTIONNALITES.md) - Découvrez ce que fait l'application
2. [Guide Utilisateur](guide/02-GUIDE-UTILISATEUR.md) - Mode d'emploi détaillé

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

📖 **Lire** : [Fonctionnalités](guide/01-FONCTIONNALITES.md)

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

📖 **Lire** : [Migration Angular 12 → 20](guide/03-MIGRATION-ANGULAR.md)

**Résultats Mesurés** :
- 🚀 Build **73% plus rapide** (120s → 32s)
- 📦 Bundle size **-36%** (485kb → 312kb)
- ⚡ Time to Interactive **-50%** (2.8s → 1.4s)
- 🧹 **-35%** de fichiers (suppression des NgModules)

#### 3️⃣ Explorer l'Architecture

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

📖 **Lire** : [Architecture](architecture/00-OVERVIEW.md)

---

## Déploiement

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

📖 **Guide Complet** : [Docker Compose](architecture/03-DOCKER-COMPOSE-COMPLETE.md)

---

## Résolution des Problèmes

### Guide WebSocket

Si vous rencontrez des problèmes de connexion WebSocket :

📖 **Lire** : [Guide WebSocket](TROUBLESHOOTING-WEBSOCKET.md)

**Problèmes Courants** :
- Erreurs "Cannot send data if the connection is not in the 'Connected' State"
- Erreurs "Handshake was canceled"
- Connexions WebSocket qui échouent immédiatement
- Messages et notifications non fonctionnels

---

## Support et Ressources

### Documentation

- **Documentation en ligne** : [thoumi.github.io/CrushApp2025](https://thoumi.github.io/CrushApp2025)
- **Repository GitHub** : [github.com/thoumi/CrushApp2025](https://github.com/thoumi/CrushApp2025)

### Communauté

- **GitHub Issues** : Signaler bugs et proposer features
- **GitHub Discussions** : Poser des questions générales
- **Pull Requests** : Contribuer au code

---

## Technologies et Outils

### Frontend

| Technologie | Version | Usage |
|------------|---------|-------|
| **Angular** | 20.3.2 | Framework SPA |
| **TypeScript** | 5.8.2 | Langage principal |
| **TailwindCSS** | 4.1.7 | Styling utility-first |
| **DaisyUI** | 5.0.37 | Composants UI |
| **SignalR Client** | 8.0.7 | WebSockets temps réel |

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

---

## Métriques du Projet

### Statistiques Générales

- **Lignes de code** : ~15 000 (Frontend + Backend)
- **Composants Angular** : 25
- **Services Angular** : 15
- **Controllers ASP.NET** : 8
- **Entités** : 7
- **Migrations EF** : 12
- **Tests unitaires** : 80+ (couverture 70%)

### Performance Build

- **Cold Build** : 12s (vs 45s Angular 12)
- **Incremental Build** : 2s (vs 8s)
- **Production Build** : 32s (vs 120s)
- **Hot Reload** : 0.5s (vs 3s)

---

## Contribuer au Projet

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

---

## Licence

Ce projet est sous licence MIT. Voir le fichier `LICENSE` pour plus de détails.

---

**Bonne exploration de la documentation ! 🚀**

*Documentation mise à jour : Octobre 2025*  
*Version : 1.0.0*
