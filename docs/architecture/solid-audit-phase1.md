# Audit SOLID · Phase 1

Audit du code existant (`Infrastructure/Services`, `Infrastructure/Data`,
`Application/Interfaces`) après la restructuration en couches
([ADR-0001](adr/0001-clean-architecture-foundations.md)). Chaque constat cite
l'emplacement réel dans le code, avec une sévérité et une recommandation de
phase.

**Mise à jour** : les deux constats de sévérité Haute ci-dessous sont
maintenant corrigés (agrégat `Match` implémenté et utilisé comme source
unique de vérité, algorithme du Rendez-vous du jour extrait vers
`Application`). Ce document garde l'historique des constats d'origine —
chaque section porte son statut réel plutôt que d'être réécrite comme si les
problèmes n'avaient jamais existé.

## Corrigé pendant cette phase

### DIP · `LogUserActivity` dépendait d'Infrastructure depuis Application

**Avant** : `Application/Helpers/LogUserActivity.cs` faisait
`resultContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>()`,
un **Service Locator** qui va chercher `AppDbContext` (un type
`Infrastructure`) à l'exécution, depuis une classe rangée dans `Application`.

Deux problèmes cumulés :
1. La dépendance à `AppDbContext` était cachée (résolue via le conteneur au
   runtime) au lieu d'être visible dans le constructeur, impossible à tester
   sans démarrer tout le pipeline HTTP.
2. `Application` référençait `Infrastructure`, exactement le sens de
   dépendance interdit par la Clean Architecture qu'on vient d'établir.

**Correction appliquée** : classe déplacée vers `Api/Filters/LogUserActivity.cs`
(namespace `API.Api.Filters`), `AppDbContext` injecté par constructeur au lieu
d'un `GetRequiredService`. `Api` a le droit de composer des types concrets
`Infrastructure` (comme le fait déjà `Program.cs`) ; `Application`, non. La
dépendance est maintenant explicite et le filtre reste testable.

## Constats documentés (backlog, pas corrigés maintenant)

### ISP · `IUnitOfWork` est une interface fourre-tout

`Application/Interfaces/IUnitOfWork.cs` expose les 5 repositories
(`MemberRepository`, `MessageRepository`, `LikesRepository`, `PhotoRepository`,
`AdminRepository`) plus `Complete()`/`HasChanges()`. `MembersController`
(`Api/Controllers/MembersController.cs`) ne consomme que
`MemberRepository`/`PhotoService`, mais dépend transitivement des 5.

**Sévérité** : Moyenne. **Recommandation** : à surveiller en Phase 3 quand
MediatR/CQRS introduira des handlers avec des dépendances plus fines. Pas la
peine de fragmenter `IUnitOfWork` avant, ça ajouterait de la complexité sans
bénéfice immédiat.

### DIP · `IPhotoService` fuite le SDK Cloudinary dans le contrat Application

`Application/Interfaces/IPhotoService.cs` retourne `ImageUploadResult` et
`DeletionResult` (types de `CloudinaryDotNet.Actions`). N'importe quel
consommateur de ce port "Application" doit référencer le SDK Cloudinary : la
couche métier dépend d'un vendor concret, pas d'une abstraction.

**Sévérité** : Moyenne. **Recommandation** : introduire un
`PhotoUploadResult`/`PhotoDeleteResult` (DTOs Application) et faire le mapping
dans `PhotoService` (Infrastructure). Bon candidat pour la Phase 2 (les tests
unitaires sur les contrôleurs de photos deviendront possibles sans référencer
Cloudinary).

### SRP · logique métier enterrée dans les repositories EF Core

Deux cas concrets identifiés :

- `MemberRepository.GetDailySelectionAsync` contenait l'algorithme de
  sélection déterministe du jour (hash stable + `Random` seedé) directement
  dans la couche données. C'est une **règle métier** ("comment on choisit le
  Rendez-vous du jour"), pas un détail de persistance.
- `LikesRepository.GetMemberLikes`, cas `"mutual"`, recalculait la
  réciprocité à la volée, déjà identifié dans l'ADR-0001 comme la raison
  d'être de l'agrégat `Match`.

**Sévérité** : Haute. C'était directement le problème que la Phase 1 est
censée régler ("les tests finissent par tester EF Core plutôt que la logique
métier").

**✅ Statut : corrigé.**
- Le cas `"mutual"` de `LikesRepository.GetMemberLikes` interroge maintenant
  `context.Matches` (la vraie table, alimentée par `MatchingService` sur
  chaque like ajouté/retiré via `LikesController`) — une seule source de
  vérité, plus de recalcul.
- `GetDailySelectionAsync` a été extrait vers
  [`Application/Services/DailySelectionService.cs`](https://github.com/thoumi/HalloApp/blob/main/API/Application/Services/DailySelectionService.cs)
  (interface `IDailySelectionService`, brique `IDailySelectionUnitOfWork`).
  `MemberRepository` n'expose plus que des primitives de données pures
  (`GetCandidateMemberIdsAsync`, `GetMembersByIdsAsync`) ; l'algorithme de
  sélection (hash stable, seed, tri) vit dans `Application`, testable sans
  base de données dès que la Phase 2 écrit des tests.

### OCP · branchement par chaîne de caractères

`MemberRepository.GetMembersAsync` (`OrderBy: "created"`/défaut) et
`MessageRepository.GetMessagesForMember` (`Container: "Outbox"`/défaut) : un
`switch` sur des littéraux `string`. Ajouter une option demande de modifier la
méthode existante plutôt que de l'étendre.

**Sévérité** : Basse (2 branches, pas encore de code dupliqué). **Recommandation** :
pas d'action maintenant : bon exemple pédagogique OCP à citer, mais un `enum`
ou pattern Strategy serait de la sur-ingénierie pour 2 cas.

### DIP · `UnitOfWork` instancie ses repositories avec `new`

`Infrastructure/Data/UnitOfWork.cs` fait `new MemberRepository(context)`,
`new AdminRepository(context, userManager)`, etc. au lieu de les résoudre via
DI. Ajouter un repository oblige à modifier `UnitOfWork` plutôt que
d'enregistrer une implémentation dans `Program.cs`.

**Sévérité** : Basse. `UnitOfWork` vit dans `Infrastructure`, il a le droit de
connaître des types concrets ; c'est un choix de câblage manuel, pas une
violation de couche. **Recommandation** : à reconsidérer si `UnitOfWork` est
remplacé par des handlers MediatR en Phase 3, sinon laisser tel quel.

### Dépendance injectée mais jamais utilisée

`AdminRepository(AppDbContext context, UserManager<AppUser> userManager)`
(`Infrastructure/Data/AdminRepository.cs:13`) : `userManager` n'était jamais
lu (confirmé par le warning compilateur `CS9113`). Toutes les requêtes
passaient par `context.Users`/`context.UserRoles`/`context.Roles`
directement.

**Sévérité** : Basse.

**✅ Statut : corrigé.** Paramètre supprimé de `AdminRepository` et de
`UnitOfWork` (qui ne le consommait que pour construire `AdminRepository`) —
un signal qu'un audit SOLID vaut aussi pour repérer du code mort, pas
seulement des principes violés.

## Non trouvé

**LSP** : aucune violation identifiée. Le code n'a pas de hiérarchie
d'héritage custom à part `AppUser : IdentityUser` (fournie par le framework
ASP.NET Core Identity, pas un design utilisateur), donc rien à substituer
incorrectement.
