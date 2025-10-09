# Setup Documentation - Guide Complet
## 3 Options pour Exporter et Visualiser la Documentation

---

## 🎯 Vue d'ensemble

Voici **3 options** pour rendre la documentation accessible, classées par facilité et professionnalisme.

---

## ⭐ **Option 1 : MkDocs + GitHub Pages (RECOMMANDÉE)**

**Résultat** : Site web professionnel comme https://squidfunk.github.io/mkdocs-material/

### Avantages
- ✅ Site web statique moderne et rapide
- ✅ Recherche intégrée puissante
- ✅ Navigation automatique
- ✅ Thème Material Design professionnel
- ✅ Hébergement gratuit GitHub Pages
- ✅ Export PDF intégré
- ✅ SEO optimisé

### Installation (5 minutes)

#### Étape 1 : Installer Python & MkDocs

```powershell
# Vérifier Python (requis Python 3.8+)
python --version

# Installer si nécessaire
winget install Python.Python.3.12

# Installer MkDocs et dépendances
pip install -r requirements.txt

# OU installation manuelle
pip install mkdocs
pip install mkdocs-material
pip install mkdocs-minify-plugin
pip install mkdocs-git-revision-date-localized-plugin
```

#### Étape 2 : Servir Localement

```powershell
# Démarrer serveur local
mkdocs serve

# Ouvrir navigateur
start http://127.0.0.1:8000
```

**✨ Résultat** : Site documentation accessible sur http://localhost:8000

#### Étape 3 : Build Site Statique

```powershell
# Générer site statique dans /site
mkdocs build

# Ouvrir site généré
start site\index.html
```

**📁 Résultat** : Dossier `site/` contenant HTML, CSS, JS prêts à déployer

#### Étape 4 : Déployer sur GitHub Pages (Gratuit)

```powershell
# Option A : Déploiement automatique
mkdocs gh-deploy

# Option B : Workflow GitHub Actions (automatique sur push)
# (voir section CI/CD ci-dessous)
```

**🌐 Résultat** : Site accessible sur `https://yourusername.github.io/datingapp-2025/`

### Workflow GitHub Actions (Déploiement Auto)

Créer `.github/workflows/docs.yml` :

```yaml
name: Deploy Documentation

on:
  push:
    branches:
      - main
    paths:
      - 'docs/**'
      - 'mkdocs.yml'

permissions:
  contents: write

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Python
        uses: actions/setup-python@v5
        with:
          python-version: '3.12'
      
      - name: Install dependencies
        run: |
          pip install -r requirements.txt
      
      - name: Deploy to GitHub Pages
        run: |
          git config user.name github-actions
          git config user.email github-actions@github.com
          mkdocs gh-deploy --force
```

**🔄 Résultat** : Chaque push vers `main` déploie automatiquement la doc

---

## 📄 **Option 2 : Export PDF Professionnel**

**Résultat** : Fichier PDF unique avec toute la documentation

### Méthode A : Via MkDocs PDF Export

```powershell
# Installer plugin PDF
pip install mkdocs-pdf-export-plugin

# Build avec PDF
mkdocs build

# PDF généré dans site/pdf/datingapp-microservices-docs.pdf
start site\pdf\datingapp-microservices-docs.pdf
```

**📁 Résultat** : `datingapp-microservices-docs.pdf` (~50-80 pages)

### Méthode B : Pandoc (Qualité Supérieure)

```powershell
# Installer Pandoc
winget install pandoc

# Concaténer tous les MD
$files = @(
    "docs\MICROSERVICES-MIGRATION.md",
    "docs\architecture\00-OVERVIEW.md",
    "docs\architecture\01-ADR-INDEX.md",
    "docs\architecture\02-MIGRATION-GUIDE.md",
    "docs\architecture\03-DOCKER-COMPOSE-COMPLETE.md",
    "docs\architecture\04-MONITORING-OBSERVABILITY.md",
    "docs\architecture\05-CI-CD-PIPELINE.md"
)

# Générer PDF
pandoc $files `
    -o DatingApp-Microservices-Documentation.pdf `
    --toc `
    --toc-depth=3 `
    --metadata title="DatingApp Microservices - Documentation Technique" `
    --metadata author="DatingApp Dev Team" `
    --metadata date="2025-01-09" `
    --pdf-engine=xelatex `
    -V geometry:margin=1in `
    -V fontsize=11pt `
    -V documentclass=report `
    --highlight-style=tango

# Ouvrir PDF
start DatingApp-Microservices-Documentation.pdf
```

**📁 Résultat** : PDF professionnel avec table des matières, navigation, syntax highlighting

### Méthode C : Print to PDF depuis MkDocs

1. Démarrer serveur : `mkdocs serve`
2. Ouvrir navigateur : http://localhost:8000
3. `Ctrl+P` → "Enregistrer en PDF"
4. Répéter pour chaque page (ou use Print CSS media)

**📁 Résultat** : Multiple PDFs (un par page)

---

## 📁 **Option 3 : Dossier HTML Statique (Sans Build)**

**Résultat** : Dossier HTML simple, ouvrable dans navigateur

### Méthode A : Markdown to HTML Simple

```powershell
# Installer markdown
pip install markdown

# Convertir chaque fichier
Get-ChildItem docs\architecture\*.md | ForEach-Object {
    $htmlFile = $_.FullName.Replace('.md', '.html')
    python -m markdown $_.FullName > $htmlFile
}

# Créer index
New-Item -Path docs-html -ItemType Directory -Force
Copy-Item docs\architecture\*.html docs-html\
```

**📁 Résultat** : Dossier `docs-html/` avec fichiers HTML basiques

### Méthode B : MkDocs Build (Meilleure Option)

```powershell
# Build site
mkdocs build

