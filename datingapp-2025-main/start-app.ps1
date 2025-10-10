# Script de démarrage automatique de l'application Dating App

param(
    [switch]$Docker,
    [switch]$Local,
    [switch]$Stop
)

function Show-Menu {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║      DATING APP - SCRIPT DE DÉMARRAGE             ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Choisissez une option:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  [1] 🐳 Lancer avec Docker Compose (RECOMMANDÉ)" -ForegroundColor Green
    Write-Host "      → Tous les services (API, DB, Frontend, etc.)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  [2] 🛠️  Lancer en mode développement local" -ForegroundColor Yellow
    Write-Host "      → API + Frontend (SQL Server via Docker)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  [3] 🔍 Diagnostic / Tester l'API" -ForegroundColor Blue
    Write-Host ""
    Write-Host "  [4] 🛑 Arrêter tous les services Docker" -ForegroundColor Red
    Write-Host ""
    Write-Host "  [0] ❌ Quitter" -ForegroundColor Gray
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
    $choice = Read-Host "Votre choix"
    return $choice
}

function Start-WithDocker {
    Write-Host ""
    Write-Host "🐳 Démarrage avec Docker Compose..." -ForegroundColor Cyan
    Write-Host ""
    
    # Vérifier si .env existe
    if (-not (Test-Path ".env")) {
        Write-Host "⚠️  Le fichier .env n'existe pas." -ForegroundColor Yellow
        Write-Host "📝 Création du fichier .env..." -ForegroundColor Yellow
        & .\setup-env.ps1
    }
    
    # Vérifier que Docker est lancé
    try {
        docker ps | Out-Null
    }
    catch {
        Write-Host "❌ Docker n'est pas lancé!" -ForegroundColor Red
        Write-Host "   Veuillez lancer Docker Desktop d'abord." -ForegroundColor Red
        Read-Host "Appuyez sur Entrée pour continuer"
        return
    }
    
    Write-Host "🚀 Lancement de tous les services..." -ForegroundColor Green
    docker-compose up -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Services démarrés avec succès!" -ForegroundColor Green
        Write-Host ""
        Write-Host "⏳ Attendez environ 30-60 secondes que tout démarre..." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "📊 Services disponibles:" -ForegroundColor Cyan
        Write-Host "   • Frontend:        http://localhost:4200" -ForegroundColor Gray
        Write-Host "   • API Gateway:     http://localhost:5000/info" -ForegroundColor Gray
        Write-Host "   • Core API:        http://localhost:5001/info" -ForegroundColor Gray
        Write-Host "   • RabbitMQ UI:     http://localhost:15672 (admin/REDACTED_RABBITMQ_PASSWORD)" -ForegroundColor Gray
        Write-Host "   • Seq Logs:        http://localhost:5341" -ForegroundColor Gray
        Write-Host ""
        Write-Host "🔑 Comptes de test: admin@test.com, lisa@test.com, karen@test.com" -ForegroundColor Cyan
        Write-Host "   Mot de passe: Pa`$`$w0rd" -ForegroundColor Cyan
        Write-Host ""
        
        $openBrowser = Read-Host "Voulez-vous ouvrir l'application dans le navigateur? (O/N)"
        if ($openBrowser -eq "O" -or $openBrowser -eq "o") {
            Start-Sleep -Seconds 5
            Start-Process "http://localhost:4200"
        }
    }
    else {
        Write-Host "❌ Erreur lors du démarrage des services" -ForegroundColor Red
    }
    
    Read-Host "`nAppuyez sur Entrée pour continuer"
}

function Start-LocalDev {
    Write-Host ""
    Write-Host "🛠️  Démarrage en mode développement local..." -ForegroundColor Cyan
    Write-Host ""
    
    # Vérifier Docker
    try {
        docker ps | Out-Null
    }
    catch {
        Write-Host "❌ Docker n'est pas lancé!" -ForegroundColor Red
        Write-Host "   Nous avons besoin de Docker pour SQL Server." -ForegroundColor Red
        Read-Host "Appuyez sur Entrée pour continuer"
        return
    }
    
    # Vérifier si SQL Server existe déjà
    $sqlExists = docker ps -a --filter "name=sqlserver-dev" --format "{{.Names}}"
    
    if ($sqlExists) {
        Write-Host "✅ Conteneur SQL Server trouvé, démarrage..." -ForegroundColor Green
        docker start sqlserver-dev
    }
    else {
        Write-Host "📦 Création du conteneur SQL Server..." -ForegroundColor Yellow
        docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=REDACTED_DB_PASSWORD" `
            -p 1433:1433 --name sqlserver-dev `
            -d mcr.microsoft.com/mssql/server:2022-latest
    }
    
    Write-Host ""
    Write-Host "✅ SQL Server démarré sur localhost:1433" -ForegroundColor Green
    Write-Host ""
    Write-Host "⚠️  IMPORTANT: Vous devez maintenant:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Ouvrir un nouveau terminal et lancer l'API:" -ForegroundColor Cyan
    Write-Host "   cd API" -ForegroundColor Gray
    Write-Host "   dotnet run" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. Ouvrir un autre terminal et lancer le Frontend:" -ForegroundColor Cyan
    Write-Host "   cd client" -ForegroundColor Gray
    Write-Host "   ng serve" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. Vérifier que client/src/environments/environment.development.ts" -ForegroundColor Cyan
    Write-Host "   pointe vers: https://localhost:5001/api" -ForegroundColor Gray
    Write-Host ""
    
    Read-Host "Appuyez sur Entrée pour continuer"
}

function Run-Diagnostic {
    Write-Host ""
    Write-Host "🔍 Exécution du diagnostic..." -ForegroundColor Cyan
    Write-Host ""
    
    if (Test-Path ".\test-api.ps1") {
        & .\test-api.ps1
    }
    else {
        Write-Host "❌ Le script test-api.ps1 est introuvable!" -ForegroundColor Red
    }
    
    Read-Host "`nAppuyez sur Entrée pour continuer"
}

function Stop-AllServices {
    Write-Host ""
    Write-Host "🛑 Arrêt de tous les services Docker..." -ForegroundColor Red
    Write-Host ""
    
    docker-compose down
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Tous les services ont été arrêtés" -ForegroundColor Green
    }
    else {
        Write-Host "❌ Erreur lors de l'arrêt des services" -ForegroundColor Red
    }
    
    Read-Host "`nAppuyez sur Entrée pour continuer"
}

# Script principal
if ($Docker) {
    Start-WithDocker
    exit 0
}

if ($Local) {
    Start-LocalDev
    exit 0
}

if ($Stop) {
    Stop-AllServices
    exit 0
}

# Menu interactif
while ($true) {
    $choice = Show-Menu
    
    switch ($choice) {
        "1" { Start-WithDocker }
        "2" { Start-LocalDev }
        "3" { Run-Diagnostic }
        "4" { Stop-AllServices }
        "0" {
            Write-Host ""
            Write-Host "👋 Au revoir!" -ForegroundColor Cyan
            exit 0
        }
        default {
            Write-Host "❌ Choix invalide!" -ForegroundColor Red
            Start-Sleep -Seconds 1
        }
    }
}

