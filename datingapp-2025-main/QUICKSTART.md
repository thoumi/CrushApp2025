# 🚀 Quick Start - Documentation MkDocs

## ✅ Installation Terminée !

Félicitations ! Tu as maintenant :
- ✅ Python 3.12.10 installé
- ✅ MkDocs + Material Theme installés
- ✅ Serveur de documentation démarré

---

## 🌐 Accéder à la Documentation

**Ta documentation est accessible ici** :

👉 **http://127.0.0.1:8000** ou **http://localhost:8000**

Un navigateur devrait s'être ouvert automatiquement. Si ce n'est pas le cas :

```powershell
start http://localhost:8000
```

---

## 📖 Navigation

Sur le site, tu trouveras :

- **Page d'accueil** : Vue d'ensemble avec liens rapides
- **Barre latérale gauche** : Navigation entre les documents
- **Barre de recherche** : Recherche dans toute la documentation
- **Thème clair/sombre** : Bouton en haut à droite

---

## 🛠️ Commandes Utiles

### Démarrer le serveur (si arrêté)

```powershell
cd d:\datingapp-2025-main\datingapp-2025-main
mkdocs serve
```

Puis ouvrir : http://localhost:8000

### Arrêter le serveur

Appuyer sur `Ctrl+C` dans le terminal

### Générer le site statique (HTML)

```powershell
mkdocs build
```

Résultat dans le dossier `site/`

### Déployer sur GitHub Pages

```powershell
mkdocs gh-deploy
```

Site accessible sur : `https://ton-username.github.io/datingapp-2025/`

---

## 📁 Structure Documentation

```
docs/
├── index.md                                 # Page d'accueil
├── MICROSERVICES-MIGRATION.md              # Introduction
└── architecture/
    ├── README.md                            # Guide utilisation
    ├── 00-OVERVIEW.md                       # Vue d'ensemble
    ├── 01-ADR-INDEX.md                      # Décisions architecturales
    ├── 02-MIGRATION-GUIDE.md                # Guide migration
    ├── 03-DOCKER-COMPOSE-COMPLETE.md        # Docker
    ├── 04-MONITORING-OBSERVABILITY.md       # Monitoring
    └── 05-CI-CD-PIPELINE.md                 # CI/CD
```

---

## ✏️ Modifier la Documentation

1. **Éditer un fichier Markdown** dans `docs/`
2. **Sauvegarder** (Ctrl+S)
3. **Le site se recharge automatiquement** dans le navigateur

Exemple : Ouvre `docs/index.md` dans VS Code, modifie, sauvegarde → site mis à jour !

---

## 🎨 Personnalisation

### Changer le titre du site

Éditer `mkdocs.yml` ligne 2 :

```yaml
site_name: Mon Nouveau Titre
```

### Changer les couleurs

Éditer `mkdocs.yml` lignes 13-14 :

```yaml
theme:
  palette:
    primary: blue  # indigo, blue, red, green, etc.
    accent: amber
```

### Ajouter une page

1. Créer `docs/nouvelle-page.md`
2. Ajouter dans `mkdocs.yml` section `nav:` :

```yaml
nav:
  - Nouvelle Page: nouvelle-page.md
```

---

## 📤 Partager la Documentation

### Option 1 : GitHub Pages (Gratuit, En Ligne)

```powershell
# Une seule commande
mkdocs gh-deploy
```

Résultat : `https://ton-username.github.io/datingapp-2025/`

### Option 2 : Export ZIP (Offline)

```powershell
# Générer site statique
mkdocs build

# Créer ZIP
Compress-Archive -Path site -DestinationPath datingapp-docs.zip
```

Partager le ZIP, dézipper, ouvrir `index.html`

### Option 3 : PDF (Document Unique)

Installation supplémentaire nécessaire :

```powershell
pip install mkdocs-pdf-export-plugin
mkdocs build
```

PDF généré dans `site/pdf/`

---

## 🆘 Problèmes Courants

### Serveur ne démarre pas

```powershell
# Vérifier qu'aucun autre serveur n'utilise le port 8000
netstat -ano | findstr :8000

# Ou utiliser un autre port
mkdocs serve -a localhost:8001
```

### Modifications non visibles

1. Vérifier que le serveur tourne (`mkdocs serve`)
2. Rafraîchir le navigateur (`F5` ou `Ctrl+F5`)
3. Vider le cache navigateur

### Erreur "command not found: mkdocs"

```powershell
# Recharger PATH
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# Ou redémarrer terminal
```

---

## 📞 Support

- **Documentation MkDocs** : https://www.mkdocs.org/
- **Material Theme** : https://squidfunk.github.io/mkdocs-material/
- **Guide Setup Complet** : `SETUP-DOCUMENTATION.md`

---

## 🎯 Prochaines Étapes

1. ✅ **Explorer la doc** sur http://localhost:8000
2. 📖 **Lire** l'Introduction (MICROSERVICES-MIGRATION.md)
3. 🏗️ **Comprendre** l'architecture (00-OVERVIEW.md)
4. 🚀 **Commencer** la migration (02-MIGRATION-GUIDE.md)
5. 🌐 **Déployer** sur GitHub Pages (`mkdocs gh-deploy`)

---

**Bon courage pour ta migration microservices ! 🎉**

**Ta documentation est maintenant professionnelle et accessible !** 💼✨

