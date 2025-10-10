# Script de diagnostic pour tester l'API Dating App

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  DIAGNOSTIC API DATING APP" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Vérifier si l'API est en ligne
Write-Host "[1/4] Test de connectivité API..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/info" -Method Get -UseBasicParsing -ErrorAction Stop
    Write-Host "✅ API est en ligne!" -ForegroundColor Green
    $info = $response.Content | ConvertFrom-Json
    Write-Host "   Service: $($info.service)" -ForegroundColor Gray
    Write-Host "   Version: $($info.version)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "❌ API n'est pas accessible sur http://localhost:5000" -ForegroundColor Red
    Write-Host "   Assurez-vous que l'API est lancée!" -ForegroundColor Red
    Write-Host ""
    exit 1
}

# Test 2: Vérifier les utilisateurs dans la base de données
Write-Host "[2/4] Vérification des utilisateurs dans la base de données..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/account/check-users" -Method Get -UseBasicParsing -ErrorAction Stop
    $userData = $response.Content | ConvertFrom-Json
    
    Write-Host "✅ Base de données accessible!" -ForegroundColor Green
    Write-Host "   Nombre d'utilisateurs: $($userData.TotalUsers)" -ForegroundColor Gray
    
    if ($userData.TotalUsers -eq 0) {
        Write-Host "⚠️  ATTENTION: Aucun utilisateur dans la base de données!" -ForegroundColor Red
        Write-Host "   Le seed n'a pas fonctionné. Redémarrez l'API." -ForegroundColor Red
    } else {
        Write-Host "   Exemples d'utilisateurs:" -ForegroundColor Gray
        $userData.SampleUsers | ForEach-Object {
            Write-Host "     - $($_.email) ($($_.displayName))" -ForegroundColor Gray
        }
    }
    Write-Host ""
}
catch {
    Write-Host "❌ Erreur lors de la vérification des utilisateurs" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# Test 3: Tester la connexion avec Lisa
Write-Host "[3/4] Test de connexion avec lisa@test.com..." -ForegroundColor Yellow
try {
    $loginData = @{
        email = "lisa@test.com"
        password = "Pa`$`$w0rd"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/account/login" `
        -Method Post `
        -Body $loginData `
        -ContentType "application/json" `
        -UseBasicParsing `
        -ErrorAction Stop

    $user = $response.Content | ConvertFrom-Json
    Write-Host "✅ Connexion réussie!" -ForegroundColor Green
    Write-Host "   Email: $($user.email)" -ForegroundColor Gray
    Write-Host "   Display Name: $($user.displayName)" -ForegroundColor Gray
    Write-Host "   Token: $($user.token.Substring(0, 50))..." -ForegroundColor Gray
    Write-Host ""
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "❌ Échec de connexion (Code: $statusCode)" -ForegroundColor Red
    
    if ($statusCode -eq 401) {
        Write-Host "   Email ou mot de passe incorrect" -ForegroundColor Red
        Write-Host "   OU les utilisateurs n'ont pas été seedés" -ForegroundColor Red
    }
    
    Write-Host "   Détails: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# Test 4: Tester la connexion admin
Write-Host "[4/4] Test de connexion admin..." -ForegroundColor Yellow
try {
    $loginData = @{
        email = "admin@test.com"
        password = "Pa`$`$w0rd"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/account/login" `
        -Method Post `
        -Body $loginData `
        -ContentType "application/json" `
        -UseBasicParsing `
        -ErrorAction Stop

    $user = $response.Content | ConvertFrom-Json
    Write-Host "✅ Connexion admin réussie!" -ForegroundColor Green
    Write-Host "   Email: $($user.email)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "❌ Échec de connexion admin" -ForegroundColor Red
    Write-Host ""
}

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  FIN DU DIAGNOSTIC" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "COMPTES DE TEST DISPONIBLES:" -ForegroundColor Yellow
Write-Host "  - admin@test.com / Pa`$`$w0rd (Admin)" -ForegroundColor Gray
Write-Host "  - lisa@test.com / Pa`$`$w0rd (Membre)" -ForegroundColor Gray
Write-Host "  - karen@test.com / Pa`$`$w0rd (Membre)" -ForegroundColor Gray
Write-Host "  - todd@test.com / Pa`$`$w0rd (Membre)" -ForegroundColor Gray
Write-Host ""

