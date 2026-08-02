# HalloApp - Application de Rencontres

## Description

Application web moderne de rencontres développée avec **ASP.NET Core 9** et **Angular 20**. Architecture microservices avec intégration IA conversationnelle.

## Technologies

- **Frontend** : Angular 20, TypeScript, TailwindCSS
- **Backend** : ASP.NET Core 9, C#, Entity Framework Core
- **Base de données** : SQL Server 2022
- **Temps réel** : SignalR WebSockets
- **IA** : Ollama + Phi-3 (chatbot conversationnel)
- **Architecture** : Microservices avec API Gateway (Ocelot)
- **DevOps** : Docker, Docker Compose

## Démarrage Rapide

### Développement Local

```bash
# Backend
cd API
dotnet restore
dotnet ef database update
dotnet run

# Frontend
cd client
npm install
npm start

# IA Chatbot
ollama run phi3
```

### Déploiement Microservices

```bash
docker-compose up -d
```

## Documentation

**Documentation technique complète** : https://thoumi.github.io/CrushApp2025/

## Fonctionnalités

- Authentification JWT avec refresh tokens
- Système de matching et likes
- Messagerie temps réel (SignalR)
- Chatbot IA conversationnel
- Gestion des médias (Cloudinary)
- Panel d'administration
- Architecture microservices

## Architecture

```
Angular SPA → API Gateway → [Core API, Chatbot Service, Media Service]
                    ↓
            [SQL Server, RabbitMQ, Seq Logs]
```

*Projet développé pour démontrer l'expertise technique en développement full-stack et architecture moderne*
