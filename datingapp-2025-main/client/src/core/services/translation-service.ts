import { Injectable, signal } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

interface Translation {
  [key: string]: string;
}

interface Translations {
  [language: string]: Translation;
}

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  private currentLanguage = signal<string>('fr');
  
  // Traductions par défaut
  private translations: Translations = {
    fr: {
      // Navigation
      'nav.home': 'Accueil',
      'nav.daily': 'Rendez-vous du jour',
      'nav.members': 'Explorer',
      'nav.lists': 'Ma Liste',
      'nav.messages': 'Messages',
      'nav.admin': 'Administration',
      'nav.logout': 'Déconnexion',
      'nav.login': 'Connexion',
      'nav.register': 'Inscription',

      // Accueil
      'home.title': 'Moins de profils. De vraies raisons d\'écrire.',
      'home.subtitle': 'Hallo vous propose une sélection restreinte chaque jour, avec de vraies amorces de conversation, pas juste une photo.',
      'home.register': 'Commencer',
      'home.login': 'Se connecter',
      'home.learnMore': 'Comment ça marche',

      // Rendez-vous du jour
      'daily.title': 'Votre rendez-vous du jour',
      'daily.subtitle': 'Une sélection restreinte, renouvelée chaque jour : regardez-la vraiment plutôt que de défiler.',
      'daily.empty': 'Plus personne à découvrir aujourd\'hui. Revenez demain, ou essayez Explorer.',

      // Coach (chatbot)
      'chatbot.title': 'Coach Hallo',
      'chatbot.subtitle': 'Des conseils de conversation, à la demande',
      'chatbot.placeholder': 'Posez votre question...',
      'chatbot.send': 'Envoyer',
      'chatbot.thinking': 'Réflexion...',
      'chatbot.error': 'Erreur lors de la génération de la réponse',

      // Authentification
      'auth.email': 'Email',
      'auth.password': 'Mot de passe',
      'auth.displayName': 'Nom d\'affichage',
      'auth.gender': 'Genre',
      'auth.dateOfBirth': 'Date de naissance',

      // Administration
      'admin.title': 'Administration',
      'admin.userManagement': 'Gestion des utilisateurs',
      'admin.photoModeration': 'Modération des photos',
      'admin.users': 'Utilisateurs',
      'admin.role': 'Rôle',
      'admin.roles': 'Rôles',
      'admin.status': 'Statut',
      'admin.active': 'Actif',
      'admin.blocked': 'Bloqué',
      'admin.allRoles': 'Tous les rôles',
      'admin.allStatuses': 'Tous les statuts',
      'admin.selectFilters': 'Sélectionner les filtres',
      'admin.resetFilters': 'Réinitialiser les filtres',
      'admin.filter': 'Filtrer',
      'admin.applyFilters': 'Appliquer les filtres',
      'admin.cancel': 'Annuler',
      'admin.searchUsers': 'Rechercher par email ou nom d\'utilisateur',
      'admin.editRoles': 'Modifier les rôles',
      'admin.blockUser': 'Bloquer l\'utilisateur',
      'admin.unblockUser': 'Débloquer l\'utilisateur',

      // Membres
      'members.title': 'Membres',
      'members.search': 'Rechercher des membres',
      'members.age': 'Âge',
      'members.location': 'Localisation',
      'members.lastActive': 'Dernière activité',
      'members.viewProfile': 'Voir le profil',
      'members.like': 'J\'aime',
      'members.unlike': 'Je n\'aime plus',
      'members.selectFilters': 'Sélectionner les filtres',
      'members.resetFilters': 'Réinitialiser les filtres',
      'members.notFound': 'Membre non trouvé',

      // Messages
      'messages.title': 'Messages',
      'messages.send': 'Envoyer',
      'messages.typeMessage': 'Tapez votre message...',
      'messages.noMessages': 'Aucun message',
      'messages.online': 'En ligne',
      'messages.offline': 'Hors ligne',
      'messages.senderRecipient': 'Expéditeur / Destinataire',
      'messages.message': 'Message',
      'messages.date': 'Date',
      'messages.noResults': 'Il n\'y a aucun résultat pour ce filtre',
      'messages.inbox': 'Boîte de réception',
      'messages.outbox': 'Boîte d\'envoi',
      'messages.seen': 'Vu',
      'messages.notRead': 'Non lu',
      'messages.delivered': 'Livré',
      'messages.enterMessage': 'Tapez votre message',

      // Listes
      'lists.title': 'Ma Liste',
      'lists.likes': 'J\'aime',
      'lists.likedBy': 'Qui m\'aime',
      'lists.mutual': 'Mutuel',
      'lists.noResults': 'Il n\'y a aucun résultat pour ce filtre',

      // Profil
      'profile.title': 'Profil',
      'profile.edit': 'Modifier',
      'profile.save': 'Enregistrer',
      'profile.cancel': 'Annuler',
      'profile.addPhoto': 'Ajouter une photo',
      'profile.setMainPhoto': 'Définir comme photo principale',
      'profile.deletePhoto': 'Supprimer la photo',
      'profile.memberSince': 'Membre depuis',
      'profile.lastActive': 'Dernière activité',
      'profile.about': 'À propos de',
      'profile.displayName': 'Nom d\'affichage',
      'profile.description': 'Description',
      'profile.city': 'Ville',
      'profile.country': 'Pays',
      'profile.submit': 'Soumettre',
      'profile.profile': 'Profil',
      'profile.photos': 'Photos',
      'profile.messages': 'Messages',

      // Photos
      'photo.approve': 'Approuver',
      'photo.reject': 'Rejeter',
      'photo.management': 'Gestion des photos',


      // Images
      'image.cancel': 'Annuler',

      // Dialogues
      'dialog.cancel': 'Annuler',
      'dialog.confirm': 'Confirmer',

      // Filtres
      'filter.title': 'Sélectionner les filtres',
      'filter.selectGender': 'Sélectionner le genre',
      'filter.selectAge': 'Sélectionner l\'âge',
      'filter.orderBy': 'Trier par',
      'filter.male': 'Homme',
      'filter.female': 'Femme',
      'filter.newestMembers': 'Nouveaux membres',
      'filter.lastActive': 'Dernière activité',
      'filter.submit': 'Soumettre',
      'filter.close': 'Fermer',

      // Pagination
      'pagination.itemsPerPage': 'Éléments par page',
      'pagination.of': 'sur',

      // Inscription
      'register.title': 'S\'inscrire',
      'register.credentials': 'Identifiants',
      'register.profile': 'Profil',
      'register.confirmPassword': 'Confirmer le mot de passe',
      'register.cancel': 'Annuler',
      'register.next': 'Suivant',
      'register.male': 'Homme',
      'register.female': 'Femme',
      'register.back': 'Retour',
      'register.register': 'S\'inscrire',
      'register.prompts': 'Prompts',
      'register.promptsIntro': 'Choisissez jusqu\'à 3 amorces : elles apparaîtront sur votre profil et donnent aux autres une vraie raison de vous écrire.',
      'register.choosePrompt': 'Choisir une amorce',
      'register.yourAnswer': 'Votre réponse',
      'register.skip': 'Passer',
      'register.finish': 'Terminer',

      // Commun
      'common.back': 'Retour',
      'common.edit': 'Modifier',
      'common.cancel': 'Annuler',
      'common.submit': 'Soumettre',
      'common.close': 'Fermer',
      'common.loading': 'Chargement...',
      'common.save': 'Enregistrer',
      'common.delete': 'Supprimer',
      'common.confirm': 'Confirmer',
      'common.yes': 'Oui',
      'common.no': 'Non',
      'common.ok': 'OK',
      'common.page': 'Page',
      'common.of': 'sur',
      'common.total': 'Total',
      'common.items': 'éléments',
      'common.selectRoles': 'Sélectionner les rôles',
      'common.editRolesFor': 'Modifier les rôles pour',
      'common.loadingUsers': 'Chargement des utilisateurs...',
      'common.filterUsers': 'Filtrer les utilisateurs',
      'common.description': 'Description',
      'common.city': 'Ville',
      'common.country': 'Pays'
    },
    en: {
      // Navigation
      'nav.home': 'Home',
      'nav.daily': 'Today\'s Picks',
      'nav.members': 'Explore',
      'nav.lists': 'Lists',
      'nav.messages': 'Messages',
      'nav.admin': 'Administration',
      'nav.logout': 'Logout',
      'nav.login': 'Login',
      'nav.register': 'Register',

      // Home
      'home.title': 'Fewer profiles. Better reasons to talk.',
      'home.subtitle': 'Hallo shows you a small, curated selection each day, with real conversation starters, not just a photo.',
      'home.register': 'Get started',
      'home.login': 'Login',
      'home.learnMore': 'How it works',

      // Today's picks
      'daily.title': 'Your picks for today',
      'daily.subtitle': 'A small selection, refreshed daily: worth actually looking at instead of scrolling past.',
      'daily.empty': 'No one new to discover today. Check back tomorrow, or try Explore.',

      // Coach (chatbot)
      'chatbot.title': 'Hallo Coach',
      'chatbot.subtitle': 'Conversation tips, on demand',
      'chatbot.placeholder': 'Ask your question...',
      'chatbot.send': 'Send',
      'chatbot.thinking': 'Thinking...',
      'chatbot.error': 'Error generating response',

      // Authentication
      'auth.email': 'Email',
      'auth.password': 'Password',
      'auth.displayName': 'Display Name',
      'auth.gender': 'Gender',
      'auth.dateOfBirth': 'Date of Birth',

      // Administration
      'admin.title': 'Administration',
      'admin.userManagement': 'User Management',
      'admin.photoModeration': 'Photo Moderation',
      'admin.users': 'Users',
      'admin.role': 'Role',
      'admin.roles': 'Roles',
      'admin.status': 'Status',
      'admin.active': 'Active',
      'admin.blocked': 'Blocked',
      'admin.allRoles': 'All Roles',
      'admin.allStatuses': 'All Statuses',
      'admin.selectFilters': 'Select Filters',
      'admin.resetFilters': 'Reset Filters',
      'admin.filter': 'Filter',
      'admin.applyFilters': 'Apply Filters',
      'admin.cancel': 'Cancel',
      'admin.searchUsers': 'Search by email or username',
      'admin.editRoles': 'Edit Roles',
      'admin.blockUser': 'Block User',
      'admin.unblockUser': 'Unblock User',

      // Members
      'members.title': 'Members',
      'members.search': 'Search members',
      'members.age': 'Age',
      'members.location': 'Location',
      'members.lastActive': 'Last Active',
      'members.viewProfile': 'View Profile',
      'members.like': 'Like',
      'members.unlike': 'Unlike',
      'members.selectFilters': 'Select Filters',
      'members.resetFilters': 'Reset Filters',
      'members.notFound': 'Member not found',

      // Messages
      'messages.title': 'Messages',
      'messages.send': 'Send',
      'messages.typeMessage': 'Type your message...',
      'messages.noMessages': 'No messages',
      'messages.online': 'Online',
      'messages.offline': 'Offline',
      'messages.senderRecipient': 'Sender / Recipient',
      'messages.message': 'Message',
      'messages.date': 'Date',
      'messages.noResults': 'There are no results for this filter',
      'messages.inbox': 'Inbox',
      'messages.outbox': 'Outbox',
      'messages.seen': 'Seen',
      'messages.notRead': 'Not read',
      'messages.delivered': 'Delivered',
      'messages.enterMessage': 'Enter your message',

      // Lists
      'lists.title': 'Lists',
      'lists.likes': 'Likes',
      'lists.likedBy': 'Liked By',
      'lists.mutual': 'Mutual',
      'lists.noResults': 'There are no results for this filter',

      // Profile
      'profile.title': 'Profile',
      'profile.edit': 'Edit',
      'profile.save': 'Save',
      'profile.cancel': 'Cancel',
      'profile.addPhoto': 'Add Photo',
      'profile.setMainPhoto': 'Set as Main Photo',
      'profile.deletePhoto': 'Delete Photo',
      'profile.memberSince': 'Member since',
      'profile.lastActive': 'Last active',
      'profile.about': 'About',
      'profile.displayName': 'Display Name',
      'profile.description': 'Description',
      'profile.city': 'City',
      'profile.country': 'Country',
      'profile.submit': 'Submit',
      'profile.profile': 'Profile',
      'profile.photos': 'Photos',
      'profile.messages': 'Messages',

      // Photos
      'photo.approve': 'Approve',
      'photo.reject': 'Reject',
      'photo.management': 'Photo Management',


      // Images
      'image.cancel': 'Cancel',

      // Dialogs
      'dialog.cancel': 'Cancel',
      'dialog.confirm': 'Confirm',

      // Filters
      'filter.title': 'Select filters',
      'filter.selectGender': 'Select gender',
      'filter.selectAge': 'Select age',
      'filter.orderBy': 'Order by',
      'filter.male': 'Male',
      'filter.female': 'Female',
      'filter.newestMembers': 'Newest members',
      'filter.lastActive': 'Last active',
      'filter.submit': 'Submit',
      'filter.close': 'Close',

      // Pagination
      'pagination.itemsPerPage': 'Items per page',
      'pagination.of': 'of',

      // Register
      'register.title': 'Sign up',
      'register.credentials': 'Credentials',
      'register.profile': 'Profile',
      'register.confirmPassword': 'Confirm Password',
      'register.cancel': 'Cancel',
      'register.next': 'Next',
      'register.male': 'Male',
      'register.female': 'Female',
      'register.back': 'Back',
      'register.register': 'Register',
      'register.prompts': 'Prompts',
      'register.promptsIntro': 'Pick up to 3 prompts: they\'ll show on your profile and give people a real reason to write to you.',
      'register.choosePrompt': 'Choose a prompt',
      'register.yourAnswer': 'Your answer',
      'register.skip': 'Skip',
      'register.finish': 'Finish',

      // Common
      'common.back': 'Back',
      'common.edit': 'Edit',
      'common.cancel': 'Cancel',
      'common.submit': 'Submit',
      'common.close': 'Close',
      'common.loading': 'Loading...',
      'common.save': 'Save',
      'common.delete': 'Delete',
      'common.confirm': 'Confirm',
      'common.yes': 'Yes',
      'common.no': 'No',
      'common.ok': 'OK',
      'common.page': 'Page',
      'common.of': 'of',
      'common.total': 'Total',
      'common.items': 'items',
      'common.selectRoles': 'Select roles',
      'common.editRolesFor': 'Edit roles for',
      'common.loadingUsers': 'Loading users...',
      'common.filterUsers': 'Filter users',
      'common.description': 'Description',
      'common.city': 'City',
      'common.country': 'Country'
    }
  };

  // Observable pour les changements de langue
  private languageSubject = new BehaviorSubject<string>('fr');
  public language$ = this.languageSubject.asObservable();

  constructor() {
    // Charger la langue sauvegardée ou utiliser le français par défaut
    const savedLanguage = localStorage.getItem('preferred-language') || 'fr';
    this.setLanguage(savedLanguage);
  }

  /**
   * Obtient la traduction pour une clé donnée
   */
  translate(key: string, params?: { [key: string]: string | number }): string {
    const currentLang = this.currentLanguage();
    const translation = this.translations[currentLang]?.[key] || key;

    if (params) {
      return this.interpolate(translation, params);
    }

    return translation;
  }

  /**
   * Interpole les paramètres dans la traduction
   */
  private interpolate(text: string, params: { [key: string]: string | number }): string {
    return text.replace(/\{\{(\w+)\}\}/g, (match, key) => {
      return params[key]?.toString() || match;
    });
  }

  /**
   * Change la langue actuelle
   */
  setLanguage(language: string): void {
    this.currentLanguage.set(language);
    this.languageSubject.next(language);
    localStorage.setItem('preferred-language', language);
  }

  /**
   * Obtient la langue actuelle
   */
  getCurrentLanguage(): string {
    return this.currentLanguage();
  }

  /**
   * Obtient la langue actuelle comme signal
   */
  getCurrentLanguageSignal() {
    return this.currentLanguage;
  }

  /**
   * Obtient les langues disponibles
   */
  getAvailableLanguages(): string[] {
    return Object.keys(this.translations);
  }

  /**
   * Obtient le nom d'affichage d'une langue
   */
  getLanguageName(language: string): string {
    const languageNames: { [key: string]: string } = {
      'fr': 'Français',
      'en': 'English'
    };
    return languageNames[language] || language;
  }
}