# FAQ

## Ce projet est-il open source ?

Non. Le code est visible publiquement sur GitHub à des fins d'évaluation
technique (recrutement, revue de code), mais reste sous licence "tous
droits réservés" — voir [LICENSE](https://github.com/thoumi/HalloApp/blob/main/LICENSE).

## Pourquoi documenter des failles de sécurité connues au lieu de les cacher ?

Parce que la transparence est elle-même une compétence évaluée : savoir
identifier, prioriser et planifier la correction d'un risque est plus
convaincant qu'un README qui prétend qu'il n'y en a aucun. Voir la section
[Sécurité & transparence](https://github.com/thoumi/HalloApp/blob/main/README.md#-sécurité--transparence)
du README.

## Pourquoi RabbitMQ *et* Kafka sont prévus, plutôt qu'un seul des deux ?

Ils répondent à des besoins différents. RabbitMQ reste pertinent pour la
messagerie inter-services synchrone/orientée tâche (déjà en place). Kafka
(Phase 3 de la [roadmap](roadmap.md)) est introduit pour un flux
d'événements d'activité qui a besoin d'être rejouable et conservé dans le
temps — un besoin que RabbitMQ ne couvre pas nativement. Ce n'est pas un
choix "resume-driven development" : chaque techno de la roadmap est
justifiée par un besoin identifié dans l'audit du code existant.

## Pourquoi le chatbot utilise Ollama en local plutôt qu'une API cloud (OpenAI, Azure OpenAI) ?

Choix initial pour ne pas dépendre d'une clé API tierce pendant le
développement. Une intégration Azure OpenAI/AI Foundry en alternative est
prévue en [Phase 8](roadmap.md#phase-8--ia-avancée) pour démontrer aussi
cette compétence, sans réécrire le service existant (juste un second
provider derrière la même interface).

## Le projet tourne-t-il vraiment en local avec Docker ?

Oui — `docker-compose up -d` démarre les 8 services (SQL Server, RabbitMQ,
Seq, Core API, Chatbot, Media, API Gateway, frontend). Voir
[Démarrage rapide](https://github.com/thoumi/HalloApp#démarrage-rapide) dans
le README. Le chatbot nécessite en plus qu'Ollama tourne sur la machine hôte
(`ollama run phi3`) — il n'est pas conteneurisé pour l'instant.

## Où en est le projet dans sa roadmap actuellement ?

Les Phases 0 (blueprint produit) et 1 (fondations Clean Architecture) sont
terminées. La Phase 2 (tests automatisés) est la prochaine étape. Voir le
détail phase par phase dans [roadmap.md](roadmap.md).
