# Guide Utilisateur - CrushApp

## Mode d'Emploi Complet de l'Application

---

## 📋 Table des Matières

1. [Premiers Pas](#premiers-pas)
2. [Créer et Gérer son Profil](#créer-et-gérer-son-profil)
3. [Découvrir des Membres](#découvrir-des-membres)
4. [Système de Likes et Favoris](#système-de-likes-et-favoris)
5. [Messagerie](#messagerie)
6. [Utiliser le Chatbot](#utiliser-le-chatbot)
7. [Paramètres et Personnalisation](#paramètres-et-personnalisation)
8. [Panel Administrateur](#panel-administrateur)
9. [Conseils et Bonnes Pratiques](#conseils-et-bonnes-pratiques)
10. [FAQ et Dépannage](#faq-et-dépannage)

---

## 🚀 Premiers Pas

### Inscription

#### Étape 1 : Accéder à l'Application

Ouvrez votre navigateur et allez sur : `https://crushapp.com` (ou `http://localhost:4200` en développement)

#### Étape 2 : Remplir le Formulaire d'Inscription

1. Cliquez sur **"S'inscrire"** dans la barre de navigation
2. Remplissez les champs obligatoires :
   - **Nom d'affichage** : Le nom qui sera visible par les autres membres
   - **Email** : Votre adresse email (doit être valide)
   - **Mot de passe** : 
     - Minimum 8 caractères
     - Au moins une majuscule
     - Au moins un chiffre
     - Au moins un caractère spécial
   - **Date de naissance** : Vous devez avoir 18 ans minimum
   - **Genre** : Homme / Femme / Autre
   - **Ville** : Votre ville de résidence
   - **Pays** : Votre pays

3. Acceptez les conditions d'utilisation
4. Cliquez sur **"Créer mon compte"**

#### Étape 3 : Validation et Connexion

- Si tout est correct, vous serez automatiquement connecté
- Un token de session sera créé (valable 15 minutes, renouvelable)
- Vous serez redirigé vers la page d'accueil des membres

### Connexion (Utilisateurs Existants)

1. Cliquez sur **"Se connecter"** dans la navigation
2. Entrez votre **email** et **mot de passe**
3. Cliquez sur **"Connexion"**
4. Option **"Se souvenir de moi"** pour rester connecté (7 jours)

### Déconnexion

- Cliquez sur votre nom d'utilisateur en haut à droite
- Sélectionnez **"Déconnexion"**

---

## 👤 Créer et Gérer son Profil

### Compléter son Profil

#### Accéder à son Profil

1. Cliquez sur votre **photo/avatar** dans la barre de navigation
2. Ou allez dans la liste des membres et cliquez sur votre carte

#### Onglet "Profil"

**Informations Modifiables** :
- **Description** : Parlez de vous (500 caractères max)
  - Vos passions, hobbies
  - Ce que vous recherchez
  - Votre personnalité
- **Ville** : Mise à jour possible
- **Pays** : Mise à jour possible

**Informations Non Modifiables** :
- Nom d'affichage (fixe après inscription)
- Date de naissance / Âge
- Genre

**Enregistrement** :
- Le bouton **"Enregistrer"** apparaît dès que vous modifiez un champ
- Si vous quittez sans enregistrer, une alerte vous prévient

#### Onglet "Photos"

**Upload de Photos** :

1. Cliquez sur **"Ajouter une photo"**
2. Sélectionnez une image depuis votre ordinateur
   - Formats acceptés : JPG, PNG, GIF, WEBP
   - Taille maximale : 10 MB
3. L'image est uploadée vers Cloudinary (optimisation automatique)
4. **Attention** : Votre photo doit être approuvée par un modérateur avant d'être visible publiquement

**Gérer ses Photos** :

- **Photo Principale** : Cliquez sur **"Définir comme principale"** sous une photo
  - Cette photo sera votre avatar dans toute l'application
- **Supprimer une Photo** : Cliquez sur l'icône 🗑️ (confirmation demandée)
- **Galerie** : Jusqu'à 10 photos par profil

**Modération** :
- ⏳ **En attente de modération** : Photo uploadée mais pas encore approuvée
- ✅ **Approuvée** : Photo visible par tous
- ❌ **Rejetée** : Photo non conforme (vous recevrez une notification avec la raison)

#### Indicateur de Dernière Activité

Votre statut "Dernière activité" est mis à jour automatiquement :
- **En ligne** : Vert (actif dans les 5 dernières minutes)
- **Récent** : Jaune (actif dans l'heure)
- **Inactif** : Gris (plus d'une heure)

---

## 🔍 Découvrir des Membres

### Liste des Membres

#### Navigation

- Menu principal : **"Membres"**
- URL directe : `/members`

#### Affichage

- **Grille de cartes** : Chaque membre est représenté par une carte avec :
  - Photo principale
  - Nom d'affichage
  - Âge
  - Ville, Pays
  - Statut en ligne (pastille de couleur)
  - Bouton étoile (like)

#### Pagination

- **12 membres par page** (par défaut)
- Navigateur de pages en bas de la liste
- Affichage du nombre total : "X membres trouvés"

### Filtres Avancés

#### Ouvrir le Panneau de Filtres

- Cliquez sur l'icône **🔍 "Filtrer"** en haut de la liste

#### Filtres Disponibles

1. **Genre** :
   - Homme
   - Femme
   - Tous

2. **Tranche d'Âge** :
   - Curseur double pour min/max
   - Plage : 18 à 99 ans

3. **Localisation** :
   - Ville (saisie libre)
   - Pays (liste déroulante)

4. **Dernière Activité** :
   - Actifs aujourd'hui
   - Actifs cette semaine
   - Actifs ce mois
   - Tous

#### Appliquer les Filtres

- Cliquez sur **"Appliquer"**
- La liste se rafraîchit automatiquement
- Nombre de résultats affiché

#### Réinitialiser les Filtres

- Cliquez sur **"Réinitialiser"** pour revenir à la vue complète

### Tri

Options de tri :
- **Plus récents** : Membres inscrits récemment
- **Plus actifs** : Membres les plus connectés
- **Dernière connexion** : Membres connectés récemment

### Consulter un Profil Détaillé

1. Cliquez sur une **carte de membre** ou sur le **nom d'affichage**
2. Vous accédez au profil détaillé avec 3 onglets :
   - **📋 Profil** : Informations complètes
   - **📸 Photos** : Galerie photos (si plusieurs photos)
   - **💬 Messages** : Conversation directe

---

## ⭐ Système de Likes et Favoris

### Liker un Membre

#### Depuis la Liste des Membres

- Cliquez sur l'**icône étoile** ⭐ sur une carte
- Animation de remplissage de l'étoile
- Le like est enregistré instantanément

#### Depuis le Profil Détaillé

- Bouton **"Ajouter aux favoris"** en haut du profil
- L'étoile se remplit si le like est réussi

### Retirer un Like

- Cliquez à nouveau sur l'étoile remplie ⭐
- Confirmation demandée : **"Retirer des favoris ?"**
- Le like est supprimé

### Matches Mutuels (Crush)

**Qu'est-ce qu'un Match ?**
- Quand vous likez quelqu'un **ET** cette personne vous like aussi
- Une notification spéciale apparaît : **"C'est un match ! 🎉"**

**Avantages d'un Match** :
- Priorité dans la liste des messages
- Badge "Match" visible sur le profil
- Notification push (si activées)

### Consulter ses Favoris

#### Accéder à l'Onglet "Lists"

- Menu principal : **"Favoris"** ou **"Lists"**
- URL : `/lists`

#### Filtres de la Page Lists

1. **Membres que j'ai likés** :
   - Liste de tous les profils que vous avez likés
   - Indique si le like est réciproque

2. **Membres qui m'ont liké** :
   - Découvrez qui vous a ajouté aux favoris
   - Opportunité de créer des matches

3. **Mes Matches** (Filtré automatiquement) :
   - Likes mutuels uniquement
   - Bouton **"Envoyer un message"** directement accessible

---

## 💬 Messagerie

### Accéder à la Messagerie

- Menu principal : **"Messages"**
- URL : `/messages`
- Ou depuis un profil : onglet **"Messages"**

### Interface Messagerie

#### Vue d'Ensemble

**Colonne Gauche** : Liste des conversations
- Photo et nom du contact
- Dernier message (prévisualisation)
- Timestamp (il y a X minutes)
- Badge de messages non lus
- Indicateur de présence (en ligne / hors ligne)

**Colonne Droite** : Fil de conversation
- Historique complet des messages
- Vos messages : alignés à droite (fond bleu)
- Messages reçus : alignés à gauche (fond gris)
- Timestamps sur chaque message
- Indicateur "lu" (double coche bleue)

### Envoyer un Message

#### Depuis le Profil d'un Membre

1. Ouvrez le profil du membre
2. Allez dans l'onglet **"Messages"**
3. Tapez votre message dans le champ en bas
4. Appuyez sur **Entrée** ou cliquez sur **"Envoyer"**

#### Depuis la Page Messages

1. Sélectionnez une conversation existante
2. Tapez votre message
3. Envoyez

### Fonctionnalités Temps Réel

#### Indicateur de Présence

- **Pastille verte** : Membre en ligne actuellement
- **Pastille grise** : Membre hors ligne

#### Indicateur de Frappe

- Quand votre contact tape un message, vous voyez : **"Est en train d'écrire..."**

#### Réception Instantanée

- Les messages apparaissent en temps réel (pas besoin de rafraîchir)
- Notification sonore légère (si activée)
- Badge de notification sur l'icône "Messages" dans la navigation

### Marquer comme Lu

- Les messages sont automatiquement marqués comme lus quand vous ouvrez la conversation
- Le compteur de non lus se met à jour

### Supprimer un Message

#### Supprimer pour Vous

1. Survolez un message (le vôtre uniquement)
2. Cliquez sur l'icône **🗑️**
3. Confirmez : **"Supprimer ce message ?"**
4. Le message disparaît de votre vue (mais reste pour l'autre personne)

**Note** : Vous ne pouvez supprimer que vos propres messages.

### Filtrer les Conversations

Options de filtres :
- **Toutes** : Toutes les conversations
- **Non lues** : Conversations avec nouveaux messages
- **Récentes** : Conversations actives dans les 24h
- **Matches** : Conversations avec matches mutuels uniquement

### Rechercher dans les Conversations

- Barre de recherche en haut de la liste des conversations
- Recherche par nom d'utilisateur
- Filtrage instantané

---

## 🤖 Utiliser le Chatbot

### Accéder au Chatbot

- Menu principal : **"Chatbot"** ou **"Assistance"**
- URL : `/chat`
- Icône de robot dans la navigation

### Interface du Chatbot

**Style Messagerie** :
- Interface similaire à la messagerie classique
- Vos questions : alignées à droite
- Réponses du bot : alignées à gauche avec avatar robot

### Poser une Question

#### Types de Questions Possibles

1. **Conseils de Profil** :
   - "Comment rédiger une bonne description ?"
   - "Quelle photo de profil choisir ?"
   - "Comment être plus attractif ?"

2. **Conseils de Communication** :
   - "Comment briser la glace ?"
   - "Idées de premier message ?"
   - "Comment maintenir une conversation ?"

3. **Utilisation de l'App** :
   - "Comment fonctionnent les matches ?"
   - "Comment envoyer un message ?"
   - "C'est quoi un like réciproque ?"

4. **Conseils Relationnels** :
   - "Quelles questions poser lors d'un premier rendez-vous ?"
   - "Comment savoir si quelqu'un est intéressé ?"

#### Envoyer une Question

1. Tapez votre question dans le champ en bas
2. Appuyez sur **Entrée** ou cliquez sur **"Envoyer"**
3. Le chatbot traite votre demande (peut prendre 2-5 secondes)
4. La réponse s'affiche avec un effet de typing

### Fonctionnalités du Chatbot

#### Contexte de Conversation

- Le chatbot se souvient de la conversation en cours
- Vous pouvez poser des questions de suivi
- Exemple :
  - Vous : "Comment écrire un bon profil ?"
  - Bot : [Conseils détaillés]
  - Vous : "Et pour la longueur ?"
  - Bot : [Comprend que vous parlez toujours du profil]

#### Historique

- Toutes vos conversations sont sauvegardées
- Accessible à tout moment
- Scroll infini vers le haut pour voir l'historique

#### Réinitialiser la Conversation

- Bouton **"Nouveau sujet"** en haut de l'interface
- Efface le contexte actuel
- Utile pour changer complètement de sujet

### Modèle IA Utilisé

**Ollama (Local LLM)** :
- Modèle : Phi-3 / Llama 3 (configurable)
- Exécution locale pour confidentialité
- Vos conversations ne sont PAS envoyées à des services externes
- Réponses en français naturel

### Limitations

- Le chatbot est informatif, pas thérapeutique
- Ne remplace pas un conseiller professionnel
- Basé sur des connaissances générales
- Peut parfois donner des réponses imprécises (recouper avec d'autres sources)

---

## ⚙️ Paramètres et Personnalisation

### Langue de l'Interface

#### Changer la Langue

1. Cliquez sur le **sélecteur de langue** (🌐) dans la barre de navigation
2. Choisissez entre :
   - **🇫🇷 Français**
   - **🇬🇧 English**
3. L'interface se traduit instantanément

**Langues Disponibles** :
- Tous les textes statiques (boutons, labels, menus)
- Messages d'erreur
- Notifications

**Langues NON Traduites** :
- Contenus utilisateurs (descriptions, messages)
- Réponses du chatbot (suivent la langue de la question)

### Thème Visuel

#### Mode Clair / Sombre

1. Icône **☀️ / 🌙** dans la barre de navigation (en haut à droite)
2. Bascule instantanée
3. Préférence sauvegardée dans le navigateur

**Thème Clair** (par défaut) :
- Fond blanc
- Texte sombre
- Accent indigo

**Thème Sombre** :
- Fond gris foncé
- Texte clair
- Accent violet

### Notifications

#### Types de Notifications

1. **Navigateur** (si autorisées) :
   - Nouveaux messages
   - Nouveaux likes
   - Matches mutuels
   - Photo approuvée/rejetée

2. **In-App** (toujours actives) :
   - Badge sur l'icône Messages
   - Popup de match
   - Alertes système

#### Activer les Notifications Navigateur

1. À la première visite, une demande d'autorisation apparaît
2. Cliquez sur **"Autoriser"**
3. Ou dans les paramètres du navigateur :
   - Chrome : Paramètres > Confidentialité > Paramètres du site > Notifications
   - Firefox : Options > Vie privée > Permissions > Notifications

---

## 🛡️ Panel Administrateur

*Réservé aux utilisateurs avec rôle Administrateur ou Modérateur*

### Accéder au Panel Admin

- Menu principal : **"Admin"** (visible uniquement si vous avez les droits)
- URL : `/admin`

### Vue d'Ensemble

Le panel admin comprend deux sections principales :

1. **Gestion des Utilisateurs**
2. **Modération des Photos**

---

### 1. Gestion des Utilisateurs

#### Liste des Utilisateurs

**Colonnes Affichées** :
- Nom d'utilisateur
- Email (masqué partiellement)
- Rôles actuels
- Date d'inscription
- Dernière activité
- Statut (actif / suspendu)

#### Modifier les Rôles

**Rôles Disponibles** :
- **User** (par défaut) : Utilisateur standard
- **Moderator** : Peut modérer les photos
- **Admin** : Accès complet (gestion utilisateurs + modération)

**Procédure** :
1. Cliquez sur **"Modifier rôles"** à côté d'un utilisateur
2. Cochez/décochez les rôles :
   - ☑️ Admin
   - ☑️ Moderator
3. Cliquez sur **"Enregistrer"**
4. Les changements sont effectifs immédiatement

**Restrictions** :
- Un Admin ne peut pas retirer son propre rôle Admin
- Au moins un Admin doit toujours exister dans le système

#### Suspendre un Compte

1. Cliquez sur **"Suspendre"** à côté d'un utilisateur
2. Sélectionnez la raison :
   - Violation des conditions d'utilisation
   - Contenu inapproprié
   - Spam / Comportement abusif
   - Autre (spécifier)
3. Durée de suspension :
   - 24 heures
   - 7 jours
   - 30 jours
   - Permanent
4. Confirmez

**Effets d'une Suspension** :
- L'utilisateur ne peut plus se connecter
- Son profil est caché de la liste des membres
- Ses messages existants restent visibles

#### Réactiver un Compte

1. Filtrer par **"Comptes suspendus"**
2. Cliquez sur **"Réactiver"** à côté du compte
3. Confirmez
4. L'utilisateur peut à nouveau se connecter

#### Statistiques Utilisateurs

**Dashboard en Haut de Page** :
- **Utilisateurs Totaux** : Nombre total de comptes
- **Nouveaux (24h)** : Inscriptions des dernières 24h
- **Actifs (7j)** : Utilisateurs connectés dans les 7 derniers jours
- **Admins** : Nombre d'administrateurs
- **Modérateurs** : Nombre de modérateurs

---

### 2. Modération des Photos

#### Accéder à la Queue de Modération

- Onglet **"Modération des Photos"** dans le panel admin

#### Liste des Photos en Attente

**Affichage** :
- Grille de photos non approuvées
- Informations par photo :
  - Utilisateur qui a uploadé
  - Date d'upload
  - Dimensions et poids
- Prévisualisation grand format au clic

#### Approuver une Photo

1. Examinez la photo (contenu approprié ?)
2. Cliquez sur **"✅ Approuver"**
3. La photo devient immédiatement visible pour tous
4. L'utilisateur reçoit une notification

**Critères d'Approbation** :
- ✅ Visage clairement visible
- ✅ Pas de nudité explicite
- ✅ Pas de contenu violent ou offensant
- ✅ Pas de texte promotionnel
- ✅ Qualité suffisante (pas trop floue)

#### Rejeter une Photo

1. Examinez la photo (contenu inapproprié ?)
2. Cliquez sur **"❌ Rejeter"**
3. **Obligatoire** : Sélectionnez une raison :
   - Photo de mauvaise qualité
   - Visage non visible
   - Contenu inapproprié / nudité
   - Spam / promotion
   - Photo de groupe (impossible d'identifier la personne)
   - Autre (spécifier)
4. Confirmez
5. La photo est supprimée
6. L'utilisateur reçoit une notification avec la raison

#### Priorités de Modération

Photos triées par :
1. **Priorité haute** : Photos de profil principal (utilisateur n'a aucune photo approuvée)
2. **Priorité normale** : Photos supplémentaires
3. **Ordre** : Date d'upload (plus anciennes en premier)

#### Statistiques de Modération

- **En attente** : Nombre de photos à modérer
- **Approuvées (24h)** : Photos approuvées aujourd'hui
- **Rejetées (24h)** : Photos rejetées aujourd'hui
- **Temps moyen de modération** : Délai entre upload et décision

---

## 💡 Conseils et Bonnes Pratiques

### Pour un Profil Attractif

#### 1. Photo de Profil

✅ **À Faire** :
- Photo récente (moins de 6 mois)
- Visage clairement visible
- Sourire naturel
- Bonne luminosité
- Fond neutre ou agréable

❌ **À Éviter** :
- Selfie dans un miroir de salle de bain
- Photo de groupe (on ne sait pas qui vous êtes)
- Photo trop retouchée (filtres excessifs)
- Photo floue ou de mauvaise qualité
- Lunettes de soleil cachant le visage

#### 2. Description

✅ **À Faire** :
- Parlez de vos passions réelles
- Soyez authentique et positif
- Incluez des détails concrets (voyages, hobbies, musique)
- Posez une question pour encourager les messages
- Longueur idéale : 100-300 caractères

❌ **À Éviter** :
- Liste de ce que vous NE voulez pas
- Négatif ou cynique
- Trop vague ("J'aime m'amuser")
- Trop long (pavé de texte)
- Fautes d'orthographe répétées

**Exemple de Bonne Description** :
> "Passionné de cuisine italienne et de randonnée 🏔️. Développeur le jour, chef amateur le soir ! J'adore découvrir de nouveaux restaurants et voyager (prochain stop : Japon 🇯🇵). Toujours partant pour un bon film ou une soirée jeux de société. Et toi, quel est ton plat réconfort ?"

### Pour Bien Communiquer

#### Premier Message

✅ **Bonnes Pratiques** :
- Personnalisez (mentionnez un élément de leur profil)
- Posez une question ouverte
- Soyez léger et amical
- Correct mais pas trop formel

❌ **À Éviter** :
- "Salut" uniquement
- Compliment trop appuyé sur le physique
- Message copié-collé générique
- Message trop long (roman)

**Exemples de Premiers Messages** :

✅ **Bon** :
> "Hello ! J'ai vu que tu adorais la randonnée, tu as des spots préférés dans la région ? Je cherche de nouveaux sentiers à explorer 😊"

❌ **Mauvais** :
> "Salut t trop belle"

#### Maintenir la Conversation

- **Posez des questions** : Montrez de l'intérêt
- **Partagez des anecdotes** : Rendez la conversation vivante
- **Soyez réactif** : Répondez dans un délai raisonnable
- **Passez à l'action** : Proposez un café après quelques échanges
- **Respectez le rythme** : Ne bombardez pas de messages

### Sécurité et Vie Privée

#### Protégez vos Informations

🔒 **À NE PAS partager immédiatement** :
- Nom de famille complet
- Adresse précise
- Numéro de téléphone
- Profils réseaux sociaux
- Lieu de travail exact

#### Premier Rendez-vous

✅ **Conseils de Sécurité** :
- Choisissez un lieu public
- Prévenez un ami de votre rendez-vous
- Prévoyez votre propre transport
- Gardez votre téléphone chargé
- Faites confiance à votre instinct

#### Signaler un Comportement Inapproprié

Si un utilisateur :
- Envoie du contenu explicite non sollicité
- Harcèle ou insulte
- Demande de l'argent
- Semble utiliser une fausse identité

**Action** :
1. Ne répondez pas
2. Prenez des captures d'écran
3. Contactez un administrateur via le chatbot : "Je veux signaler un utilisateur"
4. Bloquez l'utilisateur (fonctionnalité à venir)

---

## ❓ FAQ et Dépannage

### Questions Fréquentes

#### 1. "Je n'arrive pas à me connecter"

**Solutions** :
- Vérifiez votre email et mot de passe (sensible à la casse)
- Cliquez sur **"Mot de passe oublié ?"** pour réinitialiser
- Videz le cache de votre navigateur (Ctrl+Shift+Suppr)
- Essayez un autre navigateur
- Vérifiez que les cookies sont autorisés

#### 2. "Ma photo n'est pas visible"

**Raisons Possibles** :
- ⏳ **En attente de modération** : Patience, sous 24-48h généralement
- ❌ **Rejetée** : Vérifiez vos notifications pour la raison
- 🐛 **Erreur d'upload** : Réessayez (format/taille OK ?)

**Solution** :
- Attendez l'approbation
- Uploadez une photo conforme si rejetée
- Contactez le support si problème persiste

#### 3. "Je ne reçois pas de messages"

**Vérifications** :
- Votre profil est-il complet ? (photo approuvée + description)
- Êtes-vous actif ? (dernière connexion récente)
- Avez-vous initié des conversations ?

**Astuce** :
- Likez des profils qui vous intéressent
- Envoyez des premiers messages personnalisés
- Améliorez votre profil (description, photos)

#### 4. "Le chatbot ne répond pas"

**Causes Possibles** :
- Le serveur Ollama est hors ligne (en développement)
- Votre message est trop long (> 500 caractères)
- Charge serveur élevée

**Solution** :
- Reformulez votre question plus simplement
- Attendez 10-15 secondes
- Rafraîchissez la page
- Contactez un admin si problème persiste

#### 5. "Mon âge est incorrect"

**Explication** :
- L'âge est calculé automatiquement depuis votre date de naissance
- La date de naissance n'est pas modifiable après inscription (politique anti-fraude)

**Solution** :
- Si vraiment erroné, contactez un administrateur avec preuve d'identité

#### 6. "Comment supprimer mon compte ?"

**Procédure** :
1. Contactez un administrateur via le chatbot : "Je veux supprimer mon compte"
2. Confirmez votre identité (email)
3. Votre compte sera supprimé sous 7 jours (délai légal)
4. Toutes vos données seront effacées (RGPD compliant)

**Alternatives** :
- **Désactiver temporairement** : Demandez une suspension de votre propre compte

### Problèmes Techniques

#### "La page ne charge pas"

**Checklist** :
- Vérifiez votre connexion internet
- Rafraîchissez la page (F5 ou Ctrl+F5)
- Videz le cache navigateur
- Testez sur un autre navigateur
- Vérifiez si le site est en maintenance (page d'accueil)

#### "Les messages ne s'envoient pas"

**Solutions** :
- Vérifiez votre connexion internet
- Reconnectez-vous (déconnexion puis connexion)
- Vérifiez que SignalR est connecté (indicateur dans la console développeur)
- Essayez de recharger la page

#### "Les notifications ne fonctionnent pas"

**Solutions** :
- Vérifiez les autorisations du navigateur :
  - Chrome : `chrome://settings/content/notifications`
  - Firefox : `about:preferences#privacy` > Permissions > Notifications
- Autorisez les notifications pour le site
- Rechargez la page après avoir autorisé

#### "L'application est lente"

**Optimisations** :
- Fermez les autres onglets du navigateur
- Videz le cache (Ctrl+Shift+Suppr)
- Vérifiez votre connexion internet (test de débit)
- Mettez à jour votre navigateur vers la dernière version
- Désactivez les extensions navigateur (bloqueurs de pub, etc.)

### Support

#### Obtenir de l'Aide

**Option 1 : Chatbot**
- Posez votre question au chatbot (réponses instantanées)
- Commandes spéciales :
  - "Contacter un admin"
  - "Signaler un bug"
  - "Demander une fonctionnalité"

**Option 2 : Email**
- support@crushapp.com
- Réponse sous 24-48h ouvrées

**Option 3 : FAQ en Ligne**
- Documentation complète : [thoumi.github.io/CrushApp2025](https://thoumi.github.io/CrushApp2025)

---

## 🎯 Pour Aller Plus Loin

### Ressources Complémentaires

- [Fonctionnalités Détaillées](01-FONCTIONNALITES.md) : Documentation technique
- [Migration Angular](03-MIGRATION-ANGULAR.md) : Aspects techniques pour développeurs
- [Architecture Microservices](../architecture/00-OVERVIEW.md) : Architecture du système

### Contribuer au Projet

CrushApp est un projet open-source !

- **Repository GitHub** : [github.com/thoumi/CrushApp2025](https://github.com/thoumi/CrushApp2025)
- **Signaler un bug** : Ouvrez une issue sur GitHub
- **Proposer une fonctionnalité** : Discussions GitHub
- **Contribuer au code** : Pull Requests bienvenues !

---

**Bon matching ! 💜**

---

**Version** : 1.0.0  
**Dernière mise à jour** : Octobre 2025


