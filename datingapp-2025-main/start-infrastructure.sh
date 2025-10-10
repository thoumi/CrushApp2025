#!/bin/bash
# ============================================
# Script de Démarrage de l'Infrastructure
# CrushApp Microservices (Linux/Mac)
# ============================================

echo "🚀 Démarrage de l'infrastructure CrushApp..."
echo ""

# Vérifier si Docker est lancé
echo "✓ Vérification de Docker..."
if ! docker version > /dev/null 2>&1; then
    echo "  ❌ Docker n'est pas lancé. Lancez Docker et réessayez."
    exit 1
fi
echo "  Docker est actif ✓"

echo ""

# Vérifier si le fichier .env existe
if [ ! -f ".env" ]; then
    echo "⚠️  Fichier .env non trouvé"
    echo "  Création depuis .env.example..."
    
    if [ -f ".env.example" ]; then
        cp .env.example .env
        echo "  ✓ Fichier .env créé. Veuillez le remplir avec vos credentials."
        echo "  📝 Important : Ajoutez vos clés Cloudinary dans .env"
        echo ""
        echo "Appuyez sur Entrée pour continuer après avoir édité .env..."
        read
    else
        echo "  ❌ .env.example non trouvé"
        exit 1
    fi
fi

echo ""
echo "📦 Phase 1 : Démarrage de l'infrastructure de base..."
echo ""

# Arrêter les conteneurs existants
echo "🛑 Arrêt des conteneurs existants..."
docker-compose down > /dev/null 2>&1

echo ""

# Démarrer les services d'infrastructure uniquement
echo "▶️  Démarrage des services :"
echo "   - SQL Server"
echo "   - RabbitMQ"
echo "   - Seq"
echo ""

docker-compose up -d sqlserver rabbitmq seq

echo ""
echo "⏳ Attente du démarrage des services (30 secondes)..."
sleep 30

echo ""
echo "📊 État des services :"
docker-compose ps

echo ""
echo "✅ Infrastructure démarrée !"
echo ""
echo "🌐 Accès aux services :"
echo "   • RabbitMQ Management UI : http://localhost:15672"
echo "     Username: admin"
echo "     Password: REDACTED_RABBITMQ_PASSWORD"
echo ""
echo "   • Seq Logs : http://localhost:5341"
echo "     Username: admin"
echo "     Password: admin"
echo ""
echo "   • SQL Server : localhost:1433"
echo "     Username: sa"
echo "     Password: REDACTED_DB_PASSWORD"
echo ""

echo "📝 Prochaines étapes :"
echo "   1. Vérifier RabbitMQ : http://localhost:15672"
echo "   2. Vérifier Seq : http://localhost:5341"
echo "   3. Builder l'API Core : docker-compose build core-api"
echo "   4. Lancer l'API : docker-compose up -d core-api"
echo ""

echo "🛠️  Commandes utiles :"
echo "   • Voir les logs : docker-compose logs -f"
echo "   • Arrêter tout : docker-compose down"
echo "   • Redémarrer : docker-compose restart"
echo ""

echo "✨ Infrastructure prête pour la Phase 2 !"

