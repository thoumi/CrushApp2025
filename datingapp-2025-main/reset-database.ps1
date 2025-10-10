# Script pour réinitialiser la base de données et relancer le seeding
Write-Host "🔄 Réinitialisation de la base de données..." -ForegroundColor Yellow

# Arrêter l'API si elle tourne
Write-Host "⏹️ Arrêt de l'API..." -ForegroundColor Yellow
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -like "*API*" } | Stop-Process -Force -ErrorAction SilentlyContinue

# Attendre un peu
Start-Sleep -Seconds 3

# Se connecter à SQL Server et vider les tables
Write-Host "🗑️ Nettoyage de la base de données..." -ForegroundColor Yellow
$connectionString = "Server=localhost,1433;User Id=SA;Password=REDACTED_DB_PASSWORD;Database=CrushAppDb;TrustServerCertificate=True"

# Script SQL pour nettoyer la base
$cleanupScript = @"
USE CrushAppDb;
GO

-- Supprimer les données dans l'ordre des dépendances
DELETE FROM AspNetUserTokens;
DELETE FROM AspNetUserRoles;
DELETE FROM AspNetUserLogins;
DELETE FROM AspNetUserClaims;
DELETE FROM AspNetRoleClaims;
DELETE FROM AspNetUsers;
DELETE FROM AspNetRoles;
DELETE FROM Connections;
DELETE FROM Messages;
DELETE FROM Likes;
DELETE FROM Photos;
DELETE FROM Members;

-- Réinitialiser les identités
DBCC CHECKIDENT ('Members', RESEED, 0);
DBCC CHECKIDENT ('Photos', RESEED, 0);

PRINT 'Base de données nettoyée avec succès';
GO
"@

# Exécuter le script de nettoyage
try {
    sqlcmd -S localhost,1433 -U SA -P "REDACTED_DB_PASSWORD" -d CrushAppDb -Q $cleanupScript
    Write-Host "✅ Base de données nettoyée" -ForegroundColor Green
} catch {
    Write-Host "❌ Erreur lors du nettoyage: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Relancer l'API
Write-Host "🚀 Relance de l'API..." -ForegroundColor Yellow
Set-Location "API"
Start-Process -FilePath "dotnet" -ArgumentList "run" -WindowStyle Hidden

# Attendre que l'API démarre
Write-Host "⏳ Attente du démarrage de l'API..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Tester l'API
Write-Host "🧪 Test de l'API..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5001/api/account/check-users" -Method GET
    Write-Host "✅ API fonctionne - Utilisateurs: $($response.userCount)" -ForegroundColor Green
} catch {
    Write-Host "❌ Erreur API: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "🎉 Réinitialisation terminée!" -ForegroundColor Green
