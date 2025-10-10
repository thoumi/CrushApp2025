# Script pour créer le fichier .env automatiquement

Write-Host "🔧 Configuration de l'environnement..." -ForegroundColor Cyan
Write-Host ""

$envContent = @"
# Configuration Cloudinary pour Dating App
CLOUDINARY_CLOUD_NAME=dwbnufbrg
CLOUDINARY_API_KEY=REDACTED_CLOUDINARY_API_KEY
CLOUDINARY_API_SECRET=REDACTED_CLOUDINARY_API_SECRET
"@

$envPath = Join-Path $PSScriptRoot ".env"

if (Test-Path $envPath) {
    Write-Host "⚠️  Le fichier .env existe déjà." -ForegroundColor Yellow
    $response = Read-Host "Voulez-vous le remplacer? (O/N)"
    if ($response -ne "O" -and $response -ne "o") {
        Write-Host "❌ Opération annulée." -ForegroundColor Red
        exit 0
    }
}

try {
    $envContent | Out-File -FilePath $envPath -Encoding utf8 -NoNewline
    Write-Host "✅ Fichier .env créé avec succès!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📝 Contenu du fichier .env:" -ForegroundColor Cyan
    Write-Host $envContent -ForegroundColor Gray
    Write-Host ""
    Write-Host "✅ Vous pouvez maintenant lancer: docker-compose up -d" -ForegroundColor Green
}
catch {
    Write-Host "❌ Erreur lors de la création du fichier .env: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

