# Guide de Résolution des Problèmes WebSocket

## Résumé des Problèmes Rencontrés

### Problème Principal
L'application rencontrait des erreurs de connexion WebSocket empêchant le bon fonctionnement des messages et notifications en temps réel.

### Symptômes Observés
- Erreurs "Cannot send data if the connection is not in the 'Connected' State"
- Erreurs "Handshake was canceled" 
- Connexions WebSocket qui échouent immédiatement
- Messages et notifications non fonctionnels
- Indicateurs de frappe non opérationnels

## Solutions Appliquées

### 1. Routage WebSocket Optimisé

**Problème :** Les connexions WebSocket passaient par l'API Gateway (port 5000) causant des problèmes de proxy.

**Solution :** Configuration directe vers l'API Core (port 5001).

```typescript
// client/src/environments/environment.ts
export const environment = {
    production: true,
    apiUrl: 'http://localhost:5000/api/',      // API Gateway pour les requêtes HTTP
    hubUrl: 'http://localhost:5001/hubs/'       // API Core direct pour les WebSockets
};
```

### 2. Gestion d'État de Connexion Robuste

**Problème :** Tentatives d'envoi de données sur des connexions fermées.

**Solution :** Vérifications d'état avant chaque envoi.

```typescript
// client/src/core/services/message-service.ts
sendMessage(recipientId: string, content: string) {
    if (this.hubConnection?.state !== HubConnectionState.Connected) {
        console.error('❌ Impossible d\'envoyer le message: connexion non établie');
        this.toast.error('Erreur de connexion. Veuillez réessayer.');
        return Promise.reject('Connection not established');
    }
    return this.hubConnection.invoke('SendMessage', {recipientId, content})
}
```

### 3. Reconnexion Automatique Intelligente

**Problème :** Pas de gestion des déconnexions et reconnexions.

**Solution :** Stratégie de backoff exponentiel.

```typescript
// client/src/core/services/presence-service.ts
.withAutomaticReconnect({
    nextRetryDelayInMilliseconds: retryContext => {
        if (retryContext.previousRetryCount === 0) {
            return 0;
        }
        return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
    }
})
```

### 4. Prévention des Connexions Multiples

**Problème :** Création de multiples connexions simultanées.

**Solution :** Vérification et arrêt des connexions existantes.

```typescript
// client/src/core/services/presence-service.ts
createHubConnection(user: User) {
    // Vérifier si une connexion existe déjà
    if (this.hubConnection?.state === HubConnectionState.Connected) {
        console.log('⚠️ PresenceService déjà connecté, arrêt de la connexion existante');
        this.stopHubConnection();
    }
    // ... reste du code
}
```

### 5. Configuration API Gateway Améliorée

**Problème :** API Gateway "unhealthy" à cause de routes manquantes.

**Solution :** Ajout de la route `/health` avec priorité élevée.

```json
// ApiGateway/ocelot.json
{
    "UpstreamPathTemplate": "/health",
    "UpstreamHttpMethod": [ "Get", "Head" ],
    "DownstreamPathTemplate": "/health",
    "DownstreamScheme": "http",
    "DownstreamHostAndPorts": [
        {
            "Host": "core-api",
            "Port": 80
        }
    ],
    "Priority": 10
}
```

### 6. Gestion d'Erreurs Côté Serveur

**Problème :** Erreurs de handshake non gérées.

**Solution :** Amélioration de la gestion d'erreurs dans les hubs SignalR.

```csharp
// API/SignalR/PresenceHub.cs
public override async Task OnConnectedAsync()
{
    try
    {
        var userId = GetUserId();
        await presenceTracker.UserConnected(userId, Context.ConnectionId);
        // ... reste du code
        Console.WriteLine($"User {userId} connected to PresenceHub");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in PresenceHub OnConnectedAsync: {ex.Message}");
        throw;
    }
}
```

## Fichiers Modifiés

### Client (Angular)
- `client/src/core/services/presence-service.ts` - Gestion robuste des connexions
- `client/src/core/services/message-service.ts` - Vérifications d'état avant envoi
- `client/src/core/services/account-service.ts` - Prévention des connexions multiples
- `client/src/app/app.ts` - Amélioration de l'initialisation
- `client/src/environments/environment.ts` - Routage direct vers Core API

### Serveur (API)
- `API/Program.cs` - Gestion d'erreurs d'authentification améliorée
- `API/SignalR/PresenceHub.cs` - Logs et gestion d'erreurs
- `API/SignalR/MessageHub.cs` - Robustesse améliorée

### API Gateway
- `ApiGateway/ocelot.json` - Route `/health` ajoutée avec priorité
- `ApiGateway/Program.cs` - Configuration CORS étendue

## Commandes de Résolution

### Reconstruction du Frontend
```bash
# Forcer la reconstruction avec les nouvelles configurations
docker-compose build frontend
docker-compose up -d frontend
```

### Redémarrage des Services
```bash
# Redémarrer tous les services
docker-compose restart core-api api-gateway frontend

# Vérifier l'état des services
docker-compose ps
```

### Vérification des Logs
```bash
# Logs API Gateway
docker-compose logs api-gateway --tail=20

# Logs Core API
docker-compose logs core-api --tail=20

# Logs Frontend
docker-compose logs frontend --tail=20
```

## Résultats Attendus

Après application de ces corrections :

- Plus d'erreurs "Cannot send data" - Vérifications d'état avant envoi
- Plus d'erreurs "Handshake was canceled" - Routage direct vers Core API
- Reconnexion automatique - En cas de perte de connexion
- Gestion robuste des états - Prévention des conflits
- Messages et notifications fonctionnels - WebSockets stables
- API Gateway healthy - Route de santé configurée

## Diagnostic

### Vérification des Connexions
1. Ouvrir http://localhost:4200
2. Se connecter avec un compte utilisateur
3. Aller dans la section Messages
4. Ouvrir la console du navigateur (F12)
5. Vérifier que les connexions se font vers `localhost:5001/hubs/`

### Logs de Succès Attendus
```
PresenceService connecté - Notifications globales activées
MessageService connecté
WebSocket connected to ws://localhost:5001/hubs/presence
WebSocket connected to ws://localhost:5001/hubs/messages
```

## Notes Importantes

- **Cache du navigateur** : Toujours vider le cache (Ctrl+F5) après reconstruction
- **Ordre des services** : Core API doit être démarré avant l'API Gateway
- **Ports** : 5000 (API Gateway), 5001 (Core API), 4200 (Frontend)
- **Environnement** : Utiliser l'environnement de production pour les WebSockets directs

## En Cas de Problème Persistant

1. **Vérifier l'état des services** : `docker-compose ps`
2. **Consulter les logs** : `docker-compose logs [service-name]`
3. **Reconstruire le frontend** : `docker-compose build frontend`
4. **Redémarrer tous les services** : `docker-compose restart`
5. **Vider le cache du navigateur** : Ctrl+F5
