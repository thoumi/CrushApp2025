# Système de Navigation Flexible pour MkDocs

## Vue d'Ensemble

Ce système permet de gérer facilement la navigation de la documentation MkDocs de manière modulaire et flexible. Il utilise des fichiers YAML de configuration et des scripts automatisés pour simplifier la maintenance.

## Structure des Fichiers

```
📁 Navigation System
├── 📄 navigation.yml              # Configuration principale
├── 📄 navigation-advanced.yml     # Configuration avancée
├── 📄 navigation-shortcuts.yml    # Raccourcis et personnalisation
├── 📄 generate_navigation.py      # Script Python de génération
├── 📄 nav-manager.ps1            # Script PowerShell simple
├── 📄 manage-navigation.ps1      # Script PowerShell avancé
└── 📄 NAVIGATION-README.md       # Ce fichier
```

## Utilisation Rapide

### 1. Générer la Navigation

```bash
# Méthode 1: Script Python direct
python generate_navigation.py

# Méthode 2: Script PowerShell
.\nav-manager.ps1 generate
```

### 2. Afficher la Navigation Actuelle

```bash
.\nav-manager.ps1 show
```

### 3. Aide

```bash
.\nav-manager.ps1 help
```

## Configuration de la Navigation

### Fichier Principal: `navigation.yml`

```yaml
# Structure de base
base_structure:
  - Accueil: index-simple.md

# Sections principales
sections:
  user_guide:
    title: "Guide Utilisateur"
    items:
      - Fonctionnalités: guide/01-FONCTIONNALITES-SIMPLE.md
      - Guide Utilisateur: guide/02-GUIDE-UTILISATEUR.md
  
  technical_guide:
    title: "Guide Technique"
    items:
      - Migration Angular: guide/03-MIGRATION-ANGULAR.md
      - Migration .NET: guide/04-MIGRATION-DOTNET.md
      - Migration Microservices: guide/05-MIGRATION-MICROSERVICES.md
      - Architecture: architecture/00-OVERVIEW.md

# Ordre des sections
section_order:
  - user_guide
  - technical_guide
  - deployment
  - troubleshooting
```

### Ajouter une Nouvelle Section

1. **Modifier `navigation.yml`** :
```yaml
sections:
  nouvelle_section:
    title: "Nouvelle Section"
    items:
      - Page 1: path/to/page1.md
      - Page 2: path/to/page2.md

section_order:
  - user_guide
  - nouvelle_section  # Ajouter ici
  - technical_guide
```

2. **Régénérer la navigation** :
```bash
python generate_navigation.py
```

### Ajouter une Nouvelle Page

1. **Modifier `navigation.yml`** :
```yaml
sections:
  technical_guide:
    title: "Guide Technique"
    items:
      - Migration Angular: guide/03-MIGRATION-ANGULAR.md
      - Nouvelle Page: guide/06-NOUVELLE-PAGE.md  # Ajouter ici
      - Migration .NET: guide/04-MIGRATION-DOTNET.md
```

2. **Régénérer la navigation** :
```bash
python generate_navigation.py
```

## Configuration Avancée

### Fichier: `navigation-advanced.yml`

Ce fichier permet des configurations plus avancées :

- **Icônes** pour les sections et pages
- **Descriptions** détaillées
- **Groupes** de sections
- **Raccourcis** rapides
- **Tags** pour le filtrage
- **Métadonnées** complètes

### Exemple d'Utilisation Avancée

```yaml
sections:
  technical_guide:
    title: "Guide Technique"
    icon: "material/cog"
    description: "Guides techniques et migrations"
    order: 2
    items:
      - title: "Migration Angular"
        path: "guide/03-MIGRATION-ANGULAR.md"
        description: "Migration Angular 12 → 20"
        icon: "material/update"
```

## Raccourcis et Personnalisation

### Fichier: `navigation-shortcuts.yml`

Ce fichier permet de définir :

- **Raccourcis rapides** vers les pages importantes
- **Sections personnalisées** par type d'utilisateur
- **Badges** de statut et version
- **Métadonnées** du projet
- **Configuration des thèmes**

