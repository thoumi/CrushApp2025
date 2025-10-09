# Guide de Déploiement - Site Documentation en Ligne

## Déployer Gratuitement sur GitHub Pages

Ce guide explique comment mettre votre documentation en ligne gratuitement avec GitHub Pages.

**Résultat** : Site accessible sur `https://votre-username.github.io/crushapp-2025/`

---

## Méthode 1 : Déploiement en Une Commande (Recommandé)

### Prérequis

1. Compte GitHub (gratuit)
2. Repository GitHub créé (si pas déjà fait)
3. Git configuré localement

### Étapes

#### 1. Commiter tous les fichiers de documentation

```powershell
cd d:\datingapp-2025-main\datingapp-2025-main

# Ajouter tous les fichiers de documentation
git add docs/ mkdocs.yml requirements.txt .github/

# Commiter
git commit -m "docs: add complete microservices documentation with MkDocs"

# Pusher vers GitHub
git push origin main
```

#### 2. Déployer sur GitHub Pages

```powershell
# Une seule commande !
mkdocs gh-deploy
```

**C'est tout !** 🎉

Votre site sera accessible sur : `https://votre-username.github.io/datingapp-2025/`

(Remplacez `votre-username` par votre nom d'utilisateur GitHub)

---

## Méthode 2 : Déploiement Automatique avec GitHub Actions

Pour que le site se mette à jour automatiquement à chaque commit :

### Étape 1 : Vérifier le workflow

Le fichier `.github/workflows/docs.yml` est déjà créé et configuré.

### Étape 2 : Activer GitHub Pages

1. Aller sur votre repository GitHub
2. Cliquer sur **Settings** (Paramètres)
3. Dans le menu latéral, cliquer sur **Pages**
4. Sous **Source**, sélectionner :
   - Branch: `gh-pages`
   - Folder: `/ (root)`
5. Cliquer **Save**

### Étape 3 : Pusher vers GitHub

```powershell
cd d:\datingapp-2025-main\datingapp-2025-main

git add .
git commit -m "docs: setup automated deployment with GitHub Actions"
git push origin main
```

### Étape 4 : Attendre le déploiement

1. Aller dans l'onglet **Actions** de votre repository GitHub
2. Vous verrez le workflow "Deploy Documentation" en cours
3. Attendre 2-3 minutes
4. Le site sera accessible sur `https://votre-username.github.io/datingapp-2025/`

**Avantage** : Chaque fois que vous pushez vers `main`, le site se met à jour automatiquement !

---

## Méthode 3 : Autres Options Gratuites

### Netlify (Alternative Simple)

1. Créer compte sur https://netlify.com (gratuit)
2. Connecter votre repository GitHub
3. Configuration build :
   - **Build command** : `mkdocs build`
   - **Publish directory** : `site/`
4. Déployer

**URL** : `https://votre-site.netlify.app`

**Avantage** : URL personnalisable, SSL automatique, déploiement automatique

### Vercel (Alternative Moderne)

1. Créer compte sur https://vercel.com (gratuit)
2. Importer repository GitHub
3. Configuration :
   - **Framework Preset** : Other
   - **Build Command** : `pip install -r requirements.txt && mkdocs build`
   - **Output Directory** : `site/`
4. Déployer

**URL** : `https://votre-site.vercel.app`

### Azure Static Web Apps (Microsoft)

1. Installer Azure CLI : `winget install Microsoft.AzureCLI`
2. Login : `az login`
3. Créer Static Web App :

```powershell
az staticwebapp create `
    --name crushapp-docs `
    --resource-group myResourceGroup `
    --source https://github.com/votre-username/datingapp-2025 `
    --location "westeurope" `
    --branch main `
    --app-location "docs" `
    --output-location "site" `
    --login-with-github
```

**URL** : `https://crushapp-docs.azurestaticapps.net`

---

## Comparaison des Options

| Option | Gratuit | SSL | URL Personnalisable | Auto-Deploy | Facilité |
|--------|---------|-----|-------------------|-------------|----------|
| **GitHub Pages** | ✅ Oui | ✅ Oui | ⚠️ Limité | ✅ Oui | ⭐⭐⭐⭐⭐ |
| **Netlify** | ✅ Oui | ✅ Oui | ✅ Oui | ✅ Oui | ⭐⭐⭐⭐ |
| **Vercel** | ✅ Oui | ✅ Oui | ✅ Oui | ✅ Oui | ⭐⭐⭐⭐ |
| **Azure Static** | ✅ Oui | ✅ Oui | ✅ Oui | ✅ Oui | ⭐⭐⭐ |

---

## Commandes Utiles

### Tester avant de déployer

```powershell
# Build local pour vérifier qu'il n'y a pas d'erreurs
mkdocs build

# Vérifier le dossier site/ généré
ls site/

# Tester localement
mkdocs serve
```

### Mettre à jour le site

```powershell
# Après modifications de la doc
git add docs/
git commit -m "docs: update architecture guide"
git push origin main

# Si déploiement manuel
mkdocs gh-deploy
```

### Supprimer le déploiement

```powershell
# Supprimer la branche gh-pages
git push origin --delete gh-pages
```

---

## Personnaliser l'URL (Domaine Personnalisé)

### Pour GitHub Pages

1. Acheter un domaine (ex: namecheap.com, ~10€/an)
2. Dans votre repository GitHub :
   - Settings → Pages → Custom domain
   - Entrer : `docs.crushapp.com`
3. Chez votre registrar DNS, ajouter :
   ```
   CNAME docs votre-username.github.io
   ```
4. Attendre propagation DNS (5-30 minutes)

### Pour Netlify/Vercel

Interface graphique simple pour ajouter domaine personnalisé.

---

## Vérifier le Déploiement

### GitHub Pages

1. Aller sur : `https://votre-username.github.io/datingapp-2025/`
2. Vérifier que "CrushApp" apparaît dans le titre
3. Tester la recherche
4. Vérifier la navigation

### Problèmes Courants

**Erreur 404** :
- Attendre 5-10 minutes après premier déploiement
- Vérifier que branche `gh-pages` existe
- Vérifier Settings → Pages activé

**CSS ne charge pas** :
- Vérifier `site_url` dans `mkdocs.yml`
- Faire un hard refresh : `Ctrl+Shift+R`

**Build échoue** :
- Vérifier les logs dans Actions
- Vérifier que `requirements.txt` est à jour
- Tester `mkdocs build` localement

---

## Statistiques (Optionnel)

### Ajouter Google Analytics

Dans `mkdocs.yml` :

```yaml
extra:
  analytics:
    provider: google
    property: G-XXXXXXXXXX
```

Puis créer propriété sur https://analytics.google.com

### Voir les Visiteurs GitHub

GitHub fournit des stats basiques :
- Aller sur repository → Insights → Traffic

---

## Sécurité

### HTTPS

✅ Automatiquement activé sur :
- GitHub Pages
- Netlify
- Vercel
- Azure Static Web Apps

### Protection Branche gh-pages

1. Settings → Branches
2. Add rule : `gh-pages`
3. Cocher "Require pull request reviews"

---

## Prochaines Étapes

Après déploiement :

1. ✅ Partager l'URL sur LinkedIn/CV
2. ✅ Ajouter badge dans README :
   ```markdown
   [![Documentation](https://img.shields.io/badge/docs-live-blue)](https://votre-username.github.io/datingapp-2025/)
   ```
3. ✅ Configurer domaine personnalisé (optionnel)
4. ✅ Activer analytics (optionnel)

---

## Commandes de Déploiement Rapide

```powershell
# Setup initial (une fois)
cd d:\datingapp-2025-main\datingapp-2025-main
git add .
git commit -m "docs: add microservices documentation"
git push origin main

# Déployer sur GitHub Pages
mkdocs gh-deploy

# Ou laisser GitHub Actions le faire automatiquement
git push origin main
```

---

**Votre documentation sera en ligne en moins de 5 minutes !** 🚀

**URL finale** : `https://votre-username.github.io/datingapp-2025/`

---

**Date** : 2025-01-09  
**Auteur** : CrushApp Dev Team

