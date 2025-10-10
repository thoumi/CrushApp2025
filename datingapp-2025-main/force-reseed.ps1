# Script pour forcer le re-seeding de la base de données
Write-Host "🔄 Forçage du re-seeding..." -ForegroundColor Yellow

# Arrêter l'API
Write-Host "⏹️ Arrêt de l'API..." -ForegroundColor Yellow
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# Supprimer tous les utilisateurs sauf l'admin
Write-Host "🗑️ Suppression des utilisateurs existants..." -ForegroundColor Yellow
$cleanupScript = @"
USE CrushAppDb;
DELETE FROM AspNetUserTokens WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email != 'admin@test.com');
DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email != 'admin@test.com');
DELETE FROM AspNetUserLogins WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email != 'admin@test.com');
DELETE FROM AspNetUserClaims WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email != 'admin@test.com');
DELETE FROM AspNetUsers WHERE Email != 'admin@test.com';
DELETE FROM Members WHERE Id IN (SELECT Id FROM AspNetUsers WHERE Email != 'admin@test.com');
DELETE FROM Photos WHERE MemberId IN (SELECT Id FROM AspNetUsers WHERE Email != 'admin@test.com');
PRINT 'Utilisateurs supprimés (sauf admin)';
"@

try {
    sqlcmd -S localhost,1433 -U SA -P "REDACTED_DB_PASSWORD" -d CrushAppDb -Q $cleanupScript
    Write-Host "✅ Utilisateurs supprimés" -ForegroundColor Green
} catch {
    Write-Host "❌ Erreur lors de la suppression: $($_.Exception.Message)" -ForegroundColor Red
}

# Relancer l'API
Write-Host "🚀 Relance de l'API..." -ForegroundColor Yellow
Set-Location "API"
Start-Process -FilePath "dotnet" -ArgumentList "run" -WindowStyle Hidden

# Attendre le démarrage
Write-Host "⏳ Attente du démarrage..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Vérifier le nombre d'utilisateurs
Write-Host "🧪 Vérification du nombre d'utilisateurs..." -ForegroundColor Yellow
$userCount = sqlcmd -S localhost,1433 -U SA -P "REDACTED_DB_PASSWORD" -d CrushAppDb -Q "SELECT COUNT(*) FROM AspNetUsers" -h -1
Write-Host "Nombre d'utilisateurs: $userCount" -ForegroundColor Cyan

if ($userCount -ge 30) {
    Write-Host "✅ Re-seeding réussi!" -ForegroundColor Green
} else {
    Write-Host "❌ Re-seeding échoué - seulement $userCount utilisateurs" -ForegroundColor Red
}

Write-Host "🎉 Script terminé!" -ForegroundColor Green
