# Renommer le Repository pour l'URL Personnalisée

## Objectif

Avoir l'URL : `https://thoumi-Lhoussaine.github.io/crushApp/`

## Étapes à Suivre

### Méthode 1 : Renommer le Repository Actuel

1. **Va sur GitHub** : https://github.com/thoumi/datingapp-2025-main
2. **Clique sur "Settings"** (Paramètres)
3. Dans **"Repository name"**, change :
   - De : `datingapp-2025-main`
   - Vers : `crushApp`
4. **Clique sur "Rename"**

### Méthode 2 : Créer un Nouveau Repository

Si tu veux créer un nouveau repository propre :

1. **Sur GitHub**, crée un nouveau repository nommé **`crushApp`**
2. **Sur ton PC**, change le remote :

```powershell
cd d:\datingapp-2025-main\datingapp-2025-main

# Supprimer l'ancien remote
git remote remove origin

# Ajouter le nouveau remote
git remote add origin https://github.com/thoumi/crushApp.git

# Pousser vers le nouveau repo
git push -u origin main
```

### Après le Renommage

Une fois le repository renommé, redéployer :

```powershell
cd d:\datingapp-2025-main\datingapp-2025-main

# Commiter la nouvelle config
git add mkdocs.yml
git commit -m "config: update site URL to crushApp"
git push origin main

# Redéployer sur GitHub Pages
mkdocs gh-deploy --force
```

### Résultat Final

✅ URL : `https://thoumi-Lhoussaine.github.io/crushApp/`

---

## Note Importante sur le Username GitHub

Tu as mentionné `thoumi-Lhoussaine` mais ton username GitHub semble être `thoumi`.

**Vérifie ton username exact** :
1. Va sur https://github.com
2. Clique sur ton profil en haut à droite
3. Ton username est affiché

**Si ton username est `thoumi`** :
- L'URL sera : `https://thoumi.github.io/crushApp/`

**Si ton username est `thoumi-Lhoussaine`** :
- L'URL sera : `https://thoumi-Lhoussaine.github.io/crushApp/`

---

## Vérification

Après déploiement, attendre 2-3 minutes puis aller sur :
- `https://TON-USERNAME.github.io/crushApp/`

