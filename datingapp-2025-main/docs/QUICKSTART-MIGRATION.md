# ⚡ Quick Start - Migration Microservices

## 🎯 Démarrage Rapide en 5 Minutes

### 1️⃣ Prérequis Vérifiés
```bash
docker --version  # 24.0+
docker-compose --version  # 2.20+
```

### 2️⃣ Configuration
```bash
# Copier et éditer .env
cp .env.example .env
notepad .env  # Ajouter vos clés Cloudinary
```

### 3️⃣ Lancement
```bash
# Lancer tous les services
docker-compose up -d

# Attendre ~30 secondes pour l'initialisation
```

### 4️⃣ Vérification
```bash
# Health checks
curl http://localhost:5000/health  # ✅ Gateway
curl http://localhost:5001/health  # ✅ Core API
curl http://localhost:5002/health  # ✅ Chatbot
curl http://localhost:5003/health  # ✅ Media Service
```

### 5️⃣ Accès
- **Application**: http://localhost:4200
- **RabbitMQ**: http://localhost:15672 (admin/REDACTED_RABBITMQ_PASSWORD)
- **Seq Logs**: http://localhost:8081

---

## 🧪 Test Rapide

### Créer un compte
```bash
curl -X POST http://localhost:5000/api/account/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Pa$$w0rd",
    "displayName": "Test User",
    "dateOfBirth": "1990-01-01",
    "city": "Paris",
    "country": "France",
    "gender": "male"
  }'
```

### Utiliser le Chatbot
```bash
# Récupérer le token de la réponse ci-dessus, puis:
curl -X POST http://localhost:5000/api/chatbot/chat \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"message": "Bonjour !"}'
```

---

## 📊 Architecture

```
Frontend (4200) → Gateway (5000) → [ Core API (5001) ]
                                   [ Chatbot (5002) ]
                                   [ Media (5003)   ]
                 ↓
             RabbitMQ (5672) + Seq (8081)
```

---

## 🛠️ Commandes Essentielles

```bash
# Voir les logs
docker-compose logs -f

# Redémarrer
docker-compose restart

# Arrêter
docker-compose down

# Tout nettoyer
docker-compose down -v
```

---

## 📚 Documentation Complète

- **Migration Complète**: `MIGRATION-COMPLETE.md`
- **Tests E2E**: `TESTS-E2E.md`
- **Architecture**: `docs/architecture/`
- **En ligne**: https://yourusername.github.io/datingapp-2025-main/

---

## 🐛 Problèmes Courants

### Service ne démarre pas
```bash
docker-compose logs SERVICE_NAME
docker-compose restart SERVICE_NAME
```

### Port déjà utilisé
```bash
# Modifier dans docker-compose.yml
ports:
  - "NOUVEAU_PORT:8080"
```

### Base de données non créée
```bash
# Vérifier les migrations
docker-compose logs core-api | grep Migration
```

---

**🚀 Prêt à démarrer !**

