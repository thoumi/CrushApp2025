# Roadmap

HalloApp suit une feuille de route en 9 phases. Chaque phase démontre un
ensemble cohérent de compétences techniques, construit sur la précédente —
l'ordre n'est pas arbitraire (par exemple, CQRS/Kafka en Phase 3 suppose les
fondations Clean Architecture de la Phase 1).

## Phase 0 — Blueprint produit ✅

Positionnement produit ("intentionnalité & profondeur" plutôt que swipe
infini), identité de marque, remap de l'IA (écran par écran), 6 fonctionnalités
différenciantes scorées (UX / maintenabilité / performance / trade-offs).
Détail complet : [Vision produit](product/vision.md).

## Phase 1 — Fondations ✅

Clean Architecture (couches `Domain` / `Application` / `Infrastructure` /
`Api`), SOLID, DDD tactique, API-first.

- [ADR-0001 — Fondations Clean Architecture](architecture/adr/0001-clean-architecture-foundations.md)
- [Audit SOLID Phase 1](architecture/solid-audit-phase1.md) — les deux
  constats de sévérité Haute (agrégat `Match` comme source unique de
  vérité, extraction de l'algorithme du Rendez-vous du jour vers
  `Application`) sont corrigés. Les constats Medium/Basse restants sont
  volontairement reportés (`IPhotoService` → Phase 2, `IUnitOfWork` fourre-tout
  → Phase 3) plutôt que corrigés prématurément.
- Client TypeScript généré depuis l'OpenAPI (`openapi-typescript`) branché
  côté `client/src/core/api/schema.d.ts`.

**Non vérifié dans cette phase** : la parité manuelle des endpoints après
restructuration (critère de validation de l'ADR-0001) n'a été confirmée que
par la compilation (`dotnet build`), pas par un test end-to-end de l'API en
cours d'exécution — les tests automatisés arrivent en Phase 2.

## Phase 2 — Tests

xUnit (tests unitaires), Testcontainers (tests d'intégration avec vraie
base de données), Playwright (E2E), SonarCloud (qualité continue). Aujourd'hui :
0 spec malgré Jasmine/Karma déjà configuré côté Angular, 0 projet de test
côté .NET.

## Phase 3 — CQRS & événementiel

MediatR pour séparer commands/queries, véritable pattern Outbox (les
"Events" actuels n'en sont pas un), et introduction de Kafka **en complément**
de RabbitMQ (pas en remplacement — RabbitMQ reste pertinent pour la
messagerie inter-services synchrone, Kafka pour un flux d'événements
d'activité à replay). Entité `Match` indexée en base plutôt que réciprocité
recalculée à l'exécution.

## Phase 4 — Sécurité

OIDC/Entra ID, Azure Key Vault, rate limiting sur `/register`/`/login`,
conformité RGPD. Corrige les gaps documentés dans
[SECURITY.md](https://github.com/thoumi/HalloApp/blob/main/SECURITY.md) :
validation JWT issuer/audience désactivée, cookies refresh token non
sécurisés en dev.

## Phase 5 — Data

Redis comme backplane SignalR (vrai besoin de scaling à plusieurs instances,
pas ajouté pour la forme), MongoDB pour l'historique de chat, digest ETL
batch.

## Phase 6 — Observabilité

OpenTelemetry, Prometheus, Grafana, Application Insights — en plus de
Serilog/Seq déjà en place.

## Phase 7 — Industrialisation

Kubernetes/AKS, Terraform, Bicep, pipeline CI/CD complet (aujourd'hui,
GitHub Actions ne build/déploie que l'API — pas les 3 autres services).

## Phase 8 — IA avancée

Azure OpenAI/AI Foundry comme fournisseur alternatif à Ollama local, RAG,
Semantic Kernel — pour enrichir le "Coach de conversation" existant.

## Phase 9 — Polish 🔄 en cours

Documentation, storytelling, réorganisation du dépôt (cette page en fait
partie).

---

**Note de méthode** : cette roadmap ne cherche pas à cocher un maximum de
technologies pour la forme — chaque techno ajoutée résout un besoin réel
identifié dans l'audit du code existant (voir par exemple la justification
Kafka-en-complément-de-RabbitMQ ci-dessus, ou pourquoi MySQL/HDS ne
sont volontairement pas dans la liste).
