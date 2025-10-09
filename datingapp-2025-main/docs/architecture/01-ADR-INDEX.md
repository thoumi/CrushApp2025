# Architecture Decision Records (ADR)
## Index des Décisions Architecturales

---

## 📋 Qu'est-ce qu'un ADR ?

Un **Architecture Decision Record** documente :
- ✅ **Quoi** : Quelle décision a été prise
- ✅ **Pourquoi** : Contexte et motivations
- ✅ **Alternatives** : Options considérées et rejetées
- ✅ **Conséquences** : Impacts positifs et négatifs

---

## 📚 Liste des ADRs

### ADR-001: Adoption de l'Architecture Microservices Progressive
**Date** : 2025-01-09  
**Statut** : ✅ Accepté  
**[Lire le détail](#adr-001-adoption-de-larchitecture-microservices-progressive)

### ADR-002: Choix de l'API Gateway (Ocelot vs YARP)
**Date** : 2025-01-09  
**Statut** : ✅ Accepté  
**[Lire le détail](#adr-002-choix-de-lapi-gateway-ocelot-vs-yarp)

### ADR-003: Extraction du Chatbot comme Premier Microservice
**Date** : 2025-01-09  
**Statut** : ✅ Accepté  
**[Lire le détail](#adr-003-extraction-du-chatbot-comme-premier-microservice)

### ADR-004: Base de Données par Service vs Shared Database
**Date** : 2025-01-09  
**Statut** : ✅ Accepté (Hybride)  
**[Lire le détail](#adr-004-base-de-données-par-service-vs-shared-database)

### ADR-005: Communication Asynchrone avec RabbitMQ
**Date** : 2025-01-09  
**Statut** : ✅ Accepté  
**[Lire le détail](#adr-005-communication-asynchrone-avec-rabbitmq)

### ADR-006: Garder SignalR dans le Core API (pas de microservice)
**Date** : 2025-01-09  
**Statut** : ✅ Accepté  
**[Lire le détail](#adr-006-garder-signalr-dans-le-core-api)

### ADR-007: Docker Compose vs Kubernetes
**Date** : 2025-01-09  
**Statut** : ✅ Accepté (Docker Compose)  
**[Lire le détail](#adr-007-docker-compose-vs-kubernetes)

### ADR-008: Authentification Centralisée vs Distribuée
**Date** : 2025-01-09  
**Statut** : ✅ Accepté (Centralisée)  
**[Lire le détail](#adr-008-authentification-centralisée-vs-distribuée)

---

## 📄 Détails des ADRs

---

## ADR-001: Adoption de l'Architecture Microservices Progressive

### Contexte

L'application DatingApp est actuellement un **monolithe ASP.NET Core** avec :
- 10+ controllers
- Base de données SQL Server partagée
- SignalR pour temps réel
- Intégrations externes (Cloudinary, Ollama)

**Problèmes identifiés** :
- Difficulté à scaler le chatbot AI indépendamment (CPU intensive)
- Déploiement "big bang" risqué
- Couplage fort entre modules
- Objectif d'apprentissage : maîtriser les architectures distribuées

### Décision

**Nous adoptons une architecture microservices PROGRESSIVE (Phase 2)** :

1. **Extraire 2 microservices stratégiques** :
   - Chatbot Service (AI/Ollama)
   - Media Service (Photos/Cloudinary)

2. **Garder un Core API monolithique** pour :
   - Authentication/Identity
   - Members, Messages, Likes (fortement couplés)
   - SignalR (state partagé)

3. **Ajouter infrastructure minimale** :
   - API Gateway (Ocelot/YARP)
   - Message Broker (RabbitMQ)
   - Docker Compose orchestration

### Alternatives Considérées

#### Option A: Garder le Monolithe Complet ❌
**Rejetée** car :
- Ne permet pas d'apprendre les microservices
- Pas de scalabilité indépendante du chatbot
- Limite l'évolutivité technologique

#### Option B: Full Microservices (6+ services) ❌
**Rejetée** car :
- Complexité excessive pour la taille du projet
- Overhead opérationnel énorme
- SignalR difficile à distribuer
- Members/Messages/Likes trop couplés

#### Option C: Microservices Progressifs (Phase 2) ✅
**Choisie** car :
- Balance apprentissage / pragmatisme
- Services indépendants faciles à extraire
- Garde la cohérence transactionnelle où nécessaire
- Permet migration future si besoin

### Conséquences

#### Positives ✅
- Scalabilité indépendante du chatbot (CPU/RAM)
- Isolation des risques (failure chatbot ≠ failure app)
- Technologie flexible (changement LLM facile)
- Déploiement indépendant par service
- Expérience pratique microservices
- Portfolio technique moderne

#### Négatives ⚠️
- Complexité opérationnelle accrue (3 services vs 1)
- Latence réseau inter-services (minime en local)
- Debugging distribué plus difficile
- Infrastructure supplémentaire (Gateway, RabbitMQ)
- Courbe d'apprentissage initiale

#### Risques 🚨
- **Over-engineering** → Mitigé par approche progressive
- **Distributed monolith** → Évité via bounded contexts clairs
- **Performance dégradée** → Monitoring à mettre en place

---

## ADR-002: Choix de l'API Gateway (Ocelot vs YARP)

### Contexte

Une architecture microservices nécessite un **API Gateway** pour :
- Point d'entrée unique pour le frontend
- Routing vers les services backend
- Authentification centralisée (JWT)
- Cross-cutting concerns (CORS, Rate Limiting)

Options .NET disponibles :
- **Ocelot** : Gateway mature, riche en features
- **YARP** : Reverse proxy Microsoft, moderne et performant

### Décision

**Nous utilisons Ocelot comme API Gateway principal**

Configuration JSON simple :
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/chat/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{"Host": "chatbot-service", "Port": 5002}],
      "UpstreamPathTemplate": "/api/chatbot/{everything}",
      "AuthenticationOptions": {"AuthenticationProviderKey": "Bearer"}
    }
  ]
}
```

### Alternatives Considérées

#### Option A: Ocelot ✅
**Avantages** :
- Configuration JSON déclarative
- Support JWT built-in
- Rate limiting, caching, QoS
- Documentation mature
- Communauté active

**Inconvénients** :
- Performances légèrement inférieures à YARP
- Maintenance communautaire (pas Microsoft)

#### Option B: YARP (Yet Another Reverse Proxy) 🟡
**Avantages** :
- Microsoft officiel (.NET team)
- Performances excellentes (benchmarks)
- Configuration code-first flexible
- Future-proof (support long terme)

**Inconvénients** :
- Configuration C# (moins déclarative)
- Moins de features built-in
- Documentation moins fournie
- Courbe d'apprentissage plus raide

#### Option C: Kong / Nginx ❌
**Rejetée** car :
- Sortie de l'écosystème .NET
- Complexité de configuration
- Pas d'intégration native JWT .NET

### Décision Finale

**Ocelot pour démarrage rapide**, avec possibilité de migrer vers YARP plus tard si performances critiques.

### Conséquences

#### Positives ✅
- Setup rapide (< 1h)
- Configuration lisible (JSON)
- Auth JWT simple
- Features avancées disponibles (retry, circuit breaker)

#### Négatives ⚠️
- Légère dépendance communautaire
- Peut nécessiter migration YARP si scaling extrême

---

## ADR-003: Extraction du Chatbot comme Premier Microservice

### Contexte

Plusieurs candidats pour extraction :
- Chatbot (AI/Ollama)
- Media/Photos (Cloudinary)
- Members
- Messages

**Critères d'extraction** :
1. Indépendance fonctionnelle
2. Pas de transactions cross-service
3. Ressources isolables (CPU/RAM)
4. Complexité d'extraction faible
5. Valeur business/apprentissage élevée

### Décision

**Le Chatbot Service est extrait EN PREMIER** pour :

1. **Indépendance totale** : pas de dépendances sur Members/Messages
2. **Isolation ressources** : Ollama consomme beaucoup de CPU/RAM
3. **Évolutivité tech** : changement LLM (Llama → GPT) facile
4. **Faible risque** : fonctionnalité non-critique (nice-to-have)
5. **Complexité minimale** : 1 controller, 1 service, pas de DB

### Alternatives Considérées

#### Option A: Extraire Media Service en premier ❌
**Rejetée** car :
- Modération photos couplée à Admin
- Nécessite migration data (photos metadata)
- Events complexes (upload → approval → notification)

#### Option B: Extraire Members Service ❌
**Rejetée** car :
- Couplage fort avec Messages, Likes
- Transactions ACID nécessaires
- SignalR dépend des Members

#### Option C: Chatbot Service ✅
**Choisie** car :
- Quick win (1 semaine implémentation)
- Prouve la faisabilité microservices
- Apprentissage rapide Gateway + Events

### Conséquences

#### Positives ✅
- Premier microservice en production rapidement
- Apprentissage pratique sans risque
- Isolation CPU/RAM chatbot
- Template pour autres services

#### Négatives ⚠️
- Latence réseau supplémentaire (minime)
- Infrastructure Gateway nécessaire dès le début

---

## ADR-004: Base de Données par Service vs Shared Database

### Contexte

Principe microservices : **database per service** (isolation data).

Notre réalité :
- Core API : Members, Messages, Likes fortement couplés
- Chatbot : pas besoin de persistance (ou cache Redis)
- Media : metadata photos séparable

### Décision

**Approche HYBRIDE** :

1. **Core API** : garde SQL Server partagé (Users, Members, Messages, Likes)
2. **Chatbot Service** : 
   - Option A: sans DB (stateless)
   - Option B: Redis pour cache conversation
   - Option C: MongoDB pour historique
3. **Media Service** : **propre base de données** (SQL Server ou PostgreSQL)

### Justification

#### Core API: Shared Database ✅
**Raison** :
- Members ↔ Messages ↔ Likes sont transactionnelles
- Évite saga patterns complexes
- Performances (pas de distributed joins)

#### Media Service: Database per Service ✅
**Raison** :
- Bounded context clair (photos metadata)
- Communication async via events (RabbitMQ)
- Peut évoluer vers PostgreSQL (meilleur blob support)

### Conséquences

#### Positives ✅
- Évite saga complexes pour transactions critiques
- Permet isolation data où c'est utile (Media)
- Pragmatique vs dogmatique

#### Négatives ⚠️
- Core API reste couplé à la DB
- Futur découpage Core nécessiterait migration

---

## ADR-005: Communication Asynchrone avec RabbitMQ

### Contexte

Services doivent communiquer pour :
- Photo uploadée → notifier Core API
- Photo approuvée → mettre à jour Member
- User registered → initialiser services

Options :
- **Sync HTTP** : REST entre services
- **Async Messaging** : RabbitMQ, Azure Service Bus, Kafka

### Décision

**RabbitMQ pour communication inter-services asynchrone**

Events définis :
```csharp
public class PhotoUploadedEvent
{
    public Guid PhotoId { get; set; }
    public string UserId { get; set; }
    public string Url { get; set; }
}

public class PhotoApprovedEvent
{
    public Guid PhotoId { get; set; }
    public string ApprovedBy { get; set; }
}
```

### Alternatives Considérées

#### Option A: HTTP Sync (REST) ❌
**Rejetée** car :
- Couplage temporal (service doit être up)
- Pas de retry automatique
- Difficulté à broadcaster events

#### Option B: RabbitMQ ✅
**Choisie** car :
- Découpling complet
- Retry & Dead Letter Queue
- Facile à setup (Docker)
- Gratuit, open-source

#### Option C: Azure Service Bus ❌
**Rejetée** car :
- Coût Azure (learning project)
- Lock-in cloud

#### Option D: Kafka ❌
**Rejetée** car :
- Over-engineering (pas de streaming massif)
- Complexité opérationnelle élevée

### Conséquences

#### Positives ✅
- Resilience (service down = message queued)
- Scalabilité (multiple consumers)
- Audit trail (messages persistés)
- Apprentissage messaging patterns

#### Négatives ⚠️
- Infrastructure supplémentaire
- Debugging async plus difficile
- Eventual consistency à gérer

---

## ADR-006: Garder SignalR dans le Core API

### Contexte

SignalR gère :
- Messaging temps réel (MessageHub)
- Présence utilisateurs (PresenceHub, PresenceTracker)

Question : extraire dans un "Real-time Service" ?

### Décision

**SignalR RESTE dans le Core API**, pas de microservice dédié.

### Justification

#### Raisons Techniques
1. **State partagé** : PresenceTracker in-memory singleton
2. **Couplage fort** : Messages ↔ Members ↔ DB
3. **Complexité Redis Backplane** : nécessaire pour scaling SignalR distribué
4. **Low ROI** : extraction complexe, peu de bénéfices

#### Raisons Pragmatiques
- Fonction core business (messages = feature principale)
- Pas de gains isolation ressources
- Scaling horizontal possible (sticky sessions)

### Alternatives Considérées

#### Option A: Extraire Real-time Service ❌
**Rejetée** car :
- Nécessite Redis Backplane
- Complexité state management
- Latence accrue (service → DB)

#### Option B: Garder dans Core API ✅
**Choisie** car :
- Simplicité
- Performances optimales
- Scaling possible si besoin (load balancer sticky sessions)

### Conséquences

#### Positives ✅
- Simplicité architecture
- Performances temps réel optimales
- Pas de Redis Backplane nécessaire

#### Négatives ⚠️
- Core API légèrement plus lourd
- Scaling SignalR = scaling Core API complet

---

## ADR-007: Docker Compose vs Kubernetes

### Contexte

Orchestration container nécessaire pour :
- Multi-services (Core, Chatbot, Media, Gateway, RabbitMQ, SQL)
- Networking entre services
- Variables d'environnement
- Volumes persistants

Options :
- **Docker Compose** : simple, local/small prod
- **Kubernetes** : production-grade, complexe

### Décision

**Docker Compose pour développement ET production initiale**

Exemple `docker-compose.yml` :
```yaml
services:
  gateway:
    build: ./Gateway
    ports: ["5000:8080"]
    depends_on: [core-api, chatbot-service]
  
  core-api:
    build: ./API
    environment:
      - ConnectionStrings__DefaultConnection=Server=sql;...
    depends_on: [sql, rabbitmq]
  
  chatbot-service:
    build: ./ChatbotService
    environment:
      - Ollama__BaseUrl=http://host.docker.internal:11434
  
  media-service:
    build: ./MediaService
    depends_on: [rabbitmq]
  
  sql:
    image: mcr.microsoft.com/mssql/server:2022-latest
  
  rabbitmq:
    image: rabbitmq:3-management
    ports: ["5672:5672", "15672:15672"]
```

### Alternatives Considérées

#### Option A: Docker Compose ✅
**Avantages** :
- Setup simple (1 fichier YAML)
- Dev = Prod (consistency)
- Courbe d'apprentissage faible
- Suffisant pour <10 services

**Inconvénients** :
- Scaling limité (single host)
- Pas d'auto-healing avancé

#### Option B: Kubernetes ❌
**Rejetée** car :
- Over-engineering pour learning project
- Complexité énorme (manifests, helm, etc.)
- Coût cloud élevé (AKS/EKS)
- Temps d'apprentissage > temps dev features

#### Option C: Docker Swarm ❌
**Rejetée** car :
- Docker décline Swarm (focus Compose + K8s)
- Pas d'écosystème

### Décision Finale

**Docker Compose maintenant**, migration K8s optionnelle si réel besoin scaling.

### Conséquences

#### Positives ✅
- Démarrage rapide (`docker-compose up`)
- Même stack dev/prod
- Apprentissage Docker Compose (utile en soi)
- Facile à debugger

#### Négatives ⚠️
- Scaling limité à vertical scaling
- Migration K8s future si croissance

---

## ADR-008: Authentification Centralisée vs Distribuée

### Contexte

JWT authentication nécessaire pour :
- Protéger API Gateway
- Protéger chaque microservice
- Valider tokens

Options :
1. **Centralisée** : Gateway valide JWT, propage userId
2. **Distribuée** : Chaque service valide JWT
3. **Auth Service** : Service dédié (OAuth server)

### Décision

**Authentification CENTRALISÉE au niveau API Gateway**

Flow :
```
Client → Gateway (valide JWT) → Service (reçoit userId via header)
```

Implementation :
```csharp
// Gateway (Ocelot)
.AddJwtBearer("Bearer", options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Services reçoivent header:
// X-User-Id: "user-guid"
// X-User-Roles: "Member,Admin"
```

### Alternatives Considérées

#### Option A: Auth Centralisée (Gateway) ✅
**Avantages** :
- Single point validation
- Services plus simples (trust Gateway)
- Pas de duplication code JWT

**Inconvénients** :
- Gateway = single point of failure auth

#### Option B: Auth Distribuée (chaque service) ❌
**Rejetée** car :
- Duplication code validation
- Complexité configuration (shared secret)
- Performance (validation multiple)

#### Option C: Auth Service dédié (OAuth) ❌
**Rejetée** car :
- Over-engineering (déjà Identity dans Core API)
- Latence supplémentaire
- Complexité OAuth flows

### Conséquences

#### Positives ✅
- Simplicité microservices (pas de JWT logic)
- Validation unique (performances)
- Facile à changer stratégie auth

#### Négatives ⚠️
- Gateway doit être trusted
- Services exposés directement = vulnérabilité (mitigation: network isolation)

---

## 📊 Résumé des Décisions

| ADR | Décision | Rationale Clé |
|-----|----------|---------------|
| **001** | Microservices Progressifs (Phase 2) | Balance apprentissage / pragmatisme |
| **002** | Ocelot API Gateway | Setup rapide, features riches |
| **003** | Chatbot = premier microservice | Indépendant, faible risque, high learning |
| **004** | Hybride DB (shared + per-service) | Pragmatique vs dogmatique |
| **005** | RabbitMQ messaging | Async resilient, industry standard |
| **006** | SignalR dans Core API | Complexité extraction > bénéfices |
| **007** | Docker Compose | Simple, suffisant, même stack dev/prod |
| **008** | Auth centralisée (Gateway) | Single validation, services simples |

---

## 🔄 Révision des ADRs

Les ADRs peuvent être :
- **Acceptés** ✅ : Décision active
- **Deprecated** ⚠️ : Remplacée par nouveau ADR
- **Superseded** 🔄 : Évolution de décision
- **Rejected** ❌ : Alternative refusée

**Process de révision** :
1. Créer nouveau ADR si changement majeur
2. Référencer ADR précédent
3. Expliquer pourquoi changement nécessaire

---

## 📚 Ressources

- [ADR GitHub](https://adr.github.io/)
- [Microservices Patterns - Chris Richardson](https://microservices.io/patterns/)
- [.NET Microservices Architecture Guide](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/)

---

**Dernière mise à jour** : 2025-01-09  
**Prochain review** : Après implémentation Chatbot Service