### Exemple de Raccourcis

```yaml
quick_links:
  - title: "🏠 Accueil"
    path: "index-simple.md"
    icon: "material/home"
    description: "Page d'accueil de la documentation"
  
  - title: "🚀 Démarrage Rapide"
    path: "index-simple.md#démarrage-rapide"
    icon: "material/play"
    description: "Guide de démarrage rapide"
```

## Workflow de Maintenance

### 1. Modification de la Navigation

```bash
# 1. Modifier navigation.yml
# 2. Régénérer la navigation
python generate_navigation.py

# 3. Vérifier le résultat
.\nav-manager.ps1 show

# 4. Déployer si satisfait
mkdocs gh-deploy --config-file mkdocs-simple.yml --force
```

### 2. Ajout d'une Nouvelle Page

```bash
# 1. Créer le fichier Markdown
# 2. Ajouter à navigation.yml
# 3. Régénérer la navigation
python generate_navigation.py

# 4. Déployer
mkdocs gh-deploy --config-file mkdocs-simple.yml --force
```

### 3. Réorganisation des Sections

```bash
# 1. Modifier section_order dans navigation.yml
# 2. Régénérer la navigation
python generate_navigation.py

# 3. Déployer
mkdocs gh-deploy --config-file mkdocs-simple.yml --force
```

## Avantages du Système

### ✅ **Flexibilité**
- Configuration modulaire en YAML
- Réorganisation facile des sections
- Ajout/suppression de pages simple

### ✅ **Maintenabilité**
- Séparation de la configuration et du code
- Scripts automatisés pour la génération
- Validation automatique de la structure

### ✅ **Évolutivité**
- Support des configurations avancées
- Extensibilité via des plugins
- Personnalisation par type d'utilisateur

### ✅ **Simplicité**
- Interface simple avec scripts PowerShell
- Documentation claire et exemples
- Workflow de maintenance standardisé

## Dépannage

### Problème: Script Python ne fonctionne pas

```bash
# Vérifier que Python est installé
python --version

# Vérifier que PyYAML est installé
pip install PyYAML
```

### Problème: Script PowerShell ne fonctionne pas

```bash
# Vérifier la politique d'exécution
Get-ExecutionPolicy

# Autoriser l'exécution si nécessaire
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Problème: Navigation non mise à jour

```bash
# Vérifier que le fichier navigation.yml est valide
python -c "import yaml; yaml.safe_load(open('navigation.yml'))"

# Régénérer la navigation
python generate_navigation.py

# Vérifier le résultat
.\nav-manager.ps1 show
```

## Exemples d'Utilisation

### Scénario 1: Ajouter une Section "API"

1. **Modifier `navigation.yml`** :
```yaml
sections:
  api_docs:
    title: "Documentation API"
    items:
      - Endpoints: api/endpoints.md
      - Authentification: api/auth.md
      - Exemples: api/examples.md

section_order:
  - user_guide
  - technical_guide
  - api_docs  # Nouvelle section
  - deployment
```

2. **Régénérer** :
```bash
python generate_navigation.py
```

### Scénario 2: Réorganiser les Sections

1. **Modifier `section_order`** :
```yaml
section_order:
  - technical_guide  # Mettre en premier
  - user_guide
  - deployment
  - troubleshooting
```

2. **Régénérer** :
```bash
python generate_navigation.py
```

### Scénario 3: Ajouter des Icônes

1. **Utiliser `navigation-advanced.yml`** :
```yaml
sections:
  technical_guide:
    title: "Guide Technique"
    icon: "material/cog"
    items:
      - title: "Migration Angular"
        path: "guide/03-MIGRATION-ANGULAR.md"
        icon: "material/update"
```

2. **Régénérer** :
```bash
python generate_navigation.py
```

---

## Conclusion

Ce système de navigation flexible permet de gérer efficacement la documentation MkDocs avec une approche modulaire et automatisée. Il facilite la maintenance, l'évolution et la personnalisation de la navigation selon les besoins du projet.

**Pour commencer** : Modifiez `navigation.yml` selon vos besoins, puis exécutez `python generate_navigation.py` pour appliquer les changements.
