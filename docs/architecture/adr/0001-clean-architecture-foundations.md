# ADR-0001 : Fondations Clean Architecture + DDD tactique pour l'API

## Statut

Accepté · 2026-07-25

## Contexte

L'API (`API/`) est aujourd'hui organisée en couches techniques plates (`Controllers/`,
`Entities/`, `Data/`, `Services/`, `Interfaces/`, `DTOs/`, `Helpers/`) sans séparation
explicite entre logique métier et détails d'infrastructure. Deux symptômes concrets
observés dans le code actuel motivent cette ADR :

1. **Pas d'agrégat `Match` explicite.** Un "match" (like réciproque) n'existe pas comme
   entité persistée. `LikesRepository.GetMemberLikes` (cas `"mutual"`) le recalcule à
   chaque requête en croisant les `MemberLike` des deux sens :

   ```csharp
   var likeIds = await GetCurrentMemberLikeIds(likesParams.MemberId);
   result = query.Where(x => x.TargetMemberId == likesParams.MemberId
       && likeIds.Contains(x.SourceMemberId));
   ```

   Ce calcul à la volée n'est pas indexable, ne peut pas porter d'invariants métier
   (ex. date du match, statut), et mélange logique métier et requête EF Core.

2. **Pas de value objects.** `Email` est un `string` nu sur `AppUser`/`RegisterDto`,
   validé uniquement par l'attribut `[EmailAddress]`. `MinAge`/`MaxAge` sont deux `int`
   indépendants sur `MemberParams`, sans garantie que `Min <= Max` ni de logique
   partagée entre les endroits qui manipulent une tranche d'âge.

Si on écrit des tests unitaires (Phase 2 de la roadmap) sur l'état actuel, on finit par
tester des requêtes EF Core et des contrôleurs, pas des règles métier : il n'y a pas de
domaine isolable à tester.

## Décision

### 1. Découpage en couches par dossier, pas par projet

On introduit 4 dossiers dans le projet **`API/` existant** (pas de nouveaux `.csproj`) :

```
API/
  Domain/          : entités, value objects, agrégats, exceptions métier. Zéro
                     dépendance à EF Core, ASP.NET Core ou tout package externe.
  Application/     : interfaces (ports), DTOs, validators FluentValidation,
                     paramètres de requête (MemberParams, LikesParams), services
                     d'orchestration métier (ex. détection de match).
  Infrastructure/  : AppDbContext, implémentations EF Core des repositories,
                     migrations, PhotoService (Cloudinary), TokenService,
                     configuration Fluent API.
  Api/             : Controllers, Program.cs (composition root), middlewares,
                     extensions DI, Hubs SignalR.
```

**Alternative rejetée** : un projet `.csproj` par couche (Clean Architecture "à la
Jason Taylor"). Rejeté pour cette phase parce que (a) la roadmap cible explicitement
un découpage par dossier dans `API/`, (b) multiplier les projets maintenant augmente le
blast radius pour un gain limité tant qu'il n'y a qu'un seul service concerné, et
(c) rien n'empêche d'extraire `Domain`/`Application` en projets séparés plus tard si le
besoin de réutilisation (ex. partager `Domain` avec un futur worker) apparaît.

### 2. Règle de dépendance stricte pour `Domain`

`Domain/` ne référence **aucun** `using Microsoft.EntityFrameworkCore`. Les attributs de
mapping EF actuellement présents sur les entités (`[ForeignKey]` sur `Member.User`)
migrent vers de la configuration Fluent API dans
`Infrastructure/Data/Configurations/`.

**Exception assumée** : `[JsonIgnore]` reste sur les entités `Domain` pour cette phase,
car les entités sont encore sérialisées directement vers le client sur plusieurs
endpoints (`GetMember`, `GetMembers`). Séparer complètement modèle de lecture et modèle
de domaine (DTOs partout) est repoussé à la Phase 3 (CQRS), où l'introduction de
handlers de requêtes rend ce découpage naturel. Documenté ici comme dette technique
connue, pas comme un oubli.

### 3. Value objects : `Email`, `AgeRange`

Deux value objects DDD (immutables, égalité par valeur, auto-validants à la
construction) :
- `Email` remplace le `string` sur `AppUser` et dans les DTOs qui en ont besoin.
- `AgeRange` remplace la paire `MinAge`/`MaxAge` sur `MemberParams`, garantit
  `Min <= Max` et centralise la conversion âge → `DateOnly` (actuellement dupliquée
  dans `MemberRepository.cs`).

### 4. Agrégat `Match`

Nouvelle entité `Match` (agrégat racine) persistée en base, créée au moment où un like
devient réciproque (détecté dans un service d'orchestration `Application`, pas dans le
contrôleur). Remplace le calcul `"mutual"` de `LikesRepository`. Porte les invariants :
paire de membres non ordonnée mais stockée de façon déterministe (évite les doublons
`(A,B)` / `(B,A)`), horodatage de création.

Pas de domain events / outbox à ce stade (Phase 3). La détection reste synchrone dans
le même `DbContext.SaveChanges`.

### 5. FluentValidation, en complément, pas en remplacement immédiat

FluentValidation est introduit pour les DTOs touchés par cette phase
(`RegisterDto`, `MemberUpdateDto`, `SavePromptAnswersDto`). Les DTOs non touchés
gardent leurs `DataAnnotations` existantes ; la migration complète se fait
opportunistement au fil des phases suivantes plutôt qu'en un seul gros commit.

### 6. OpenAPI versionné

Swashbuckle génère la spec (`/swagger/v1/swagger.json`), versionnée dès maintenant
(`v1`) même à service unique, pour que le contrat soit stable quand `ApiGateway`
agrège plusieurs services. `openapi-typescript` génère le client TS consommé par
`client/src/core/services`.

## Conséquences

**Positif**
- Un domaine isolable existe : la Phase 2 peut écrire des tests unitaires sur
  `Match`, `Email`, `AgeRange` sans base de données ni `WebApplicationFactory`.
- Le "match" devient une donnée réelle, indexable, extensible (statut, date),
  prérequis pour toute feature future (notifications de match, historique).
- Le contrat HTTP est explicite et versionné, prérequis pour le client Angular
  généré et pour `ApiGateway`.

**Négatif / dette assumée**
- Domaine et sérialisation JSON restent couplés (`[JsonIgnore]`) jusqu'à la Phase 3.
- Migration EF Core nécessaire pour la table `Matches` ; les `MemberLike` existants
  doivent être rejoués pour backfiller les matchs déjà réciproques en prod/dev.
- Deux systèmes de validation (DataAnnotations + FluentValidation) coexistent
  temporairement sur des DTOs différents.

## Critères de validation

- Tous les endpoints existants répondent identiquement après la restructuration
  (vérifié manuellement / via les requêtes `.http` existantes, pas de test auto,
  arrive en Phase 2).
- `grep -r "Microsoft.EntityFrameworkCore" API/Domain/` ne retourne aucun résultat.
