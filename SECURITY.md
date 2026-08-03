# Politique de sécurité

## Signaler une vulnérabilité

Ne pas ouvrir d'issue publique pour une vulnérabilité. Contacter directement
l'auteur (voir le profil GitHub [@thoumi](https://github.com/thoumi)) avec :

- une description du problème et son impact potentiel
- les étapes pour le reproduire
- si possible, une suggestion de correction

Un retour est visé sous 7 jours.

## Lacunes connues (transparence assumée)

HalloApp est un projet en développement actif qui documente volontairement
ses lacunes de sécurité plutôt que de les cacher — la Phase 4 de la
[roadmap](README.md#-roadmap) existe précisément pour les corriger.

| Constat | Sévérité | Statut |
|---|---|---|
| `ValidateIssuer` / `ValidateAudience` désactivés sur la validation JWT (les 3 services) | Élevée | Connu — Phase 4 |
| Pas de rate limiting sur `/register` et `/login` | Élevée | Connu — Phase 4 |
| Cookie de refresh token `Secure=false` / `SameSite=Lax` | Moyenne (config dev uniquement) | Connu — Phase 4 |
| Matching réciproque recalculé à l'exécution plutôt qu'une entité `Match` indexée | Faible (perf, pas sécurité) | Connu — Phase 3 |

## Bonnes pratiques déjà en place

- Secrets (JWT signing key, identifiants Cloudinary/DB/RabbitMQ) injectés via
  variables d'environnement (`.env`, jamais committé — voir
  [`.env.example`](.env.example)), pas en dur dans le code.
- Historique Git nettoyé après une fuite ponctuelle de secrets (rotation
  effectuée, voir commentaire dans `.env`).