# Copier dossier site vers autre location
Copy-Item -Path site -Destination ..\datingapp-docs-html -Recurse

# Ouvrir
start ..\datingapp-docs-html\index.html
```

**📁 Résultat** : Dossier complet avec site statique professionnel

### Partager le Dossier

**Option 1 : ZIP et Envoyer**
```powershell
Compress-Archive -Path site -DestinationPath datingapp-docs.zip
```

**Option 2 : Upload sur serveur/cloud**
- Google Drive / Dropbox
- Azure Static Web Apps (gratuit)
- Netlify (gratuit)
- Vercel (gratuit)

---

## 🌐 **Bonus : Autres Options Hébergement**

### Netlify (Gratuit, Simple)

1. Créer compte sur https://netlify.com
2. Connecter GitHub repo
3. Build command: `mkdocs build`
4. Publish directory: `site/`
5. Deploy automatique sur chaque push

**🌐 Résultat** : `https://datingapp-docs.netlify.app`

### Azure Static Web Apps (Gratuit)

```powershell
# Installer Azure CLI
winget install Microsoft.AzureCLI

# Login
az login

# Créer Static Web App
az staticwebapp create `
    --name datingapp-docs `
    --resource-group myResourceGroup `
    --source https://github.com/yourusername/datingapp-2025 `
    --location "westeurope" `
    --branch main `
    --app-location "docs" `
    --output-location "site"
```

**🌐 Résultat** : `https://datingapp-docs.azurestaticapps.net`

### ReadTheDocs (Gratuit, Spécialisé Docs)

1. Créer compte sur https://readthedocs.org
2. Importer GitHub repo
3. Configuration automatique avec `mkdocs.yml`
4. Build automatique sur push

**🌐 Résultat** : `https://datingapp-docs.readthedocs.io`

---

## 📊 Comparaison Options

| Option | Difficulté | Temps Setup | Hébergement | Export PDF | Recherche | Pro Look |
|--------|-----------|-------------|-------------|------------|-----------|----------|
| **MkDocs + GitHub Pages** | ⭐⭐ | 10 min | ✅ Gratuit | ✅ Oui | ✅ Oui | ⭐⭐⭐⭐⭐ |
| **PDF Pandoc** | ⭐⭐ | 5 min | N/A | ✅ Oui | ❌ Non | ⭐⭐⭐⭐ |
| **Dossier HTML** | ⭐ | 2 min | ⚠️ Manuel | ❌ Non | ❌ Non | ⭐⭐ |
| **Netlify** | ⭐ | 5 min | ✅ Gratuit | ✅ Oui | ✅ Oui | ⭐⭐⭐⭐⭐ |
| **Azure Static** | ⭐⭐⭐ | 15 min | ✅ Gratuit | ✅ Oui | ✅ Oui | ⭐⭐⭐⭐⭐ |
| **ReadTheDocs** | ⭐ | 5 min | ✅ Gratuit | ✅ Oui | ✅ Oui | ⭐⭐⭐⭐ |

---

## 🎯 Ma Recommandation

### Pour Apprendre & Portfolio

**1️⃣ MkDocs + GitHub Pages** (Option 1)
- Site professionnel pour CV/portfolio
- Gratuit et facile à maintenir
- Recherche + navigation automatique

**2️⃣ Netlify** (Bonus)
- Alternative si problème GitHub Pages
- Déploiement encore plus simple
- URL personnalisable

### Pour Partager/Présenter

**1️⃣ PDF Pandoc** (Option 2B)
- Document unique, facile à envoyer
- Professionnel pour entretiens
- Offline, portable

**2️⃣ MkDocs Build + ZIP** (Option 3B)
- Site complet dans un ZIP
- Ouvrable sans serveur
- Bonne alternative PDF

---

## 🚀 Quick Start - Option Recommandée

```powershell
# 1. Installer MkDocs
pip install -r requirements.txt

# 2. Tester localement
mkdocs serve
# Ouvrir http://localhost:8000

# 3. Générer PDF
mkdocs build
start site\pdf\datingapp-microservices-docs.pdf

# 4. Déployer sur GitHub Pages
mkdocs gh-deploy

# 5. Partager
# Site: https://yourusername.github.io/datingapp-2025/
# PDF: site/pdf/datingapp-microservices-docs.pdf
```

**⏱️ Temps total** : 10-15 minutes

---

## 🆘 Troubleshooting

### Problème : Python pas installé

```powershell
winget install Python.Python.3.12
# Redémarrer terminal
python --version
```

### Problème : pip pas trouvé

```powershell
python -m ensurepip --upgrade
python -m pip install --upgrade pip
```

### Problème : MkDocs build erreur

```powershell
# Vérifier fichiers manquants
mkdocs build --verbose

# Nettoyer cache
Remove-Item -Path site -Recurse -Force
mkdocs build --clean
```

### Problème : GitHub Pages 404

```powershell
# Vérifier branche gh-pages créée
git branch -r

# Re-deploy
mkdocs gh-deploy --force
```

---

## 📞 Support

**Questions** : Créer [GitHub Discussion](https://github.com/yourusername/datingapp-2025/discussions)  
**MkDocs Docs** : https://www.mkdocs.org/  
**Material Theme** : https://squidfunk.github.io/mkdocs-material/

---

**Bon courage pour le setup ! 🚀**

Si tu choisis **Option 1 (MkDocs)**, tu auras un site comme celui-ci : https://squidfunk.github.io/mkdocs-material/

Si tu préfères **Option 2 (PDF)**, tu auras un document unique de ~60-80 pages.

**Les deux sont excellents pour le CV/Portfolio !** 💼✨

