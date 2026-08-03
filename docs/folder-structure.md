# Structure du dépôt

## Vue d'ensemble

```
HalloApp/
├── API/                  # Core API — ASP.NET Core 9 (Clean Architecture)
│   ├── Api/               # Contrôleurs, filtres, SignalR hubs
│   ├── Application/       # DTOs, interfaces, events
│   └── Infrastructure/    # Data (EF Core), services concrets
├── ApiGateway/            # API Gateway (Ocelot) — point d'entrée unique
├── Services/
│   ├── ChatbotService/    # Coach de conversation (Ollama + Phi-3)
│   └── MediaService/      # Upload/stockage média (Cloudinary)
├── client/                # Frontend Angular 20 (standalone + signals)
│   └── src/
│       ├── app/            # Composant racine, routing
│       ├── core/           # Services, guards, pipes transverses
│       ├── features/       # Modules fonctionnels (7)
│       ├── layout/         # Navigation, thème
│       └── shared/         # Composants partagés (28)
├── docs/                  # Documentation publiée (voir mkdocs.yml)
│   ├── architecture/       # Vue d'ensemble, ADR, audits
│   └── guide/              # Guides utilisateur et migrations techniques
├── portfolio-site/        # Site vitrine personnel — artefact séparé du produit
├── .github/               # Workflows CI/CD, templates issue/PR, CODEOWNERS
├── docker-compose.yml
├── HalloApp.sln
├── mkdocs.yml             # Config de la doc publiée sur GitHub Pages
└── requirements.txt       # Dépendances Python pour builder la doc (CI)
```

## Pourquoi cette structure (et pas une autre)

- **Un dossier par service déployable** (`API`, `ApiGateway`, `Services/*`,
  `client`) : chacun a son propre `Dockerfile` et se build/déploie
  indépendamment — reflète l'architecture microservices réelle plutôt que de
  forcer une structure monolithique.
- **`docs/` versionné avec le code** plutôt qu'un wiki séparé : une PR qui
  change un comportement peut/doit mettre à jour la doc dans le même commit.
- **`portfolio-site/` isolé** : c'est un artefact personnel (page vitrine),
  pas le produit — il a son propre README qui l'explique pour éviter toute
  confusion sur ce que ce dépôt représente.
- **Pas de dossiers génériques type `examples/`, `templates/`, `benchmarks/`**
  : ce ne sont pas des dossiers pertinents pour une app produit — chaque
  dossier ci-dessus a un usage réel constaté dans le code, pas une
  convention copiée d'un autre type de projet (SDK, librairie).

## Historique

Jusqu'en août 2026, l'intégralité du produit était imbriquée dans un
sous-dossier `halloapp-2025-main/` à la racine du dépôt Git — un résidu
d'extraction de zip jamais nettoyé. Le dépôt a été aplati à la racine
(historique Git préservé via `git mv`) pour que `git clone` donne
directement une structure de projet normale.
