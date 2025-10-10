# ============================================
# Script de Démarrage de l'Infrastructure
# CrushApp Microservices
# ============================================

Write-Host "🚀 Démarrage de l'infrastructure CrushApp..." -ForegroundColor Cyan
Write-Host ""

# Vérifier si Docker est lancé
Write-Host "✓ Vérification de Docker..." -ForegroundColor Yellow
try {
    docker version | Out-Null
    Write-Host "  Docker est actif ✓" -ForegroundColor Green
}
catch {
    Write-Host "  ❌ Docker n'est pas lancé. Lancez Docker Desktop et réessayez." -ForegroundColor Red
    exit 1
}

Write-Host ""

# Vérifier si le fichier .env existe
if (!(Test-Path ".env")) {
    Write-Host "⚠️  Fichier .env non trouvé" -ForegroundColor Yellow
    Write-Host "  Création depuis .env.example..." -ForegroundColor Yellow
    
    if (Test-Path ".env.example") {
        Copy-Item ".env.example" ".env"
        Write-Host "  ✓ Fichier .env créé. Veuillez le remplir avec vos credentials." -ForegroundColor Green
        Write-Host "  📝 Important : Ajoutez vos clés Cloudinary dans .env" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Appuyez sur une touche pour continuer après avoir édité .env..."
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    }
    else {
        Write-Host "  ❌ .env.example non trouvé" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "📦 Phase 1 : Démarrage de l'infrastructure de base..." -ForegroundColor Cyan
Write-Host ""

# Arrêter les conteneurs existants
Write-Host "🛑 Arrêt des conteneurs existants..." -ForegroundColor Yellow
docker-compose down 2>&1 | Out-Null

Write-Host ""

# Démarrer les services d'infrastructure uniquement
Write-Host "▶️  Démarrage des services :" -ForegroundColor Yellow
Write-Host "   - SQL Server" -ForegroundColor White
Write-Host "   - RabbitMQ" -ForegroundColor White
Write-Host "   - Seq" -ForegroundColor White
Write-Host ""

docker-compose up -d sqlserver rabbitmq seq

Write-Host ""
Write-Host "⏳ Attente du démarrage des services (30 secondes)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host ""
Write-Host "📊 État des services :" -ForegroundColor Cyan
docker-compose ps

Write-Host ""
Write-Host "✅ Infrastructure démarrée !" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Accès aux services :" -ForegroundColor Cyan
Write-Host "   • RabbitMQ Management UI : http://localhost:15672" -ForegroundColor White
Write-Host "     Username: admin" -ForegroundColor Gray
Write-Host "     Password: REDACTED_RABBITMQ_PASSWORD" -ForegroundColor Gray
Write-Host ""
Write-Host "   • Seq Logs : http://localhost:5341" -ForegroundColor White
Write-Host "     Username: admin" -ForegroundColor Gray
Write-Host "     Password: admin" -ForegroundColor Gray
Write-Host ""
Write-Host "   • SQL Server : localhost:1433" -ForegroundColor White
Write-Host "     Username: sa" -ForegroundColor Gray
Write-Host "     Password: REDACTED_DB_PASSWORD" -ForegroundColor Gray
Write-Host ""

Write-Host "📝 Prochaines étapes :" -ForegroundColor Cyan
Write-Host "   1. Vérifier RabbitMQ : http://localhost:15672" -ForegroundColor White
Write-Host "   2. Vérifier Seq : http://localhost:5341" -ForegroundColor White
Write-Host "   3. Builder l'API Core : docker-compose build core-api" -ForegroundColor White
Write-Host "   4. Lancer l'API : docker-compose up -d core-api" -ForegroundColor White
Write-Host ""

Write-Host "🛠️  Commandes utiles :" -ForegroundColor Cyan
Write-Host "   • Voir les logs : docker-compose logs -f" -ForegroundColor White
Write-Host "   • Arrêter tout : docker-compose down" -ForegroundColor White
Write-Host "   • Redémarrer : docker-compose restart" -ForegroundColor White
Write-Host ""

Write-Host "✨ Infrastructure prête pour la Phase 2 !" -ForegroundColor Green

