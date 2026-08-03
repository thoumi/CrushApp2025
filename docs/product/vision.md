# Vision produit — Phase 0

Ce document capture les décisions de positionnement et d'identité prises en
Phase 0 de la [roadmap](../roadmap.md), pour qu'elles restent une référence
versionnée du dépôt plutôt qu'un artefact de conversation éphémère.

## Positionnement

**Intentionnalité & profondeur**, plutôt que volume et swipe infini.
L'hypothèse de départ : la plupart des apps de rencontres optimisent pour la
quantité (grille infinie, swipe compulsif, matchs jetables) — HalloApp
optimise pour la qualité de la rencontre plutôt que le nombre de profils vus.

Deux autres directions de positionnement ont été envisagées et écartées :
"Affinités & activités" (matching par centres d'intérêt/événements) et
"Confiance & sécurité d'abord" (vérification renforcée en premier argument).
Retenu : intentionnalité & profondeur, jugé le plus différenciant face aux
apps mainstream.

## Identité de marque

| Élément | Valeur |
|---|---|
| Nom | HalloApp |
| Ton | Chaleureux, éditorial |
| Encre (texte/fond sombre) | `#14121F` |
| Corail (accent unique) | `#FF6B5B` |
| Sable (fond clair) | `#F5EDE3` |
| Mousse (confiance/vérifié) | `#2F6E5C` |
| Ardoise (texte secondaire) | `#6B6478` |
| Typo display/éditoriale | Fraunces |
| Typo UI/texte courant | Inter |
| Logomark | Motif "halo" en anneau brisé — remplace l'icône cœur générique |

## Fonctionnalités différenciantes

Six fonctionnalités ont été identifiées comme porteuses du positionnement,
chacune évaluée selon UX / maintenabilité / performance / trade-offs avant
d'entrer en roadmap :

1. **Prompts de profil guidés** — remplace la bio en texte libre par des
   questions structurées (force la spécificité, réduit le vide du profil).
2. **Intro vocale de 30s** — étend le `MediaService` existant.
3. **"Rendez-vous du jour"** — sélection quotidienne curatée à la place
   d'une grille infinie ; implémentée côté code par
   [`DailySelectionService`](https://github.com/thoumi/HalloApp/blob/main/API/Application/Services/DailySelectionService.cs)
   (Phase 1).
4. **Entité "Étincelle" (Match)** — un match explicite et persisté plutôt
   qu'un état recalculé à l'exécution ; implémentée en Phase 1 via
   l'agrégat `Match` (voir [ADR-0001](../architecture/adr/0001-clean-architecture-foundations.md)).
5. **Coach de conversation** — réutilise le `ChatbotService` (Ollama/Phi-3)
   existant pour suggérer des amorces de conversation plutôt que de le
   cantonner à un chatbot générique.
6. **Vérification photo légère + mode invisible temporaire** — renforce la
   confiance sans friction excessive à l'inscription.

## IA remap

La navigation existante (héritée du cours de départ) a été remappée écran
par écran vers cette nouvelle identité — les écrans génériques
("Discover"/grille de cartes, bio libre) sont remplacés par leurs
équivalents intentionnels ci-dessus au fil des phases suivantes, pas en un
seul big-bang.

## Lien avec la roadmap technique

Ce blueprint produit (positionnement + identité) est indépendant mais
complémentaire de la [roadmap technique en 9 phases](../roadmap.md) : les
fonctionnalités différenciantes ci-dessus s'implémentent au fil des phases
(ex. l'agrégat `Match` et le Rendez-vous du jour en Phase 1, le Coach de
conversation enrichi en Phase 8 avec Azure OpenAI/RAG).
