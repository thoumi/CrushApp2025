# 📸 Configuration Cloudinary - Guide Rapide

## Pourquoi Cloudinary ?
Cloudinary est un service cloud **gratuit** pour gérer vos images. L'application l'utilise pour stocker les photos de profil.

---

## 🚀 Étapes (5 minutes)

### 1️⃣ Créer un compte GRATUIT
👉 Allez sur : **https://cloudinary.com/users/register_free**

- Remplissez le formulaire d'inscription
- Confirmez votre email
- Connectez-vous

### 2️⃣ Récupérer vos credentials
Une fois connecté :

1. Vous êtes automatiquement sur le **Dashboard**
2. Vous verrez une section **"Account Details"** avec :
   - **Cloud Name** (exemple: `dab12345cd`)
   - **API Key** (exemple: `123456789012345`)
   - **API Secret** (exemple: `abcdefghijklmnopqrstuvwxyz123`)

### 3️⃣ Configurer l'application

Ouvrez le fichier **`.env`** à la racine du projet et remplacez :

```env
CLOUDINARY_CLOUD_NAME=your_cloud_name_here
CLOUDINARY_API_KEY=your_api_key_here
CLOUDINARY_API_SECRET=your_api_secret_here
```

Par vos vraies valeurs :

```env
CLOUDINARY_CLOUD_NAME=dab12345cd
CLOUDINARY_API_KEY=123456789012345
CLOUDINARY_API_SECRET=abcdefghijklmnopqrstuvwxyz123
```

### 4️⃣ Redémarrer les services

```powershell
docker-compose restart core-api media-service
```

---

## ✅ C'est tout !

Votre application peut maintenant uploader des photos ! 🎉

---

## 💡 Plan Gratuit Cloudinary

Le plan gratuit offre :
- ✅ 25 Go de stockage
- ✅ 25 Go de bande passante/mois
- ✅ Transformations d'images illimitées

**Parfait pour le développement et les petits projets !**

