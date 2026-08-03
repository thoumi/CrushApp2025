# Contribuer

HalloApp est avant tout un projet personnel de démonstration technique
(voir [LICENSE](LICENSE) — tous droits réservés). Les retours sont
bienvenus, mais la fusion de contributions reste à la discrétion de l'auteur.

## Signaler un bug ou proposer une idée

Ouvrir une [issue](https://github.com/thoumi/HalloApp/issues) en utilisant le
template correspondant (bug ou feature request). Merci d'inclure :

- pour un bug : étapes de reproduction, comportement attendu vs observé, logs pertinents
- pour une idée : le problème utilisateur ou technique résolu, pas seulement la solution proposée

## Proposer une modification (Pull Request)

1. Fork du dépôt, branche depuis `main` (`feat/...`, `fix/...`)
2. Un PR = un sujet (évitez de mélanger refactor + feature)
3. Respecter la structure existante : ne pas modifier le code métier sans
   justification liée à un bug ou une feature clairement décrite dans la PR
4. Vérifier que le projet build (`dotnet build`, `npm run build`) avant de soumettre

## Environnement de développement

Voir [Démarrage rapide](README.md#démarrage-rapide) dans le README.

## Standards de code

- **Backend** : conventions .NET standard, respect des couches Clean
  Architecture en place depuis l'[ADR-0001](docs/architecture/adr/0001-clean-architecture-foundations.md)
- **Frontend** : composants standalone Angular, signals plutôt que
  souscriptions RxJS manuelles quand c'est pertinent
- **Commits** : messages descriptifs en français ou anglais, cohérents avec l'historique existant
